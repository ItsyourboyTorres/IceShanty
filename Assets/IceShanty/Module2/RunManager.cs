using System;
using UnityEngine;

namespace IceShanty
{
    public sealed class RunManager : MonoBehaviour
    {
        [SerializeField] RunRules rules;
        public StrategyManager strategy;
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
            if (State == null || State.Phase == RunPhase.GameOver || State.Phase == RunPhase.Victory || amount <= 0 ||
                amount > int.MaxValue - State.Cash || amount > int.MaxValue - State.Earnings) return false;
            State.Cash += amount;
            State.Earnings += amount;
            Changed?.Invoke();
            return true;
        }

        public bool TrySpend(int amount)
        {
            if (State == null || State.Phase == RunPhase.GameOver || State.Phase == RunPhase.Victory || amount <= 0 || State.Cash < amount) return false;
            State.Cash -= amount;
            Changed?.Invoke();
            return true;
        }

        // Earnings are the target; submitting does not deduct rent from cash.
        public void SubmitQuota()
        {
            if (State == null || State.Phase != RunPhase.QuotaCheck || (strategy && strategy.fishing.Busy)) return;
            if (State.Earnings < State.Quota)
            {
                State.Phase = RunPhase.GameOver;
                Changed?.Invoke();
                return;
            }
            if(strategy && State.Period>=strategy.catalog.winningRound)
            {
                State.Phase=RunPhase.Victory; strategy.Win(); Changed?.Invoke(); return;
            }
            State.Period++;
            BeginPeriod();
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
