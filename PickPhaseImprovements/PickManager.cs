using System;
using System.Collections.Generic;
using HarmonyLib;
using UnboundLib;
using UnboundLib.Utils;
using UnityEngine;
using CollectionExtensions = System.Collections.Generic.CollectionExtensions;

namespace PickPhaseImprovements {
    public static class PickManager {
        internal static Dictionary<CardInfo, ShuffleData> ShuffleCards = new Dictionary<CardInfo, ShuffleData>();
        internal static Dictionary<Player,List<ShuffleData>> ShuffleQueue = new Dictionary<Player,List<ShuffleData>>();
        internal static Dictionary<Player,List<ShuffleData>> ConditionalPicks = new Dictionary<Player,List<ShuffleData>>();
        internal static Dictionary<Player,int> AdditionalPicks = new Dictionary<Player,int>();
        internal static Dictionary<Player,bool> PickedThisRound = new Dictionary<Player,bool>();
        internal static Func<CardInfo, bool> ActiveCondition = _ => true;
        internal static Action ActiveCallback = null;
        internal static Dictionary<Player,List<LimitedDraw>> LimitedDrawQueue = new Dictionary<Player,List<LimitedDraw>>();
        internal static LimitedDraw? ActiveLimitedDraw = null;
        public static int PickDepth = 0;
        public static CardInfo lastPickedCard;
        internal static int StoredHandSize = -1;

        public static bool IsShuffleCard(CardInfo cardInfo){
            return  ShuffleCards.ContainsKey(cardInfo);
        }
        
        public static void RegisterShuffleCard(CardInfo cardInfo, int handSize = 0, bool isRelative = false, Func<CardInfo,bool> condition = null, int count = 1, Action pickStartCallback = null, Action pickEndCallback = null) {
            RegisterShuffleCard(cardInfo,new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition,count = count, pickStartCallback =  pickStartCallback,pickEndCallback = pickEndCallback});
        }
        public static void RegisterShuffleCard(CardInfo cardInfo, ShuffleData data){
            ShuffleCards.TryAdd(cardInfo, data);
        }
        public static void QueueShuffleForPicker(Player picker, int handSize = 0, bool isRelative = false, Func<CardInfo, bool> condition = null){
            QueueShuffleForPicker(picker, new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition});
            
        }

        public static void QueueLimitedDraw(Player picker, List<CardInfo> deck, bool isShuffle = true, bool ignoreRestrictions = false, int handSize = 0, bool isRelative = false,  Action pickStartCallback = null, Action pickEndCallback = null){
            if(!LimitedDrawQueue.ContainsKey(picker)) LimitedDrawQueue[picker] = new List<LimitedDraw>();
            LimitedDrawQueue[picker].Add(new LimitedDraw(){deck=deck, isShuffle = isShuffle, ignoreRestrictions = ignoreRestrictions,data = new ShuffleData(){ HandSize = handSize, Relative = isRelative, pickStartCallback = pickStartCallback, pickEndCallback = pickEndCallback }});
        }

        internal struct LimitedDraw : IEquatable<LimitedDraw>{

            internal List<CardInfo> deck;
            internal bool isShuffle;
            internal bool ignoreRestrictions;
            internal ShuffleData data;
            
            public bool Equals(LimitedDraw other){
                return deck.Equals(other.deck) && isShuffle == other.isShuffle && ignoreRestrictions == other.ignoreRestrictions;
            }

            public override bool Equals(object? obj){
                return obj is LimitedDraw other && Equals(other);
            }

            public override int GetHashCode(){
                return HashCode.Combine(deck, isShuffle, ignoreRestrictions);
            }

            public static bool operator ==(LimitedDraw left, LimitedDraw right){
                return left.Equals(right);
            }

            public static bool operator !=(LimitedDraw left, LimitedDraw right){
                return !left.Equals(right);
            }
        }
        public static void QueueShuffleForPicker(Player picker, ShuffleData data){ 
            if(!ShuffleQueue.ContainsKey(picker)) {
                ShuffleQueue.Add(picker,new List<ShuffleData>());
            }
            ShuffleQueue[picker].Add(data);
        }

        //These Needs to be manually cleaned up by card removal, is automatically reset on GameStart.
        public static void GiveAdditionalPicks(this Player picker,int picks = 1){
            if(!AdditionalPicks.TryAdd(picker, picks)){
                AdditionalPicks[picker] += picks;  
            }
        }
        
        public static ShuffleData GiveConditionalPick(this Player picker, int handSize = 0, bool isRelative = false, Func<CardInfo, bool> condition = null, Action pickStartCallback = null, Action pickEndCallback = null){
           return GiveConditionalPick(picker, new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition, pickStartCallback =  pickStartCallback,pickEndCallback = pickEndCallback});
        }
        public static ShuffleData GiveConditionalPick(this Player picker, ShuffleData data){
            if(!ConditionalPicks.ContainsKey(picker)) {
                ConditionalPicks.Add(picker,new List<ShuffleData>());
            }
            ConditionalPicks[picker].Add(data);
            return data;
        }
        
        public static bool RemoveConditionalPick(this Player picker, int handSize = 0, bool isRelative = false, Func<CardInfo, bool> condition = null, Action pickStartCallback = null, Action pickEndCallback = null){
            return RemoveConditionalPick(picker, new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition, pickStartCallback =  pickStartCallback,pickEndCallback = pickEndCallback});
        }
        
        public static bool RemoveConditionalPick(this Player picker, ShuffleData data){
            return ConditionalPicks.ContainsKey(picker) && ConditionalPicks[picker].Remove(data);
        }

        public static int GetPlayerPickCount(this Player picker) =>
            ConditionalPicks.ContainsKey(picker) ? ConditionalPicks[picker].Count + CollectionExtensions.GetValueOrDefault(AdditionalPicks, picker) : CollectionExtensions.GetValueOrDefault(AdditionalPicks, picker);
        
        
        public struct ShuffleData : IEquatable<ShuffleData>{
            public int HandSize;
            public bool Relative;
            public Func<CardInfo, bool> Condition;
            public Action pickStartCallback;
            public Action pickEndCallback;
            internal int count;

            public bool Equals(ShuffleData other){
                return HandSize == other.HandSize && Relative == other.Relative && Condition.Equals(other.Condition);
            }

            public override bool Equals(object? obj){
                return obj is ShuffleData other && Equals(other);
            }

            public override int GetHashCode(){
                return HashCode.Combine(HandSize, Relative, Condition);
            }
        }



        internal struct HandModification{
            internal Func<CardInfo[], CardInfo[]> Func;
            internal int Priority;
        }
        
        public enum ValidationResult{
            Valid,
            Invalid,
            CurrentlyInvalid,
        }

        public static bool Synchronous = false;
        internal static List<Func<CardInfo[],CardInfo,ValidationResult> > DrawValidationFunctions = new List<Func<CardInfo[],CardInfo,ValidationResult>>();
        internal static List<HandModification> HandModifications = new List<HandModification>();
        internal static List<Action<GameObject>> CardSpawnCallbacks = new List<Action<GameObject>>();
        internal static List<Action<CardInfo[]>> FinalizationActions = new List<Action<CardInfo[]>>();
        internal static Dictionary<CardInfo,object[]> CustomPhotonData = new Dictionary<CardInfo,object[]>();
        internal static Dictionary<CardInfo,string> AlternetSpawnName = new Dictionary<CardInfo,string>();
        
        public static void RegisterDrawValidationFunction(Func<CardInfo[],CardInfo,ValidationResult> func)=> DrawValidationFunctions.Add(func);

        public static void RegisterHandModificationFunction(Func<CardInfo[], CardInfo[]> func, int priority = Priority.Normal){
            HandModifications.Add(new HandModification{Func = func, Priority = priority});
            HandModifications.Sort((a, b) => b.Priority - a.Priority);
        }
        public static void RegisterCardSpawnCallbacks(Action<GameObject> func) => CardSpawnCallbacks.Add(func);
        public static void RegisterHandFinalizationAction(Action<CardInfo[]> func) => FinalizationActions.Add(func);
        public static void RegisterCustomPhotonData(CardInfo card, params object[] data) => CustomPhotonData[card] = data;
        public static void RegisterAlternetSpawnName(CardInfo card, string name) => AlternetSpawnName[card] = name;
        

        internal static void SetPickerDraws(int pickerIDToSet, int drawCountToSet){
            
            drawCountToSet = Mathf.Clamp(drawCountToSet, 1, 30);
            NetworkingManager.RPC(typeof (DrawNCards.DrawNCards), "RPCA_SetPickerDraws", (object) pickerIDToSet, (object) drawCountToSet);
        }
    }
}