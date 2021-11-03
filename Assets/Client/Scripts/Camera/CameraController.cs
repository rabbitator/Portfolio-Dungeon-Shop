using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DungeonShop.GameLogic
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform cameraTarget;

        [Space(5.0f)]
        public Vector3 additionalOffset;
        public Vector3 additionalRotate;

        [Space(5.0f)]
        [SerializeField]
        private float distance = 1.0f;
        [SerializeField]
        private float smoothFactor = 5.0f;

        [Header("Camera Limits")]
        [Space(10.0f)]
        [SerializeField]
        private Transform limitLeftBottom;
        [SerializeField]
        private Transform limitRightTop;

        public float Distance { get { return distance; } set { distance = value; } }

        public Transform LimitLeftBottom { get => limitLeftBottom; set => limitLeftBottom = value; }
        public Transform LimitRightTop { get => limitRightTop; set => limitRightTop = value; }

        private Camera thisCamera;
        private Vector3 target;
        private Quaternion startRotation;
        private Transform thisTransform;
        private Vector3 startDifference;
        private Vector3 overridedPosition;
        private Quaternion overridedRotation;
        private bool overrided = false;

        private Vector3[] outCorners = new Vector3[4];
#pragma warning restore 0649

        private void Awake()
        {
            thisCamera = GetComponent<Camera>();
            thisTransform = transform;
            startDifference = cameraTarget.position - thisTransform.position;
            startRotation = thisTransform.rotation;
        }

        private void LateUpdate()
        {
            target = cameraTarget.position;

            if (limitLeftBottom != null && limitRightTop != null)
            {
                target = LimitPosition(cameraTarget.position);
            }

            Vector3 targetPosition = overrided ? overridedPosition : Vector3.LerpUnclamped(target, target - startDifference + additionalOffset, distance);
            Quaternion targetRotation = overrided ? overridedRotation : startRotation * Quaternion.Euler(additionalRotate);

            thisTransform.position = Vector3.Lerp(thisTransform.position, targetPosition, smoothFactor * Time.deltaTime);
            thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, targetRotation, smoothFactor * Time.deltaTime);
        }

        private Vector3 LimitPosition(Vector3 targetPosition)
        {
            Vector3 avg = (limitLeftBottom.position + limitRightTop.position) * 0.5f;
            Ray ray;
            Plane plane = new Plane(Vector3.up, avg);

            Vector3[] temp = new Vector3[4];
            thisCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), 1.0f, Camera.MonoOrStereoscopicEye.Mono, temp);

            outCorners[0] = thisTransform.TransformPoint(temp[0]);
            outCorners[1] = thisTransform.TransformPoint(temp[1]);
            outCorners[2] = thisTransform.TransformPoint(temp[2]);
            outCorners[3] = thisTransform.TransformPoint(temp[3]);

            ray = new Ray(thisTransform.position, (outCorners[0] - thisTransform.position).normalized);
            if (plane.Raycast(ray, out float enter1))
            {
                outCorners[0] = ray.GetPoint(enter1);
            }

            ray = new Ray(thisTransform.position, (outCorners[1] - thisTransform.position).normalized);
            if (plane.Raycast(ray, out float enter2))
            {
                outCorners[1] = ray.GetPoint(enter2);
            }

            ray = new Ray(thisTransform.position, (outCorners[2] - thisTransform.position).normalized);
            if (plane.Raycast(ray, out float enter3))
            {
                outCorners[2] = ray.GetPoint(enter3);
            }

            ray = new Ray(thisTransform.position, (outCorners[3] - thisTransform.position).normalized);
            if (plane.Raycast(ray, out float enter4))
            {
                outCorners[3] = ray.GetPoint(enter4);
            }

            float widthBottom = outCorners[3].x - outCorners[0].x;
            float widthTop = outCorners[2].x - outCorners[1].x;
            float heightLeft = (outCorners[1].z - outCorners[0].z);
            float heightRight = (outCorners[2].z - outCorners[3].z);

            float lowerPoint = Mathf.Min(outCorners[0].z, outCorners[3].z);
            float higherPoint = Mathf.Max(outCorners[1].z, outCorners[2].z);

#pragma warning disable 0162

            // Clamping X position
            float maxWidth = Mathf.Max(widthBottom, widthTop);
            if (false) // maxWidth > limitRightTop.position.x - limitLeftBottom.position.x)
            {
                targetPosition = new Vector3(Vector3.Lerp(limitLeftBottom.position, limitRightTop.position, 0.5f).x, targetPosition.y, targetPosition.z);
            }
            else
            {
                float halfWidth = Mathf.Min(widthBottom, widthTop) * 0.5f;
                if (targetPosition.x - halfWidth < limitLeftBottom.position.x)
                    targetPosition = new Vector3(limitLeftBottom.position.x + halfWidth, targetPosition.y, targetPosition.z);
                if (targetPosition.x + halfWidth > limitRightTop.position.x)
                    targetPosition = new Vector3(limitRightTop.position.x - halfWidth, targetPosition.y, targetPosition.z);
            }

            // Clamping Z position
            float maxHeight = Mathf.Max(heightLeft, heightRight);
            if (false) // maxHeight > limitRightTop.position.z - limitLeftBottom.position.z)
            {
                targetPosition = new Vector3(targetPosition.x, targetPosition.y, Vector3.Lerp(limitLeftBottom.position, limitRightTop.position, 0.5f).z);
                // targetPosition += Vector3.forward * maxHeight * 0.5f * -Mathf.Clamp(thisTransform.eulerAngles.x - 90.0f, -90.0f, 90.0f) * 0.01111111f;
            }
            else
            {
                float halfHeight = Mathf.Min(heightLeft, heightRight) * 0.5f;
                // if (/*targetPosition.z - halfHeight*/ lowerPoint < limitLeftBottom.position.z)
                    // targetPosition = new Vector3(targetPosition.x, targetPosition.y, limitLeftBottom.position.z + halfHeight);
                    targetPosition += Vector3.forward * (limitLeftBottom.position.z - lowerPoint);
                // if (/*targetPosition.z + halfHeight*/ higherPoint > limitRightTop.position.z)
                    // targetPosition = new Vector3(targetPosition.x, targetPosition.y, limitRightTop.position.z - halfHeight);
                    targetPosition -= Vector3.forward * (higherPoint - limitRightTop.position.z);
            }

#pragma warning restore 0162

            return targetPosition;
        }

        public void OverrideCameraTransform(Vector3 position, Quaternion rotation)
        {
            overridedPosition = position;
            overridedRotation = rotation;
            overrided = true;
        }

        public void RemoveTransformOverride()
        {
            overrided = false;
        }

        private void OnDrawGizmos()
        {
            if (thisTransform == null)
                return;

            Gizmos.color = Color.green;
            float sphereRadius = 0.4f + Mathf.Sin(Time.time * 5.0f) * 0.1f + 0.1f;
            Vector3 pos;

            if (limitLeftBottom == null || limitRightTop == null)
                return;

            pos = new Vector3(limitLeftBottom.position.x, limitLeftBottom.position.y, limitLeftBottom.position.z);
            Gizmos.DrawWireSphere(pos, sphereRadius);

            pos = new Vector3(limitRightTop.position.x, limitLeftBottom.position.y, limitLeftBottom.position.z);
            Gizmos.DrawWireSphere(pos, sphereRadius);

            pos = new Vector3(limitRightTop.position.x, limitRightTop.position.y, limitRightTop.position.z);
            Gizmos.DrawWireSphere(pos, sphereRadius);

            pos = new Vector3(limitLeftBottom.position.x, limitRightTop.position.y, limitRightTop.position.z);
            Gizmos.DrawWireSphere(pos, sphereRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(outCorners[0], sphereRadius);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(outCorners[1], sphereRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(outCorners[2], sphereRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(outCorners[3], sphereRadius);

            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(target, sphereRadius);
        }
    }
}