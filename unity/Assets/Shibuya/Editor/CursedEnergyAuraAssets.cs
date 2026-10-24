using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace JJKGame.Shibuya.EditorTools
{
    public static class CursedEnergyAuraAssets
    {
        public const string Root="Assets/Shibuya/VFX/CursedEnergyAura";
        public const string DefaultPath="Assets/Shibuya/Resources/Cursed Energy Aura.asset";
        [MenuItem("JJK Game/시부야 PASS4B/기본 주력 아우라 자산 만들기 (기존 값 보존)")]
        public static void Create()
        {
            if(EditorApplication.isPlaying || EditorSceneManager.GetActiveScene().isDirty)
                throw new System.InvalidOperationException("Play Mode를 종료하고 미저장 씬을 먼저 확인하세요.");
            Directory.CreateDirectory(Root);Directory.CreateDirectory(Root+"/Presets");AssetDatabase.Refresh();
            var energy=Material("Energy","JJK/Shibuya/Cursed Energy Volume");
            var ink=Material("Ink","JJK/Shibuya/Cursed Energy Volume");
            if(!ink.HasProperty("_Ink"))throw new System.InvalidOperationException("아우라 셰이더 연결 확인이 필요합니다.");
            // Only initialize material values when first creating the assets.
            var rim=Material("BodyRim","JJK/Shibuya/Cursed Energy Rim");
            var mote=Material("Motes","JJK/Shibuya/Cursed Energy Mote");
            string prefabPath=Root+"/Cursed Energy Aura.prefab";
            if(!File.Exists(prefabPath)) {
                // Author only in an owned temporary scene. Do not dirty the user's open scene.
                var previous=SceneManager.GetActiveScene();
                var temporary=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
                try {
                    SceneManager.SetActiveScene(temporary);
                    var go=new GameObject("CursedEnergyAuraRoot");go.AddComponent<CursedEnergyAuraVisual>().SetTemplates(energy,ink,rim,mote);
                    PrefabUtility.SaveAsPrefabAsset(go,prefabPath);
                } finally {
                    SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(temporary,true);
                }
            }
            var profile=AssetDatabase.LoadAssetAtPath<CursedEnergyAuraProfile>(DefaultPath);
            if(profile==null) {
                profile=ScriptableObject.CreateInstance<CursedEnergyAuraProfile>();
                profile.visualPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                // Author the approved common look into NEW saved assets; runtime needs no preview code.
                CursedEnergyAuraEndPreview.Candidate(profile);
                AssetDatabase.CreateAsset(profile,DefaultPath);
            }
            Preset("Pink Aura",new Color(1,.035f,.36f),profile);
            Preset("Green Aura",new Color(.08f,1,.24f),profile);
        }
        private static Material Material(string name,string shaderName)
        {
            string path=Root+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m!=null)return m;
            var shader=Shader.Find(shaderName);if(shader==null)throw new System.InvalidOperationException("주력 셰이더를 찾을 수 없습니다: "+shaderName);
            m=new Material(shader){name=name};
            if(name=="Ink") {m.SetFloat("_Ink",1);m.SetFloat("_Cull",1);m.renderQueue=2990;}
            AssetDatabase.CreateAsset(m,path);return m;
        }
        private static void Preset(string name,Color color,CursedEnergyAuraProfile source)
        {
            string path=Root+"/Presets/"+name+".asset";if(File.Exists(path))return;
            var p=Object.Instantiate(source);p.name=name;p.mainColor=color;AssetDatabase.CreateAsset(p,path);
        }
    }
}
