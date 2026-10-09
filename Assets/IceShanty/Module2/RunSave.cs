using System;
using System.Linq;
using UnityEngine;

namespace IceShanty
{
    [DefaultExecutionOrder(100)]
    public sealed class RunSave : MonoBehaviour
    {
        [Serializable] public class Data
        {
            public int version=1, period, cash, earnings, attempts, reel=-1, chumBites, chumBuckets;
            public int line;
            public int tool;
            public bool holeOpen;
            public float holeQuality=1;
            public RunPhase phase;
            public Demand market;
            public Loadout loadout;
            public bool sonar,heater,bobble,glow;
            public float ice;
            public FishingController.Catch[] catches;
        }
        public StrategyManager game;
        public string Key => game.profileKey+".Run.v1";
        float nextSave;
        bool ready;
        void Start() { LoadNow(); ready=true; }
        void Update() { if(Time.unscaledTime<nextSave) return; nextSave=Time.unscaledTime+1; SaveNow(); }
        void OnApplicationQuit() { if(ready && game && game.run.State!=null) SaveNow(); }
        public void SaveNow()
        {
            var s=game.run.State; if(s==null) return;
            var data=new Data { period=s.Period,cash=s.Cash,earnings=s.Earnings,attempts=s.Attempts,phase=s.Phase,
                market=game.Market,loadout=game.profile.selected,reel=game.Reel.HasValue?(int)game.Reel.Value:-1,
                line=game.Line==Upgrade.ReinforcedLine?1:game.Line==Upgrade.SilkLine?2:0,
                tool=(int)game.Tool,holeOpen=game.HoleOpen,holeQuality=game.HoleQuality,
                sonar=game.Sonar,heater=game.Heater,bobble=game.Bobble,glow=game.Glow,ice=game.Ice,
                chumBites=game.ChumBites,chumBuckets=game.ChumBuckets,catches=game.fishing.Inventory.ToArray() };
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(data)); PlayerPrefs.Save();
        }
        public bool LoadNow()
        {
            if(!PlayerPrefs.HasKey(Key)) return false;
            Data data;
            try { data=JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key)); }
            catch { return false; }
            if(data==null || data.version!=1 || data.period<1 || data.period>game.catalog.winningRound || data.cash<0 || data.earnings<0 || data.attempts<0 || data.attempts>game.run.AttemptsPerPeriod || !Enum.IsDefined(typeof(RunPhase),data.phase) || !Enum.IsDefined(typeof(Demand),data.market) || !game.Unlocked(data.loadout) || data.reel < -1 || data.reel>1 || data.catches==null || data.catches.Length>(data.loadout==Loadout.Commercial?12:6) || data.chumBites<0 || data.chumBuckets<0 || float.IsNaN(data.ice)) return false;
            if(data.line<0 || data.line>2) return false;
            if(data.tool<0 || data.tool>4 || float.IsNaN(data.holeQuality) || float.IsInfinity(data.holeQuality)) return false;
            foreach(var fish in data.catches) if(fish.value<0 || !Enum.IsDefined(typeof(FishTag),fish.tag) || float.IsNaN(fish.weight) || float.IsInfinity(fish.weight) || fish.weight<0) return false;
            game.profile.selected=data.loadout;
            // Closing during a cast keeps the spent attempt but never invents a catch.
            if(data.phase==RunPhase.Fishing && data.attempts==0) data.phase=RunPhase.QuotaCheck;
            game.run.RestoreRun(data.period,data.cash,data.earnings,data.attempts,data.phase);
            data.ice=Mathf.Clamp01(data.ice); game.RestoreEquipment(data); game.fishing.RestoreInventory(data.catches);
            game.run.CheckFailure();
            return true;
        }
    }
}
