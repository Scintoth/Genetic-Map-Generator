using System;
using System.Threading;
using System.Threading.Tasks;
using GeneticMapGenerator.Application.Composition;
using GeneticMapGenerator.Application.Ports;
using GeneticMapGenerator.Unity.Configuration;
using GeneticMapGenerator.Unity.Presentation;
using GeneticMapGenerator.Unity.Threading;
using GeneticMapGenerator.Unity.Views;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Bootstrap
{
    /// <summary>
    /// Unity composition root. Builds the scene (camera, light, views), wires the core's use case to the
    /// Unity observer adapter and runs generation on a worker thread so the editor stays responsive.
    /// Added automatically to any scene on Play, so the project needs no authored scene or prefabs.
    /// </summary>
    public sealed class WorldGeneratorBootstrap : MonoBehaviour
    {
        [SerializeField] private WorldGenerationSettings settings = new WorldGenerationSettings();

        private readonly MainThreadDispatcher _dispatcher = new MainThreadDispatcher();
        private readonly HudModel _hud = new HudModel();

        private IGenerateWorldUseCase? _useCase;
        private WorldPresenter? _presenter;
        private CancellationTokenSource? _cancellation;
        private Task? _generation;
        private int _seed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureExists()
        {
            if (FindFirstObjectByType<WorldGeneratorBootstrap>() == null)
            {
                new GameObject("World Generator").AddComponent<WorldGeneratorBootstrap>();
            }
        }

        public void Regenerate()
        {
            if (!settings.reuseSeed)
            {
                _seed = unchecked(_seed * 1103515245 + 12345) & int.MaxValue;
            }

            StartGeneration();
        }

        private void Awake()
        {
            _useCase = WorldGenerator.Create();
            _seed = settings.seed;

            var terrainView = CreateChild<TerrainView>("Terrain");
            var settlementView = CreateChild<SettlementView>("Settlement");
            var cameraRig = SetUpCamera();
            SetUpLighting();

            var hud = gameObject.AddComponent<GenerationHud>();
            hud.Bind(_hud);
            hud.RegenerateRequested += Regenerate;

            _presenter = new WorldPresenter(terrainView, settlementView, cameraRig, _hud);
            hud.FocusToggleRequested += _presenter.ToggleFocus;
        }

        private void Start() => StartGeneration();

        private void Update()
        {
            _dispatcher.Drain();

            if (_generation is { IsFaulted: true })
            {
                var failure = _generation.Exception!.GetBaseException();
                _generation = null;
                Debug.LogException(failure, this);
                _presenter!.ShowFailure(failure);
            }
        }

        private void OnDestroy() => CancelGeneration();

        private void StartGeneration()
        {
            CancelGeneration();
            _dispatcher.Clear();

            var command = settings.ToCommand(_seed);
            var observer = new UnityWorldGenerationObserver(_presenter!, _dispatcher);
            var cancellation = _cancellation = new CancellationTokenSource();
            var useCase = _useCase!;

            _presenter!.ShowStarted(_seed);
            _generation = Task.Run(() => useCase.Execute(command, observer, cancellation.Token), cancellation.Token);
        }

        private void CancelGeneration()
        {
            if (_cancellation == null)
            {
                return;
            }

            // The worker observes the token and unwinds; its OperationCanceledException is expected.
            // Not disposed: the worker may still be reading the token, and a timer-less source holds no resources.
            _cancellation.Cancel();
            _cancellation = null;
            _generation = null;
        }

        private T CreateChild<T>(string childName)
            where T : Component
        {
            var child = new GameObject(childName);
            child.transform.SetParent(transform, worldPositionStays: false);
            return child.AddComponent<T>();
        }

        private static OrbitCameraRig SetUpCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                camera = new GameObject("Main Camera") { tag = "MainCamera" }.AddComponent<Camera>();
            }

            camera.farClipPlane = 5000f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.72f, 0.83f, 0.92f);

            var rig = camera.GetComponent<OrbitCameraRig>();
            return rig != null ? rig : camera.gameObject.AddComponent<OrbitCameraRig>();
        }

        private static void SetUpLighting()
        {
            foreach (var existing in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (existing.type == LightType.Directional)
                {
                    return;
                }
            }

            var sun = new GameObject("Sun").AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.1f;
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
    }
}
