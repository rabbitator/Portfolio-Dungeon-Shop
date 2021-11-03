using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.GameResources
{
    [RequireComponent(typeof(Animator))]
    public class GameResourceChest : GameResource
    {
        [Space(20.0f)]
        public UnityEvent OnChestOpen;

        private Animator animator;
        private bool opened = false;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public override int GetResource(ref List<Transform> debris)
        {
            if (!opened)
            {
                animator.SetBool("Open", true);
                opened = true;

                OnChestOpen.Invoke();
            }

            return base.GetResource(ref debris);
        }
    }
}