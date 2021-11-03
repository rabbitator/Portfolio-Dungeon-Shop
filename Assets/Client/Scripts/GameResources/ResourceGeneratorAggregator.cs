using DungeonShop.Miscellaneous;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonShop.GameResources
{
    public class ResourceGeneratorAggregator : MonoBehaviour
    {
        public Extensions.IntEvent SetOfflineEarning;

        private void Start()
        {
            if (PlayerPrefs.GetInt("Online", -1) < 0)
            {
                long.TryParse(PlayerPrefs.GetString("LastTimeOnline"), out long result);

                if ((DateTime.Now.Ticks - result) / (double)TimeSpan.TicksPerMinute > 5.0)
                {
                    // StartCoroutine(Extensions.RealtimeDelay(1.0f, () => SetOfflineEarning.Invoke(GetGeneratedResourceCount())));
                }

                PlayerPrefs.SetInt("Online", 1);
            }
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.SetInt("Online", -1);
            PlayerPrefs.SetString("LastTimeOnline", DateTime.Now.Ticks.ToString());
        }
    }
}