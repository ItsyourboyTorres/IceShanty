using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace IceShanty
{
    public sealed class DayTransition : MonoBehaviour
    {
        public RunManager run;
        public ScreenManager screens;
        public CanvasGroup overlay;
        public Text title;
        public bool Busy { get; private set; }
        public void Begin(RunState expected) { if(!Busy) StartCoroutine(NextDay(expected)); }
        IEnumerator NextDay(RunState expected)
        {
            Busy=true; screens.InputLocked=true; overlay.gameObject.SetActive(true); overlay.blocksRaycasts=true; title.text="";
            yield return Fade(0,1);
            run.CompleteQuotaPayment(expected); screens.ReturnToFishing();
            title.text=run.State.Phase==RunPhase.Victory?"QUOTA PAID\nRUN COMPLETE":$"QUOTA PAID\nDAY {run.State.Period}\nThe lake has frozen over.";
            var save=FindFirstObjectByType<RunSave>(); if(save) save.SaveNow();
            yield return new WaitForSecondsRealtime(1);
            yield return Fade(1,0);
            overlay.gameObject.SetActive(false); screens.InputLocked=false; Busy=false;
        }
        IEnumerator Fade(float start,float end)
        {
            for(float t=0;t<.4f;t+=Time.unscaledDeltaTime) { overlay.alpha=Mathf.Lerp(start,end,t/.4f); yield return null; }
            overlay.alpha=end;
        }
    }
}
