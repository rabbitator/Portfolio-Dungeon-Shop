using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Miscellaneous
{
    public class CopyTransform : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Transform targetTransform;
#pragma warning restore 0649

        private Transform thisTransform;

        private void Awake()
        {
            thisTransform = transform;
        }

        private void LateUpdate()
        {
            thisTransform.position = targetTransform.position;
            thisTransform.rotation = targetTransform.rotation;
        }
    }
}