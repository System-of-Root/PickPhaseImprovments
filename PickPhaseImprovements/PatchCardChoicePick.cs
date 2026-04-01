using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.Pick))]
    public class PatchCardChoicePick{
        public static void Prefix(CardChoice __instance){
            PickManager.ActiveCondition = _ => true;
            PickManager.PickDepth++;
            if (PickManager.StoredHandSize != -1){
                DrawNCards.DrawNCards.SetPickerDraws(__instance.pickrID,PickManager.StoredHandSize);
                PickManager.StoredHandSize = -1;
            }
            PickManager.ActiveCallback?.Invoke();
            PickManager.ActiveCallback = null; 
        }
    }
}