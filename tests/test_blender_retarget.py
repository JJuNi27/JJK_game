import importlib.util
from pathlib import Path

import pytest


spec = importlib.util.spec_from_file_location(
    'retarget', Path(__file__).parents[1] / 'scripts/blender_retarget_mixamo.py')
retarget = importlib.util.module_from_spec(spec)
spec.loader.exec_module(retarget)


def test_namespace_variants_are_explicit():
    assert retarget.bone_key('mixamorig:Hips') == 'hips'
    assert retarget.bone_key('mixamorigHips') == 'hips'
    assert retarget.bone_key('mixamorig:Hips.001') != 'hips'


def test_ambiguous_or_incomplete_mapping_fails_before_mutation():
    with pytest.raises(ValueError, match='Ambiguous'):
        retarget.build_bone_map(['mixamorig:Hips', 'mixamorigHips'], [])
    with pytest.raises(ValueError, match='Missing required'):
        retarget.build_bone_map(['mixamorig:Hips'], ['mixamorigHips'])
