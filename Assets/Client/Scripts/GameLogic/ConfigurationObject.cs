using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.GameLogic
{
    [DisallowMultipleComponent]
    public class ConfigurationObject : MonoBehaviour
    {
        public static GameConfiguration GameConfiguration
        {
            get { return instance == null ? null : instance.configurationFile; }
            private set { if (instance != null) instance.configurationFile = value; }
        }

        [SerializeField]
        private GameConfiguration configurationFile;

        private static ConfigurationObject instance;

        private void Awake()
        {
            if (instance != null)
            {
                Destroy(this);
                Debug.LogWarning("Check all your scenes objects, there is not supposed to be several GameConfiguration objects!");

                return;
            }

            instance = this;
        }
    }
}
