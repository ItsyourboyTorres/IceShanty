using UnityEngine;
namespace IceShanty
{
    [CreateAssetMenu(menuName="Ice Shanty/Hole Settings")]
    public sealed class HoleSettings : ScriptableObject
    {
        public int[] strikes={8,6,5,3,2};
        public float[] secondsPerStrike={1.4f,1.7f,2,2.3f,2.6f};
        [Range(.01f,.3f)] public float mistakePenalty=.14f;
    }
}
