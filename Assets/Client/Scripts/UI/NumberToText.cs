using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShop.UI
{
    [ExecuteInEditMode]
    public class NumberToText : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private string prefix = "";
        [SerializeField]
        private string postfix = "";
        [SerializeField]
        private int number;

        [Space(10.0f)]
        [SerializeField]
        private Text output;
#pragma warning restore 0649

        public int Number { get => number; set => number = value; }
        public string Postfix { get => postfix; set => postfix = value; }
        public string Prefix { get => prefix; set => prefix = value; }
        public Text Output { get => output; }

        private void Update()
        {
            if (output != null)
                output.text = $"{prefix}{number}{postfix}";
        }
    }
}