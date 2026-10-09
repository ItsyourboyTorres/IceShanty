using UnityEngine;

namespace IceShanty
{
    public enum Upgrade { HeavyWinch, GhostReel, GlowBait, Chum, Sonar, Heater, Bobblehead }
    public enum FishTag { Shallow, DeepWater, Cosmic, Relic }
    public enum Demand { DeepWater, Heavy, Cosmic }
    public enum Loadout { Angler, Commercial, Caretaker }
    [System.Serializable] public class ShopItem
    {
        public Upgrade upgrade;
        public string title, description;
        public int price;
    }
    [CreateAssetMenu(menuName="Ice Shanty/Strategy Catalog")]
    public sealed class StrategyCatalog : ScriptableObject
    {
        public ShopItem[] items = {
            new ShopItem { upgrade=Upgrade.HeavyWinch, title="Heavy Winch", description="10+ lb fish reel instantly. Under 3 lb are destroyed.", price=80 },
            new ShopItem { upgrade=Upgrade.GhostReel, title="Ghost Reel", description="Double relic drops. Fish escape 35% faster.", price=80 },
            new ShopItem { upgrade=Upgrade.GlowBait, title="Neon Glow-Bait", description="Cosmic chance: 5% to 40%. +30% line snap chance.", price=35 },
            new ShopItem { upgrade=Upgrade.Chum, title="Chum Bucket", description="Next 3 bites are instant. Sizes remain random.", price=25 },
            new ShopItem { upgrade=Upgrade.Sonar, title="Sonar Screen", description="Reveals incoming size and tag before the bite.", price=60 },
            new ShopItem { upgrade=Upgrade.Heater, title="Propane Heater", description="Prevents ice; 35% longer checks. Broken on boss rounds.", price=75 },
            new ShopItem { upgrade=Upgrade.Bobblehead, title="Lucky Bobblehead", description="Each fish sold has a 10% chance to pay twice.", price=65 }
        };
        [Min(1)] public int winningRound=6;
        [Min(0)] public float freezePerSecond=.035f;
        public float heavyPounds=10, smallPounds=3;
    }
}
