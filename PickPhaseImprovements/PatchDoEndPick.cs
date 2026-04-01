using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice),nameof(CardChoice.RPCA_DoEndPick))]
    public class PatchDoEndPick{
        public static void Prefix(CardChoice __instance){
            foreach (Player player in PickManager.ShuffleQueue.Keys){
                if (__instance.pickerType == PickerType.Player ? player.playerID == __instance.pickrID : player.teamID == __instance.pickrID){
                    if (PickManager.ShuffleQueue[player].Count > 0){
                        __instance.picks++;
                        PickManager.ActiveCondition = PickManager.ShuffleQueue[player][0].Condition;
                        if (PickManager.ShuffleQueue[player][0].HandSize != 0){
                            PickManager.StoredHandSize = DrawNCards.DrawNCards.GetPickerDraws(__instance.pickrID);
                            DrawNCards.DrawNCards.SetPickerDraws(__instance.pickrID, 
                                PickManager.ShuffleQueue[player][0].Relative? PickManager.StoredHandSize + PickManager.ShuffleQueue[player][0].HandSize : PickManager.ShuffleQueue[player][0].HandSize);
                        }
                        PickManager.ShuffleQueue[player][0].pickStartCallback?.Invoke();
                        PickManager.ActiveCallback = PickManager.ShuffleQueue[player][0].pickEndCallback;
                        PickManager.ShuffleQueue[player].RemoveAt(0);
                        return;
                    }
                }
            }
        }
    }
}