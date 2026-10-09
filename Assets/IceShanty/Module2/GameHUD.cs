using UnityEngine;
using UnityEngine.UI;

namespace IceShanty
{
    public sealed class GameHUD : MonoBehaviour
    {
        public RunManager run;
        public FishingController fishing;
        public ScreenManager screens;
        public Text status, heading, hint;
        public Button left, right, action, submit;
        public Text actionLabel;

        void Update()
        {
            if (run.State == null) return;
            var s = run.State;
            var view = screens.Current;
            bool digging=fishing.strategy.hole && fishing.strategy.hole.Busy;
            bool changingDay=run.dayTransition && run.dayTransition.Busy;
            status.text = $"DAY {s.Period}     CASH ${s.Cash}     QUOTA ${s.Quota}     CASTS {s.Attempts}     FISH {fishing.Count}";
            heading.gameObject.SetActive(view != ScreenManager.Screen.Fishing);
            heading.text = view == ScreenManager.Screen.Shop ? "TACKLE SHOP" : "SELL YOUR CATCH";
            action.gameObject.SetActive(view != ScreenManager.Screen.Shop && !digging);
            submit.gameObject.SetActive(s.Phase!=RunPhase.GameOver && s.Phase!=RunPhase.Victory);
            submit.GetComponentInChildren<Text>().text=$"PAY QUOTA / ${s.Quota}";
            hint.gameObject.SetActive(view != ScreenManager.Screen.Shop && !digging);
            hint.text = run.State.Phase == RunPhase.Victory ? "Run won! Restart or select an unlocked chair." : fishing.Busy ? fishing.Message : s.Phase == RunPhase.GameOver ? "Quota missed. Restart to try again." : s.Phase == RunPhase.QuotaCheck ? "No casts left. Sell, then submit your quota." : fishing.Message;
            if(s.Phase==RunPhase.Fishing && !fishing.strategy.HoleOpen) hint.text="Solid ice. Create today's hole before casting.";
            else if(s.Phase==RunPhase.Fishing && !fishing.Busy && fishing.strategy.FullyFrozen) hint.text="Fully frozen. Break the ice to fish again.";
            left.interactable = !fishing.Busy && !digging && !changingDay && view != ScreenManager.Screen.Shop;
            right.interactable = !fishing.Busy && !digging && !changingDay && view != ScreenManager.Screen.Sell;
            actionLabel.text = view == ScreenManager.Screen.Shop ? "CHOOSE AN UPGRADE" : view == ScreenManager.Screen.Sell ? $"SELL ALL / ${fishing.Value}" : fishing.Phase == FishingController.Stage.SkillCheck ? "HIT! / SPACE" : fishing.Busy ? "FISHING..." : "CAST LINE";
            if(view==ScreenManager.Screen.Fishing && !fishing.strategy.HoleOpen) actionLabel.text="CREATE HOLE / "+HoleController.ToolName(fishing.strategy.Tool);
            else if(view==ScreenManager.Screen.Fishing && !fishing.Busy && fishing.strategy.FullyFrozen) actionLabel.text="BREAK ICE / "+HoleController.ToolName(fishing.strategy.Tool);
            action.interactable = (!fishing.Busy || fishing.Phase == FishingController.Stage.SkillCheck) && !screens.IsTransitioning && (view != ScreenManager.Screen.Sell || fishing.Count > 0) && s.Phase != RunPhase.GameOver && s.Phase != RunPhase.Victory && (view != ScreenManager.Screen.Fishing || (s.Attempts > 0 || fishing.Phase == FishingController.Stage.SkillCheck)) && view != ScreenManager.Screen.Shop;
            action.interactable &= !digging && !changingDay;
            submit.interactable = run.CanPayQuota;
        }

        public void Act()
        {
            switch (screens.Current)
            {
                case ScreenManager.Screen.Shop: break;
                case ScreenManager.Screen.Sell: fishing.SellAll(); break;
                default: if (fishing.Phase == FishingController.Stage.SkillCheck) fishing.Strike(); else if(!fishing.strategy.HoleOpen || fishing.strategy.FullyFrozen) fishing.strategy.hole.Begin(); else fishing.Cast(); break;
            }
        }
    }
}




