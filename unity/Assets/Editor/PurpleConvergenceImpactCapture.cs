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
 public static class PurpleConvergenceImpactCapture
 {
 private static string Output=>Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/PurpleConvergenceImpactQA"));
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

        public static string Capture(int take)
        {
            const bool full=true;const int phaseStep=5;int mode=take%2;bool casterView=take>=2;
            bool oldImpact=PurpleConvergenceImpactProfile.Current.candidateEnabled;
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
            var floorMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));floorMaterial.SetColor("_BaseColor",new Color(.035f,.045f,.06f));
            floor.GetComponent<Renderer>().sharedMaterial=floorMaterial;
            Vector3 oldPosition = actor.transform.position;
            Quaternion oldRotation = actor.transform.rotation;
            bool oldCameraEnabled = caster.enabled;
            float oldFieldOfView = caster.fieldOfView;
            RenderTexture oldTarget = caster.targetTexture;
            Vector3 cameraPosition=caster.transform.position; Quaternion cameraRotation=caster.transform.rotation;
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
            try
            {
                    {
                        Select(false);PurpleIngredientFlowFirstProfile.Current.candidateEnabled=true;PurpleConvergenceImpactProfile.Current.candidateEnabled=mode==1;
                        UnityEngine.Random.InitState(240926);
                        actor.transform.SetPositionAndRotation(stage, Quaternion.identity);
                        caster.transform.SetPositionAndRotation(stage + new Vector3(0, 4, -11), Quaternion.Euler(12, 0, 0));
                        var host = new GameObject("FlowFirstABSequence");
                        var sequence = PrototypeHollowPurplePresentationRuntime.CreateCanonicalOrbSequence(host.transform, actor.transform, 0, casterView ? caster : null, casterView);
                        string stem = (casterView ? "Caster" : "Side") + (mode == 0 ? "_Baseline" : "_Candidate");
                        var impact=host.GetComponentInChildren<PurpleConvergenceImpact>();
                        Assert.That(impact!=null,Is.EqualTo(mode==1));
                        float contact=GojoPolishSettings.Current.PurpleFusionStart+PurpleIngredientCollisionAccent.FindContactNormalized(GojoPolishSettings.Current.purpleFormationSeparation,GojoPolishSettings.Current.purpleFormationScale,PurpleIngredientReboot2Profile.Current.fusionVerticalArc)*GojoPolishSettings.Current.purpleFusionDuration;
                        float hitWall=Mathf.Ceil(contact*60)/60f;
                        impact?.SetPreview(casterView?caster:side,0);
                        var poses=new System.Text.StringBuilder();
                        var clockLog=new System.Text.StringBuilder();int whiteFrames=0;
                        File.WriteAllText(Path.Combine(output,stem+".contact.json"),"{\"contact\":"+contact.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+",\"sampledAt\":"+hitWall.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"}");
                        try
                        {
                            using (var stream = full ? new FileStream(Path.Combine(output, stem + ".rgb24"), FileMode.CreateNew) : null)
                                for (int frame = 0; frame <= (full ? 192 : 3); frame++)
                                {
                                    float wall=frame/60f;
                                    // Canonical Purple is unscaled in live runtime. Do not fake a VFX pause in the comparison.
                                    float t=wall;
                                    impact?.SetPreviewTime(wall);
                                    sequence.Update(t,1/60f);
                                    Canvas.ForceUpdateCanvases();
                                    if(impact!=null && impact.FlashVisible)whiteFrames++;
                                    clockLog.AppendLine(wall+","+t+","+(impact!=null && impact.FlashVisible));
                                    foreach(string item in new[]{"HollowPurpleBlueOrbRoot","HollowPurpleRedOrbRoot","HollowPurpleDenseBody"})
                                    {
                                        var pose=host.transform.Find("HollowPurpleCanonicalOrbSequence/"+item);
                                        poses.AppendLine(t.ToString("R",System.Globalization.CultureInfo.InvariantCulture)+"|"+item+"|"+pose.position.ToString("F7")+"|"+pose.localScale.ToString("F7")+"|"+pose.gameObject.activeSelf);
                                    }
                                    Vector3 offset = new Vector3(5, 2, -12);
                                    side.transform.SetPositionAndRotation(origin + Vector3.forward * Mathf.Clamp01((t - GojoPolishSettings.Current.PurpleReleaseTime) / 1.6f) * 48 + offset, Quaternion.LookRotation(-offset));
                                    RenderPipeline.SubmitRenderRequest(casterView ? caster : side, new UniversalRenderPipeline.SingleCameraRequest { destination = target });
                                    var previous = RenderTexture.active;
                                    RenderTexture.active = target;
                                    var image = new Texture2D(960, 540, TextureFormat.RGB24, false);
                                    image.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
                                    image.Apply();
                                    RenderTexture.active = previous;
                                    if (full) stream.Write(image.GetRawTextureData<byte>().ToArray(), 0, 960 * 540 * 3);
                                    if (frame==86 || frame==87 || frame==88 || frame==90 || frame==120)
                                        File.WriteAllBytes(Path.Combine(output, stem + "_" + frame.ToString("D3") + ".png"), image.EncodeToPNG());
                                    Object.DestroyImmediate(image);
                                }
                            File.WriteAllText(Path.Combine(output,stem+".poses.txt"),poses.ToString());
                            File.WriteAllText(Path.Combine(output,stem+".clock.csv"),clockLog.ToString());
                            Assert.That(whiteFrames,Is.EqualTo(mode==1?2:0),"Flash must cover exactly two nominal 60Hz samples");
                            // Compare the protected body after all contact feedback has ended.
                            impact?.SetPreviewTime(5);sequence.Update(3.4f,1/60f);
                            side.transform.SetPositionAndRotation(origin+Vector3.forward*Mathf.Clamp01((3.4f-GojoPolishSettings.Current.PurpleReleaseTime)/1.6f)*48+new Vector3(5,2,-12),Quaternion.LookRotation(-new Vector3(5,2,-12)));
                            RenderPipeline.SubmitRenderRequest(casterView?caster:side,new UniversalRenderPipeline.SingleCameraRequest{destination=target});
                            var prior=RenderTexture.active;RenderTexture.active=target;var still=new Texture2D(960,540,TextureFormat.RGB24,false);still.ReadPixels(new Rect(0,0,960,540),0,0);still.Apply();RenderTexture.active=prior;
                            File.WriteAllBytes(Path.Combine(output,stem+"_ProtectedPhase3_4.png"),still.EncodeToPNG());Object.DestroyImmediate(still);
                        }
                        finally { sequence.Dispose(); Object.Destroy(host); }
                    }
                File.WriteAllText(Path.Combine(output, "format.json"), "{\"width\":960,\"height\":540,\"fps\":60,\"frames\":" + (full ? 193 : 4) + ",\"baseline\":\"FlowFirst\",\"candidate\":\"FlowFirst + ConvergenceImpact\"}");
                return output;
            }
            finally
            {
                PurpleConvergenceImpactProfile.Current.candidateEnabled=oldImpact;
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
                caster.enabled = oldCameraEnabled;
                caster.fieldOfView = oldFieldOfView;
                caster.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
                target.Release();
                Object.Destroy(target);
                Object.Destroy(side.gameObject);
                Object.Destroy(floor);Object.Destroy(floorMaterial);
            }
        }
 }
}
