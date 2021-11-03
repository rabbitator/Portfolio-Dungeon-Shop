using DungeonShop.Miscellaneous;
using DungeonShop.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Interactables
{
    public class Intakeble : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private float delay = 0.0f;
        [SerializeField]
        private float duration = 1.0f;
        [SerializeField]
        private bool intakeOnStart = true;
        [SerializeField]
        private bool selfDestruct = true;
#pragma warning restore 0649

        Transform thisTransform;

        private void Awake()
        {
            thisTransform = transform;
        }

        private void Start()
        {
            if (intakeOnStart)
                Intake();
        }

        public void Intake()
        {
            if (thisTransform == null)
                thisTransform = transform;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                StartCoroutine(Extensions.ScaledTimeDelay(delay, () => rb.isKinematic = true));

            Collider col = GetComponent<Collider>();
            if (col != null)
                StartCoroutine(Extensions.ScaledTimeDelay(delay, () => col.enabled = false));

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                void SetPosition(float t)
                {
                    Vector3 startPosition = thisTransform.position;
                    thisTransform.position = Vector3.Lerp(startPosition, player.ResourceIntakePoint.position, Mathf.Pow(t, 2.0f));
                }

                void SelfDestruct()
                {
                    if (selfDestruct)
                        Destroy(gameObject);
                }

                StartCoroutine(Extensions.UniversalScaledTimeCoroutine(delay, duration, SetPosition, SelfDestruct));
            }
        }
    }
}