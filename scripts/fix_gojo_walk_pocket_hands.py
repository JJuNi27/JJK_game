r"""Build a non-destructive Gojo pocket-walk hand correction in Blender 5.2+.

Architecture:
  * Copy Gojo_Walk_Pocket_ShortStride so lower-body curves remain byte-for-byte inherited.
  * Read only elbow/wrist world positions from Gojo_Walk_Pocket_Final_v3.
  * Aim Arm and ForeArm at those positional targets with world-space swing quaternions.
  * Rebuild Hand at the corrected wrist translation with the clean ShortStride world rotation.
  * Copy each Finger's complete Gojo_Idle frame-2 matrix_basis; never guess or add Euler axes.

Blender Text Editor usage:
    exec(compile(open(r'D:\JJK_game\scripts\fix_gojo_walk_pocket_hands.py',
                      encoding='utf-8').read(), 'hand_fix', 'exec'))
    build_hand_fix(apply=False)  # validation/report only
    build_hand_fix(apply=True)   # creates Gojo_Walk_Pocket_HandFix in memory

The script never saves the .blend, exports FBX, deletes Actions, or modifies either source Action.
Inspect the new Action over frames 1..38, then save manually only if accepted.
"""

import math
import sys

import bpy
from mathutils import Matrix


ARMATURE_NAME = "Armature"
IDLE_ACTION = "Gojo_Idle"
SOURCE_ACTION = "Gojo_Walk_Pocket_ShortStride"
# This Action contains useful small Arm/ForeArm placement edits but unsafe Hand/Finger edits.
POSITION_GUIDE_ACTION = "Gojo_Walk_Pocket_Final_v3"
OUTPUT_ACTION = "Gojo_Walk_Pocket_HandFix"
IDLE_FRAME = 2
FRAME_START = 1
FRAME_END = 38
# Additional position-only pocket depth, expressed in scene world units. Directions are derived
# geometrically from both hip joints and the spine, never from guessed Hand/Finger Euler axes.
EXTRA_WRIST_INWARD = 0.010
EXTRA_WRIST_BACK = 0.004
SIDES = ("Left", "Right")
FINGERS = ("Thumb", "Index", "Middle", "Ring", "Pinky")


def _bone(side, suffix):
    return f"mixamorig:{side}{suffix}"


def _finger(side, finger, segment):
    return f"mixamorig:{side}Hand{finger}{segment}"


def _set_action(animation, action):
    animation.action = action
    object_slots = [slot for slot in action.slots if slot.target_id_type == "OBJECT"]
    if not object_slots:
        raise RuntimeError(f"Action {action.name!r} has no OBJECT slot")
    animation.action_slot = object_slots[0]


def _evaluate(armature, action, frame):
    _set_action(armature.animation_data, action)
    bpy.context.scene.frame_set(frame)
    bpy.context.view_layer.update()


def _world_matrix(armature, pose_bone):
    return armature.matrix_world @ pose_bone.matrix


def _world_head(armature, pose_bone):
    return armature.matrix_world @ pose_bone.head


def _world_tail(armature, pose_bone):
    return armature.matrix_world @ pose_bone.tail


def _body_axes(armature):
    left_hip = _world_head(armature, armature.pose.bones["mixamorig:LeftUpLeg"])
    right_hip = _world_head(armature, armature.pose.bones["mixamorig:RightUpLeg"])
    hips = _world_head(armature, armature.pose.bones["mixamorig:Hips"])
    spine_top = _world_tail(armature, armature.pose.bones["mixamorig:Spine2"])
    lateral = (left_hip - right_hip).normalized()  # character right -> left
    up = (spine_top - hips).normalized()
    forward = lateral.cross(up).normalized()
    return lateral, up, forward


def _angular_difference_degrees(a, b):
    return math.degrees(a.rotation_difference(b).angle)


def _matrix_max_abs_difference(a, b):
    return max(abs(a[row][column] - b[row][column])
               for row in range(4) for column in range(4))


def _set_world_rotation(armature, pose_bone, world_rotation):
    current = _world_matrix(armature, pose_bone)
    desired = Matrix.LocRotScale(current.translation, world_rotation, current.to_scale())
    pose_bone.matrix = armature.matrix_world.inverted_safe() @ desired


def _aim_bone_at_world_point(armature, pose_bone, target):
    head = _world_head(armature, pose_bone)
    tail = _world_tail(armature, pose_bone)
    current_direction = tail - head
    desired_direction = target - head
    if current_direction.length < 1e-8 or desired_direction.length < 1e-8:
        raise RuntimeError(f"Degenerate aim for {pose_bone.name}")
    swing = current_direction.normalized().rotation_difference(desired_direction.normalized())
    _set_world_rotation(armature, pose_bone, swing @ _world_matrix(armature, pose_bone).to_quaternion())


def _key_rotation(pose_bone, frame, previous_quaternion):
    if pose_bone.rotation_mode != "QUATERNION":
        raise RuntimeError(
            f"{pose_bone.name} uses {pose_bone.rotation_mode}; refusing to guess Euler axes")
    quaternion = pose_bone.rotation_quaternion.copy().normalized()
    if previous_quaternion is not None:
        quaternion.make_compatible(previous_quaternion)
    pose_bone.rotation_quaternion = quaternion
    pose_bone.keyframe_insert(
        data_path="rotation_quaternion", frame=frame, group=pose_bone.name)
    return quaternion.copy()


def _key_exact_basis(pose_bone, matrix_basis, frame):
    """Key Blender's decomposition of an exact basis matrix; never edit an Euler component."""
    pose_bone.matrix_basis = matrix_basis
    pose_bone.keyframe_insert(data_path="location", frame=frame, group=pose_bone.name)
    if pose_bone.rotation_mode == "QUATERNION":
        pose_bone.keyframe_insert(
            data_path="rotation_quaternion", frame=frame, group=pose_bone.name)
    elif pose_bone.rotation_mode == "AXIS_ANGLE":
        pose_bone.keyframe_insert(
            data_path="rotation_axis_angle", frame=frame, group=pose_bone.name)
    else:
        pose_bone.keyframe_insert(
            data_path="rotation_euler", frame=frame, group=pose_bone.name)
    pose_bone.keyframe_insert(data_path="scale", frame=frame, group=pose_bone.name)


def _capture_inputs(armature, idle, source, guide, extra_wrist_inward, extra_wrist_back):
    idle_finger_basis = {}
    _evaluate(armature, idle, IDLE_FRAME)
    for side in SIDES:
        for finger in FINGERS:
            for segment in range(1, 5):
                name = _finger(side, finger, segment)
                idle_finger_basis[name] = armature.pose.bones[name].matrix_basis.copy()

    frames = {}
    source_finger_max_difference = {name: {"value": 0.0, "frame": None}
                                    for name in idle_finger_basis}
    for frame in range(FRAME_START, FRAME_END + 1):
        _evaluate(armature, source, frame)
        source_frame = {"sides": {}}
        for side in SIDES:
            hand = armature.pose.bones[_bone(side, "Hand")]
            source_frame["sides"][side] = {
                "hand_world_rotation": _world_matrix(armature, hand).to_quaternion().normalized(),
                "source_elbow": _world_head(armature, armature.pose.bones[_bone(side, "ForeArm")]),
                "source_wrist": _world_head(armature, hand),
            }
        for name, reference in idle_finger_basis.items():
            difference = _matrix_max_abs_difference(armature.pose.bones[name].matrix_basis, reference)
            if difference > source_finger_max_difference[name]["value"]:
                source_finger_max_difference[name] = {"value": difference, "frame": frame}

        _evaluate(armature, guide, frame)
        lateral, _up, forward = _body_axes(armature)
        for side in SIDES:
            arm = armature.pose.bones[_bone(side, "Arm")]
            forearm = armature.pose.bones[_bone(side, "ForeArm")]
            hand = armature.pose.bones[_bone(side, "Hand")]
            source_side = source_frame["sides"][side]
            guide_shoulder = _world_head(armature, arm)
            _evaluate(armature, source, frame)
            source_shoulder = _world_head(armature, armature.pose.bones[_bone(side, "Arm")])
            if (guide_shoulder - source_shoulder).length > 1e-4:
                raise RuntimeError(
                    f"{POSITION_GUIDE_ACTION} changes the {side} shoulder parent position; "
                    "it is unsafe as a position-only guide")
            _evaluate(armature, guide, frame)
            source_side["target_elbow"] = _world_head(armature, forearm)
            inward = -lateral if side == "Left" else lateral
            back = -forward
            guide_wrist = _world_head(armature, hand)
            guide_forearm = guide_wrist - source_side["target_elbow"]
            raw_target_wrist = (
                guide_wrist
                + inward * extra_wrist_inward
                + back * extra_wrist_back)
            # Project onto the fixed-length forearm sphere. This converts the desired pocket
            # offset into a reachable direction without stretch or translation keys.
            desired_direction = raw_target_wrist - source_side["target_elbow"]
            source_side["target_wrist"] = (
                source_side["target_elbow"]
                + desired_direction.normalized() * guide_forearm.length)
        frames[frame] = source_frame
    return frames, source_finger_max_difference


def _validate_output(armature, output, frames, idle_finger_basis):
    report = {"frames": {}, "max_hand_world_orientation_error_degrees": 0.0,
              "max_elbow_target_error": 0.0, "max_wrist_target_error": 0.0,
              "max_finger_basis_matrix_error": 0.0}
    for frame in range(FRAME_START, FRAME_END + 1):
        _evaluate(armature, output, frame)
        frame_report = {}
        for side in SIDES:
            expected = frames[frame]["sides"][side]
            forearm = armature.pose.bones[_bone(side, "ForeArm")]
            hand = armature.pose.bones[_bone(side, "Hand")]
            elbow_error = (_world_head(armature, forearm) - expected["target_elbow"]).length
            wrist_error = (_world_head(armature, hand) - expected["target_wrist"]).length
            angle_error = _angular_difference_degrees(
                _world_matrix(armature, hand).to_quaternion().normalized(),
                expected["hand_world_rotation"])
            frame_report[side] = {
                "elbow_target_error": elbow_error,
                "wrist_target_error": wrist_error,
                "hand_world_orientation_error_degrees": angle_error,
            }
            report["max_elbow_target_error"] = max(report["max_elbow_target_error"], elbow_error)
            report["max_wrist_target_error"] = max(report["max_wrist_target_error"], wrist_error)
            report["max_hand_world_orientation_error_degrees"] = max(
                report["max_hand_world_orientation_error_degrees"], angle_error)
        for name, reference in idle_finger_basis.items():
            difference = _matrix_max_abs_difference(armature.pose.bones[name].matrix_basis, reference)
            report["max_finger_basis_matrix_error"] = max(
                report["max_finger_basis_matrix_error"], difference)
        report["frames"][frame] = frame_report
    return report


def build_hand_fix(*, apply=False, extra_wrist_inward=EXTRA_WRIST_INWARD,
                   extra_wrist_back=EXTRA_WRIST_BACK):
    armature = bpy.data.objects.get(ARMATURE_NAME)
    if armature is None or armature.type != "ARMATURE":
        raise RuntimeError(f"Armature object {ARMATURE_NAME!r} was not found")
    animation = armature.animation_data
    if animation is None:
        raise RuntimeError("Armature has no animation_data")
    idle = bpy.data.actions.get(IDLE_ACTION)
    source = bpy.data.actions.get(SOURCE_ACTION)
    guide = bpy.data.actions.get(POSITION_GUIDE_ACTION)
    if idle is None or source is None or guide is None:
        raise RuntimeError("Idle, source, and position-guide Actions must all exist")
    if OUTPUT_ACTION in bpy.data.actions:
        raise RuntimeError(f"Delete or rename the existing {OUTPUT_ACTION!r} manually first")
    if any(not track.mute for track in animation.nla_tracks):
        raise RuntimeError("Mute active NLA tracks before evaluating standalone Actions")
    if animation.drivers:
        raise RuntimeError("Armature animation drivers require manual review")
    constrained = [bone.name for bone in armature.pose.bones if bone.constraints]
    if constrained:
        raise RuntimeError("Constrained pose bones require manual review: " + ", ".join(constrained))

    required = [_bone(side, suffix) for side in SIDES for suffix in ("Arm", "ForeArm", "Hand")]
    required += [_finger(side, finger, segment) for side in SIDES
                 for finger in FINGERS for segment in range(1, 5)]
    missing = [name for name in required if name not in armature.pose.bones]
    if missing:
        raise RuntimeError("Missing required bones: " + ", ".join(missing))

    old_action = animation.action
    old_slot_identifier = animation.action_slot.identifier if animation.action_slot else None
    old_frame = bpy.context.scene.frame_current
    idle_finger_basis = {}
    _evaluate(armature, idle, IDLE_FRAME)
    for side in SIDES:
        for finger in FINGERS:
            for segment in range(1, 5):
                name = _finger(side, finger, segment)
                idle_finger_basis[name] = armature.pose.bones[name].matrix_basis.copy()
    if extra_wrist_inward < 0.0 or extra_wrist_back < 0.0:
        raise ValueError("Extra wrist offsets must be non-negative")
    frames, source_finger_max_difference = _capture_inputs(
        armature, idle, source, guide, extra_wrist_inward, extra_wrist_back)
    plan = {
        "source": SOURCE_ACTION,
        "position_guide": POSITION_GUIDE_ACTION,
        "output": OUTPUT_ACTION,
        "frames": [FRAME_START, FRAME_END],
        "modified_arm_bones": [_bone(side, suffix) for side in SIDES
                               for suffix in ("Arm", "ForeArm", "Hand")],
        "finger_policy": "copy complete Idle frame-2 matrix_basis; no Euler component edits",
        "extra_wrist_position_world_units": {
            "inward": extra_wrist_inward,
            "back": extra_wrist_back,
            "axis_source": "hip-joint lateral and spine-derived forward",
        },
        "source_finger_basis_mismatches": {
            name: value for name, value in source_finger_max_difference.items()
            if value["value"] > 1e-5
        },
        "save_or_export": False,
    }
    print("JJK_HAND_FIX_PLAN", plan)
    if not apply:
        _set_action(animation, old_action) if old_action is not None else setattr(animation, "action", None)
        if old_action is not None and old_slot_identifier is not None:
            slots = [slot for slot in old_action.slots if slot.identifier == old_slot_identifier]
            if slots:
                animation.action_slot = slots[0]
        bpy.context.scene.frame_set(old_frame)
        bpy.context.view_layer.update()
        return plan

    output = source.copy()
    output.name = OUTPUT_ACTION
    output.use_fake_user = True
    succeeded = False
    previous_quaternions = {_bone(side, suffix): None for side in SIDES
                            for suffix in ("Arm", "ForeArm", "Hand")}
    try:
        for frame in range(FRAME_START, FRAME_END + 1):
            _evaluate(armature, output, frame)
            for side in SIDES:
                targets = frames[frame]["sides"][side]
                arm = armature.pose.bones[_bone(side, "Arm")]
                forearm = armature.pose.bones[_bone(side, "ForeArm")]
                hand = armature.pose.bones[_bone(side, "Hand")]

                _aim_bone_at_world_point(armature, arm, targets["target_elbow"])
                previous_quaternions[arm.name] = _key_rotation(
                    arm, frame, previous_quaternions[arm.name])
                bpy.context.view_layer.update()

                _aim_bone_at_world_point(armature, forearm, targets["target_wrist"])
                previous_quaternions[forearm.name] = _key_rotation(
                    forearm, frame, previous_quaternions[forearm.name])
                bpy.context.view_layer.update()

                # Position comes from the corrected ForeArm. Rotation comes only from the clean
                # ShortStride frame, so parent rotations cannot roll the palm outward.
                _set_world_rotation(armature, hand, targets["hand_world_rotation"])
                previous_quaternions[hand.name] = _key_rotation(
                    hand, frame, previous_quaternions[hand.name])
                bpy.context.view_layer.update()

                # The reference matrix is authoritative. Blender may store its decomposition as
                # Euler for these bones, but no Euler axis is inferred, incremented, or mirrored.
                for finger in FINGERS:
                    for segment in range(1, 5):
                        finger_name = _finger(side, finger, segment)
                        _key_exact_basis(
                            armature.pose.bones[finger_name],
                            idle_finger_basis[finger_name], frame)
                bpy.context.view_layer.update()

        report = _validate_output(armature, output, frames, idle_finger_basis)
        print("JJK_HAND_FIX_VALIDATION", {
            key: value for key, value in report.items() if key != "frames"
        })
        if report["max_hand_world_orientation_error_degrees"] > 0.01:
            raise RuntimeError("Hand orientation preservation exceeded 0.01 degrees")
        if report["max_finger_basis_matrix_error"] > 1e-5:
            raise RuntimeError("Finger basis changed unexpectedly")
        _evaluate(armature, output, 12)
        succeeded = True
        return {"plan": plan, "validation": report, "created_action": output.name}
    finally:
        if not succeeded:
            animation.action = old_action
            if output.name in bpy.data.actions:
                bpy.data.actions.remove(output)
            if old_action is not None and old_slot_identifier is not None:
                slots = [slot for slot in old_action.slots if slot.identifier == old_slot_identifier]
                if slots:
                    animation.action_slot = slots[0]
            bpy.context.scene.frame_set(old_frame)
            bpy.context.view_layer.update()


if __name__ == "__main__":
    # Command-line execution defaults to a dry run; --apply is still memory-only.
    build_hand_fix(apply="--apply" in sys.argv)
