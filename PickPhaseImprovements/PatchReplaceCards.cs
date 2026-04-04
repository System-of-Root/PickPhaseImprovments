using HarmonyLib;
using UnboundLib;
using UnboundLib.Networking;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.ReplaceCards))]
    public class PatchReplaceCards{
        public static void Prefix(CardChoice __instance){
            NetworkingManager.RPC(typeof (PatchReplaceCards), nameof(FixHandSize), __instance.pickrID);
        }

        [UnboundRPC]
        public static void FixHandSize(int pickrID){
            DrawNCards.CardChoicePatchStartPick.Prefix(CardChoice.instance, pickrID);
        }
    }
}