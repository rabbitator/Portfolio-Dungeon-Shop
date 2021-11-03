using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.Interactables
{
    public class PrefabInstancer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private GameObject prefab;
#pragma warning restore 0649

        private Transform thisTransform;

        private void Awake()
        {
            thisTransform = transform;
        }

        public void InstantiatePrefab()
        {
            GameObject instance = Instantiate(prefab);
            instance.transform.SetParent(thisTransform);
            instance.transform.localPosition = Vector3.zero;
        }
    }
}