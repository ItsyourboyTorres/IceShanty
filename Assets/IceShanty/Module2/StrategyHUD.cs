using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IceShanty
{
    public sealed class StrategyHUD : MonoBehaviour
    {
        public StrategyManager game;
        public GameObject shopPanel, inventoryPanel;
        public Text market, boss, inventory, notice, loadout, sonar;
        public TextMesh marketBoard;
        public Button[] buyButtons;
        public Text[] buyLabels;
        public Button clearIce, chum, removeBait;
        void Update()
        {
            if(game.run.State==null) return;
            bool shop=game.screens.Current==ScreenManager.Screen.Shop;
            bool fishing=game.screens.Current==ScreenManager.Screen.Fishing;
            shopPanel.SetActive(shop); inventoryPanel.SetActive(game.screens.Current==ScreenManager.Screen.Sell);
            market.text=game.DemandText;
            if(marketBoard) marketBoard.text=game.DemandText.Replace(": ","\n");
            boss.text=game.Won ? "RUN WON / CARETAKER CHAIR UNLOCKED" : game.Boss ? "BOSS QUOTA: BROKEN HEATER / ICE FREEZES 2x FASTER" : "CHAIR UNLOCKS: Reach round 3 / Win round 6";
            notice.text=shop ? game.Notice : $"Ice {game.Ice:P0}  |  Chum bites {game.ChumBites}  |  Reel: {(game.Reel.HasValue ? game.Reel.ToString():"Standard")}";
            sonar.text=fishing && game.Sonar ? game.fishing.Incoming : "";
            loadout.text=$"CHAIR: {game.profile.selected}\nStorage {game.Capacity} / quota x{game.QuotaFactor:0.0}";
            for(int i=0;i<buyButtons.Length;i++)
            {
                var item=game.catalog.items[i]; buyButtons[i].interactable=game.CanBuy(i);
                string category=i<2 ? "RODS & REELS" : i<4 ? "BAIT & CHUM":"GADGETS";
                buyLabels[i].text=$"{category}: {item.title}  /  {(game.Owned(item.upgrade)?"EQUIPPED":"$"+item.price)}\n{item.description}";
            }
            clearIce.gameObject.SetActive(fishing); clearIce.interactable=game.Ice>0 && (!game.fishing.Busy || game.fishing.Phase==FishingController.Stage.Waiting);
            chum.gameObject.SetActive(fishing); chum.interactable=game.ChumBuckets>0 && !game.fishing.Busy && game.run.State.Phase==RunPhase.Fishing;
            chum.GetComponentInChildren<Text>().text=$"DUMP CHUM ({game.ChumBuckets})";
            removeBait.gameObject.SetActive(shop && game.Glow);
            if(inventoryPanel.activeSelf)
            {
                var text=new StringBuilder($"CATCH STORAGE {game.fishing.Count}/{game.Capacity}\nPrices use this round's demand.\n\n");
                foreach(var fish in game.fishing.Inventory) text.AppendLine($"{fish.name} / {fish.weight*2.20462262f:0.0} lb / {fish.tag} / ${StrategyManager.Quote(fish,game.Market)}");
                text.Append($"\nTOTAL ${game.fishing.Value}"+(game.Bobble ? " + possible lucky duplicates":"")); inventory.text=text.ToString();
            }
        }
    }
}
