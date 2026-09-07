"""Run with Blender --background --factory-startup --python-exit-code 1 --python this_file.
Creates synthetic armatures only. Never opens or saves the user's .blend/FBX.
"""
import importlib.util
from pathlib import Path

import bpy

spec = importlib.util.spec_from_file_location(
    'retarget', Path(__file__).parents[1] / 'scripts/blender_retarget_mixamo.py')
retarget = importlib.util.module_from_spec(spec)
spec.loader.exec_module(retarget)

names = ['Hips', 'Spine', 'Head', 'LeftArm', 'RightArm', 'LeftForeArm', 'RightForeArm',
         'LeftHand', 'RightHand', 'LeftUpLeg', 'RightUpLeg', 'LeftLeg', 'RightLeg',
         'LeftFoot', 'RightFoot']


def rig(name, prefix):
    data = bpy.data.armatures.new(name)
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode='EDIT')
    for i, bone_name in enumerate(names):
        bone = data.edit_bones.new(prefix + bone_name)
        bone.head = (i * 0.2, 0, 0)
        bone.tail = (i * 0.2, 0, 1)
        parent = {'Spine': 'Hips', 'Head': 'Spine',
                  'LeftArm': 'Spine', 'RightArm': 'Spine',
                  'LeftForeArm': 'LeftArm', 'RightForeArm': 'RightArm',
                  'LeftHand': 'LeftForeArm', 'RightHand': 'RightForeArm',
                  'LeftUpLeg': 'Hips', 'RightUpLeg': 'Hips',
                  'LeftLeg': 'LeftUpLeg', 'RightLeg': 'RightUpLeg',
                  'LeftFoot': 'LeftLeg', 'RightFoot': 'RightLeg'}.get(bone_name)
        if parent:
            bone.parent = data.edit_bones[prefix + parent]
    bpy.ops.object.mode_set(mode='OBJECT')
    obj.select_set(False)
    return obj


source = rig('SmokeSource', 'mixamorig:')
target = rig('SmokeTarget', 'mixamorig')
target.rotation_euler.z = 0.3
target.scale = (0.5, 0.5, 0.5)
arm = source.pose.bones['mixamorig:LeftArm']
hips = source.pose.bones['mixamorig:Hips']
arm.rotation_mode = 'XYZ'
for frame, angle, x in [(1, 0.1, 0.0), (10, 0.9, 2.0)]:
    arm.rotation_euler.z = angle
    arm.keyframe_insert('rotation_euler', frame=frame)
    hips.location.x = x
    hips.keyframe_insert('location', frame=frame)
source_action = source.animation_data.action
target.pose.bones['mixamorigHips'].keyframe_insert('location', frame=1)
old_action = target.animation_data.action
old_action.name = 'SmokeExistingAction'
old_action.use_fake_user = True

retarget.bake_retarget(source.name, target.name, 1, 10, 'SmokeBake', apply=False)
assert target.animation_data.action == old_action
report = retarget.bake_retarget(source.name, target.name, 1, 10, 'SmokeBake', apply=True)
assert report['created_action'] == 'SmokeBake'
assert source.animation_data.action == source_action
assert old_action.name in bpy.data.actions
assert not any(b.constraints for b in target.pose.bones)
for frame in (1, 5, 10):
    bpy.context.scene.frame_set(frame)
    bpy.context.view_layer.update()
    source_world = source.matrix_world @ arm.matrix
    target_world = target.matrix_world @ target.pose.bones['mixamorigLeftArm'].matrix
    angle_error = source_world.to_quaternion().rotation_difference(target_world.to_quaternion()).angle
    assert angle_error < 0.001, (frame, angle_error)
    source_hips = (source.matrix_world @ hips.matrix).translation
    target_hips = (target.matrix_world @ target.pose.bones['mixamorigHips'].matrix).translation
    assert (source_hips - target_hips).length < 0.001, (frame, source_hips, target_hips)
print('PASS: WORLD rotation, Hips location, dry run, source/old Action preservation, constraint cleanup')
