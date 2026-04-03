using BepInEx;
using HarmonyLib;
using PCE.Cards;
using PickPhaseImprovements;

namespace MSGA{
    [BepInDependency("pykess.rounds.plugins.pykesscardexpansion")]
    [BepInDependency("Systems.R00t.PickPhaseImprovements")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    [HarmonyPatch(typeof(ShuffleCard))]
    public class Patch: BaseUnityPlugin{ 
        private const string ModId = "Systems.R00t.msga";
        private const string ModName = "Make Shuffle Great Again";
        public const string Version = "0.1.0";

        void Awake(){
            new Harmony(ModId).PatchAll();
        }
        
        [HarmonyPatch(nameof(ShuffleCard.SetupCard))]
        [HarmonyPrefix]
        public static bool Setup(CardInfo cardInfo){
            PickManager.RegisterShuffleCard(cardInfo);
            return false;
        }
        [HarmonyPatch(nameof(ShuffleCard.OnAddCard))]
        [HarmonyPrefix]
        public static bool Add(){
            return false;
        }
        
    }
}