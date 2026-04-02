using HarmonyLib;
using UnityEngine;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.Pick))]
    public class PatchCardChoicePick{
        public static void Prefix(CardChoice __instance, GameObject pickedCard){
            if (pickedCard == null) return;
            PickManager.lastPickedCard = pickedCard.GetComponent<CardInfo>();
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