using DungeonShop.Miscellaneous;
using UnityEngine;

class Citizen : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private Animator regularAnimator;
    [SerializeField]
    private Animator simpleAnimator;
#pragma warning restore 0649

    private Transform awayPoint;
    private Transform thisTransform;

    public Transform AwayPoint { get => awayPoint; set => awayPoint = value; }

    public void Awake()
    {
        thisTransform = transform;
    }

    public void WalkAway(float delay, float duration)
    {
        void SetCustomerAnimatorFloat(string floatName, float condition)
        {
            if (regularAnimator != null)
            {
                if (regularAnimator != null)
                    regularAnimator.SetFloat(floatName, condition);

                if (simpleAnimator != null)
                    simpleAnimator.SetFloat(floatName, condition);
            }
        }

        void SetCustomerOnPathAway(Vector3 startPoint, Vector3 awayPoint, float t)
        {
            thisTransform.position = Vector3.Lerp(startPoint, awayPoint, t);
            Vector3 lookAt = new Vector3(awayPoint.x, thisTransform.position.y, awayPoint.z);
            thisTransform.LookAt(lookAt);
            SetCustomerAnimatorFloat("Speed", 0.5f);

        }

        void FinalizeCustomerAwayPosition(Vector3 lookAtPoint)
        {
            Vector3 lookAt = new Vector3(lookAtPoint.x, thisTransform.position.y, lookAtPoint.z);
            thisTransform.LookAt(lookAt);
            SetCustomerAnimatorFloat("Speed", 0.0f);

            Destroy(this);
        }

        Vector3 start = thisTransform.transform.position;
        Vector3 away = awayPoint.position;
        StartCoroutine(Extensions.UniversalScaledTimeCoroutine(delay, duration, (t) =>
            SetCustomerOnPathAway(start, away, t), () =>
            FinalizeCustomerAwayPosition(start)));
    }
}
