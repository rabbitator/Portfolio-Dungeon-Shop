using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShop.UI
{
    public class ResourceDisplay : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private Text outputField;
#pragma warning restore 0649

        private int currentCount;
        private int capacity;

        private void UpdateDisplay()
        {
            outputField.text = $"{currentCount}/{capacity}";
        }

        public void UpdateCount(int value)
        {
            currentCount = value;
            UpdateDisplay();
        }

        public void UpdateCapacity(int value)
        {
            capacity = value;
            UpdateDisplay();
        }
    }
}