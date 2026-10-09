using System;
using UnityEngine;

namespace IceShanty
{
    public sealed class StrategyManager : MonoBehaviour
    {
        [Serializable] public class Profile { public int version=1, bestRound, wins; public Loadout selected; }
        public const string ProfileKey="IceShanty.Profile.v1";
        public string profileKey=ProfileKey;
        public StrategyCatalog catalog;
        public RunManager run;
        public FishingController fishing;
        public ScreenManager screens;
        public GameObject sonarProp, heaterProp, bobbleProp, winchProp, ghostProp, glowProp, chumProp, iceCover;
        public TextMesh sonarLabel, heaterLabel, chairLabel;
        public Transform chair;
        public Demand Market { get; private set; }
        public Upgrade? Reel { get; private set; }
        public bool Sonar { get; private set; }
        public bool Heater { get; private set; }
        public bool Bobble { get; private set; }
        public bool Glow { get; private set; }
        public int ChumBites { get; private set; }
        public int ChumBuckets { get; private set; }
        public float Ice { get; private set; }
        public bool Boss => run.State != null && run.State.Period % 3 == 0;
        public bool Warm => Heater && !Boss;
        public int Capacity => profile.selected == Loadout.Commercial ? 12 : 6;
        public float QuotaFactor => profile.selected == Loadout.Commercial ? 1.5f : 1;
        public bool Won => run.State != null && run.State.Phase == RunPhase.Victory;
        public Profile profile = new Profile();
        public string Notice { get; private set; } = "Choose equipment to match the market.";
        int lastRound;
        RunState activeRun;

        void Awake()
        {
            try { profile = JsonUtility.FromJson<Profile>(PlayerPrefs.GetString(profileKey,"{}")) ?? new Profile(); }
            catch { profile = new Profile(); }
            if (!Unlocked(profile.selected)) profile.selected=Loadout.Angler;
            run.Changed += Refresh;
        }
        void OnDestroy() { if(run) run.Changed -= Refresh; }
        void Refresh()
        {
            if (run.State == null) return;
            if (activeRun != run.State)
            {
                activeRun=run.State; lastRound=0; Reel=null; Sonar=Bobble=Glow=false;
                Heater=profile.selected==Loadout.Caretaker; ChumBites=ChumBuckets=0; Ice=0;
            }
            if (lastRound != run.State.Period)
            {
                int next=UnityEngine.Random.Range(0,3);
                if(lastRound>0 && next==(int)Market) next=(next+1)%3;
                Market=(Demand)next; lastRound=run.State.Period; Ice=0;
                Notice=Boss ? "TAX MAN: heater broken. Ice grows twice as fast!" : "New market demand posted.";
                if (profile.bestRound < lastRound) { profile.bestRound=lastRound; SaveProfile(); }
            }
            Props();
        }
        void Update()
        {
            if(run.State==null || run.State.Phase==RunPhase.GameOver || Won) return;
            if(sonarLabel) sonarLabel.text=fishing.Incoming.Replace(" / ","\n");
            if(heaterLabel) { heaterLabel.text=Boss ? "BROKEN":"HEATING"; heaterLabel.color=Boss ? Color.red:Color.yellow; }
            if(Warm) Ice=0;
            else if(screens.Current==ScreenManager.Screen.Fishing && (!fishing.Busy || fishing.Phase==FishingController.Stage.Waiting))
                Ice=Mathf.Clamp01(Ice+Time.deltaTime*catalog.freezePerSecond*(Boss?2:1));
            if(UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame) ClearIce();
            if(iceCover) { iceCover.SetActive(Ice>.05f); iceCover.transform.localScale=new Vector3(1.05f*Ice,.025f,1.05f*Ice); }
        }
        public void ClearIce()
        {
            if(screens.Current!=ScreenManager.Screen.Fishing || (fishing.Busy && fishing.Phase!=FishingController.Stage.Waiting)) return;
            Ice=Mathf.Max(0,Ice-.32f);
        }
        public string DemandText => Market==Demand.DeepWater ? "MARKET: 2x for Deep-Water fish" : Market==Demand.Heavy ? "MARKET: +$50 for fish OVER 10 lb" : "MARKET: 2x for Cosmic fish";
        public static int Quote(FishingController.Catch fish,Demand demand)
        {
            if(fish.value<=0) return 0;
            if(fish.tag==FishTag.Relic) return fish.value;
            if((demand==Demand.DeepWater && fish.tag==FishTag.DeepWater) || (demand==Demand.Cosmic && fish.tag==FishTag.Cosmic)) return fish.value*2;
            return fish.value+(demand==Demand.Heavy && fish.weight*2.20462262f>10 ? 50:0);
        }
        public bool Owned(Upgrade item) => item==Upgrade.HeavyWinch || item==Upgrade.GhostReel ? Reel==item : item==Upgrade.Sonar ? Sonar : item==Upgrade.Heater ? Heater : item==Upgrade.Bobblehead ? Bobble : false;
        public bool CanBuy(int index)
        {
            if(index<0 || index>=catalog.items.Length || run.State==null || fishing.Busy || screens.IsTransitioning || screens.Current!=ScreenManager.Screen.Shop || run.State.Phase==RunPhase.GameOver || Won) return false;
            var item=catalog.items[index]; return !Owned(item.upgrade) && run.State.Cash>=item.price;
        }
        public void Buy(int index)
        {
            if(!CanBuy(index)) return;
            var item=catalog.items[index]; if(!run.TrySpend(item.price)) return;
            switch(item.upgrade)
            {
                case Upgrade.HeavyWinch: case Upgrade.GhostReel: Reel=item.upgrade; break;
                case Upgrade.GlowBait: Glow=true; break;
                case Upgrade.Chum: ChumBuckets++; break;
                case Upgrade.Sonar: Sonar=true; break;
                case Upgrade.Heater: Heater=true; break;
                case Upgrade.Bobblehead: Bobble=true; break;
            }
            Notice="Purchased "+item.title+". "+item.description; Props();
        }
        public float TakeBiteDelay(float normal) { if(ChumBites<=0) return normal; ChumBites--; Props(); return .05f; }
        public void DumpChum()
        {
            if(ChumBuckets<=0 || fishing.Busy || screens.Current!=ScreenManager.Screen.Fishing || run.State.Phase!=RunPhase.Fishing) return;
            ChumBuckets--; ChumBites+=3; Notice="Chum dumped: next 3 bites are instant."; Props();
        }
        public void ToggleGlow() { if(Glow) { Glow=false; Notice="Glow bait removed. Snap risk back to normal."; Props(); } }
        public void Win() { profile.wins++; SaveProfile(); Notice="Run won! Caretaker chair unlocked."; }
        public bool Unlocked(Loadout setup) => setup==Loadout.Angler || (setup==Loadout.Commercial ? profile.bestRound>=3 : setup==Loadout.Caretaker && profile.wins>0);
        public void ChooseLoadout()
        {
            if(fishing.Busy || (run.State != null && run.State.Phase!=RunPhase.GameOver && !Won && (run.State.Period!=1 || run.State.Attempts!=run.AttemptsPerPeriod || run.State.Cash!=run.StartingCash))) return;
            for(int i=1;i<=3;i++) { var next=(Loadout)(((int)profile.selected+i)%3); if(!Unlocked(next)) continue; profile.selected=next; SaveProfile(); run.StartRun(); break; }
        }
        public void SaveProfile() { PlayerPrefs.SetString(profileKey,JsonUtility.ToJson(profile)); PlayerPrefs.Save(); }
        public void RestoreEquipment(RunSave.Data data)
        {
            Market=data.market; Reel=data.reel<0 ? (Upgrade?)null:(Upgrade)data.reel;
            Sonar=data.sonar; Heater=data.heater; Bobble=data.bobble; Glow=data.glow;
            ChumBites=data.chumBites; ChumBuckets=data.chumBuckets; Ice=data.ice;
            Notice="Saved run restored."; Props();
        }
        void Props()
        {
            if(chair) chair.localScale=profile.selected==Loadout.Commercial ? new Vector3(.9f,.7f,.8f):new Vector3(.6f,.7f,.6f);
            if(chairLabel) chairLabel.text=profile.selected.ToString();
            if(sonarProp) sonarProp.SetActive(Sonar); if(heaterProp) heaterProp.SetActive(Heater); if(bobbleProp) bobbleProp.SetActive(Bobble);
            if(winchProp) winchProp.SetActive(Reel==Upgrade.HeavyWinch); if(ghostProp) ghostProp.SetActive(Reel==Upgrade.GhostReel);
            if(glowProp) glowProp.SetActive(Glow); if(chumProp) chumProp.SetActive(ChumBuckets>0 || ChumBites>0);
        }
    }
}
