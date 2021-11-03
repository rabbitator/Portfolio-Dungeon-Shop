using DungeonShop.Player;
using UnityEngine;

namespace DungeonShop.Levels
{
    [RequireComponent(typeof(Animator))]
    public class Door : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private ParticleSystem openedDoorFX;
        [SerializeField]
        private PlayerTriggerEvents playerEvents;

        public PlayerTriggerEvents PlayerEvents { get => playerEvents; }
#pragma warning restore 0649

        private Animator thisAnimator;

        private void Awake()
        {
            thisAnimator = GetComponent<Animator>();
        }

        public void Open()
        {
            thisAnimator.SetBool("Open", true);

            if (openedDoorFX != null)
                openedDoorFX.Play();
        }

        public void Close()
        {
            thisAnimator.SetBool("Open", false);

            if (openedDoorFX != null)
                openedDoorFX.Stop();
        }
    }
}