using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnboundLib.GameModes;
using UnityEngine;

namespace PickPhaseImprovements{
    [BepInDependency("pykess.rounds.plugins.moddingutils")]
    [BepInDependency("pykess.rounds.plugins.pickncards")]
    [BepInDependency("Systems.R00t.MorePicksPatch")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class Plugin : BaseUnityPlugin{
        private const string ModId = "Systems.R00t.PickPhaseImprovements";
        private const string ModName = "Pick Phase Improvments";
        public const string Version = "0.1.3";
        void Awake(){
            new Harmony(ModId).PatchAll();
            GameModeManager.AddHook(GameModeHooks.HookGameStart, Reset,Priority.First);
            ModdingUtils.Utils.Cards.instance.AddCardValidationFunction(CheckCondition);
        }

        bool CheckCondition(Player _, CardInfo cardInfo){
            return PickManager.ActiveCondition(cardInfo);
        }

        private IEnumerator Reset(IGameModeHandler _){
            PickManager.ShuffleQueue.Clear();
            PickManager.ConditionalPicks.Clear();
            PickManager.AdditionalPicks.Clear();
            PickManager.ActiveCondition = _ => true;
            PickManager.StoredHandSize = -1;
            yield break;
        }
    }
}