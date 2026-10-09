using UnityEngine;

namespace IceShanty
{
    [CreateAssetMenu(menuName = "Ice Shanty/Fishing Settings")]
    public sealed class FishingSettings : ScriptableObject
    {
        public Vector2 biteDelay = new Vector2(1.5f, 3.5f);
        [Min(.5f)] public float checkDuration = 1.6f;
        [Range(.08f,.4f)] public float successWidth = .22f;
        [Min(.2f)] public float reelDuration = .9f;
        public string[] fishNames = { "Perch", "Trout", "Pike" };
    }
}
