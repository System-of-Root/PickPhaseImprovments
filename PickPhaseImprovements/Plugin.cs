using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using TMPro;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnboundLib.Utils.UI;
using UnityEngine;
using Photon.Pun;

namespace PickPhaseImprovements{
    [BepInDependency("pykess.rounds.plugins.moddingutils")]
    [BepInDependency("pykess.rounds.plugins.pickncards")]
    [BepInDependency("Systems.R00t.MorePicksPatch")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Plugin : BaseUnityPlugin{
        private const string ModId = "Systems.R00t.PickPhaseImprovements";
        private const string ModName = "Pick Phase Improvments";
        public const string Version = "0.4.2";
        public static ConfigEntry<int> PickNModeConfig;
        public static PickNMode PickNModeSetting;
        public static int BonusStartingPicks;
        public static ConfigEntry<int> BonusStartingPicksConfig;
        private static UnityEngine.UI.Slider slider = null;
        private static Harmony harmony = null;

        public enum PickNMode{
            Normal,
            Uncollated,
            FirstOnly
        }
        
        void Awake(){
            PickNModeConfig = Config.Bind(ModId, "PickNModeSetting", 0);
            PickNModeSetting = (PickNMode)PickNModeConfig.Value;
            BonusStartingPicksConfig = Config.Bind(ModId, "BonusStartingPicksSetting", 0);
            BonusStartingPicks = BonusStartingPicksConfig.Value;
            harmony = new Harmony(ModId);
            harmony.PatchAll();
            GameModeManager.AddHook(GameModeHooks.HookGameStart, ResetData,Priority.First);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, NewRound);
            ModdingUtils.Utils.Cards.instance.AddCardValidationFunction(CheckCondition);
            ModdingUtils.Utils.Cards.instance.AddCardValidationFunction(LockGunToDefualtCheck);
            Unbound.RegisterMenu(ModName, () => { }, ModGUI, null, false);
            Unbound.RegisterHandshake(ModId, () => {
                if (PhotonNetwork.IsMasterClient) NetworkingManager.RPC_Others(typeof(Plugin),nameof(SyncSettings), (int)PickNModeSetting, BonusStartingPicks);
            });

        }

        IEnumerator Start(){
            for(int _ = 0; _<5;_++) yield return null;
            
            // get the MethodBase of the original
            var SpawnUniqueCard = typeof(CardChoice).GetMethod(nameof(CardChoice.SpawnUniqueCard),BindingFlags.Instance| BindingFlags.NonPublic);
            harmony.Unpatch(SpawnUniqueCard, HarmonyPatchType.Prefix, "pykess.rounds.plugins.cardchoicespawnuniquecardpatch");
        }

        bool CheckCondition(Player _, CardInfo cardInfo){
            return PickManager.ActiveCondition(cardInfo);
        }

        bool LockGunToDefualtCheck(Player player, CardInfo card){
            Holdable holdable = player.data.GetComponent<Holding>().holdable;
            if(holdable) 
            {
                Gun component2 = holdable.GetComponent<Gun>();
                Gun component3 = card.GetComponent<Gun>();
                if(component3 && component2 && component3.lockGunToDefault && component2.lockGunToDefault) 
                {
                    return false;
                }
            }
            return true;
        }

        private IEnumerator NewRound(IGameModeHandler _){
            foreach (var player in PlayerManager.instance.players){
                PickManager.PickedThisRound[player] = false;
            }
            yield break;
        }
        
        private IEnumerator ResetData(IGameModeHandler _){
            PickNCards.PickNCards.extraPicksInPorgress = false;
            PickManager.ShuffleQueue.Clear();
            foreach (var player in PlayerManager.instance.players){
                for(int i = 0; i<BonusStartingPicks;i++){
                    PickManager.QueueShuffleForPicker(player);
                }
            }
            PickManager.ConditionalPicks.Clear();
            PickManager.AdditionalPicks.Clear();
            PickManager.ActiveCondition = _ => true;
            PickManager.StoredHandSize = -1;
            PickManager.LimitedDrawQueue.Clear();
            yield return NewRound(_);
        }


        [UnboundRPC]
        private static void SyncSettings(int pickNMode, int bonusStatingPicks)
        {
            PickNModeSetting = (PickNMode)pickNMode;
            BonusStartingPicks = bonusStatingPicks;
        }


        public void ModGUI(GameObject menu){
            MenuHandler.CreateText(ModName+" Settings:", menu,out _,80,alignmentOptions:TextAlignmentOptions.Center);
            MenuHandler.CreateSlider("Bonus Starting Picks", menu, 30, 0, 10, BonusStartingPicks, (val) => {
                BonusStartingPicksConfig.Value = (int)val;
                BonusStartingPicks = (int)val;
            }, out _, true);
            MenuHandler.CreateText("Pick N Compatibility Mode", menu,out _,60,alignmentOptions:TextAlignmentOptions.Center);
            MenuHandler.CreateSlider(PickNModeSetting.ToString(), menu, 30, 0, 2, (int)PickNModeSetting, (val) => {
                PickNModeConfig.Value=(int)val;
                PickNModeSetting = (PickNMode)PickNModeConfig.Value;
                slider.transform.parent.GetChild(slider.transform.parent.childCount - 1).GetComponent<TextMeshProUGUI>().text = PickNModeSetting.ToString();
            },out slider,true);
            Destroy(slider.transform.parent.GetChild(1).gameObject);
        }
    }
}