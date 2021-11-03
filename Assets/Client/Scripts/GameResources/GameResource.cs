using DungeonShop.Interactables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.GameResources
{
    public abstract class GameResource : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField][Min(1)]
        protected int hitCount = 1;
        [SerializeField][Min(1)]
        protected int resourceCount = 1;
#pragma warning restore 0649

        public int ResourceCount { get => resourceCount; }

        private DestructableObject destructable;
        private int startResourceCount;
        private int startHitCount;
        private bool awaked;

        private void Awake()
        {
            if (awaked)
                return;

            destructable = GetComponent<DestructableObject>();
            startResourceCount = resourceCount;
            startHitCount = hitCount;
            awaked = true;
        }

        public virtual int GetResource(ref List<Transform> debris)
        {
            if (!awaked)
                Awake();

            if (resourceCount <= 0)
                return 0;

            int tookCount = startResourceCount / startHitCount;
            tookCount -= Mathf.Clamp(tookCount - resourceCount, 0, int.MaxValue);

            if (resourceCount - tookCount < tookCount)
                tookCount += resourceCount - tookCount;

            if(hitCount <= 1)
                tookCount += resourceCount - tookCount;

            resourceCount -= tookCount;

            if (tookCount > 0)
                Destruct(ref debris);

            return tookCount;
        }

        protected virtual void Destruct(ref List<Transform> debris)
        {
            if (!awaked)
                Awake();

            if (hitCount < 0)
                return;

            hitCount--;
            if (destructable != null)
            {
                destructable.DestructPart(hitCount / (float)startHitCount, 1.0f, ref debris);
            }
        }
    }
}