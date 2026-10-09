using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IceShanty.Editor
{
    public static class QuotaTestScene
    {
        [MenuItem("Ice Shanty/Create Quota Test Scene")]
        public static void Create()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string folder = "Assets/IceShanty";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "IceShanty");
            var rules = AssetDatabase.LoadAssetAtPath<RunRules>(folder + "/RunRules.asset");
            if (!rules)
            {
                rules = ScriptableObject.CreateInstance<RunRules>();
                AssetDatabase.CreateAsset(rules, folder + "/RunRules.asset");
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Test Camera").AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.14f, 0.2f);
            camera.transform.position = new Vector3(0, 5, -7);
            camera.transform.LookAt(Vector3.zero);
            var ice = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ice.name = "Ice Greybox";
            ice.transform.localScale = new Vector3(12, 0.2f, 12);
            var light = new GameObject("Daylight").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            var run = new GameObject("Run Manager").AddComponent<RunManager>();
            run.Configure(rules);
            run.gameObject.AddComponent<QuotaDebugPanel>().Configure(run);
            EditorSceneManager.SaveScene(scene, AssetDatabase.GenerateUniqueAssetPath(folder + "/QuotaTest.unity"));
            Selection.activeGameObject = run.gameObject;
        }

        public static void ValidateBatch()
        {
            var rules = ScriptableObject.CreateInstance<RunRules>();
            var owner = new GameObject("Quota validation");
            var run = owner.AddComponent<RunManager>();
            run.Configure(rules);
            run.StartRun();
            Check(run.State.Quota == 100 && run.State.Attempts == 5, "Initial state");
            Check(!run.TrySpend(1) && !run.RecordSale(-1), "Invalid transactions");
            Check(run.RecordSale(100) && run.TrySpend(25), "Sale and purchase");
            Check(run.State.Earnings == 100 && run.State.Cash == 75, "Independent earnings");
            run.SubmitQuota();
            Check(run.State.Period == 1, "Premature submission");
            for (int i = 0; i < 5; i++) Check(run.TryUseAttempt(), "Attempt");
            Check(!run.TryUseAttempt(), "Exhaustion");
            run.SubmitQuota();
            Check(run.State.Period == 2 && run.State.Quota == 150 && run.State.Earnings == 0 && run.State.Cash == 75, "Advance");
            for (int i = 0; i < 5; i++) run.TryUseAttempt();
            Check(run.RecordSale(25), "Final sale allowed");
            run.SubmitQuota();
            Check(run.State.Phase == RunPhase.GameOver && !run.RecordSale(25) && !run.TrySpend(1), "Failure locks run");
            run.StartRun();
            Check(run.State.Period == 1 && run.State.Cash == 0, "Restart");
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(rules);
            Create();
            Debug.Log("MODULE2_VALIDATION_PASSED");
        }

        static void Check(bool result, string label)
        {
            if (!result) throw new System.Exception("Quota validation failed: " + label);
        }
    }
}
