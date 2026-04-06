using System.Collections;
using HarmonyLib;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(PickNCards.PickNCards),nameof(PickNCards.PickNCards.ExtraPicks))]
    public class PatchPickNCards{
        public static void Prefix(){
            if (Plugin.PickNModeSetting == Plugin.PickNMode.Uncollated)
                PickNCards.PickNCards.extraPicksInPorgress = true;
            
        }
    }
}