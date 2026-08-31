using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.Unity.Adapters;
using Game.Unity.Composition;
using Game.Unity.Input;
using Game.Unity.Presenters;
using Game.Unity.View;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Game.Unity.Editor
{
    /// <summary>
    /// Authors the whole playable scene from code so it can be rebuilt headlessly.
    /// Entry point named per CLAUDE.md section 6: -executeMethod Game.Unity.Editor.ProjectSetup.Setup
    /// </summary>
    public static class ProjectSetup
    {
        private const string ScenePath = "Assets/Scenes/Prototype.unity";

        public static void Setup()
        {
            // Before anything else: an auto-bake would hang batchmode forever.
            Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var sun = CreateSunAndAtmosphere();
            CreateGround();
            var library = CreateVolumeAndLibrary();
            var trees = PlantClusters(library);
            FrameWithCliffs();
            var (player, controller, animator) = CreatePlayer();
            var followCamera = CreateCamera(player.transform);
            Compose(library, sun, player, controller, animator, followCamera);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();

            Debug.Log($"[ProjectSetup] Scene authored: {trees} trees. Saved to {ScenePath}");
            EditorApplication.Exit(0);
        }

        private static Light CreateSunAndAtmosphere()
        {
            var go = new GameObject("Sun");
            var sun = go.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.77f, 0.48f);
            sun.intensity = 1.2f;
            sun.shadows = LightShadows.Soft;
            go.transform.rotation = Quaternion.Euler(18f, -35f, 0f);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(1f, 0.83f, 0.62f);
            RenderSettings.fogStartDistance = 5f;
            RenderSettings.fogEndDistance = 500f;

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.62f, 0.60f, 0.78f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.52f, 0.62f);
            RenderSettings.ambientGroundColor = new Color(0.32f, 0.30f, 0.36f);

            return sun;
        }

        private static void CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(20f, 1f, 20f);

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                color = new Color(0.30f, 0.38f, 0.22f)
            };
            AssetDatabase.CreateAsset(material, "Assets/Scenes/GroundMaterial.mat");
            ground.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static AssetLibrary CreateVolumeAndLibrary()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
            var volumeGo = new GameObject("Global Volume");
            var volume = volumeGo.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;

            if (profile != null)
            {
                if (profile.TryGet<Bloom>(out var bloom))
                {
                    bloom.active = true;
                    bloom.intensity.overrideState = true;
                    bloom.intensity.value = 0.7f;
                    bloom.threshold.overrideState = true;
                    bloom.threshold.value = 0.9f;
                }

                if (profile.TryGet<Vignette>(out var vignette))
                {
                    vignette.active = true;
                    vignette.intensity.overrideState = true;
                    vignette.intensity.value = 0.3f;
                }

                if (profile.TryGet<Tonemapping>(out var tonemapping))
                {
                    tonemapping.active = true;
                    tonemapping.mode.overrideState = true;
                    tonemapping.mode.value = TonemappingMode.Neutral;
                }

                EditorUtility.SetDirty(profile);
            }

            var libraryGo = new GameObject("AssetLibrary");
            var library = libraryGo.AddComponent<AssetLibrary>();

            var treePrefabs = LoadAll(new[]
            {
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Tree_Pine_01.prefab",
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Tree_Pine_02.prefab",
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Tree_Pine_03.prefab",
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Tree_01.prefab"
            });

            var wall = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Log_01.prefab");

            var so = new SerializedObject(library);
            var array = so.FindProperty("_treePrefabs");
            array.arraySize = treePrefabs.Count;
            for (var i = 0; i < treePrefabs.Count; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = treePrefabs[i];
            }

            so.FindProperty("_buildingPiecePrefab").objectReferenceValue = wall;
            so.ApplyModifiedProperties();

            return library;
        }

        private static List<GameObject> LoadAll(IEnumerable<string> paths) =>
            paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).Where(p => p != null).ToList();

        /// <summary>
        /// Two dense clusters, not a scatter. Procedural scattering reads as emptiness and
        /// this project has paid for that lesson twice (CLAUDE.md section 6).
        /// </summary>
        private static int PlantClusters(AssetLibrary library)
        {
            var centres = new[] { new Vector3(9f, 0f, 7f), new Vector3(-11f, 0f, 12f) };
            var planted = 0;

            foreach (var centre in centres)
            {
                for (var i = 0; i < 7; i++)
                {
                    var prefab = library.RandomTreePrefab();
                    if (prefab == null)
                    {
                        continue;
                    }

                    var angle = i / 7f * Mathf.PI * 2f;
                    var radius = 2.2f + (i % 3) * 1.4f;
                    var position = centre + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                    var tree = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    tree.name = $"Tree_{planted:D2}";
                    tree.transform.position = position;
                    tree.transform.rotation = Quaternion.Euler(0f, i * 47f, 0f);

                    EnsureCollider(tree);
                    tree.AddComponent<TreeView>();
                    planted++;
                }
            }

            return planted;
        }

        private static void FrameWithCliffs()
        {
            var cliff = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Synty/PolygonGeneric/Prefabs/Environment/SM_Gen_Env_Cliff_01.prefab");
            if (cliff == null)
            {
                return;
            }

            // Ridges at the edges of the shot; the valley reads as a place, not a plane.
            var spots = new[]
            {
                new Vector3(-26f, 0f, 30f), new Vector3(0f, 0f, 40f), new Vector3(28f, 0f, 32f),
                new Vector3(-34f, 0f, -6f), new Vector3(34f, 0f, -4f)
            };

            for (var i = 0; i < spots.Length; i++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(cliff);
                instance.name = $"Ridge_{i}";
                instance.transform.position = spots[i];
                instance.transform.rotation = Quaternion.Euler(0f, i * 63f, 0f);
                instance.transform.localScale = Vector3.one * Random.Range(2.4f, 3.6f);
            }
        }

        private static void EnsureCollider(GameObject go)
        {
            if (go.GetComponentInChildren<Collider>() != null)
            {
                return;
            }

            var capsule = go.AddComponent<CapsuleCollider>();
            capsule.height = 6f;
            capsule.radius = 0.6f;
            capsule.center = new Vector3(0f, 3f, 0f);
        }

        private static (GameObject player, CharacterController controller, Animator animator) CreatePlayer()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1.2f, 0f);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            Animator animator = null;

            // A ready-made preset, not a modular part: the pack ships 121 whole characters
            // alongside 721 loose limbs, and picking the first "Character" match lands on an arm.
            const string presets = "Assets/Synty/PolygonFantasyHeroCharacters/Prefabs/Characters_Presets";
            var characterPath = AssetDatabase
                .FindAssets("t:Prefab", new[] { presets })
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(p => p.Contains("Preset_1.prefab"))
                ?? AssetDatabase
                    .FindAssets("t:Prefab", new[] { presets })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .FirstOrDefault();

            if (characterPath != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
                if (prefab != null)
                {
                    var body = (GameObject)PrefabUtility.InstantiatePrefab(prefab, player.transform);
                    body.transform.localPosition = Vector3.zero;
                    animator = body.GetComponent<Animator>() ?? body.AddComponent<Animator>();
                    Debug.Log($"[ProjectSetup] Player body: {characterPath}");
                }
            }

            if (animator == null)
            {
                // Visible stand-in rather than an invisible player.
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.transform.SetParent(player.transform, false);
                capsule.transform.localPosition = new Vector3(0f, 0.9f, 0f);
                Object.DestroyImmediate(capsule.GetComponent<Collider>());
                Debug.LogWarning("[ProjectSetup] No Synty character prefab found; using a capsule.");
            }

            player.AddComponent<PlayerInputRouter>();
            return (player, controller, animator);
        }

        private static FollowCamera CreateCamera(Transform target)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";

            var camera = go.AddComponent<Camera>();
            camera.allowHDR = true;
            camera.farClipPlane = 800f;

            var urp = go.AddComponent<UniversalAdditionalCameraData>();
            urp.renderPostProcessing = true;
            urp.antialiasing = AntialiasingMode.FastApproximateAntialiasing;

            go.AddComponent<AudioListener>();

            var follow = go.AddComponent<FollowCamera>();
            new SerializedObject(follow).Update();
            var so = new SerializedObject(follow);
            so.FindProperty("_target").objectReferenceValue = target;
            so.ApplyModifiedProperties();

            return follow;
        }

        private static void Compose(
            AssetLibrary library,
            Light sun,
            GameObject player,
            CharacterController controller,
            Animator animator,
            FollowCamera followCamera)
        {
            var go = new GameObject("__Bootstrap");
            var clock = go.AddComponent<UnityClock>();
            var sky = go.AddComponent<SkyPresenter>();
            var bootstrap = go.AddComponent<GameBootstrap>();

            var router = player.GetComponent<PlayerInputRouter>();

            var so = new SerializedObject(bootstrap);
            so.FindProperty("_library").objectReferenceValue = library;
            so.FindProperty("_clock").objectReferenceValue = clock;
            so.FindProperty("_sky").objectReferenceValue = sky;
            so.FindProperty("_player").objectReferenceValue = router;
            so.FindProperty("_followCamera").objectReferenceValue = followCamera;
            so.FindProperty("_sun").objectReferenceValue = sun;
            so.ApplyModifiedProperties();

            var skySo = new SerializedObject(sky);
            skySo.FindProperty("_sun").objectReferenceValue = sun;
            skySo.ApplyModifiedProperties();
        }
    }
}
