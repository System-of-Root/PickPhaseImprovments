using System;
using System.Collections.Generic;

namespace PickPhaseImprovements {
    public static class PickManager {
        internal static Dictionary<CardInfo, ShuffleData> ShuffleCards = new Dictionary<CardInfo, ShuffleData>();
        internal static Dictionary<Player,List<ShuffleData>> ShuffleQueue = new Dictionary<Player,List<ShuffleData>>();
        internal static Dictionary<Player,List<ShuffleData>> ConditionalPicks = new Dictionary<Player,List<ShuffleData>>();
        internal static Dictionary<Player,int> AdditionalPicks = new Dictionary<Player,int>();
        internal static Func<CardInfo, bool> ActiveCondition = _ => true;
        internal static Action ActiveCallback = null;
        internal static int PickDepth = 0;
        internal static int StoredHandSize = -1;
        public static void RegisterShuffleCard(CardInfo cardInfo, int handSize = 0, bool isRelative = false, Func<CardInfo,bool> condition = null, int count = 1, Action pickStartCallback = null, Action pickEndCallback = null) {
            RegisterShuffleCard(cardInfo,new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition,count = count, pickStartCallback =  pickStartCallback,pickEndCallback = pickEndCallback});
        }
        public static void RegisterShuffleCard(CardInfo cardInfo, ShuffleData data){
            ShuffleCards.TryAdd(cardInfo, data);
        }
        public static void QueueShuffleForPicker(Player picker, int handSize = 0, bool isRelative = false, Func<CardInfo, bool> condition = null){
            QueueShuffleForPicker(picker, new ShuffleData(){HandSize = handSize, Relative = isRelative, Condition = condition});
            
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
    }
}