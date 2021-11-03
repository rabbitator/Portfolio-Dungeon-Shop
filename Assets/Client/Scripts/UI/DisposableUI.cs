using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonShop.UI
{
    [ExecuteInEditMode]
    public class DisposableUI : MonoBehaviour
    {
#pragma warning disable 0649
#pragma warning disable 0414
        [Header("Animation")]
        [SerializeField]
        private float duration = 1.0f;
        public float Duration { get => duration; }
        [Space(5.0f)]
        [SerializeField]
        private Vector3 positionOffset;
        [SerializeField]
        private AnimationCurve positionCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        [SerializeField]
        private Vector3 angularOffset;
        [SerializeField]
        private AnimationCurve rotationCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));
        [SerializeField]
        private Vector3 scaleOffset;
        [SerializeField]
        private AnimationCurve scaleCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(1.0f, 1.0f));

        [Header("Main Settings")]
        [Space(10.0f)]
        [SerializeField]
        private bool activateOnEnable = true;

        [Header("Debug")]
        [Space(10.0f)]
        [SerializeField]
        private bool debug = false;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float debugSlider = 0.0f;

        private float startTime;
        private bool animating = false;
        private Transform thisTransform;
        [SerializeField]
        [HideInInspector]
        private TransformData initialData = new TransformData();
#pragma warning restore 0649
#pragma warning restore 0414

        [System.Serializable]
        private class TransformData
        {
            public Vector3 position;
            public Vector3 eulers;
            public Vector3 scale;
        }

        private void OnEnable()
        {
            thisTransform = transform;
            initialData = GetTransformData();

#if !UNITY_EDITOR
            if (activateOnEnable)
            {
                StartAnimation();
            }
#endif

#if UNITY_EDITOR
            if (EditorApplication.isPlaying && activateOnEnable)
            {
                StartAnimation();
            }
#endif
        }

        private void OnDisable()
        {
            thisTransform.position = initialData.position;
            thisTransform.eulerAngles = initialData.eulers;
            thisTransform.localScale = initialData.scale;
        }

        private void Reset()
        {
            thisTransform.position = initialData.position;
            thisTransform.eulerAngles = initialData.eulers;
            thisTransform.localScale = initialData.scale;
            animating = false;
        }

        public void SetInitialData(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            initialData.position = position;
            initialData.eulers = rotation;
            initialData.scale = scale;
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                if (debug)
                {
                    thisTransform.position = initialData.position + positionOffset * positionCurve.Evaluate(debugSlider);
                    thisTransform.eulerAngles = initialData.eulers + angularOffset * rotationCurve.Evaluate(debugSlider);
                    thisTransform.localScale = initialData.scale + scaleOffset * scaleCurve.Evaluate(debugSlider);
                }
                else
                {
                    thisTransform.position = initialData.position;
                    thisTransform.eulerAngles = initialData.eulers;
                    thisTransform.localScale = initialData.scale;
                }

                return;
            }
#endif
            if (animating)
            {
                float value = (Time.unscaledTime - startTime) / duration;
                thisTransform.position = initialData.position + positionOffset * positionCurve.Evaluate(value);
                thisTransform.eulerAngles = initialData.eulers + angularOffset * rotationCurve.Evaluate(value);
                thisTransform.localScale = initialData.scale + scaleOffset * scaleCurve.Evaluate(value);

                if (value > 1.0f)
                    Destroy(gameObject);
            }
        }

        public void StartAnimation()
        {
            animating = true;
            startTime = Time.unscaledTime;
            thisTransform.position = initialData.position + positionOffset * positionCurve.Evaluate(0.0f);
            thisTransform.eulerAngles = initialData.eulers + angularOffset * rotationCurve.Evaluate(0.0f);
            thisTransform.localScale = initialData.scale + scaleOffset * scaleCurve.Evaluate(0.0f);
        }

        private TransformData GetTransformData()
        {
            TransformData data = new TransformData
            {
                position = thisTransform.position,
                eulers = thisTransform.eulerAngles,
                scale = thisTransform.localScale
            };

            return data;
        }
    }
}