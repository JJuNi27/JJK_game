"""Read-only Gojo hand/arm Action diagnostics for Blender 5.2+.

Open Gojo_Animation_Work.blend, then run this file from Blender's Text Editor, or:

    blender --background Gojo_Animation_Work.blend --python diagnose_gojo_walk_hands.py

The script temporarily evaluates Actions and frames in memory, restores the original Action,
Action slot, and frame, and prints JSON between the JJK_HAND_DIAGNOSTIC markers. It does not
create/delete Actions, insert keys, save the .blend, or export anything.
"""

import json
import math
import sys

import bpy
from mathutils import Matrix, Quaternion, Vector


ARMATURE_NAME = "Armature"
IDLE_ACTION = "Gojo_Idle"
IDLE_FRAME = 2
DIAGNOSTIC_FRAME = 12
LOOP_START = 1
LOOP_END = 38

SIDES = ("Left", "Right")
CHAIN_SUFFIXES = ("Shoulder", "Arm", "ForeArm", "Hand")
FINGER_NAMES = ("Thumb", "Index", "Middle", "Ring", "Pinky")
MIRROR_X = Matrix(((-1.0, 0.0, 0.0),
                   (0.0, 1.0, 0.0),
                   (0.0, 0.0, 1.0)))


def _bone(side, suffix):
    return f"mixamorig:{side}{suffix}"


def _finger(side, finger, segment):
    return f"mixamorig:{side}Hand{finger}{segment}"


def _vec(value):
    return [round(float(component), 7) for component in value]


def _quat(value):
    value = value.normalized()
    return [round(float(value.w), 7), round(float(value.x), 7),
            round(float(value.y), 7), round(float(value.z), 7)]


def _matrix(value):
    return [[round(float(component), 7) for component in row] for row in value]


def _angle_degrees(rotation_a, rotation_b):
    return math.degrees(rotation_a.rotation_difference(rotation_b).angle)


def _world_matrix(armature, pose_bone):
    return armature.matrix_world @ pose_bone.matrix


def _head_tail_world(armature, pose_bone):
    head = armature.matrix_world @ pose_bone.head
    tail = armature.matrix_world @ pose_bone.tail
    vector = tail - head
    return head, tail, vector


def _axes(world_matrix):
    rotation = world_matrix.to_3x3().normalized()
    return {
        "x": _vec(rotation.col[0].normalized()),
        "y": _vec(rotation.col[1].normalized()),
        "z": _vec(rotation.col[2].normalized()),
    }


def _evaluate_action(armature, action, frame):
    animation = armature.animation_data
    animation.action = action
    if len(action.slots):
        object_slots = [slot for slot in action.slots if slot.target_id_type == "OBJECT"]
        if not object_slots:
            raise RuntimeError(f"Action {action.name!r} has no OBJECT slot")
        animation.action_slot = object_slots[0]
    bpy.context.scene.frame_set(frame)
    bpy.context.view_layer.update()


def _bone_snapshot(armature, pose_bone):
    world = _world_matrix(armature, pose_bone)
    head, tail, vector = _head_tail_world(armature, pose_bone)
    if pose_bone.parent:
        parent_relative = pose_bone.parent.matrix.inverted_safe() @ pose_bone.matrix
    else:
        parent_relative = pose_bone.matrix.copy()
    return {
        "matrix_basis": _matrix(pose_bone.matrix_basis),
        "pose_matrix_armature_space": _matrix(pose_bone.matrix),
        "world_matrix": _matrix(world),
        "world_position": _vec(head),
        "world_quaternion_wxyz": _quat(world.to_quaternion()),
        "parent_relative_quaternion_wxyz": _quat(parent_relative.to_quaternion()),
        "head_world": _vec(head),
        "tail_world": _vec(tail),
        "head_to_tail_world": _vec(vector),
        "local_axes_in_world": _axes(world),
    }


def _palm_geometry(armature, side):
    roots = {finger: armature.pose.bones[_finger(side, finger, 1)]
             for finger in FINGER_NAMES}
    forwards = {}
    root_heads = {}
    for finger, pose_bone in roots.items():
        head, _tail, vector = _head_tail_world(armature, pose_bone)
        root_heads[finger] = head
        forwards[finger] = vector.normalized() if vector.length else Vector((0.0, 0.0, 0.0))
    non_thumb = [forwards[name] for name in ("Index", "Middle", "Ring", "Pinky")]
    finger_forward = sum(non_thumb, Vector()).normalized()
    index_head = root_heads["Index"]
    pinky_head = root_heads["Pinky"]
    pinky_to_index = (index_head - pinky_head).normalized()
    wrist, _tail, _vector = _head_tail_world(armature, armature.pose.bones[_bone(side, "Hand")])
    palm_center = sum((root_heads[name] for name in ("Index", "Middle", "Ring", "Pinky")), Vector()) / 4.0
    palm_forward = (palm_center - wrist).normalized()
    palm_normal = pinky_to_index.cross(palm_forward).normalized()
    return {
        "finger_forward_world": _vec(finger_forward),
        "wrist_to_knuckle_center_world": _vec(palm_forward),
        "pinky_to_index_world": _vec(pinky_to_index),
        "geometric_palm_normal_world": _vec(palm_normal),
        "finger_root_forward_world": {name: _vec(vector) for name, vector in forwards.items()},
    }


def _side_snapshot(armature, side):
    chain = {}
    for suffix in CHAIN_SUFFIXES:
        pose_bone = armature.pose.bones[_bone(side, suffix)]
        chain[pose_bone.name] = _bone_snapshot(armature, pose_bone)
    finger_basis = {}
    for finger in FINGER_NAMES:
        finger_basis[finger] = {}
        for segment in range(1, 5):
            pose_bone = armature.pose.bones[_finger(side, finger, segment)]
            world = _world_matrix(armature, pose_bone)
            _head, _tail, vector = _head_tail_world(armature, pose_bone)
            finger_basis[finger][str(segment)] = {
                "matrix_basis_quaternion_wxyz": _quat(pose_bone.matrix_basis.to_quaternion()),
                "world_quaternion_wxyz": _quat(world.to_quaternion()),
                "head_to_tail_world": _vec(vector),
            }
    hand = armature.pose.bones[_bone(side, "Hand")]
    forearm = armature.pose.bones[_bone(side, "ForeArm")]
    hand_head, _tail, _vector = _head_tail_world(armature, hand)
    return {
        "chain": chain,
        "wrist_world_position": _vec(hand_head),
        "hand_local_axes_in_world": _axes(_world_matrix(armature, hand)),
        "forearm_local_axes_in_world": _axes(_world_matrix(armature, forearm)),
        "palm_geometry": _palm_geometry(armature, side),
        "fingers": finger_basis,
    }


def _capture(armature, action, frame):
    _evaluate_action(armature, action, frame)
    return {side: _side_snapshot(armature, side) for side in SIDES}


def _rotation_from_snapshot(side_snapshot, side, suffix, field="world_matrix"):
    name = _bone(side, suffix)
    return Matrix(side_snapshot["chain"][name][field]).to_quaternion()


def _position_from_snapshot(side_snapshot):
    return Vector(side_snapshot["wrist_world_position"])


def _comparison(snapshot, idle):
    result = {}
    for side in SIDES:
        side_result = {"bone_angular_difference_from_idle_degrees": {}}
        for suffix in CHAIN_SUFFIXES:
            current_world = _rotation_from_snapshot(snapshot[side], side, suffix)
            idle_world = _rotation_from_snapshot(idle[side], side, suffix)
            basis_name = _bone(side, suffix)
            current_basis = Matrix(snapshot[side]["chain"][basis_name]["matrix_basis"]).to_quaternion()
            idle_basis = Matrix(idle[side]["chain"][basis_name]["matrix_basis"]).to_quaternion()
            side_result["bone_angular_difference_from_idle_degrees"][suffix] = {
                "matrix_basis": round(_angle_degrees(idle_basis, current_basis), 5),
                "world": round(_angle_degrees(idle_world, current_world), 5),
            }
        side_result["wrist_offset_from_idle_world"] = _vec(
            _position_from_snapshot(snapshot[side]) - _position_from_snapshot(idle[side]))
        side_result["finger_basis_difference_from_idle_degrees"] = {}
        for finger in FINGER_NAMES:
            side_result["finger_basis_difference_from_idle_degrees"][finger] = {}
            for segment in range(1, 5):
                current = snapshot[side]["fingers"][finger][str(segment)]["matrix_basis_quaternion_wxyz"]
                reference = idle[side]["fingers"][finger][str(segment)]["matrix_basis_quaternion_wxyz"]
                current = Quaternion(current)
                reference = Quaternion(reference)
                side_result["finger_basis_difference_from_idle_degrees"][finger][str(segment)] = round(
                    _angle_degrees(reference, current), 5)
        result[side] = side_result

    # Compare each side's world-space rotation delta from its own Idle pose. This avoids
    # assuming that mirrored Mixamo bones use matching local axes.
    left_now = Matrix(snapshot["Left"]["chain"][_bone("Left", "Hand")]["world_matrix"]).to_3x3().normalized()
    left_idle = Matrix(idle["Left"]["chain"][_bone("Left", "Hand")]["world_matrix"]).to_3x3().normalized()
    right_now = Matrix(snapshot["Right"]["chain"][_bone("Right", "Hand")]["world_matrix"]).to_3x3().normalized()
    right_idle = Matrix(idle["Right"]["chain"][_bone("Right", "Hand")]["world_matrix"]).to_3x3().normalized()
    left_delta = left_now @ left_idle.inverted_safe()
    right_delta = right_now @ right_idle.inverted_safe()
    expected_right_delta = MIRROR_X @ left_delta @ MIRROR_X
    result["left_right_hand_delta_symmetry_degrees"] = round(
        _angle_degrees(expected_right_delta.to_quaternion(), right_delta.to_quaternion()), 5)
    left_offset = _position_from_snapshot(snapshot["Left"]) - _position_from_snapshot(idle["Left"])
    right_offset = _position_from_snapshot(snapshot["Right"]) - _position_from_snapshot(idle["Right"])
    result["left_right_wrist_offset_symmetry_error"] = round(
        (MIRROR_X @ left_offset - right_offset).length, 7)
    return result


def _loop_summary(armature, action, idle):
    summary = {side: {"max_hand_world_angle_from_idle_degrees": {"value": 0.0, "frame": None},
                      "max_hand_basis_angle_from_idle_degrees": {"value": 0.0, "frame": None},
                      "max_wrist_offset_from_idle": {"value": 0.0, "frame": None}}
               for side in SIDES}
    summary["max_hand_delta_symmetry_degrees"] = {"value": 0.0, "frame": None}
    summary["max_wrist_offset_symmetry_error"] = {"value": 0.0, "frame": None}
    for frame in range(LOOP_START, LOOP_END + 1):
        snapshot = _capture(armature, action, frame)
        comparison = _comparison(snapshot, idle)
        for side in SIDES:
            angles = comparison[side]["bone_angular_difference_from_idle_degrees"]["Hand"]
            offset = Vector(comparison[side]["wrist_offset_from_idle_world"]).length
            values = (("max_hand_world_angle_from_idle_degrees", angles["world"]),
                      ("max_hand_basis_angle_from_idle_degrees", angles["matrix_basis"]),
                      ("max_wrist_offset_from_idle", offset))
            for key, value in values:
                if value > summary[side][key]["value"]:
                    summary[side][key] = {"value": round(value, 7), "frame": frame}
        symmetry_values = (
            ("max_hand_delta_symmetry_degrees",
             comparison["left_right_hand_delta_symmetry_degrees"]),
            ("max_wrist_offset_symmetry_error",
             comparison["left_right_wrist_offset_symmetry_error"]),
        )
        for key, value in symmetry_values:
            if value > summary[key]["value"]:
                summary[key] = {"value": round(value, 7), "frame": frame}
    return summary


def _compact_report(report):
    compact = {key: report[key] for key in (
        "read_only", "blend_file", "blender_version", "armature",
        "armature_matrix_world", "original_active_action", "diagnostic_frame",
        "loop_range_inclusive")}
    compact["reference"] = {
        "action": report["reference"]["action"],
        "frame": report["reference"]["frame"],
        "snapshot": report["reference"]["snapshot"],
    }
    compact["actions"] = {}
    for name, action in report["actions"].items():
        entry = {
            "frame_range": action["frame_range"],
            "frame_12_comparison_to_idle": action["frame_12_comparison_to_idle"],
            "loop_summary": action["loop_summary"],
        }
        if "frame_12_snapshot" in action:
            entry["frame_12_snapshot"] = {}
            for side, side_snapshot in action["frame_12_snapshot"].items():
                entry["frame_12_snapshot"][side] = {
                    "chain": side_snapshot["chain"],
                    "wrist_world_position": side_snapshot["wrist_world_position"],
                    "hand_local_axes_in_world": side_snapshot["hand_local_axes_in_world"],
                    "forearm_local_axes_in_world": side_snapshot["forearm_local_axes_in_world"],
                    "palm_geometry": side_snapshot["palm_geometry"],
                }
        compact["actions"][name] = entry
    return compact


def diagnose(*, compact=False):
    armature = bpy.data.objects.get(ARMATURE_NAME)
    if armature is None or armature.type != "ARMATURE":
        raise RuntimeError(f"Armature object {ARMATURE_NAME!r} was not found")
    required_bones = [_bone(side, suffix) for side in SIDES for suffix in CHAIN_SUFFIXES]
    required_bones += [_finger(side, finger, segment) for side in SIDES
                       for finger in FINGER_NAMES for segment in range(1, 5)]
    missing = [name for name in required_bones if name not in armature.pose.bones]
    if missing:
        raise RuntimeError("Missing required bones: " + ", ".join(missing))
    idle_action = bpy.data.actions.get(IDLE_ACTION)
    if idle_action is None:
        raise RuntimeError(f"Idle Action {IDLE_ACTION!r} was not found")
    animation = armature.animation_data
    if animation is None:
        raise RuntimeError("Armature has no animation_data")
    active_nla = [track.name for track in animation.nla_tracks if not track.mute]
    constrained = [bone.name for bone in armature.pose.bones if bone.constraints]
    if active_nla or animation.drivers or constrained:
        raise RuntimeError(
            f"Refusing contaminated evaluation: active_nla={active_nla}, "
            f"drivers={len(animation.drivers)}, constrained_bones={constrained}")

    old_action = animation.action
    old_slot = animation.action_slot
    old_slot_identifier = old_slot.identifier if old_slot is not None else None
    old_frame = bpy.context.scene.frame_current
    action_names_before = tuple(action.name for action in bpy.data.actions)
    actions = [action for action in bpy.data.actions
               if action.name in {"Gojo_Walk", "Gojo_Walk_Pocket", "Gojo_Walk_Pocket_ShortStride"}
               or action.name.startswith("Gojo_Walk_Pocket_")]
    actions.sort(key=lambda action: action.name)
    try:
        idle = _capture(armature, idle_action, IDLE_FRAME)
        report = {
            "read_only": True,
            "blend_file": bpy.data.filepath,
            "blender_version": bpy.app.version_string,
            "armature": armature.name,
            "armature_matrix_world": _matrix(armature.matrix_world),
            "original_active_action": old_action.name if old_action else None,
            "reference": {"action": IDLE_ACTION, "frame": IDLE_FRAME, "snapshot": idle},
            "diagnostic_frame": DIAGNOSTIC_FRAME,
            "loop_range_inclusive": [LOOP_START, LOOP_END],
            "actions": {},
        }
        detailed_actions = {"Gojo_Walk_Pocket", "Gojo_Walk_Pocket_ShortStride"}
        if old_action is not None and old_action.name.startswith("Gojo_Walk_Pocket"):
            detailed_actions.add(old_action.name)
        for action in actions:
            snapshot = _capture(armature, action, DIAGNOSTIC_FRAME)
            report["actions"][action.name] = {
                "frame_range": _vec(action.frame_range),
                "frame_12_comparison_to_idle": _comparison(snapshot, idle),
                "loop_summary": _loop_summary(armature, action, idle),
            }
            if action.name in detailed_actions:
                report["actions"][action.name]["frame_12_snapshot"] = snapshot
        print("JJK_HAND_DIAGNOSTIC_BEGIN")
        output = _compact_report(report) if compact else report
        print(json.dumps(output, ensure_ascii=False, indent=2, sort_keys=True))
        print("JJK_HAND_DIAGNOSTIC_END")
        return report
    finally:
        animation.action = old_action
        if old_action is not None and old_slot_identifier is not None:
            restored_slots = [slot for slot in old_action.slots
                              if slot.identifier == old_slot_identifier]
            if restored_slots:
                animation.action_slot = restored_slots[0]
        bpy.context.scene.frame_set(old_frame)
        bpy.context.view_layer.update()
        if tuple(action.name for action in bpy.data.actions) != action_names_before:
            raise RuntimeError("Diagnostic unexpectedly changed the Action collection")


if __name__ == "__main__":
    diagnose(compact="--compact" in sys.argv)
