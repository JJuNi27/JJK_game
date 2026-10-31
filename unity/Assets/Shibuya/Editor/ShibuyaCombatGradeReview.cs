using System;
using JJKGame.Core;
using UnityEditor;
using UnityEngine;
namespace JJKGame.Shibuya.EditorTools
{
    // Runtime copies only. Closing the window restores only the configuration owned by this tool.
    public sealed class ShibuyaCombatGradeReview : EditorWindow
    {
        ShibuyaFighter actor;
        ShibuyaCombatProfile original,copy;
        ShibuyaAttackGradeProfile grades;
        CharacterStatsProfile originalStats;
        int seed=42;
        sealed class FixedGradeRandom : IShibuyaHitRandom{
            readonly int mode;int draw;
            public FixedGradeRandom(int value){mode=value;}
            public void BeginHit(){draw=0;}
            public float Next01(){if(mode==0)return .99999f;return mode==1?(draw++%2==0?0:.99999f):0;}
        }
        [MenuItem("JJK Game/시부야 PASS4G/공격 등급·반격 검수")]
        public static void Open()=>GetWindow<ShibuyaCombatGradeReview>("공격 등급·반격 검수");
        void Bind(ShibuyaFighter value){Restore();actor=value;if(actor==null||!Application.isPlaying||actor.Profile.attackGrades==null)return;
            original=actor.Profile;originalStats=actor.StatsProfile;copy=Instantiate(original);grades=Instantiate(original.attackGrades);
            copy.name="PASS4G QA runtime combat";grades.name="PASS4G QA runtime grades";copy.attackGrades=grades;
            actor.Configure(copy,originalStats);}
        void Restore(){if(actor!=null&&copy!=null&&actor.Profile==copy){actor.Configure(original,originalStats);actor.SetCombatRandom(null);}
            if(copy!=null)DestroyImmediate(copy);if(grades!=null)DestroyImmediate(grades);copy=null;grades=null;original=null;}
        void OnDisable()=>Restore();
        void OnInspectorUpdate(){if(!Application.isPlaying)Restore();Repaint();}
        void OnGUI(){
            EditorGUILayout.HelpBox("Play Mode 전용입니다. 테스트 흑섬 허용과 강제 추첨은 런타임 사본에만 적용됩니다. 창을 닫으면 복원합니다. 일반 HUD는 변경하지 않습니다.",MessageType.Info);
            using(new EditorGUI.DisabledScope(!Application.isPlaying)){
                var selected=(ShibuyaFighter)EditorGUILayout.ObjectField("검수 캐릭터",actor,typeof(ShibuyaFighter),true);if(selected!=actor)Bind(selected);
                if(GUILayout.Button("현재 플레이어 연결")){var match=FindFirstObjectByType<ShibuyaMatch>();if(match!=null)Bind(match.Player);}
                if(actor==null||grades==null)return;
                var numbers=actor.GetComponent<ShibuyaPlayerInput>()?.DamageNumbers;
                if(numbers!=null){
                    bool show=EditorGUILayout.Toggle("데미지 숫자 표시",numbers.Options.Enabled);numbers.SetDisplayEnabled(show);
                    EditorGUILayout.LabelField("마지막 표시 피해 / 등급",$"{numbers.LastDisplayedDamage:G9} / {Grade(numbers.LastDisplayedGrade)}");
                    EditorGUILayout.LabelField("마지막 표시 성공·차단 사유",numbers.LastReason);
                    EditorGUILayout.LabelField("현재 / 누적 표시 개수",$"{numbers.ActiveCount} / {numbers.PresentedCount}");
                    if(GUILayout.Button("데미지 표시 설정 선택"))Selection.activeObject=numbers.Profile;
                }
                EditorGUILayout.HelpBox("아래 추첨 버튼은 다음 한 타 예약이 아니라, 이후 각 타격이 읽는 QA RNG를 교체합니다. 등급 자격·확률 경계·특수 방어를 우회하지 않습니다. 고정 시드 또는 창 닫기로 종료하세요.",MessageType.Info);
                grades.blackFlashCapable=EditorGUILayout.Toggle("QA 전용 흑섬 사용 허용",grades.blackFlashCapable);
                seed=EditorGUILayout.IntField("QA 고정 시드",seed);if(GUILayout.Button("고정 시드 적용"))actor.SetCombatRandom(new ShibuyaSeededRandom(seed));
                EditorGUILayout.BeginHorizontal();if(GUILayout.Button("일반 추첨"))actor.SetCombatRandom(new FixedGradeRandom(0));
                if(GUILayout.Button("치명타 추첨"))actor.SetCombatRandom(new FixedGradeRandom(1));if(GUILayout.Button("흑섬 추첨"))actor.SetCombatRandom(new FixedGradeRandom(2));EditorGUILayout.EndHorizontal();
                var strike=actor.CurrentStrike??actor.Profile.strikes[0];var meta=strike.attack;var p=actor.GetGradeProbabilities(meta,actor.CounterTarget);
                EditorGUILayout.LabelField("공격 분류",meta==null?"미지정":$"{Reach(meta.reach)} / {Nature(meta.nature)} / {Origin(meta.origin)}");
                EditorGUILayout.LabelField("치명타 / 조건부 흑섬",$"{p.Critical:P2} / {p.RawConditional:P2}");
                EditorGUILayout.LabelField("실제 조건부 / 최종 흑섬",$"{p.EffectiveConditional:P2} / {p.FinalBlackFlash:P2}");
                EditorGUILayout.LabelField("일반 / Zone 최종 상한",$"{grades.normalFinalCap}% / {grades.zoneFinalCap}%");
                var counter=actor.GetGradeProbabilities(meta,actor.CounterTarget,true,actor.CounterSettings!=null?actor.CounterSettings.counterBlackFlashConditional:0);
                EditorGUILayout.LabelField("반격 치명타 / 흑섬",$"{counter.Critical:P0} / {counter.FinalBlackFlash:P0} (반격 전용 예외)");
                EditorGUILayout.LabelField("흑섬 성공 / Zone 스택",$"{actor.BlackFlashCount} / {actor.ZoneStacks}");
                EditorGUILayout.LabelField("Zone 남은 시간",$"{actor.ZoneRemaining:F3}초");
                var resource=actor.GetComponent<CursedEnergyController>();EditorGUILayout.LabelField("현재 / 최대 주력",resource!=null?$"{resource.CurrentEnergy:F2} / {resource.MaxEnergy:F2}":"주력 연결 없음");
                EditorGUILayout.LabelField("마지막 적용 등급 / HP 감소",$"{Grade(actor.LastAttackGrade)} / {actor.LastAppliedGradeDamage:F2}");
                EditorGUILayout.LabelField("반격 입력 기회",$"{(actor.HasCounterOpportunity?"활성":"없음")} / {actor.CounterOpportunityRemaining:F3}초");
                EditorGUILayout.ObjectField("반격 예약 대상",actor.CounterTarget,typeof(ShibuyaFighter),true);
                EditorGUILayout.LabelField("반격 실행 / 적용",$"{actor.CounterStartedCount} / {actor.CounterAppliedCount}");
                EditorGUILayout.LabelField("반격 상태·취소 원인",Reason(actor.LastCounterReason));
            }
        }
        static string Reach(ShibuyaAttackReach v)=>v==ShibuyaAttackReach.Melee?"근접":v==ShibuyaAttackReach.Ranged?"원거리":v==ShibuyaAttackReach.Special?"특수":"미지정";
        static string Nature(ShibuyaDamageNature v)=>v==ShibuyaDamageNature.Taijutsu?"체술":v==ShibuyaDamageNature.Technique?"술식":v==ShibuyaDamageNature.Hybrid?"복합":"미지정";
        static string Origin(ShibuyaAttackOrigin v)=>v==ShibuyaAttackOrigin.Basic?"평타":v==ShibuyaAttackOrigin.Skill?"스킬":v==ShibuyaAttackOrigin.Counter?"반격":v==ShibuyaAttackOrigin.Ultimate?"궁극기":v==ShibuyaAttackOrigin.Other?"기타":"미지정";
        static string Grade(ShibuyaAttackGrade v)=>v==ShibuyaAttackGrade.BlackFlash?"흑섬":v==ShibuyaAttackGrade.Critical?"치명타":"일반";
        static string Reason(string value){switch(value){case "none":return "없음";case "opportunity":return "반격 입력 대기";case "reserved":return "대상 예약·후속 모션 대기";
            case "executing":return "반격 실행 중";case "applied":return "유효 피해 적용";case "applied-complete":return "반격 적중·종료";
            case "world-obstruction":return "지형 차단으로 취소";case "target-special-invulnerable":return "특수 무적으로 취소";
            case "opportunity-expired-or-invalid":return "입력 기회 만료 또는 대상 무효";default:return "종료·취소 코드: "+value;}}
    }
}
