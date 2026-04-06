using HarmonyLib;
using Photon.Pun;
using UnboundLib;
using UnityEngine;

namespace PickPhaseImprovements{
    [HarmonyPatch(typeof(CardChoice),nameof(CardChoice.SpawnUniqueCard))]
    [HarmonyPriority(Priority.First)]
    public class PatchSpawnUniqueCard{
        public static bool Prefix(CardChoice __instance, Vector3 pos, Quaternion rot, ref GameObject __result){
            CardInfo CardToSpawn = PatchReplaceCards.GeneratedCards[PatchReplaceCards.GeneratingCard];
            if (PickManager.Synchronous){
                __result = PhotonNetwork.prefabPool.Instantiate(CardToSpawn.name, pos, rot);
                __result.GetComponent<PhotonView>().instantiationDataField = PickManager.CustomPhotonData.GetValueOrDefault(CardToSpawn);
                __result.GetComponent<IPunInstantiateMagicCallback>().OnPhotonInstantiate(new PhotonMessageInfo(PhotonNetwork.LocalPlayer,PhotonNetwork.ServerTimestamp,__result.GetComponent<PhotonView>()));
                __result.transform.localScale = DrawNCards.DrawNCards.GetScale(DrawNCards.DrawNCards.numDraws);
                __instance.spawnedCards.Add(__result);
            }
            else{
                __result = PhotonNetwork.Instantiate(CardToSpawn.name, pos, rot,data:PickManager.CustomPhotonData.GetValueOrDefault(CardToSpawn));
                __result.GetComponent<CardInfo>().sourceCard = CardToSpawn.GetComponent<CardInfo>();
                ((Behaviour) __result.GetComponentInChildren<DamagableEvent>().GetComponent<Collider2D>()).enabled = false;
                DrawNCards.CardChoicePatchSpawn.Postfix(ref __result);
            }
            return false;
        }
    }
}