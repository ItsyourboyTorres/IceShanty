namespace IceShanty
{
    public enum RunPhase { Fishing, QuotaCheck, GameOver, Victory }

    public sealed class RunState
    {
        public int Period { get; internal set; }
        public int Quota { get; internal set; }
        public int Cash { get; internal set; }
        public int Earnings { get; internal set; }
        public int Attempts { get; internal set; }
        public RunPhase Phase { get; internal set; }
    }
}
