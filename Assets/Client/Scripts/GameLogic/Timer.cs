using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.GameLogic
{
    public class Timer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        protected float timerLimit = 1.0f;
        [SerializeField]
        protected bool runOnEnable = false;
        [SerializeField]
        protected bool unscaledTime = true;

        [Space(10f)]
        [Header("Timer Events")]
        public UnityEvent OnTimerRun;
        public UnityEvent OnNewFrame;
        public UnityEvent OnTimeIsUp;

        private float timeValue = 0.0f;
        private bool timerActive = false;
#pragma warning restore 0649

        protected virtual void OnEnable()
        {
            if (runOnEnable)
                RunTimer();
        }

        protected virtual void FixedUpdate()
        {
            if (timerActive)
            {
                timeValue += unscaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;

                if (timeValue >= timerLimit)
                {
                    timeValue = 0.0f;
                    timerActive = false;

                    OnTimeIsUp.Invoke();
                }
            }
        }

        protected virtual void Update()
        {
            if (timerActive)
                OnNewFrame.Invoke();
        }

        public virtual void RunTimer()
        {
            timeValue = 0.0f;
            timerActive = true;

            OnTimerRun.Invoke();
        }

        public virtual void StopTimer()
        {
            timeValue = 0.0f;
            timerActive = false;
        }
    }
}