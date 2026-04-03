using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using PickTimer.Util;
using UnboundLib;
using UnityEngine;

namespace PickTimerCompat{
    [BepInDependency("ot.dan.rounds.picktimer")]
    [BepInDependency("Systems.R00t.PickPhaseImprovements")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    [HarmonyPatch]
    public class Plugin: BaseUnityPlugin{
        private const string ModId = "Systems.R00t.PickPhaseImprovements.PickTimerCompat";
        private const string ModName = "PickTimerCompat";
        public const string Version = "0.1.0";

        void Awake(){
            new Harmony(ModId).PatchAll();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.ReplaceCards))]
        public static void RetriggerTimer(CardChoice __instance){
            if (__instance.picks > 0){
                __instance.StartCoroutine(PickTimer.Util.TimerHandler.Start(null));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PickTimerController), nameof(PickTimerController.StartPickTimer))]
        public static bool FixTimerSoftlock(CardChoice instance, ref IEnumerator __result){
            __result = FixedStartPickTimer(instance);
            return false;
        }

        public static IEnumerator FixedStartPickTimer(CardChoice instance){
            if (PickTimer.PickTimer.PickTimerEnabled)
            {
                if (PickTimerController.TimerCr != null)
                    Unbound.Instance.StopCoroutine(PickTimerController.TimerCr);
                PickTimerController.TimerCr = Unbound.Instance.StartCoroutine(PickTimerController.Timer((float) PickTimer.PickTimer.PickTimerTime));
                yield return (object) new WaitForSecondsRealtime((float) PickTimer.PickTimer.PickTimerTime);
                Traverse traverse = Traverse.Create((object) instance);
                List<GameObject> spawnedCards = (List<GameObject>) traverse.Field("spawnedCards").GetValue();
                instance.Pick(spawnedCards[PickTimerController.Random.Next(0, spawnedCards.Count)]);
            }
        }
    }
}