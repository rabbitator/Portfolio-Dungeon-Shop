using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.UI
{
    public class ProgressBar : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Progress Bar Components")]
        [SerializeField]
        private RectTransform fillRect;

        [Space(10.0f)]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float normalizedValue = 1.0f;

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
                normalizedValue = Mathf.Clamp01(value);
                UpdateBar();
            }
        }

        private void Start()
        {
            startSize = fillRect.localScale;

            initialized = true;
            UpdateBar();
        }

        private void UpdateBar()
        {
            if (!initialized)
                Start();

            fillRect.localScale = new Vector2(startSize.x * normalizedValue, startSize.y);
            fillRect.ForceUpdateRectTransforms();
        }
    }
}