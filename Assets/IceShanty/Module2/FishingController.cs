using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace IceShanty
{
    public sealed class FishingController : MonoBehaviour
    {
        public enum Stage { Ready, Casting, Waiting, SkillCheck, Reeling }
        [System.Serializable] public struct Catch { public string name; public float weight; public int value; public FishTag tag; }
        public StrategyManager strategy;
        public IReadOnlyList<Catch> Inventory => inventory;
        public string Incoming { get; private set; } = "No signal";
        public RunManager run;
        public ScreenManager screens;
        public FishingSettings settings;
        public Transform rod, tip, bobber, fishVisual;
        public LineRenderer line;
        public GameObject checkPanel;
        public RectTransform zone, needle;
        public Text checkLabel;
        public Stage Phase { get; private set; }
        public bool Busy => Phase != Stage.Ready;
        public string Message { get; private set; } = "Cast into the hole to begin.";
        public int Count => inventory.Count;
        public int Value { get { int total = 0; foreach (var fish in inventory) total += strategy ? StrategyManager.Quote(fish,strategy.Market) : fish.value; return total; } }
        readonly List<Catch> inventory = new List<Catch>();
        public void RestoreInventory(Catch[] catches) { inventory.Clear(); inventory.AddRange(catches); }
        RunState activeRun;
        Quaternion rest;
        Vector3 water;
        float progress, zoneStart;
        bool resolved, success;

        void Start()
        {
            rest = rod.localRotation; water = bobber.position;
            activeRun = run.State; run.Changed += OnRunChanged;
            ResetRig();
        }
        void OnDestroy() { if (run) run.Changed -= OnRunChanged; }
        void OnRunChanged()
        {
            if (activeRun == run.State) return;
            StopAllCoroutines(); activeRun = run.State; inventory.Clear();
            Phase = Stage.Ready; screens.InputLocked = false;
            Message = "Fresh run. Cast into the hole."; ResetRig();
        }
        void ResetRig()
        {
            rod.localRotation = rest; line.enabled = false;
            bobber.gameObject.SetActive(false); fishVisual.gameObject.SetActive(false); checkPanel.SetActive(false);
        }
        void Update()
        {
            if (Phase == Stage.SkillCheck && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) Strike();
            if (line.enabled) { line.SetPosition(0,tip.position); line.SetPosition(1,bobber.position); }
        }
        public void Cast()
        {
            if(strategy && ((!strategy.HoleOpen) || (strategy.hole && strategy.hole.Busy) || (run.dayTransition && run.dayTransition.Busy))) { Message="Create the ice hole first."; return; }
            if(strategy && (Count>=strategy.Capacity || strategy.FullyFrozen)) { Message=Count>=strategy.Capacity ? "Storage full. Sell your catch." : "Hole fully frozen! Press E or BREAK ICE."; return; }
            if (Busy || screens.IsTransitioning || screens.Current != ScreenManager.Screen.Fishing || !run.TryUseAttempt()) return;
            screens.InputLocked = true; StartCoroutine(Fish());
        }
        public static bool IsHit(float cursor, float start, float width) => cursor >= start && cursor <= start + width;
        public void Strike()
        {
            if (Phase != Stage.SkillCheck || resolved) return;
            success = IsHit(progress,zoneStart,settings.successWidth); resolved = true;
        }
        IEnumerator Fish()
        {
            float weight = Mathf.Round(Random.Range(.5f,9.1f)*10)/10;
            bool relic = Random.value<.18f;
            FishTag tag = relic ? FishTag.Relic : Random.value<(strategy && strategy.Glow ? .4f:.05f) ? FishTag.Cosmic : Random.value<.45f ? FishTag.DeepWater:FishTag.Shallow;
            string name = relic ? "Lake Relic" : tag==FishTag.Cosmic ? "Cosmic Char" : tag==FishTag.DeepWater ? "Abyss Pike" : "Lake Trout";
            Incoming = strategy && strategy.Sonar ? $"SONAR: {name} / {weight*2.20462262f:0.0} lb" : "SONAR OFFLINE";
            Phase = Stage.Casting; Message = "Casting...";
            bobber.gameObject.SetActive(true); line.enabled = true;
            var launch = tip.position;
            for (float t = 0; t < .55f; t += Time.deltaTime)
            {
                float u = t / .55f;
                rod.localRotation = rest * Quaternion.Euler(-30 * Mathf.Sin(u*Mathf.PI),0,0);
                bobber.position = Vector3.Lerp(launch,water,u) + Vector3.up * Mathf.Sin(u*Mathf.PI)*.6f;
                yield return null;
            }
            rod.localRotation = rest; bobber.position = water;
            Phase = Stage.Waiting; Message = "Waiting for a bite...";
            float wait=Random.Range(settings.biteDelay.x,settings.biteDelay.y);
            yield return new WaitForSeconds(strategy ? strategy.TakeBiteDelay(wait):wait);
            Phase = Stage.SkillCheck; Message = "BITE! Press SPACE or HIT inside the green zone.";
            zoneStart = Random.Range(.3f,.75f-settings.successWidth);
            zone.anchorMin = new Vector2(zoneStart,0); zone.anchorMax = new Vector2(zoneStart+settings.successWidth,1);
            zone.offsetMin = zone.offsetMax = Vector2.zero;
            progress = 0; resolved = success = false; checkPanel.SetActive(true);
            checkLabel.text = "BITE!  SPACE / HIT IN THE GREEN ZONE";
            while (progress < 1 && !resolved)
            {
                float duration=settings.checkDuration*(strategy ? strategy.CheckMultiplier*(.7f+.3f*strategy.HoleQuality):1)*(strategy && strategy.Warm ? 1.35f:1)*(strategy && strategy.Reel==Upgrade.GhostReel && !relic ? .65f:1);
                progress = Mathf.Min(1, progress + Time.deltaTime/Mathf.Max(.2f,duration));
                needle.anchorMin = needle.anchorMax = new Vector2(progress,.5f);
                needle.anchoredPosition = Vector2.zero;
                yield return null;
            }
            bool snapped=success && strategy && Random.value<strategy.SnapChance;
            if(snapped) success=false;
            Phase = Stage.Reeling; checkPanel.SetActive(false);
            Message = success ? "Hooked! Reeling in..." : "Missed! Reeling the line back in...";
            fishVisual.gameObject.SetActive(success);
            float reelTime=strategy && strategy.Reel==Upgrade.HeavyWinch && !relic && weight*2.20462262f>=strategy.catalog.heavyPounds ? .08f:settings.reelDuration;
            for (float t = 0; t < reelTime; t += Time.deltaTime)
            {
                float u = t/reelTime;
                rod.localRotation = rest * Quaternion.Euler(-25*u,0,Mathf.Sin(u*25)*2);
                bobber.position = Vector3.Lerp(water,tip.position,u);
                fishVisual.position = bobber.position - Vector3.up*.13f;
                yield return null;
            }
            if (success)
            {
                int value=relic ? 40 : Mathf.RoundToInt(weight*18)*(tag==FishTag.Cosmic ? 2:1);
                if(strategy && strategy.Reel==Upgrade.HeavyWinch && !relic && weight*2.20462262f<strategy.catalog.smallPounds) value=0;
                var fish = new Catch { name=name,weight=weight,value=value,tag=tag };
                inventory.Add(fish);
                if(relic && strategy && strategy.Reel==Upgrade.GhostReel && Count<strategy.Capacity) inventory.Add(fish);
                Message = value==0 ? "Heavy Winch destroyed the small fish. Worth $0." : $"Caught {name}: {weight*2.20462262f:0.0} lb / {tag}. Sell for the current market price.";
            }
            else Message = snapped ? "The line snapped! Catch lost." : "The fish escaped. Try another cast.";
            Incoming="No signal";
            ResetRig(); Phase = Stage.Ready; screens.InputLocked = false;
            run.CheckFailure();
        }
        public void SellAll()
        {
            if (Busy || Count == 0 || run.State==null || run.State.Phase==RunPhase.GameOver || run.State.Phase==RunPhase.Victory) return;
            int total=Value, duplicates=0;
            if(strategy && strategy.Bobble) foreach(var fish in inventory)
                if(fish.tag!=FishTag.Relic && fish.value>0 && Random.value<.1f) { total+=StrategyManager.Quote(fish,strategy.Market); duplicates++; }
            if(total>0 && !run.RecordSale(total)) return;
            Message = $"Sold {Count} catches for ${total}. Bonus duplicates: {duplicates}."; inventory.Clear();
            run.CheckFailure();
        }
    }
}
