using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(ApplyCardStats), nameof(ApplyCardStats.ApplyStats))]
    public class PatchApplyCardStats{
        public static void Prefix(ApplyCardStats __instance){
            if (PickManager.ShuffleCards.ContainsKey(__instance.GetComponent<CardInfo>())){
                for(int i = 0; i<PickManager.ShuffleCards[__instance.GetComponent<CardInfo>()].count;i++)
                    PickManager.QueueShuffleForPicker(__instance.playerToUpgrade, PickManager.ShuffleCards[__instance.GetComponent<CardInfo>()]);

            }
        }
    }
}