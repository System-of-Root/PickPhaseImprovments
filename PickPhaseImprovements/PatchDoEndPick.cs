using System.Linq;
using HarmonyLib;
using UnboundLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice),nameof(CardChoice.RPCA_DoEndPick))]
    public class PatchDoEndPick{
        public static void Prefix(CardChoice __instance){
            foreach (Player player in PickManager.ShuffleQueue.Keys){
                if (__instance.pickerType == PickerType.Player ? player.playerID == __instance.pickrID : player.teamID == __instance.pickrID){
                    if (PickManager.ShuffleQueue[player].Count > 0){
                        __instance.picks++;
                        if (player.data.view.IsMine){
                            var condition = PickManager.ShuffleQueue[player][0].Condition;
                            Unbound.Instance.ExecuteAfterSeconds(0.25f, () => PickManager.ActiveCondition = condition ?? PickManager.ActiveCondition);
                        }

                        if (PickManager.ShuffleQueue[player][0].HandSize != 0){
                            PickManager.StoredHandSize = DrawNCards.DrawNCards.GetPickerDraws(__instance.pickrID);
                            PickManager.SetPickerDraws(__instance.pickrID, 
                                PickManager.ShuffleQueue[player][0].Relative? PickManager.StoredHandSize + PickManager.ShuffleQueue[player][0].HandSize : PickManager.ShuffleQueue[player][0].HandSize);
                        }
                        PickManager.ShuffleQueue[player][0].pickStartCallback?.Invoke();
                        PickManager.ActiveCallback = PickManager.ShuffleQueue[player][0].pickEndCallback;
                        PickManager.ShuffleQueue[player].RemoveAt(0);
                        return;
                    }
                }
            }
            foreach (Player player in PickManager.LimitedDrawQueue.Keys){
                if (__instance.pickerType == PickerType.Player ? player.playerID == __instance.pickrID : player.teamID == __instance.pickrID){
                    if (PickManager.LimitedDrawQueue[player].Count(ld => ld.isShuffle) > 0){
                        __instance.picks++;
                        PickManager.LimitedDraw limitedDraw = PickManager.LimitedDrawQueue[player].First(ld => ld.isShuffle);
                        PickManager.LimitedDrawQueue[player].Remove(limitedDraw);
                        if (player.data.view.IsMine)
                            PickManager.ActiveLimitedDraw = limitedDraw;
                        if (limitedDraw.data.HandSize != 0){
                            PickManager.StoredHandSize = DrawNCards.DrawNCards.GetPickerDraws(__instance.pickrID);
                            PickManager.SetPickerDraws(__instance.pickrID, 
                                limitedDraw.data.Relative? PickManager.StoredHandSize + limitedDraw.data.HandSize : limitedDraw.data.HandSize);
                        }
                        limitedDraw.data.pickStartCallback?.Invoke();
                        PickManager.ActiveCallback = limitedDraw.data.pickEndCallback;
                        return;
                    }
                }
            }
        }
    }
}