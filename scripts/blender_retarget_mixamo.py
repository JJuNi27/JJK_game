r"""Reusable WORLD-space Mixamo -> master-armature visual bake.

Blender Text Editor:
    exec(compile(open(r'D:\JJK_game\scripts\blender_retarget_mixamo.py',
                      encoding='utf-8').read(), 'retarget', 'exec'))
    bake_retarget('Armature', 'Gojo_Master', 1, 30, 'Gojo_Walk', apply=False)
    bake_retarget('Armature', 'Gojo_Master', 1, 30, 'Gojo_Walk', apply=True)

The default validates and reports only. Applying creates a new Action in memory;
it never saves a .blend, exports FBX, or changes source animation.
"""
import re


def bone_key(name):
    """Accept Mixamo namespace variants; never guess numbered duplicate bones."""
    return re.sub(r'^mixamorig[:_]*', '', name.rsplit('|', 1)[-1], flags=re.I).lower()


def build_bone_map(source_names, target_names):
    def index(names):
        result = {}
        for name in names:
            key = bone_key(name)
            if key in result:
                raise ValueError(f'Ambiguous bone mapping: {result[key]} / {name}')
            result[key] = name
        return result

    source = index(source_names)
    target = index(target_names)
    required = {'hips', 'spine', 'head', 'leftarm', 'rightarm',
                'leftforearm', 'rightforearm', 'lefthand', 'righthand',
                'leftupleg', 'rightupleg', 'leftleg', 'rightleg',
                'leftfoot', 'rightfoot'}
    missing = required - (source.keys() & target.keys())
    if missing:
        raise ValueError('Missing required humanoid bones: ' + ', '.join(sorted(missing)))
    return {target[key]: source[key] for key in sorted(target.keys() & source.keys())}


def bake_retarget(source_name, target_name, frame_start, frame_end, action_name, *, apply=False):
    import bpy
    from mathutils import Matrix

    source = bpy.data.objects.get(source_name)
    target = bpy.data.objects.get(target_name)
    if source is None or target is None or source.type != 'ARMATURE' or target.type != 'ARMATURE':
        raise ValueError('Source and target must name existing armature objects')
    if source == target or source.data == target.data:
        raise ValueError('Source and target must use distinct armature data')
    if bpy.context.mode != 'OBJECT':
        raise ValueError('Start in Object Mode')
    if target.name not in bpy.context.view_layer.objects or target.hide_get():
        raise ValueError('Target must be visible in the active view layer')
    if not isinstance(frame_start, int) or not isinstance(frame_end, int) or frame_end < frame_start:
        raise ValueError('Frame range must contain increasing integer endpoints')
    if not action_name.strip() or action_name in bpy.data.actions:
        raise ValueError('Choose a new, non-empty output Action name')
    mapping = build_bone_map(source.pose.bones.keys(), target.pose.bones.keys())
    constrained = [name for name in mapping if target.pose.bones[name].constraints]
    if constrained:
        raise ValueError('Mapped target bones already have constraints: ' + ', '.join(constrained))
    if target.animation_data:
        if target.animation_data.drivers:
            raise ValueError('Target animation drivers require manual review before baking')
        if any(not track.mute for track in target.animation_data.nla_tracks):
            raise ValueError('Mute target NLA tracks before baking a standalone Action')
    report = {'source': source_name, 'target': target_name, 'frames': (frame_start, frame_end),
              'action': action_name, 'mapped_bones': mapping,
              'unmapped_target_bones': sorted(set(target.pose.bones.keys()) - mapping.keys())}
    print(report)
    if not apply:
        return report

    scene = bpy.context.scene
    old_frame = scene.frame_current
    old_active = bpy.context.view_layer.objects.active
    old_selected = list(bpy.context.selected_objects)
    # Blender 5 moved selection from Bone to PoseBone; keep 4.x compatibility.
    selection_bones = {pose.name: pose if hasattr(pose, 'select') else pose.bone
                       for pose in target.pose.bones}
    old_bone_selection = {name: bone.select for name, bone in selection_bones.items()}
    old_active_bone = target.data.bones.active
    old_pose = {name: target.pose.bones[name].matrix_basis.copy() for name in mapping}
    target.animation_data_create()
    old_action = target.animation_data.action
    old_slot = getattr(target.animation_data, 'action_slot', None)
    created_constraints = []
    succeeded = False
    try:
        bpy.ops.object.select_all(action='DESELECT')
        target.select_set(True)
        bpy.context.view_layer.objects.active = target
        for name, bone in selection_bones.items():
            bone.select = name in mapping
        target.data.bones.active = target.data.bones[next(iter(mapping))]
        target.animation_data.action = None
        for target_bone, source_bone in mapping.items():
            pose = target.pose.bones[target_bone]
            pose.matrix_basis = Matrix.Identity(4)
            rotation = pose.constraints.new('COPY_ROTATION')
            created_constraints.append((pose, rotation))
            rotation.name = 'JJK_Retarget_Rotation'
            rotation.target = source
            rotation.subtarget = source_bone
            rotation.owner_space = 'WORLD'
            rotation.target_space = 'WORLD'
            rotation.mix_mode = 'REPLACE'
            if bone_key(target_bone) == 'hips':
                location = pose.constraints.new('COPY_LOCATION')
                created_constraints.append((pose, location))
                location.name = 'JJK_Retarget_Hips'
                location.target = source
                location.subtarget = source_bone
                location.owner_space = 'WORLD'
                location.target_space = 'WORLD'
                location.use_offset = False
        bpy.ops.object.mode_set(mode='POSE')
        result = bpy.ops.nla.bake(
            frame_start=frame_start, frame_end=frame_end, step=1,
            only_selected=True, visual_keying=True, clear_constraints=False,
            clear_parents=False, use_current_action=False, clean_curves=False,
            bake_types={'POSE'}, channel_types={'LOCATION', 'ROTATION', 'SCALE'})
        if 'FINISHED' not in result or target.animation_data.action is None:
            raise RuntimeError('Visual bake did not create an Action')
        target.animation_data.action.name = action_name
        target.animation_data.action.use_fake_user = True
        succeeded = True
        report['created_action'] = target.animation_data.action.name
        return report
    finally:
        # Remove only constraints created by this invocation, including on failure.
        for pose, constraint in reversed(created_constraints):
            pose.constraints.remove(constraint)
        if bpy.context.object == target and target.mode != 'OBJECT':
            bpy.ops.object.mode_set(mode='OBJECT')
        if not succeeded:
            target.animation_data.action = old_action
            if old_slot is not None:
                target.animation_data.action_slot = old_slot
            for name, matrix in old_pose.items():
                target.pose.bones[name].matrix_basis = matrix
        for name, bone in selection_bones.items():
            bone.select = old_bone_selection[name]
        target.data.bones.active = old_active_bone
        bpy.ops.object.select_all(action='DESELECT')
        for obj in old_selected:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = old_active
        scene.frame_set(old_frame)
