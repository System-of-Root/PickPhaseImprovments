using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.StartPick))]
    public class PatchPickStart{
        public static void Prefix(CardChoice __instance, ref int picksToSet, int pickerIDToSet){
            if (Plugin.PickNModeSetting == Plugin.PickNMode.FirstOnly){
                if(PickManager.PickedThisRound[PlayerManager.instance.GetPlayerWithID(pickerIDToSet)])return;
                PickManager.PickedThisRound[PlayerManager.instance.GetPlayerWithID(pickerIDToSet)] = true;
            }
            PickManager.PickDepth = 0;
            if (Plugin.PickNModeSetting == Plugin.PickNMode.Uncollated){
                picksToSet = PickNCards.PickNCards.picks;
            }
            foreach (Player player in PickManager.AdditionalPicks.Keys){
                if (__instance.pickerType == PickerType.Player ? player.playerID == pickerIDToSet : player.teamID == pickerIDToSet){
                    picksToSet+= PickManager.AdditionalPicks[player];
                }
            }
            foreach (Player player in PickManager.ConditionalPicks.Keys){
                if (__instance.pickerType == PickerType.Player ? player.playerID == pickerIDToSet : player.teamID == pickerIDToSet){
                    foreach (PickManager.ShuffleData data in PickManager.ConditionalPicks[player]){
                        PickManager.QueueShuffleForPicker(player,data);
                    }

                }
            }
        }
    }
}