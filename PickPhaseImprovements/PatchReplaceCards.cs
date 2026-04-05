using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using ModdingUtils.Patches;
using Photon.Pun;
using SoundImplementation;
using UnboundLib;
using UnboundLib.Networking;
using UnboundLib.Utils;
using UnityEngine;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice), nameof(CardChoice.ReplaceCards))]
    public class PatchReplaceCards{
        internal static List<CardInfo> GeneratedCards = new List<CardInfo>();
        internal static int GeneratingCard;
        public static bool Prefix(CardChoice __instance, GameObject pickedCard, bool clear, ref IEnumerator __result){
            NetworkingManager.RPC(typeof (PatchReplaceCards), nameof(FixHandSize), __instance.pickrID);
            __result =  ReplaceCards(pickedCard, clear);
            return false;
        }

        [UnboundRPC]
        public static void FixHandSize(int pickrID){
            DrawNCards.CardChoicePatchStartPick.Prefix(CardChoice.instance, pickrID);
        }

        public static IEnumerator ReplaceCards(GameObject pickedCard = null, bool clear = false){
            CardChoice cardChoice = CardChoice.instance;
            Player player = cardChoice.pickerType != PickerType.Team ? PlayerManager.instance.players[cardChoice.pickrID] : PlayerManager.instance.GetPlayersInTeam(cardChoice.pickrID)[0];
            if (cardChoice.picks > 0)
                SoundPlayerStatic.Instance.PlayPlayerBallAppear();
            cardChoice.isPlaying = true;
            int i;
            if (clear && cardChoice.spawnedCards != null)
            {
                for (i = 0; i < cardChoice.spawnedCards.Count; ++i)
                {
                    if ((Object) pickedCard != (Object) cardChoice.spawnedCards[i])
                    {
                        cardChoice.spawnedCards[i].GetComponentInChildren<CardVisuals>().Leave();
                        yield return (object) new WaitForSecondsRealtime(0.1f);
                    }
                }
                yield return (object) new WaitForSecondsRealtime(0.2f);
                if ((bool) (Object) pickedCard)
                    pickedCard.GetComponentInChildren<CardVisuals>().Pick();
                cardChoice.spawnedCards.Clear();
            }
            yield return (object) new WaitForSecondsRealtime(0.2f);
            GeneratedCards.Clear();
            if (cardChoice.picks > 0){
                List<CardInfo> ValidCards = ModdingUtils.Utils.Cards.instance.GetAllCardsWithCondition(cardChoice, player, (p, c) => ModdingUtils.Utils.Cards.instance.PlayerIsAllowedCard(c, p)).ToList();
                List<CardInfo> TemperarilyRemoved = new List<CardInfo>();
                while (GeneratedCards.Count < cardChoice.children.Length){
                    CardInfo randomCard;
                    if (ValidCards.Count > 0){
                        randomCard = CardChoicePatchGetRanomCard.OrignialGetRanomCard(ValidCards.ToArray()).GetComponent<CardInfo>();
                        if (GeneratedCards.Count > 0){
                            bool valid = true;
                            foreach (var func in PickManager.DrawValidationFunctions){
                                var result = func(GeneratedCards.ToArray(), randomCard);
                                if (result == PickManager.ValidationResult.Valid)
                                    continue;
                                else if (result == PickManager.ValidationResult.CurrentlyInvalid)
                                    TemperarilyRemoved.Add(randomCard);
                                ValidCards.Remove(randomCard);
                                valid = false;
                                break;
                            }
                            if(!valid) continue;
                        }
                    }else{
                        randomCard = CardChoiceSpawnUniqueCardPatch.CardChoiceSpawnUniqueCardPatch.NullCard;
                    }
                    GeneratedCards.Add(randomCard);
                    ValidCards.AddRange(TemperarilyRemoved);
                    TemperarilyRemoved.Clear();
                    if (!randomCard.categories.Contains(CardChoiceSpawnUniqueCardPatch.CustomCategories.CustomCardCategories.CanDrawMultipleCategory)){
                        ValidCards.Remove(randomCard);
                    }
                }
                foreach (var handModification in PickManager.HandModifications){
                    GeneratedCards = handModification.Func.Invoke(GeneratedCards.ToArray()).ToList();
                }
                for (GeneratingCard = 0; GeneratingCard < cardChoice.children.Length; ++GeneratingCard)
                {
                    cardChoice.spawnedCards.Add(cardChoice.SpawnUniqueCard(cardChoice.children[GeneratingCard].transform.position, cardChoice.children[GeneratingCard].transform.rotation));
                    cardChoice.spawnedCards[GeneratingCard].AddComponent<PublicInt>().theInt = GeneratingCard;
                    yield return (object) new WaitForSecondsRealtime(PickNCards.PickNCards.delay);
                }
                foreach (var finalizationAction in PickManager.FinalizationActions){
                    finalizationAction.Invoke(GeneratedCards.ToArray());
                }
            }
            else
                cardChoice.GetComponent<PhotonView>().RPC("RPCA_DonePicking", RpcTarget.All);
            --cardChoice.picks;
            cardChoice.isPlaying = false;
        }
    }
}