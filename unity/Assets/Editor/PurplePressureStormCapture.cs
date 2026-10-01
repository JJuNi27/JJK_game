using System;
using System.IO;
using System.Linq;
using JJKGame.Core;
using JJKGame.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace JJKGame.EditorTools
{
 public static class PurplePressureStormCapture
 {
 private static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurplePressureStormQA"));
 private static readonly string Session=Path.Combine(Output,"AB",DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));
        private static void Select(bool electric)
        {
            PurpleIngredientElectricArcProfile.Current.candidateEnabled=false;
            PurpleIngredientReboot2Profile.Current.candidateEnabled = true;
            PurpleIngredientFinalProfile.Current.candidateEnabled = true;
            PurpleIngredientFinal2Profile.Current.candidateEnabled = true;
            PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = false;
            PurpleIngredientHybridProfile.Current.candidateEnabled = false;
            PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = false;
            PurpleIngredientFlowFirstProfile.Current.candidateEnabled = electric;
        }

        public static string Capture(int take,bool full=true)
        {
            int mode=take%2;bool casterView=take>=2 && take<4;bool farView=take>=4;
            bool oldStorm=PurplePressureStormProfile.Current.candidateEnabled;bool oldBody=PurpleIdentityWrappedProfile.Current.candidateEnabled;bool oldRupture=PurpleIdentityRuptureProfile.Current.candidateEnabled;bool oldCoupled=PurpleIdentityCoupledProfile.Current.candidateEnabled;bool oldMass=PurpleIdentityMassProfile.Current.candidateEnabled;bool oldCore=PurpleIdentityCoreProfile.Current.candidateEnabled;bool oldTurbulence=PurpleExplosionTurbulenceProfile.Current.candidateEnabled;
            bool oldImpact=PurpleConvergenceImpactProfile.Current.candidateEnabled;bool oldDiagnostic=PurpleConvergenceDiagnosticProfile.Current.candidateEnabled;
            if (!Application.isPlaying) throw new InvalidOperationException("Enter CombatMVP Play Mode before capture");
            var actor = GameObject.Find("GojoPlayer");
            if (actor == null) throw new InvalidOperationException("CombatMVP GojoPlayer is missing");
            var caster = Camera.main;
            if (caster == null) throw new InvalidOperationException("CombatMVP main camera is missing");
            var profile = PurpleIngredientFlowFirstProfile.Current;
            bool[] old = { PurpleIngredientReboot2Profile.Current.candidateEnabled, PurpleIngredientFinalProfile.Current.candidateEnabled,
                PurpleIngredientFinal2Profile.Current.candidateEnabled, PurpleIngredientOuterExplorationProfile.Current.candidateEnabled,
                PurpleIngredientHybridProfile.Current.candidateEnabled, PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled,
                profile.candidateEnabled, PurpleIngredientElectricArcProfile.Current.candidateEnabled };
            var randomState=UnityEngine.Random.state;
            Vector3 stage = new Vector3(512,1,512);
            var floor=GameObject.CreatePrimitive(PrimitiveType.Plane);floor.name="FlowFirstQAGround";
            floor.transform.position=new Vector3(stage.x,0,stage.z);floor.transform.localScale=Vector3.one*8;
            Object.Destroy(floor.GetComponent<Collider>());
            var floorMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));floorMaterial.SetColor("_BaseColor",new Color(.64f,.62f,.58f));
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
            var environment=new GameObject("DiagnosticQAFixedEnvironment");
            var markerMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));markerMaterial.SetColor("_BaseColor",new Color(.31f,.33f,.35f));
            for(int axis=0;axis<2;axis++)for(int i=-6;i<=6;i++)
            {
                var mark=GameObject.CreatePrimitive(PrimitiveType.Cube);mark.transform.SetParent(environment.transform);mark.transform.position=new Vector3(stage.x+(axis==0?i*3:0),.025f,stage.z+(axis==1?i*3:0));mark.transform.localScale=axis==0?new Vector3(.05f,.04f,40):new Vector3(40,.04f,.05f);Object.Destroy(mark.GetComponent<Collider>());mark.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            }
            for(int i=0;i<4;i++)
            {
                var column=GameObject.CreatePrimitive(PrimitiveType.Cube);column.transform.SetParent(environment.transform);column.transform.position=stage+new Vector3(i%2==0?-7:7,1.5f,i<2?8:14);column.transform.localScale=new Vector3(1,5,1);Object.Destroy(column.GetComponent<Collider>());column.GetComponent<Renderer>().sharedMaterial=markerMaterial;
            }
            Vector3 oldPosition = actor.transform.position;
            Quaternion oldRotation = actor.transform.rotation;
            bool oldCameraEnabled = caster.enabled;int oldMask=caster.cullingMask;caster.cullingMask|=1;
            float oldFieldOfView = caster.fieldOfView;
            RenderTexture oldTarget = caster.targetTexture;
            Vector3 cameraPosition=caster.transform.position; Quaternion cameraRotation=caster.transform.rotation;
            var oldClear=caster.clearFlags;var oldBackground=caster.backgroundColor;
            caster.clearFlags=CameraClearFlags.SolidColor;caster.backgroundColor=new Color(.67f,.69f,.71f);
            var side = new GameObject("FlowFirstQAObserver").AddComponent<Camera>();
            side.CopyFrom(caster);
            side.enabled = false;
            side.fieldOfView = 45;
            side.farClipPlane = 180;
            var sideData = side.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var casterData = caster.GetComponent<UniversalAdditionalCameraData>();
            sideData.renderPostProcessing = casterData != null && casterData.renderPostProcessing;
            if (casterData != null) sideData.volumeLayerMask = casterData.volumeLayerMask;
            sideData.requiresColorTexture = true;
            sideData.requiresDepthTexture = true;
            var target = new RenderTexture(960, 540, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            target.Create();
            caster.targetTexture = target;
            side.targetTexture = target;
            caster.enabled = false;
            caster.fieldOfView = 60;
            Vector3 origin = PrototypeHollowPurplePresentationRuntime.ReleaseOrigin(stage, Vector3.forward);
            string output = Session;
            Directory.CreateDirectory(output);
            PurpleBrightQaEnvironment bright=null;
            try
            {
                    {
                        bright=new PurpleBrightQaEnvironment();
                        Select(false);PurpleIngredientFlowFirstProfile.Current.candidateEnabled=true;PurpleConvergenceImpactProfile.Current.candidateEnabled=false;PurpleConvergenceDiagnosticProfile.Current.candidateEnabled=true;PurpleExplosionTurbulenceProfile.Current.candidateEnabled=false;PurpleIdentityCoreProfile.Current.candidateEnabled=false;PurpleIdentityMassProfile.Current.candidateEnabled=true;PurpleIdentityCoupledProfile.Current.candidateEnabled=false;PurpleIdentityRuptureProfile.Current.candidateEnabled=false;PurpleIdentityWrappedProfile.Current.candidateEnabled=true;PurplePressureStormProfile.Current.candidateEnabled=mode==1;
                        UnityEngine.Random.InitState(240926);
                        actor.transform.SetPositionAndRotation(stage, Quaternion.identity);
                        caster.transform.SetPositionAndRotation(stage + new Vector3(0, 4, -11), Quaternion.Euler(12, 0, 0));
                        var host = new GameObject("FlowFirstABSequence");
                        var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor.transform, 0, casterView ? caster : null, casterView);
                        string stem = (farView ? "Observer" : casterView ? "Caster" : "Side") + (mode == 0 ? "_Baseline" : "_Candidate");
                        var impact=host.GetComponentInChildren<PurpleConvergenceDiagnostic>();
                        var previousImpact=host.GetComponentInChildren<PurpleConvergenceImpact>();
                        Assert.That(impact,Is.Not.Null);
                        float contact=GojoPolishSettings.Current.PurpleFusionStart+PurpleIngredientCollisionAccent.FindContactNormalized(GojoPolishSettings.Current.purpleFormationSeparation,GojoPolishSettings.Current.purpleFormationScale,PurpleIngredientReboot2Profile.Current.fusionVerticalArc)*GojoPolishSettings.Current.purpleFusionDuration;
                        float hitWall=Mathf.Ceil(contact*60)/60f;
                        impact?.SetPreview(casterView?caster:side,0);
                        previousImpact?.SetPreview(casterView?caster:side,0);
                        var poses=new System.Text.StringBuilder();
                        var clockLog=new System.Text.StringBuilder();int whiteFrames=0;
                        var pulseLog=new System.Text.StringBuilder("wall,flash,holding,bloom,exposure,volumeWeight\n");
                        File.WriteAllText(Path.Combine(output,stem+".contact.json"),"{\"contact\":"+contact.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+",\"sampledAt\":"+hitWall.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"}");
                        try
                        {
                            using (var stream = full ? new FileStream(Path.Combine(output, stem + ".rgb24"), FileMode.CreateNew) : null)
                                foreach(int frame in (full?Enumerable.Range(0,385):new[]{162,210,340}))
                                {
                                    float wall=frame/60f;
                                    // Canonical Purple is unscaled in live runtime. Do not fake a VFX pause in the comparison.
                                    float t=wall;
                                    impact?.SetPreviewTime(wall);
                                    previousImpact?.SetPreviewTime(wall);
                                    sequence.Update(t,1/60f);
                                    Canvas.ForceUpdateCanvases();
                                    if(impact!=null && impact.FlashWeight>0)whiteFrames++;
                                    clockLog.AppendLine(wall+","+t+","+(impact!=null && impact.FlashWeight>0));
                                    foreach(string item in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                                    {
                                        var pose=host.transform.Find("HollowPurpleCanonicalOrbSequence/"+item);
                                        poses.AppendLine(t.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"+item+"|"+pose.position.ToString("F7")+"|"+pose.localScale.ToString("F7")+"|"+pose.gameObject.activeSelf);
                                    }
                                    Vector3 offset = new Vector3(5, 2, -12);
                                    side.transform.SetPositionAndRotation(origin + Vector3.forward * Mathf.Clamp01((t - GojoPolishSettings.Current.PurpleReleaseTime) / 1.6f) * 48 + offset, Quaternion.LookRotation(-offset));
                                    if(farView)side.transform.SetPositionAndRotation(origin+new Vector3(35,11,19),Quaternion.LookRotation(origin+Vector3.forward*22-(origin+new Vector3(35,11,19))));
                                    RenderPipeline.SubmitRenderRequest(casterView ? caster : side, new UniversalRenderPipeline.StandardRequest { destination = target });
                                    if(impact!=null)pulseLog.AppendLine(wall+","+impact.FlashWeight+","+impact.Holding+","+impact.LastRenderedBloom+","+impact.LastRenderedExposure+","+impact.LastRenderedPulseWeight);
                                    var previous = RenderTexture.active;
                                    RenderTexture.active = target;
                                    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
                                    image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
                                    image.Apply();
                                    RenderTexture.active = previous;
                                    if (full) stream.Write(image.GetRawTextureData<byte>().ToArray(), 0, 960 * 540 * 3);
                                    if (!full || frame>=144 && frame<=151 || frame==180 || frame==300 || frame==340)
                                        File.WriteAllBytes(Path.Combine(output, stem + "_" + frame.ToString("D3") + ".png"), image.EncodeToPNG());
                                    Object.DestroyImmediate(image);
                                }
                            File.WriteAllText(Path.Combine(output,stem+".poses.txt"),poses.ToString());
                            File.WriteAllText(Path.Combine(output,stem+".clock.csv"),clockLog.ToString());
                            File.WriteAllText(Path.Combine(output,stem+".pulse.csv"),pulseLog.ToString());
                            if(full)Assert.That(whiteFrames,Is.EqualTo(8),"Radial envelope must cover eight nominal samples");
                            // Compare the protected body after all contact feedback has ended.
                            impact?.SetPreviewTime(7);sequence.Update(7.0f,1/60f);
                            side.transform.SetPositionAndRotation(origin+Vector3.forward*Mathf.Clamp01((7.0f-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48+new Vector3(5,2,-12),Quaternion.LookRotation(-new Vector3(5,2,-12)));
                            if(farView)side.transform.SetPositionAndRotation(origin+new Vector3(35,11,19),Quaternion.LookRotation(origin+Vector3.forward*22-(origin+new Vector3(35,11,19))));
                            RenderPipeline.SubmitRenderRequest(casterView?caster:side,new UniversalRenderPipeline.StandardRequest{destination=target});
                            var prior=RenderTexture.active;RenderTexture.active=target;var still=new Texture2D(960,540,TextureFormat.RGB24,false);still.ReadPixels(new Rect(0,0,960,540),0,0);still.Apply();RenderTexture.active=prior;
                            File.WriteAllBytes(Path.Combine(output,stem+"_ProtectedTerminal7_0.png"),still.EncodeToPNG());Object.DestroyImmediate(still);
                        }
                        finally { sequence.Dispose(); Object.Destroy(host); }
                    }
                File.WriteAllText(Path.Combine(output, "format.json"), "{\"width\":960,\"height\":540,\"fps\":60,\"frames\":" + (full ? 385 : 4) + ",\"baseline\":\"Wrapped + common FlowFirst/ConvergenceDiagnostic + bright neutral QA\",\"candidate\":\"Wrapped + PressureStorm + common FlowFirst/ConvergenceDiagnostic + bright neutral QA\"}");
                return output;
            }
            finally
            {
                PurplePressureStormProfile.Current.candidateEnabled=oldStorm;PurpleIdentityWrappedProfile.Current.candidateEnabled=oldBody;PurpleIdentityRuptureProfile.Current.candidateEnabled=oldRupture;PurpleIdentityCoupledProfile.Current.candidateEnabled=oldCoupled;PurpleIdentityMassProfile.Current.candidateEnabled=oldMass;PurpleIdentityCoreProfile.Current.candidateEnabled=oldCore;PurpleExplosionTurbulenceProfile.Current.candidateEnabled=oldTurbulence;PurpleConvergenceImpactProfile.Current.candidateEnabled=oldImpact;PurpleConvergenceDiagnosticProfile.Current.candidateEnabled=oldDiagnostic;
                PurpleIngredientReboot2Profile.Current.candidateEnabled = old[0];
                PurpleIngredientFinalProfile.Current.candidateEnabled = old[1];
                PurpleIngredientFinal2Profile.Current.candidateEnabled = old[2];
                PurpleIngredientOuterExplorationProfile.Current.candidateEnabled = old[3];
                PurpleIngredientHybridProfile.Current.candidateEnabled = old[4];
                PurpleIngredientExpandedDischargeProfile.Current.candidateEnabled = old[5];
                profile.candidateEnabled = old[6];
                PurpleIngredientElectricArcProfile.Current.candidateEnabled=old[7];UnityEngine.Random.state=randomState;
                actor.transform.SetPositionAndRotation(oldPosition, oldRotation);
                caster.targetTexture = oldTarget;
                caster.enabled = oldCameraEnabled;caster.cullingMask=oldMask;
                caster.fieldOfView = oldFieldOfView;
                caster.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
                caster.clearFlags=oldClear;caster.backgroundColor=oldBackground;bright?.Dispose();
                target.Release();
                Object.Destroy(target);
                Object.Destroy(side.gameObject);
                floor.SetActive(false);environment.SetActive(false);Object.Destroy(floor);Object.Destroy(floorMaterial);Object.Destroy(environment);Object.Destroy(markerMaterial);
            }
        }
 }
}


namespace JJKGame.EditorTools
{
    public sealed class PurpleBrightQaEnvironment:System.IDisposable
    {
        private readonly AmbientMode mode=RenderSettings.ambientMode;
        private readonly Color color=RenderSettings.ambientLight;
        private readonly float intensity=RenderSettings.ambientIntensity;
        private readonly Light[] originals;
        private readonly GameObject daylight;
        public PurpleBrightQaEnvironment()
        {
            originals=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(x=>x.enabled && x.type==LightType.Directional).ToArray();
            foreach(var light in originals)light.enabled=false;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.64f,.66f,.69f);RenderSettings.ambientIntensity=1;
            daylight=new GameObject("PressureQA_Daylight");var sun=daylight.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.6f;sun.color=new Color(1,.96f,.90f);sun.shadows=LightShadows.Soft;
            daylight.transform.rotation=Quaternion.Euler(45,-35,0);
        }
        public void Dispose()
        {
            RenderSettings.ambientMode=mode;RenderSettings.ambientLight=color;RenderSettings.ambientIntensity=intensity;
            foreach(var light in originals)if(light!=null)light.enabled=true;
            Object.Destroy(daylight);
        }
    }
}
