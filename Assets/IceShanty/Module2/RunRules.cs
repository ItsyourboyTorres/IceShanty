using UnityEngine;

namespace IceShanty
{
    [CreateAssetMenu(menuName = "Ice Shanty/Run Rules")]
    public sealed class RunRules : ScriptableObject
    {
        [Min(0)] public int startingCash;
        [Min(1)] public int initialQuota = 100;
        [Min(1)] public float quotaMultiplier = 1.5f;
        [Min(1)] public int attemptsPerPeriod = 5;

        public int QuotaFor(int period) => (int)System.Math.Min(int.MaxValue,
            System.Math.Ceiling(System.Math.Max(1, initialQuota) *
                System.Math.Pow(System.Math.Max(1f, quotaMultiplier), System.Math.Max(0, period - 1))));
    }
}
