using DungeonShop.Miscellaneous;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShop.UI
{
    public class HealthBar : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Health Bar Components")]
        [SerializeField]
        private RectTransform shadowRect;
        [SerializeField]
        private RectTransform fillRect;
        [SerializeField]
        private NumberToText countOutput;
        [SerializeField]
        private NumberToText substractionOutput;

        [Space(10.0f)]
        [SerializeField]
        private float maxHealth = 100;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float normalizedValue = 1.0f;

        private float lastNormalizedValue;
        private Vector2 startSize;
        private bool initialized = false;
#pragma warning restore 0649

        public float NormalizedValue
        {
            get
            {
                return normalizedValue;
            }
            set
            {
                normalizedValue = value;
                UpdateBar(Mathf.RoundToInt((lastNormalizedValue - normalizedValue) * maxHealth));
                lastNormalizedValue = normalizedValue;
            }
        }

        public float MaxHealth { get => maxHealth; set { maxHealth = value; UpdateBar(0); } }

        private void Start()
        {
            startSize = fillRect.sizeDelta;
            shadowRect.sizeDelta = startSize;
            lastNormalizedValue = normalizedValue;

            if (substractionOutput != null)
            {
                Text substractionText = substractionOutput.Output;
                if (substractionText != null)
                {
                    Color color = substractionText.color;
                    substractionText.color = new Color(color.r, color.g, color.b, 0.0f);
                }
            }

            initialized = true;
            UpdateBar(0);
        }

        public void SetSubstractionTextColor(Color color)
        {
            if(substractionOutput != null)
            {
                Text output = substractionOutput.Output;
                if(output != null)
                {
                    Color startColor = output.color;
                    output.color = new Color(color.r, color.g, color.b, startColor.a);
                }
            }
        }

        private void UpdateBar(int substract)
        {
            if (!initialized)
                return;

            countOutput.Number = Mathf.RoundToInt(maxHealth * Mathf.Clamp(normalizedValue, 0.0f, float.PositiveInfinity));
            Vector2 prevSize = fillRect.sizeDelta;
            fillRect.sizeDelta = new Vector2(startSize.x * Mathf.Clamp01(normalizedValue), startSize.y);

            StopAllCoroutines();

            if (shadowRect != null)
            {
                if (gameObject.activeInHierarchy && enabled)
                {
                    shadowRect.sizeDelta = prevSize;
                    StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.4f, 0.15f, (t) => shadowRect.sizeDelta = Vector2.Lerp(prevSize, fillRect.sizeDelta, t), null));
                }
                else
                {
                    shadowRect.sizeDelta = fillRect.sizeDelta;
                }
            }

            if(substractionOutput != null && substract != 0)
            {
                substractionOutput.Number = Mathf.Abs(substract);
                substractionOutput.Prefix = substract <= 0 ? "+" : "-";

                Text substractionText = substractionOutput.Output;
                if (substractionText != null)
                {
                    void TextVisibility(float t)
                    {
                        Color color = substractionText.color;
                        float alpha = -Mathf.Pow(2.0f * t - 1.0f, 4.0f) + 1.0f;
                        substractionText.color = new Color(color.r, color.g, color.b, alpha);
                    }

                    TextVisibility(0.0f);

                    if (gameObject.activeInHierarchy && enabled)
                        StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 1.0f, TextVisibility, null));
                }
            }
        }
    }
}