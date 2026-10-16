"""Render read-only hand-pose reference PNGs without saving the open .blend."""

from pathlib import Path

import bpy
from mathutils import Vector


ARMATURE_NAME = "Armature"
OUTPUT_DIR = Path(r"D:\JJK_game\unity\Temp\GojoHandDiagnostic")
ACTIONS = (
    ("Gojo_Idle", 2),
    ("Gojo_Walk_Pocket", 12),
    ("Gojo_Walk_Pocket_ShortStride", 12),
    ("Gojo_Walk_Pocket_Final_SAFE", 12),
    ("Gojo_Walk_Pocket_Final_v3", 12),
    ("Gojo_Walk_Pocket_Final_v3_FIX", 12),
)
VIEWS = {
    "front": Vector((0.0, -8.0, 3.55)),
    "back": Vector((0.0, 8.0, 3.55)),
    "side": Vector((6.0, -0.1, 3.55)),
}
TARGET = Vector((0.0, -0.1, 3.55))


def point_at(obj, target):
    obj.rotation_euler = (target - obj.location).to_track_quat("-Z", "Y").to_euler()


def add_area(name, location, energy, size):
    data = bpy.data.lights.new(name, "AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.location = location
    point_at(obj, TARGET)
    return obj


def render():
    armature = bpy.data.objects[ARMATURE_NAME]
    animation = armature.animation_data
    old_action = animation.action
    old_slot_identifier = animation.action_slot.identifier if animation.action_slot else None
    old_frame = bpy.context.scene.frame_current
    scene = bpy.context.scene
    old_camera = scene.camera
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    camera_data = bpy.data.cameras.new("JJK_Diagnostic_Camera")
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = 2.15
    camera = bpy.data.objects.new("JJK_Diagnostic_Camera", camera_data)
    bpy.context.collection.objects.link(camera)
    scene.camera = camera
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 600
    scene.render.resolution_y = 600
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.world.color = (0.035, 0.035, 0.035)
    add_area("JJK_Diagnostic_Key", Vector((4.0, -5.0, 7.0)), 1100.0, 5.0)
    add_area("JJK_Diagnostic_Fill", Vector((-4.0, -2.0, 5.0)), 700.0, 4.0)
    add_area("JJK_Diagnostic_Rim", Vector((0.0, 4.0, 6.0)), 900.0, 3.0)
    try:
        for action_name, frame in ACTIONS:
            action = bpy.data.actions[action_name]
            animation.action = action
            object_slots = [slot for slot in action.slots if slot.target_id_type == "OBJECT"]
            if object_slots:
                animation.action_slot = object_slots[0]
            scene.frame_set(frame)
            bpy.context.view_layer.update()
            for view_name, location in VIEWS.items():
                camera.location = location
                point_at(camera, TARGET)
                output = OUTPUT_DIR / f"{action_name}_f{frame:02d}_{view_name}.png"
                scene.render.filepath = str(output)
                bpy.ops.render.render(write_still=True)
                print(f"JJK_HAND_RENDER={output}")
    finally:
        animation.action = old_action
        if old_action is not None and old_slot_identifier is not None:
            slots = [slot for slot in old_action.slots if slot.identifier == old_slot_identifier]
            if slots:
                animation.action_slot = slots[0]
        scene.frame_set(old_frame)
        scene.camera = old_camera


if __name__ == "__main__":
    render()
