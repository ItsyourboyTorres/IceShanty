using System;
using UnityEngine;

namespace IceShanty
{
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] RunRules rules;
        public StrategyManager strategy;
        public DayTransition dayTransition;
        public bool CanPayQuota => State!=null && (State.Phase==RunPhase.Fishing || State.Phase==RunPhase.QuotaCheck) && State.Cash>=State.Quota && !(dayTransition && dayTransition.Busy) && !(strategy && (strategy.fishing.Busy || (strategy.hole && strategy.hole.Busy) || strategy.screens.IsTransitioning));
        public int StartingCash => Mathf.Max(0,rules.startingCash);
        public int AttemptsPerPeriod => Mathf.Max(1,rules.attemptsPerPeriod);
        public RunState State { get; private set; }
        public event Action Changed;

        void Start() { if (State == null) StartRun(); }

        public void Configure(RunRules value) => rules = value;
        public void RestoreRun(int period,int cash,int earnings,int attempts,RunPhase phase)
        {
            State=new RunState { Period=period, Cash=cash, Earnings=earnings, Attempts=attempts, Phase=phase,
                Quota=(int)Math.Min(int.MaxValue,Math.Ceiling((double)rules.QuotaFor(period)*(strategy?strategy.QuotaFactor:1))) };
            Changed?.Invoke();
        }

        public void StartRun()
        {
            if (!rules) { Debug.LogError("Assign Run Rules before starting a run.", this); return; }
            State = new RunState { Period = 1, Cash = Mathf.Max(0, rules.startingCash) };
            BeginPeriod();
        }

        public bool TryUseAttempt()
        {
            if (State == null || State.Phase != RunPhase.Fishing || State.Attempts <= 0) return false;
            if (--State.Attempts == 0) State.Phase = RunPhase.QuotaCheck;
            Changed?.Invoke();
            return true;
        }

        // Called by the sell system after removing sold fish from inventory.
        public bool RecordSale(int amount)
        {
            if ((dayTransition && dayTransition.Busy) || State == null || State.Phase == RunPhase.GameOver || State.Phase == RunPhase.Victory || amount <= 0 ||
                amount > int.MaxValue - State.Cash || amount > int.MaxValue - State.Earnings) return false;
            State.Cash += amount;
            State.Earnings += amount;
            Changed?.Invoke();
            return true;
        }

        public bool TrySpend(int amount)
        {
            if ((dayTransition && dayTransition.Busy) || State == null || State.Phase == RunPhase.GameOver || State.Phase == RunPhase.Victory || amount <= 0 || State.Cash < amount) return false;
            State.Cash -= amount;
            Changed?.Invoke();
            return true;
        }

        // Payment uses cash on hand. The deduction and new day commit together at black.
        public void SubmitQuota()
        {
            if(!CanPayQuota) return;
            if(dayTransition) dayTransition.Begin(State); else CompleteQuotaPayment(State);
        }
        public void CompleteQuotaPayment(RunState expected)
        {
            if(State!=expected || State.Cash<State.Quota || (State.Phase!=RunPhase.Fishing && State.Phase!=RunPhase.QuotaCheck)) return;
            State.Cash-=State.Quota;
            if(strategy && State.Period>=strategy.catalog.winningRound)
            {
                State.Phase=RunPhase.Victory; strategy.Win(); Changed?.Invoke(); return;
            }
            State.Period++;
            BeginPeriod();
        }
        public void CheckFailure()
        {
            if(State==null || State.Phase!=RunPhase.QuotaCheck || State.Cash>=State.Quota || !strategy || strategy.fishing.Busy || strategy.fishing.Count>0 || (dayTransition && dayTransition.Busy)) return;
            State.Phase=RunPhase.GameOver; Changed?.Invoke();
        }

        void BeginPeriod()
        {
            State.Quota = (int)Math.Min(int.MaxValue, Math.Ceiling((double)rules.QuotaFor(State.Period)*(strategy ? strategy.QuotaFactor:1)));
            State.Earnings = 0;
            State.Attempts = Mathf.Max(1, rules.attemptsPerPeriod);
            State.Phase = RunPhase.Fishing;
            Changed?.Invoke();
        }
    }
}
