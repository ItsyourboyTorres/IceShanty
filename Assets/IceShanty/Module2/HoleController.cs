using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Cinemachine;
namespace IceShanty
{
    public sealed class HoleController : MonoBehaviour
    {
        public StrategyManager game;
        public HoleSettings settings;
        public CinemachineCamera workCamera;
        public Renderer[] targets;
        public GameObject[] toolModels;
        public Transform water;
        public Text instruction;
        public Material idleMaterial, highlightMaterial;
        public bool Busy { get; private set; }
        public bool AcceptingInput { get; private set; }
        public int CurrentTarget { get; private set; }
        public int Mistakes { get; private set; }
        public string Result { get; private set; }="Make a hole to start fishing.";
        int strikes, required, day;
        RunState activeRun;
        float timer,cooldown;
        Quaternion rest;
        Transform tool;
        void Awake() { game.run.Changed+=OnRunChanged; }
        void OnDestroy() { if(game && game.run) game.run.Changed-=OnRunChanged; }
        void OnRunChanged()
        {
            if(activeRun==game.run.State && day==game.run.State.Period) return;
            activeRun=game.run.State; day=activeRun.Period;
            StopAllCoroutines(); Busy=AcceptingInput=false; workCamera.Priority=0;
            if(tool) tool.localRotation=rest;
            HideWork(); game.fishing.rod.gameObject.SetActive(true);
            game.screens.InputLocked=game.run.dayTransition && game.run.dayTransition.Busy;
            Result="New ice. Make today's hole.";
        }
        void Update()
        {
            float size=game.HoleOpen ? .65f+.4f*game.HoleQuality:1.05f;
            water.localScale=new Vector3(size,.006f,size);
            if(!AcceptingInput) return;
            timer-=Time.deltaTime; cooldown-=Time.deltaTime;
            instruction.text=$"{ToolName(game.Tool)} / {strikes}/{required}\nClick the GOLD section  |  {timer:0.0}s  |  Mistakes {Mistakes}";
            if(timer<=0) { RegisterHit(-1); return; }
            if(Mouse.current==null || !Mouse.current.leftButton.wasPressedThisFrame || (EventSystem.current && EventSystem.current.IsPointerOverGameObject())) return;
            var ray=Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray,out var hit,10))
            {
                var target=hit.collider.GetComponent<HoleTarget>(); RegisterHit(target?target.index:-1);
            }
            else RegisterHit(-1);
        }
        public static string ToolName(HoleTool tool) => tool==HoleTool.IceChisel?"Ice Chisel":tool==HoleTool.HandAuger?"Hand Auger":tool==HoleTool.GasAuger?"Gas Auger":tool==HoleTool.ElectricAuger?"Electric Auger":"Axe";
        public void Begin()
        {
            if(Busy || (game.HoleOpen && !game.FullyFrozen) || game.fishing.Busy || game.screens.IsTransitioning || game.screens.Current!=ScreenManager.Screen.Fishing || game.run.State==null || game.run.State.Phase!=RunPhase.Fishing || (game.run.dayTransition && game.run.dayTransition.Busy)) return;
            StartCoroutine(CreateHole());
        }
        IEnumerator CreateHole()
        {
            Busy=true; game.screens.InputLocked=true; workCamera.Priority=30;
            game.BeginHoleWork();
            game.fishing.rod.gameObject.SetActive(false); instruction.gameObject.SetActive(true); instruction.text="Moving over the ice...";
            yield return new WaitForSeconds(.95f);
            int tier=(int)game.Tool; required=settings.strikes[tier]; strikes=Mistakes=0;
            tool=toolModels[tier].transform; rest=tool.localRotation; tool.gameObject.SetActive(true);
            foreach(var target in targets) target.gameObject.SetActive(true);
            Physics.SyncTransforms();
            AcceptingInput=true; CurrentTarget=-1; NextTarget();
            while(strikes<required) yield return null;
            AcceptingInput=false; game.OpenHole(Mathf.Max(.35f,1-Mistakes*settings.mistakePenalty));
            Result=$"{(Mistakes==0?"Clean":game.HoleQuality>.6f?"Rough":"Damaged")} hole / {game.HoleQuality:P0} quality. "+(Mistakes==0?"Ready to fish.":"Smaller opening; faster freezing.");
            instruction.text=Result;
            foreach(var target in targets) target.gameObject.SetActive(false);
            yield return new WaitForSeconds(.8f);
            tool.localRotation=rest; tool.gameObject.SetActive(false); workCamera.Priority=0;
            yield return new WaitForSeconds(.9f);
            Busy=false; game.screens.InputLocked=false; game.fishing.rod.gameObject.SetActive(true); instruction.gameObject.SetActive(false);
        }
        public void RegisterHit(int target)
        {
            if(!AcceptingInput || cooldown>0) return;
            if(target!=CurrentTarget) Mistakes++;
            strikes++; cooldown=.2f;
            StartCoroutine(Swing());
            if(strikes<required) NextTarget();
        }
        void NextTarget()
        {
            int next=Random.Range(0,targets.Length); if(next==CurrentTarget) next=(next+1)%targets.Length; CurrentTarget=next;
            for(int i=0;i<targets.Length;i++) targets[i].sharedMaterial=i==next?highlightMaterial:idleMaterial;
            timer=settings.secondsPerStrike[(int)game.Tool];
        }
        IEnumerator Swing()
        {
            for(float t=0;t<.18f;t+=Time.deltaTime) { tool.localRotation=rest*Quaternion.Euler(Mathf.Sin(t/.18f*Mathf.PI)*35,0,0); yield return null; }
            tool.localRotation=rest;
        }
        void HideWork()
        {
            instruction.gameObject.SetActive(false);
            foreach(var target in targets) target.gameObject.SetActive(false);
            foreach(var model in toolModels) model.SetActive(false);
        }
    }
}
