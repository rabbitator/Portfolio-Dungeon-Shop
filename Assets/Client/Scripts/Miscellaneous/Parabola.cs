using System;
using UnityEngine;


namespace DungeonShop.Miscellaneous
{
    public class Parabola : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform startPoint;
        [SerializeField]
        private Transform endPoint;
        [SerializeField]
        private float height;
#pragma warning restore 0649

        public Vector3 GetParabolaPoint(float t)
        {
            return GetParabolaPoint(startPoint.position, endPoint.position, height, t);
        }

        public Vector3 GetDirection()
        {
            return (endPoint.position - startPoint.position).normalized;
        }

        public static Vector3 GetParabolaPoint(Vector3 point1, Vector3 point2, float height, float t)
        {
            Vector3 p = Vector3.Lerp(point1, point2, t);
            float h = Mathf.Pow(height, 2.0f) - Mathf.Pow(height * (t * 2.0f - 1.0f), 2.0f) + p.y;

            return new Vector3(p.x, h, p.z);
        }

        public float DistanceBetweenPoints()
        {
            return Vector3.Distance(startPoint.position, endPoint.position);
        }

        private void OnDrawGizmos()
        {
            if (startPoint != null && endPoint != null)
            {
                for (int i = 1; i <= 32; i++)
                {
                    float t1 = (i - 1) / 32.0f;
                    float t2 = i / 32.0f;

                    Vector3 p1 = GetParabolaPoint(startPoint.position, endPoint.position, height, t1);
                    Vector3 p2 = GetParabolaPoint(startPoint.position, endPoint.position, height, t2);

                    Gizmos.color = new Color(0.0f, i / 32.0f, (32 - i) / 32.0f);
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }
}