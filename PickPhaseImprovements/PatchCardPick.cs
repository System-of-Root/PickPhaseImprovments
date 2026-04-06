using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnboundLib;

namespace PickPhaseImprovements{
    [HarmonyPatch()]
    public class PatchCardPick{
        [HarmonyPatch(typeof(ApplyCardStats), nameof(ApplyCardStats.Pick))]
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        static bool RedirectPick(ApplyCardStats __instance, int pickerID, bool forcePick, PickerType pickerType){
            __instance.Start();
            if(__instance.done && !forcePick)return false;
            __instance.done = true;
            
            Player[] players = PlayerManager.instance.GetPlayersInTeam(pickerID);
            if (pickerType == PickerType.Player)
                players = new Player[1]
                {
                    PlayerManager.instance.players[pickerID]
                };
            __instance.OFFLINE_Pick(players);
            return false;
        }

        [HarmonyPatch(typeof(ApplyCardStats), nameof(ApplyCardStats.OFFLINE_Pick))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspilePick(IEnumerable<CodeInstruction> instructions){
            List<CodeInstruction> code = instructions.ToList();
            List<CodeInstruction> newCode = new List<CodeInstruction>();
            for (int i = 0; i < code.Count; i++){
                if (code[i].opcode == OpCodes.Call && code[i].Calls(typeof(ApplyCardStats).GetMethod(nameof(ApplyCardStats.ApplyStats),BindingFlags.NonPublic|BindingFlags.Instance))){
                    newCode.RemoveAt(newCode.Count - 1);
                    i += 2;
                    newCode.Add(code[i++]);
                    newCode.Add(code[i++]);
                    ++i;
                    newCode.Add(code[i++]);
                    newCode.Add(code[i++]);
                    newCode.Add(code[i++]);
                    newCode.Add(CodeInstruction.Call(typeof(PatchCardPick),nameof(PatchCardPick.DoApplyCard)));
                }
                else{
                    newCode.Add(code[i]);
                }
            }

            return newCode;
        }


        public static void DoApplyCard(Player player, CardInfo cardInfo){
            if (!PickManager.Synchronous){
                string cardName = PickManager.CustomApplicationName.ContainsKey(cardInfo)? PickManager.CustomApplicationName[cardInfo] : cardInfo.name;
                NetworkingManager.RPC(typeof(ModdingUtils.Utils.Cards),nameof(ModdingUtils.Utils.Cards.RPCA_AssignCard),cardName,player.playerID,false,"",0.0f,0.0f);
            }
        }
    }
    
}