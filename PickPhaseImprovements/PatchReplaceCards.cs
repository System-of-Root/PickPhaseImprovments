using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.ReplaceCards))]
    public class PatchReplaceCards{
        public static void Prefix(CardChoice __instance){
            DrawNCards.CardChoicePatchStartPick.Prefix(__instance, __instance.pickrID);
        }
    }
}