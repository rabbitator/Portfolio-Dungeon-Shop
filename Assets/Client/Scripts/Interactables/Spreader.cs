using DungeonShop.Miscellaneous;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Interactables
{
    public class Spreader : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private GameObject prefab;
        [SerializeField]
        private List<Parabola> parabolas = new List<Parabola>();
        [SerializeField]
        private float duration = 1.0f;
        [Min(0)][SerializeField]
        private int count;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float jittering = 0.5f;
#pragma warning restore 0649

        private Transform thisTransform;

        private void Awake()
        {
            thisTransform = transform;
        }

        public void SpawnPrefabs()
        {
            for (int i = 0; i < count; i++)
            {
                GameObject instance = Instantiate(prefab);
                instance.transform.position = thisTransform.position;
                instance.transform.SetParent(thisTransform);

                Rigidbody rb = instance.GetComponent<Rigidbody>();
                Collider col = instance.GetComponent<Collider>();
                if (rb != null)
                {
                    if (col != null)
                    {
                        bool startCondition = col.enabled;
                        col.enabled = false;
                        StartCoroutine(Extensions.ScaledTimeDelay(0.5f, () => col.enabled = startCondition));
                    }
                }

                Parabola parabola = parabolas[Random.Range(0, parabolas.Count)];
                void SetOnParabola(Transform transform, float t)
                {
                    transform.position = parabola.GetParabolaPoint(t);
                }

                StartCoroutine(Extensions.UniversalScaledTimeCoroutine(Random.value * 0.5f, duration, (t) => SetOnParabola(instance.transform, t), null));
            }
        }

        private Vector3 GetRandomDirection()
        {
            if (thisTransform == null)
                return Vector3.zero;

            Vector3 xyDirection = ((Random.value * 2.0f - 1.0f) * thisTransform.up + (Random.value * 2.0f - 1.0f) * thisTransform.right).normalized;
            Vector3 zDirection = thisTransform.forward;

            return Vector3.Lerp(zDirection, xyDirection, Random.value * jittering);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.black;
            Gizmos.DrawRay(transform.position, GetRandomDirection());
        }
    }
}