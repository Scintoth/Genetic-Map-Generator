using System;
using System.Collections.Generic;
using GeneticMapGenerator.Domain.Evolution;
using UnityEngine;

namespace GeneticMapGenerator.Unity.Views
{
    /// <summary>What the HUD shows; written by the presenter, read every frame by the HUD.</summary>
    public sealed class HudModel
    {
        public string Phase { get; set; } = "Starting";

        public string Status { get; set; } = string.Empty;

        public int Seed { get; set; }

        public int Generation { get; set; }

        public int MaxGenerations { get; set; }

        public double AverageScore { get; set; }

        public Fitness Fitness { get; set; } = Fitness.Zero;

        public IReadOnlyList<string> Details { get; set; } = Array.Empty<string>();

        public bool IsRunning { get; set; }
    }

    /// <summary>Immediate-mode overlay showing evolution progress, with regenerate controls.</summary>
    public sealed class GenerationHud : MonoBehaviour
    {
        private const float Width = 330f;
        private const float BarWidth = 150f;

        private HudModel _model = new HudModel();
        private Texture2D? _barTexture;

        public event Action? RegenerateRequested;

        public event Action? FocusToggleRequested;

        public void Bind(HudModel model) => _model = model;

        private void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.R))
            {
                RegenerateRequested?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusToggleRequested?.Invoke();
            }
#endif
        }

        private void OnGUI()
        {
            _barTexture ??= Texture2D.whiteTexture;

            GUILayout.BeginArea(new Rect(12, 12, Width, Screen.height - 24), GUI.skin.box);
            GUILayout.Label($"<b>Genetic Map Generator</b>   seed {_model.Seed}", RichText());
            GUILayout.Label($"{_model.Phase} — generation {_model.Generation} / {_model.MaxGenerations}");
            GUILayout.Label($"best {_model.Fitness.Score:0.000}   average {_model.AverageScore:0.000}");

            foreach (var component in _model.Fitness.Components)
            {
                DrawBar(component.Name, (float)component.Score);
            }

            foreach (var detail in _model.Details)
            {
                GUILayout.Label(detail);
            }

            if (!string.IsNullOrEmpty(_model.Status))
            {
                GUILayout.Label(_model.Status, RichText());
            }

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_model.IsRunning ? "Restart (R)" : "New world (R)"))
            {
                RegenerateRequested?.Invoke();
            }

            if (GUILayout.Button("Toggle focus (F)"))
            {
                FocusToggleRequested?.Invoke();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawBar(string label, float score)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(Width - BarWidth - 70));
            var rect = GUILayoutUtility.GetRect(BarWidth, 14, GUILayout.Width(BarWidth));
            GUI.color = new Color(0.25f, 0.25f, 0.25f);
            GUI.DrawTexture(rect, _barTexture);
            GUI.color = Color.Lerp(new Color(0.85f, 0.3f, 0.25f), new Color(0.35f, 0.75f, 0.35f), score);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width * Mathf.Clamp01(score), rect.height), _barTexture);
            GUI.color = Color.white;
            GUILayout.Label($"{score:0.00}", GUILayout.Width(40));
            GUILayout.EndHorizontal();
        }

        private static GUIStyle RichText() => new GUIStyle(GUI.skin.label) { richText = true };
    }
}
