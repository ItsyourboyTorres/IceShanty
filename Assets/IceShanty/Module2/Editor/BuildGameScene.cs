using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Unity.Cinemachine;

namespace IceShanty.Editor
{
    public static class BuildGameScene
    {
        static Material ice, wood, dark, orange, metal;
        static Font font;
        static Transform canvas;

        public static void Build()
        {
            const string path = "Assets/Scenes/Game.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path)) throw new System.Exception("Game scene already exists; refusing to overwrite.");
            System.IO.Directory.CreateDirectory("Assets/IceShanty/GameMaterials");
            AssetDatabase.Refresh();
            ice = Mat("Ice", new Color(.49f,.73f,.8f));
            wood = Mat("Wood", new Color(.28f,.15f,.08f));
            dark = Mat("DeepWater", new Color(.025f,.07f,.09f));
            orange = Mat("Amber", new Color(.95f,.42f,.08f));
            metal = Mat("Steel", new Color(.18f,.25f,.29f));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientLight = new Color(.65f,.75f,.85f);
            var light = new GameObject("Winter Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.4f;
            light.transform.rotation = Quaternion.Euler(48,-35,0);
            Box("Frozen Lake", new Vector3(0,-.25f,0), new Vector3(40,.5f,25), ice);
            Box("Shanty Back Wall", new Vector3(0,2,3), new Vector3(19,4,.2f), wood);
            for (int x = -9; x <= 9; x += 3)
                Box("Wall Beam", new Vector3(x,2,2.8f), new Vector3(.14f,4,.22f), metal);
            Bench("Shop Bench", -6);
            Bench("Sell Counter", 6);
            for (int i = 0; i < 3; i++)
            {
                Box("Bait Container", new Vector3(-6.9f + i*.9f,1.45f,.7f), new Vector3(.55f,.55f,.55f), i == 1 ? orange : metal);
                Box("Fish Crate", new Vector3(5.1f + i*.9f,1.45f,.7f), new Vector3(.7f,.4f,.65f), ice);
            }
            var hole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hole.name = "Fishing Hole";
            hole.transform.position = new Vector3(0,.015f,0);
            hole.transform.localScale = new Vector3(1.6f,.025f,1.6f);
            hole.GetComponent<Renderer>().sharedMaterial = dark;
            Box("Fishing Seat", new Vector3(1.8f,.5f,-.2f), new Vector3(.8f,1,.8f), orange);
            var rod = Box("Fishing Rod", new Vector3(.9f,1.1f,0), new Vector3(.045f,2.5f,.045f), metal);
            rod.transform.rotation = Quaternion.Euler(0,0,-48);
            Box("Fishing Line", new Vector3(0,.55f,0), new Vector3(.012f,1.1f,.012f), metal);
            Sign("TACKLE SHOP", -6); Sign("ICE SHANTY", 0); Sign("SELL CATCH", 6);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(CinemachineBrain));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().backgroundColor = new Color(.12f,.23f,.3f);
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            var navObject = new GameObject("Screen Navigation");
            navObject.SetActive(false);
            var nav = navObject.AddComponent<ScreenManager>();
            var serialized = new SerializedObject(nav);
            string[] names = { "shop", "fishing", "sell" };
            for (int i = 0; i < 3; i++)
            {
                var cam = new GameObject(names[i] + " Camera").AddComponent<CinemachineCamera>();
                cam.transform.position = new Vector3((i-1)*6,3.7f,-6);
                cam.transform.LookAt(new Vector3((i-1)*6,.9f,.5f));
                cam.Priority = i == 1 ? 10 : 0;
                serialized.FindProperty(names[i]).objectReferenceValue = cam;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            navObject.SetActive(true);
            var rules = AssetDatabase.LoadAssetAtPath<RunRules>("Assets/IceShanty/GameRunRules.asset");
            if (!rules) { rules = ScriptableObject.CreateInstance<RunRules>(); AssetDatabase.CreateAsset(rules,"Assets/IceShanty/GameRunRules.asset"); }
            var run = new GameObject("Run Manager").AddComponent<RunManager>();
            run.Configure(rules);
            var ui = new GameObject("Game HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            ui.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = ui.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280,720);
            canvas = ui.transform;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = ui.AddComponent<GameHUD>();
            hud.run = run; hud.screens = nav;
            Panel("Top Bar", new Vector2(0,295), new Vector2(1280,130));
            Panel("Bottom Bar", new Vector2(0,-292), new Vector2(1280,136));
            hud.heading = Label("ICE SHANTY", new Vector2(0,318), new Vector2(1000,45), 30);
            hud.status = Label("Run loading...", new Vector2(0,273), new Vector2(1150,35), 22);
            hud.hint = Label("Greybox prototype", new Vector2(0,-238), new Vector2(1100,30), 18);
            hud.left = Button("< SHOP", new Vector2(-480,-295), new Vector2(190,52), nav.GoLeft);
            hud.right = Button("SELL >", new Vector2(480,-295), new Vector2(190,52), nav.GoRight);
            hud.action = Button("USE FISHING ATTEMPT", new Vector2(-140,-295), new Vector2(290,52), hud.Act);
            hud.actionLabel = hud.action.GetComponentInChildren<Text>();
            hud.submit = Button("SUBMIT QUOTA", new Vector2(175,-295), new Vector2(230,52), run.SubmitQuota);
            Button("RESTART", new Vector2(540,205), new Vector2(150,40), run.StartRun);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.SaveScene(scene,path);
            AssetDatabase.SaveAssets();
            if (Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None).Length != 3 || hud.left.onClick.GetPersistentEventCount() != 1 || !run || !rules)
                throw new System.Exception("Scene validation failed");
            Debug.Log("GAME_SCENE_BUILD_PASSED");
        }

        static Material Mat(string name, Color color)
        {
            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color = color;
            AssetDatabase.CreateAsset(m, "Assets/IceShanty/GameMaterials/" + name + ".mat");
            return m;
        }
        static GameObject Box(string name, Vector3 pos, Vector3 scale, Material material)
        {
            var o = GameObject.CreatePrimitive(PrimitiveType.Cube); o.name = name;
            o.transform.position = pos; o.transform.localScale = scale;
            o.GetComponent<Renderer>().sharedMaterial = material; return o;
        }
        static void Bench(string name, float x)
        {
            Box(name, new Vector3(x,1,.7f), new Vector3(3.4f,.25f,1.3f),wood);
            foreach (float dx in new[] {-1.3f,1.3f}) Box("Bench Leg", new Vector3(x+dx,.45f,.7f),new Vector3(.2f,.9f,1),metal);
        }
        static void Sign(string title, float x)
        {
            var go = new GameObject(title); go.transform.position = new Vector3(x,2.8f,2.65f);
            var text = go.AddComponent<TextMesh>(); text.text = title; text.anchor = TextAnchor.MiddleCenter;
            text.fontSize = 60; text.characterSize = .07f; text.color = new Color(1,.8f,.4f);
        }
        static RectTransform Rect(GameObject go, Vector2 position, Vector2 size)
        {
            go.transform.SetParent(canvas,false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(.5f,.5f);
            rt.anchoredPosition = position; rt.sizeDelta = size; return rt;
        }
        static void Panel(string name, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name,typeof(RectTransform),typeof(Image)); Rect(go,position,size);
            go.GetComponent<Image>().color = new Color(.025f,.06f,.09f,.94f);
        }
        static Text Label(string title, Vector2 position, Vector2 size, int fontSize)
        {
            var go = new GameObject(title,typeof(RectTransform),typeof(Text)); Rect(go,position,size);
            var text = go.GetComponent<Text>(); text.font = font; text.text = title; text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter; text.color = Color.white; text.raycastTarget = false; return text;
        }
        static Button Button(string title, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction callback)
        {
            var go = new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button)); Rect(go,pos,size);
            go.GetComponent<Image>().color = new Color(.12f,.3f,.37f);
            var button = go.GetComponent<Button>();
            UnityEventTools.AddPersistentListener(button.onClick,callback);
            var label = Label(title,Vector2.zero,size,18); label.transform.SetParent(go.transform,false);
            return button;
        }
    }
}
