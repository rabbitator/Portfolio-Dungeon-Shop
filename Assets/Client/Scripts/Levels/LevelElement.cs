using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Levels
{
    public abstract class LevelElement : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        protected Transform startPoint;
        [SerializeField]
        protected Transform endPoint;
#pragma warning restore 0649

        public Transform StartPoint { get => startPoint; }
        public Transform EndPoint { get => endPoint; }
        public Transform ElementTransform { get; private set; }

        protected virtual void Awake()
        {
            ElementTransform = transform;
        }
    }
}
