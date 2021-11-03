using System;
using UnityEngine;
using UnityEngine.Events;
using DungeonShop.Player;

namespace DungeonShop.GameLogic
{
    public class LevelInitializer : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Player")]
        [Space(10.0f)]
        [SerializeField]
        private PlayerController player;

        [Header("Level Loader")]
        [Space(10.0f)]
        [SerializeField]
        private GameScores gameScores;

        [Space(15.0f)]
        [Header("Events")]
        [Space(5.0f)]
        public UnityEvent OnLevelInitialized;
        public UnityEvent OnLevelFailed;
        public UnityEvent OnLevelFinished;

        private GameConfiguration gameConfiguration;
#pragma warning restore 0649

        public enum LevelMode
        {
            Regular,
            BeatEmUp
        }

        private void Start()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration == null)
                Debug.LogWarning("Can't get Game Configuration!");

            InitializeCurrentLevel();
        }

        private void Update()
        {

        }

        private void InitializeCurrentLevel()
        {
            OnLevelInitialized.Invoke();
        }

        private void FinalizeLevel()
        {
            if (gameScores != null)
            {
                gameScores.AddCurrentCoins(100);
            }

            OnLevelFinished.Invoke();
        }

        private void OnDestroy()
        {
            OnLevelInitialized.RemoveAllListeners();
            OnLevelFailed.RemoveAllListeners();
            OnLevelFinished.RemoveAllListeners();
        }
    }
}