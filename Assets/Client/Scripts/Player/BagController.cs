using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;

namespace DungeonShop.Player
{
    public class BagController : MonoBehaviour
    {
#pragma warning disable 0649     
        [SerializeField]
        private Animator bagAnimator;
        [SerializeField]
        private SkinnedMeshRenderer woodMesh;
        [SerializeField]
        private SkinnedMeshRenderer crystalMesh;

        [Space(10.0f)]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float bagSize = 0.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float woodSize = 1.0f;
        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float crystalSize = 1.0f;
#pragma warning restore 0649

        public enum ResourceType
        {
            Wood,
            Crystal
        }

        public int FreeWoodSpace
        {
            get
            {
                return pocketCapacity - takenWood;
            }
        }

        public int FreeCrystalSpace
        {
            get
            {
                return pocketCapacity - takenCrystal;
            }
        }

        public float BagSize { get => bagSize; set => bagSize = Mathf.Clamp01(value); }
        public float WoodSize { get => woodSize; set => woodSize = Mathf.Clamp01(value); }
        public float CrystalSize { get => crystalSize; set => crystalSize = Mathf.Clamp01(value); }

        private int takenCrystal = 0;
        private int takenWood = 0;
        private int pocketCapacity = 20;

/*
        private readonly string capacityPropertyName = "BagPocket_Capacity";
        private readonly string takenCrystalPropertyName = "Bag_Taken_Crystal";
        private readonly string takenWoodPropertyName = "Bag_Taken_Wood";
*/

        private void Update()
        {
/*
            pocketCapacity = PlayerPrefs.GetInt(capacityPropertyName, 0);
            takenCrystal = PlayerPrefs.GetInt(takenCrystalPropertyName, 0);
            takenWood = PlayerPrefs.GetInt(takenWoodPropertyName, 0);
*/

            if (pocketCapacity != 0)
            {
                float size = (takenCrystal + takenWood) / (2.0f * pocketCapacity);
                bagSize = size;
                crystalSize = Mathf.Sqrt(size) * (takenCrystal / (float)pocketCapacity);
                woodSize = Mathf.Sqrt(size) * (takenWood / (float)pocketCapacity);
            }

            if (bagAnimator != null)
            {
                bagAnimator.SetFloat("Size", bagSize);
            }
        }

        private void LateUpdate()
        {
            if(woodMesh != null)
            {
                woodMesh.SetBlendShapeWeight(0, woodSize * 100);
            }

            if(crystalMesh != null)
            {
                crystalMesh.SetBlendShapeWeight(0, crystalSize * 100);
            }
        }

        public void SetBagWoodCount(int count)
        {
            takenWood = count;
        }

        public void SetBagCrystalCount(int count)
        {
            takenCrystal = count;  
        }


        public void SetBagCapacity(int value)
        {
            // Bag contains two pockets, so this method 
            // exists for set up size per pocket
            // PlayerPrefs.SetInt(capacityPropertyName, value);
            pocketCapacity = value;
        }


        /*
                public void AddCrystalResources(int count)
                {
                    pocketCapacity = PlayerPrefs.GetInt(capacityPropertyName, 0);
                    takenCrystal = PlayerPrefs.GetInt(takenCrystalPropertyName, 0);

                    if (takenCrystal + count <= pocketCapacity)
                    {
                        takenCrystal += count;
                        takenCrystal = Mathf.Clamp(takenCrystal, 0, pocketCapacity);
                        PlayerPrefs.SetInt(takenCrystalPropertyName, takenCrystal);
                    }
                }

                public void AddWoodResources(int count)
                {
                    pocketCapacity = PlayerPrefs.GetInt(capacityPropertyName, 0);
                    takenWood = PlayerPrefs.GetInt(takenWoodPropertyName, 0);

                    if (takenWood + count <= pocketCapacity)
                    {
                        takenWood += count;
                        takenWood = Mathf.Clamp(takenWood, 0, pocketCapacity);
                        PlayerPrefs.SetInt(takenWoodPropertyName, takenWood);
                    }
                }
        */

        /*
                public void ResetTaken()
                {
                    PlayerPrefs.SetInt(takenCrystalPropertyName, 0);
                    PlayerPrefs.SetInt(takenWoodPropertyName, 0);
                }
        */
    }
}
