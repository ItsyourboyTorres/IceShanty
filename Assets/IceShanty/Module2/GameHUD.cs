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
            status.text = $"DAY {s.Period}     CASH ${s.Cash}     QUOTA ${s.Earnings} / ${s.Quota}     CASTS {s.Attempts}     FISH {fishing.Count}";
            heading.text = view == ScreenManager.Screen.Shop ? "TACKLE SHOP" : view == ScreenManager.Screen.Sell ? "SELL YOUR CATCH" : "ICE SHANTY";
            hint.text = run.State.Phase == RunPhase.Victory ? "Run won! Restart or select an unlocked chair." : fishing.Busy ? fishing.Message : s.Phase == RunPhase.GameOver ? "Quota missed. Restart to try again." : s.Phase == RunPhase.QuotaCheck ? "No casts left. Sell, then submit your quota." : fishing.Message;
            left.interactable = !fishing.Busy && view != ScreenManager.Screen.Shop;
            right.interactable = !fishing.Busy && view != ScreenManager.Screen.Sell;
            actionLabel.text = view == ScreenManager.Screen.Shop ? "CHOOSE AN UPGRADE" : view == ScreenManager.Screen.Sell ? $"SELL ALL / ${fishing.Value}" : fishing.Phase == FishingController.Stage.SkillCheck ? "HIT! / SPACE" : fishing.Busy ? "FISHING..." : "CAST LINE";
            action.interactable = (!fishing.Busy || fishing.Phase == FishingController.Stage.SkillCheck) && !screens.IsTransitioning && (view != ScreenManager.Screen.Sell || fishing.Count > 0) && s.Phase != RunPhase.GameOver && s.Phase != RunPhase.Victory && (view != ScreenManager.Screen.Fishing || (s.Attempts > 0 || fishing.Phase == FishingController.Stage.SkillCheck)) && view != ScreenManager.Screen.Shop;
            submit.interactable = !fishing.Busy && s.Phase == RunPhase.QuotaCheck;
        }

        public void Act()
        {
            switch (screens.Current)
            {
                case ScreenManager.Screen.Shop: break;
                case ScreenManager.Screen.Sell: fishing.SellAll(); break;
                default: if (fishing.Phase == FishingController.Stage.SkillCheck) fishing.Strike(); else fishing.Cast(); break;
            }
        }
    }
}



