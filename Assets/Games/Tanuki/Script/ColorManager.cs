using System.Collections.Generic;
using UnityEngine;
using EasyButtons;
using DG.Tweening;

namespace nostra.booboogames.Tanuki
{
    [System.Serializable]
    public class ColorSet
    {
        [Tooltip("List of colors that apply to each corresponding material.")]
        public List<Color> colors;
    }

    public class ColorManager : MonoBehaviour
    {
        [Header("Color Sets")]
        [SerializeField] private List<ColorSet> colorSets = new List<ColorSet>();

        [Header("Materials to Update")]
        [SerializeField] private List<Material> materials = new List<Material>();

        [Header("Fade Settings")]
        [SerializeField] private float fadeDuration = 0.5f;

        [SerializeField] int currentColorIndex = 0;

        private void Start()
        {
            ApplyColorSet(currentColorIndex);
        }

        [Button]
        public void NextColorSet()
        {
            currentColorIndex++;
            if (currentColorIndex >= colorSets.Count)
                currentColorIndex = 0;

            ApplyColorSet(currentColorIndex);
        }

        public void ApplyColorSet(int index)
        {
            if (index < 0 || index >= colorSets.Count)
            {
                Debug.LogWarning($"Color index {index} is out of range.");
                return;
            }

            var set = colorSets[index].colors;

            for (int i = 0; i < Mathf.Min(set.Count, materials.Count); i++)
            {
                if (materials[i] != null)
                {
                    // Smooth fade using DOTween
                    materials[i].DOColor(set[i], fadeDuration);
                }
            }

           // Debug.Log($"Fading to color set {index + 1}/{colorSets.Count}");
        }
    }
}
