using UnityEngine;

namespace IceShanty
{
    public sealed class QuotaDebugPanel : MonoBehaviour
    {
        [SerializeField] RunManager run;
        public void Configure(RunManager value) => run = value;

        void OnGUI()
        {
            if (!run || run.State == null) return;
            var s = run.State;
            GUILayout.BeginArea(new Rect(20, 20, 340, 340), GUI.skin.box);
            GUILayout.Label("ICE SHANTY / QUOTA TEST");
            GUILayout.Label($"Period {s.Period} | {s.Phase}");
            GUILayout.Label($"Cash ${s.Cash} | Earnings ${s.Earnings} / ${s.Quota}");
            GUILayout.Label($"Fishing attempts left: {s.Attempts}");
            GUI.enabled = s.Phase == RunPhase.Fishing;
            if (GUILayout.Button("Use fishing attempt")) run.TryUseAttempt();
            GUI.enabled = s.Phase != RunPhase.GameOver;
            if (GUILayout.Button("Simulate fish sale +$25")) run.RecordSale(25);
            if (GUILayout.Button("Test purchase -$25")) run.TrySpend(25);
            GUI.enabled = s.Phase == RunPhase.QuotaCheck;
            if (GUILayout.Button("Submit quota")) run.SubmitQuota();
            GUI.enabled = true;
            if (GUILayout.Button("Restart run")) run.StartRun();
            GUILayout.Label("Use all attempts, sell, then submit.\nPurchases preserve quota earnings.");
            GUILayout.EndArea();
        }
    }
}
