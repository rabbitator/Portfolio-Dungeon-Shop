using DungeonShop.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace DungeonShop.GameLogic
{
    public class LevelLoader : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private DelaySceneLoader[] tutorals;

        [Space(10.0f)]
        [Header("Current Level Number")]
        public Extensions.IntEvent SetLevelNumber;

        [Header("Debug")]
        [Space(20.0f)]
        [SerializeField]
        private bool skipTutorials = false;

        private List<DelaySceneLoader> levels = new List<DelaySceneLoader>();
        private GameConfiguration gameConfiguration;
        private Transform thisTransform;

        public static readonly string levelNumberPropertyName = "LevelNumber";
#pragma warning restore 0649

        [Serializable]
        public class ResourceEvent : UnityEvent<ResourceData> { }

        public class ResourceData
        {
            public ResourceType resourceType;
            public int resourceCount;

            public enum ResourceType
            {
                Wood,
                Crystal,
                Coin
            }
        }

        public class SomeData
        {
            public UnityEngine.Object something;
        }

        private void Awake()
        {
            thisTransform = transform;
        }

        private void OnEnable()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration != null)
            {
                if (gameConfiguration.iterations != null)
                {
                    for (int i = 0; i < gameConfiguration.iterations.Length; i++)
                    {
                        if (gameConfiguration.iterations[i].levels == null)
                            continue;

                        for (int j = 0; j < gameConfiguration.iterations[i].levels.Length; j++)
                        {
                            string sceneName = gameConfiguration.iterations[i].levels[j].sceneName;

                            DelaySceneLoader sceneLoader = null;
                            for (int c = 0; c < thisTransform.childCount; c++)
                            {
                                DelaySceneLoader childLoader = thisTransform.GetChild(c).GetComponent<DelaySceneLoader>();
                                if (childLoader != null && childLoader.GetSceneName() == sceneName)
                                {
                                    sceneLoader = childLoader;
                                    break;
                                }
                            }

                            if (sceneLoader == null)
                            {
                                GameObject loaderGO = new GameObject($"{sceneName} loader");
                                loaderGO.transform.parent = thisTransform;
                                loaderGO.transform.localPosition = Vector3.zero;
                                sceneLoader = loaderGO.AddComponent<DelaySceneLoader>();
                                sceneLoader.SetScene(gameConfiguration.iterations[i].levels[j].sceneName);
                                sceneLoader.enabled = false;
                            }

                            // Add founded or created Loader to levels array
                            levels.Add(sceneLoader);
                        }
                    }
                }
            }
            else
                Debug.LogError("Can't get levels data from Game configuration!");

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private int GetCurrentIterationIndex()
        {
            if (gameConfiguration == null)
                return -1;

            int actualNumber = GetActualLevelIndex();
            if (actualNumber < 0)
            {
                Debug.LogError("Can't get iteration index: got incorrect level number!");

                return -1;
            }

            int passedLevels = 0;
            for (int i = 0; i < gameConfiguration.iterations.Length; i++)
            {
                if (gameConfiguration.iterations[i].levels == null)
                    continue;

                int currentIterationScenesCount = gameConfiguration.iterations[i].levels.Length;
                if (actualNumber < passedLevels + currentIterationScenesCount)
                    return i;

                passedLevels += currentIterationScenesCount;
            }

            return gameConfiguration.iterations.Length - 1;
        }

        public SomeData GetSomeLevelData()
        {
            int iterationIndex = 0;
            int currentLevelIndex = GetActualLevelIndex();

            if (currentLevelIndex < 0)
            {
                Debug.LogError("Can't get level item data: got incorrect level number!");

                return null;
            }

            // print($"Start level index: {currentLevelIndex}");

            for (int i = 0; i < gameConfiguration.iterations.Length; i++)
            {
                if (currentLevelIndex < gameConfiguration.iterations[i].levels.Length)
                    break;
                else
                {
                    iterationIndex++;
                    currentLevelIndex -= gameConfiguration.iterations[i].levels.Length;
                }
            }

            // print($"Iteration index: {iterationIndex}, level index: {currentLevelIndex}");

            SomeData itemData = new SomeData()
            {
                // itemSprite = gameConfiguration.iterations[iterationIndex].levels[currentLevelIndex].openingItem,
                // openFrom = gameConfiguration.iterations[iterationIndex].levels[currentLevelIndex].openFrom,
                // openTo = gameConfiguration.iterations[iterationIndex].levels[currentLevelIndex].openTo
            };

            if (itemData.something == null)
                return null;

            return itemData;
        }

        private int GetActualLevelIndex()
        {
            if (gameConfiguration == null)
            {
                Debug.LogError($"{name}: No Game Configuration Object!");

                return -1;
            }

            // Counting starts with 1, value stores actual
            // level number which then converts to index
            int levelNumber = PlayerPrefs.GetInt(levelNumberPropertyName, 1);

            int infiniteIterationIndex = -1;
            int preInfiniteLevelsCount = 0;
            for (int i = 0; i < gameConfiguration.iterations.Length; i++)
            {
                if (!gameConfiguration.iterations[i].loopForever)
                {
                    preInfiniteLevelsCount += gameConfiguration.iterations[i].levels.Length;
                }
                else
                {
                    infiniteIterationIndex = i;
                    break;
                }
            }

            int actualNumber;
            if (infiniteIterationIndex < 0 || levelNumber - 1 < preInfiniteLevelsCount)
            {
                // Pre-infinite levels zone or all iterations are finite
                actualNumber = (levelNumber - 1) % levels.Count;
            }
            else
            {
                // Infinite levels zone
                actualNumber = (levelNumber - 1 - preInfiniteLevelsCount) % gameConfiguration.iterations[infiniteIterationIndex].levels.Length + preInfiniteLevelsCount;
            }

            return actualNumber;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            string name = SceneManager.GetActiveScene().name;
            if (levels.FirstOrDefault(x => x.GetSceneName() == name) != null || tutorals.Any(x => x.GetSceneName() == name))
            {
                SetLevelNumber.Invoke(PlayerPrefs.GetInt(levelNumberPropertyName, 1));
            }
        }

        public void RunCurrentLevel()
        {
            int actualNumber = GetActualLevelIndex();
            if (actualNumber < 0)
            {
                Debug.LogError("Can't run current level: got incorrect level number!");

                return;
            }

            levels[actualNumber].Load();
        }

        public void RunTutorialOrLevel()
        {
            if (skipTutorials || PlayerPrefs.GetInt($"Tutorial {tutorals.Length}", -1) > 0)
                RunCurrentLevel();
            else
            if (!RunTutorial())
                RunCurrentLevel();
        }

        public int GetTotalLevelsCount()
        {
            if (levels == null)
                return 0;

            return levels.Count();
        }

        public int GetIterationLevelsCount()
        {
            int levelNumber = PlayerPrefs.GetInt(levelNumberPropertyName, 1);
            int actualNumber = (levelNumber - 1) % levels.Count;

            if (gameConfiguration == null)
            {
                Debug.LogError("Can't get Game Configuration!");

                return -1;
            }

            if (gameConfiguration.iterations.Length == 0)
            {
                return 0;
            }

            int passedLevels = 0;
            for (int i = 0; i < gameConfiguration.iterations.Length; i++)
            {
                if (gameConfiguration.iterations[i].levels == null)
                    continue;

                int currentIterationScenesCount = gameConfiguration.iterations[i].levels.Length;
                if (actualNumber < passedLevels + currentIterationScenesCount)
                    return gameConfiguration.iterations[i].levels.Length;

                passedLevels += currentIterationScenesCount;
            }

            return -1;
        }

        public int GetCurrentLevelIndex()
        {
            int levelNumber = PlayerPrefs.GetInt(levelNumberPropertyName, 1);
            int actualNumber = (levelNumber - 1) % levels.Count;

            if (gameConfiguration == null)
            {
                Debug.LogError("Can't get Game Configuration!");

                return -1;
            }

            if (gameConfiguration.iterations.Length == 0)
            {
                return 0;
            }

            int passedLevels = 0;
            for (int i = 0; i < gameConfiguration.iterations.Length; i++)
            {
                if (gameConfiguration.iterations[i].levels == null)
                    continue;

                int currentIterationScenesCount = gameConfiguration.iterations[i].levels.Length;
                if (actualNumber < passedLevels + currentIterationScenesCount)
                    return actualNumber - passedLevels;

                passedLevels += currentIterationScenesCount;
            }

            return levels.Count - 1;
        }

        public int GetCurrentLevelNumber()
        {
            return PlayerPrefs.GetInt(levelNumberPropertyName, 1);
        }

        public bool IsLevel(string sceneName)
        {
            return levels.Any(x => x.GetSceneName() == sceneName);
        }

        public void LevelFinished()
        {
            PlayerPrefs.SetInt(levelNumberPropertyName, PlayerPrefs.GetInt(levelNumberPropertyName, 1) + 1);
        }

        public void FinalizeTutorial(int number)
        {
            PlayerPrefs.SetInt($"Tutorial {number}", 1);
        }

        public bool RunTutorial()
        {
            for (int i = 0; i < tutorals.Length; i++)
            {
                if (tutorals[i] != null && PlayerPrefs.GetInt($"Tutorial {i + 1}", -1) < 0)
                {
                    tutorals[i].Load();

                    return true;
                }
            }

            return false;
        }

        public void Quit()
        {
            Application.Quit();
        }
    }
}