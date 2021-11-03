using DungeonShop.Miscellaneous;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class DestructableObject : MonoBehaviour
    {
        [SerializeField]
        private string fullChildName = "full";
        [SerializeField]
        private string fractionsChildName = "fracts";
        [SerializeField]
        [Space(5.0f)]
        private bool destructOnTrigger = true;
        [SerializeField]
        [Space]
        private float forcePerKg = 10.0f;

        public bool DynamicChilds { get { return dynamicChilds; } set { dynamicChilds = value; } }
        [SerializeField]
        private bool dynamicChilds = true;
        private bool lastDynamicChilds = true;

        private Transform thisTransform;
        private Transform fullTransform;
        private Transform fractionsRootTransform;
        private Transform fallenRootTransform;
        private GameObject fullGO;
        private GameObject fractsRootGO;
        private Collider trigger;
        private List<Transform> fallenOff = new List<Transform>();
        private bool broken = false;

        [Space(10.0f)]
        public DestructableEvent OnObjectBreaking;

        [System.Serializable]
        public class DestructableEvent : UnityEvent<DestructableObject> { }

        private void Awake()
        {
            thisTransform = transform;
            trigger = GetComponent<Collider>();

            fullTransform = thisTransform.Find(fullChildName);
            fractionsRootTransform = thisTransform.Find(fractionsChildName);

            if (fullTransform == null || fractionsRootTransform == null)
            {
                Debug.LogError("Goodbye!");
                Destroy(this);

                return;
            }

            fullGO = fullTransform.gameObject;
            fractsRootGO = fractionsRootTransform.gameObject;

            fullGO.SetActive(true);
            fractsRootGO.SetActive(false);
            fallenRootTransform = new GameObject("fallen").transform;
            fallenRootTransform.SetParent(fractionsRootTransform.parent);
            fallenRootTransform.SetSiblingIndex(fractionsRootTransform.GetSiblingIndex() + 1);
        }

        private void Update()
        {
            if (dynamicChilds != lastDynamicChilds)
            {
                for (int i = 0; i < fractionsRootTransform.childCount; i++)
                {
                    Rigidbody rb = fractionsRootTransform.GetChild(i).GetComponent<Rigidbody>();
                    Collider cl = fractionsRootTransform.GetChild(i).GetComponent<Collider>();

                    if (rb != null)
                        rb.isKinematic = !dynamicChilds;

                    if (cl != null)
                        cl.enabled = dynamicChilds;
                }
            }

            lastDynamicChilds = dynamicChilds;
        }

        public void Destruct(float delay)
        {
            if (broken)
                return;

            if (trigger != null)
                trigger.enabled = false;

            StartCoroutine(Destruction(delay, null));
        }

        public void Destruct(float delay, Vector3 enterPoint)
        {
            if (broken)
                return;

            if (trigger != null)
                trigger.enabled = false;

            StartCoroutine(Destruction(delay, enterPoint));
        }

        /// <summary>
        /// Select part of object and destruct only this part
        /// Direction is down-up
        /// </summary>
        /// <param name="start">Normalized start value</param>
        /// <param name="end">Normalized end value</param>
        public void DestructPart(float start, float end, ref List<Transform> debris)
        {
            float Rand()
            {
                return 2.0f * UnityEngine.Random.value - 1.0f;
            }

            void Explode(Rigidbody rb, Vector3 direction)
            {
                rb.AddForce(direction * forcePerKg * rb.mass, ForceMode.Impulse);
            }

            debris = new List<Transform>();

            start = Mathf.Clamp01(start);
            end = Mathf.Clamp01(end);

            if (end - start <= 0.0f)
                return;

            trigger.isTrigger = true;
            fullGO.SetActive(false);
            fractsRootGO.SetActive(true);

            void SetSlope(Vector3 angles, float t)
            {
                t = Mathf.Clamp01(t);
                float angle = Mathf.Sin(12.0f * t) * (1.0f - t);
                Quaternion slope = Quaternion.Euler(angles.x * angle, angles.y * angle, angles.z * angle);
                fractionsRootTransform.rotation *= slope;
            }

            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 0.5f, (t) => SetSlope(Vector3.right, t), null));
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.2f, 0.5f, (t) => SetSlope(Vector3.forward, t), null));

            List<Transform> children = fractionsRootTransform.GetComponentsInChildren<Transform>().ToList();
            children.Remove(fractionsRootTransform);
            fallenOff.ForEach(x => children.Remove(x));

            if (children.Count == 0)
                return;

            Transform highest = children.Aggregate((x, y) => x.transform.position.y > y.transform.position.y ? x.transform : y.transform);
            Transform lowest  = children.Aggregate((x, y) => x.transform.position.y < y.transform.position.y ? x.transform : y.transform);

            Vector3 averagePosition = Vector3.zero;
            children.ForEach(x => averagePosition += x.position);
            averagePosition /= children.Count;

            for (int i = 0; i < children.Count; i++)
            {
                Transform tr = children[i];
                Rigidbody rb = children[i].GetComponent<Rigidbody>();
                if (rb == null)
                    continue;
                else
                    rb.isKinematic = true;

                float normalizedHeight = (tr.position.y - lowest.position.y) / (highest.position.y - lowest.position.y);
                if (normalizedHeight >= start && normalizedHeight <= end)
                {
                    tr.localScale *= 0.9f;
                    tr.SetParent(fallenRootTransform);
                    fallenOff.Add(tr);
                    debris.Add(tr);
                    rb.isKinematic = false;

                    Vector3 childrenPosition = tr.position;
                    Vector3 planeDirection = Vector3.ProjectOnPlane(childrenPosition - averagePosition, Vector3.up).normalized;
                    Vector3 upDirection = Vector3.up;
                    Vector3 finalDirection = Vector3.Lerp(planeDirection, upDirection, 0.75f);
                    Vector3 angularVelocity = new Vector3(1e3f * Rand(), 1e3f * Rand(), 1e3f * Rand());

                    StartCoroutine(Extensions.ScaledTimeDelay(1.0f, () => tr.gameObject.AddComponent<SelfDestructOnCollission>()));
                    StartCoroutine(Extensions.SkipFrames(3, () => Explode(rb, finalDirection)));
                    StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 0.5f, (t) => rb.angularVelocity = angularVelocity * (1.0f - t), null));

                    // DEBUG
                    StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 0.5f, (t) => Debug.DrawRay(childrenPosition, finalDirection, Color.magenta), null));
                }
            }
        }

        private IEnumerator Destruction(float delay, Vector3? enterPoint)
        {
            if (delay != 0.0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;

            fullGO.SetActive(false);
            fractsRootGO.SetActive(true);

            for (int i = 0; i < fractionsRootTransform.childCount; i++)
            {
                Rigidbody rb = fractionsRootTransform.GetChild(i).GetComponent<Rigidbody>();

                if (rb != null && enterPoint != null)
                    rb.AddExplosionForce(forcePerKg, enterPoint.Value, 3.0f);
            }

            broken = true;
            OnObjectBreaking.Invoke(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (broken || !destructOnTrigger)
                return;

            Destruct(0.0f, other.transform.position);
        }

        private IEnumerator DebrisFlyOut(Transform debris, float duration)
        {
            float Rand() => UnityEngine.Random.value * 1440.0f;

            float startTime = Time.time;
            // Vector3 startScale = debris.localScale;
            Vector3 startPosition = debris.position;
            Vector3 finalPosition = debris.position - Vector3.up * 9.81f * duration;
            Quaternion startRotation = debris.rotation;
            Quaternion finalRotation = Quaternion.Euler(Rand(), Rand(), Rand());

            while (Time.time < startTime + duration)
            {
                float value = (Time.time - startTime) / duration;
                // debris.localScale = startScale * (1.0f - value);
                debris.position = Vector3.Lerp(startPosition, finalPosition, value * value);
                debris.rotation = Quaternion.Slerp(startRotation, finalRotation, value);

                yield return null;
            }

            debris.localScale = Vector3.zero;

            yield return null;
        }
    }

    class SelfDestructOnCollission : MonoBehaviour
    {
        private Rigidbody thisRigidbody;
        private Collider thisCollider;
        private Transform thisTransfom;
        private float innerTimer = 1.0f;
        private bool working = false;

        private void Awake()
        {
            thisTransfom = transform;
            thisCollider = GetComponent<Collider>();
            thisRigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!working)
            {
                innerTimer -= Time.fixedDeltaTime;

                if (innerTimer <= 0.0f)
                    InitiateSelfDestruct();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            InitiateSelfDestruct();
        }

        private void OnCollisionStay(Collision collision)
        {
            InitiateSelfDestruct();
        }

        private void InitiateSelfDestruct()
        {
            if (working)
                return;

            if (thisTransfom == null)
                Awake();

            Vector3 startScale = thisTransfom.localScale;
            thisCollider.enabled = false;
            thisRigidbody.isKinematic = true;
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 0.5f, (t) => thisTransfom.localScale = startScale * (1.0f - t), () => Destroy(gameObject)));
            working = true;
        }
    }
}