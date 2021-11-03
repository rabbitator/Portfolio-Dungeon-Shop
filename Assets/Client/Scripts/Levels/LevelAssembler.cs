using DungeonShop.GameLogic;
using DungeonShop.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.Levels
{
    public class LevelAssembler : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Element prefabs")]
        [SerializeField]
        private GameObject[] pool1Prefabs;
        [SerializeField]
        private GameObject[] pool2Prefabs;
        [SerializeField]
        private GameObject[] pool3Prefabs;
        [SerializeField]
        private GameObject[] pool4Prefabs;
        [Space(10.0f)]
        [SerializeField]
        private GameObject exitPrefab;

        [Header("Assembler Settings")]
        [Space(10.0f)]
        [SerializeField]
        private bool assembleOnEnable = true;
#pragma warning restore 0649

        [Header("Events")]
        [Space(20.0f)]
        public UnityEvent OnPlayerExitsDungeon;

        private Transform thisTransform;
        private int roomCount = 4;

        private void Awake()
        {
            thisTransform = transform;
        }

        private void OnEnable()
        {
            if (assembleOnEnable)
                GenerateDungeon();
        }

        private void GenerateDungeon()
        {
            GameConfiguration gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration == null)
                Debug.LogError($"{name} (LevelAssembler): Can't get Game Configuration!");

            LevelElementRoom lastRoom = null;

            CameraController cameraController = FindObjectOfType<CameraController>();

            Vector3 playerPosition = Vector3.zero;
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                playerPosition = player.transform.position;
            }

            void SetRoomCameraLimits(LevelElementRoom dungeonRoom)
            {
                cameraController.LimitLeftBottom = dungeonRoom.LimitLeftBottom;
                cameraController.LimitRightTop = dungeonRoom.LimitRightTop;
            }

            for (int i = 0; i < roomCount; i++)
            {
                void OpenNextRoom(LevelElementRegularRoom room) { room.Entrance.Open(); room.DeactivateFogOfWar(); }

                if (i == 0)
                {
                    Vector3 offset = playerPosition + Vector3.forward * 100.0f;
                    GameObject selection = pool1Prefabs[UnityEngine.Random.Range(0, pool1Prefabs.Length)];
                    LevelElementStartRoom startRoom = Instantiate(selection, offset, Quaternion.identity).GetComponent<LevelElementStartRoom>();

                    Vector3 correction = Vector3.Scale(startRoom.ElementTransform.InverseTransformPoint(startRoom.PlayerSpawnPoint.position), startRoom.ElementTransform.localScale);
                    startRoom.ElementTransform.position = playerPosition - correction;
                    startRoom.ElementTransform.SetParent(thisTransform);

                    startRoom.DeactivateFogOfWar();

                    if (cameraController != null)
                    {
                        startRoom.DungeonRoom.OnRoomEntered.AddListener(() => SetRoomCameraLimits(startRoom));
                    }

                    lastRoom = startRoom;
                }
                else
                if (i == 1)
                {
                    GameObject selection = pool2Prefabs[UnityEngine.Random.Range(0, pool2Prefabs.Length)];
                    LevelElementRegularRoom regularRoom = Instantiate(selection).GetComponent<LevelElementRegularRoom>();

                    Vector3 correction = Vector3.Scale(regularRoom.ElementTransform.InverseTransformPoint(regularRoom.StartPoint.position), regularRoom.ElementTransform.localScale);
                    regularRoom.ElementTransform.position = lastRoom.EndPoint.position - correction;
                    regularRoom.ElementTransform.SetParent(thisTransform);

                    regularRoom.ActivateFogOfWar();

                    if (cameraController != null)
                    {
                        regularRoom.DungeonRoom.OnRoomEntered.AddListener(() => SetRoomCameraLimits(regularRoom));
                    }

                    if (regularRoom.Entrance != null)
                    {
                        lastRoom.DungeonRoom.OnRoomPassed.AddListener(() => OpenNextRoom(regularRoom));
                        regularRoom.DungeonRoom.OnRoomEntered.AddListener(() => regularRoom.Entrance.Close());
                    }

                    lastRoom = regularRoom;
                }
                else
                if (i < roomCount - 1)
                {
                    GameObject selection = pool3Prefabs[UnityEngine.Random.Range(0, pool3Prefabs.Length)];
                    LevelElementRegularRoom regularRoom = Instantiate(selection).GetComponent<LevelElementRegularRoom>();

                    Vector3 correction = Vector3.Scale(regularRoom.ElementTransform.InverseTransformPoint(regularRoom.StartPoint.position), regularRoom.ElementTransform.localScale);
                    regularRoom.ElementTransform.position = lastRoom.EndPoint.position - correction;
                    regularRoom.ElementTransform.SetParent(thisTransform);

                    regularRoom.ActivateFogOfWar();

                    if (cameraController != null)
                    {
                        regularRoom.DungeonRoom.OnRoomEntered.AddListener(() => SetRoomCameraLimits(regularRoom));
                    }

                    if (regularRoom.Entrance != null)
                    {
                        lastRoom.DungeonRoom.OnRoomPassed.AddListener(() => OpenNextRoom(regularRoom));
                        regularRoom.DungeonRoom.OnRoomEntered.AddListener(() => regularRoom.Entrance.Close());
                    }

                    lastRoom = regularRoom;
                }
                else
                {
                    bool generateBossRoom = UnityEngine.Random.value <= gameConfiguration.bossRoomSpawnProbability;

                    GameObject selection = generateBossRoom ? pool4Prefabs[UnityEngine.Random.Range(0, pool4Prefabs.Length)] : exitPrefab;
                    LevelElementRegularRoom finalRoom = Instantiate(selection).GetComponent<LevelElementRegularRoom>();

                    Vector3 correction = Vector3.Scale(finalRoom.ElementTransform.InverseTransformPoint(finalRoom.StartPoint.position), finalRoom.ElementTransform.localScale);
                    finalRoom.ElementTransform.position = lastRoom.EndPoint.position - correction;
                    finalRoom.ElementTransform.SetParent(thisTransform);

                    if (generateBossRoom)
                    {
                        finalRoom.ActivateFogOfWar();

                        if (cameraController != null)
                        {
                            finalRoom.DungeonRoom.OnRoomEntered.AddListener(() => SetRoomCameraLimits(finalRoom));
                        }

                        if (finalRoom.Entrance != null)
                        {
                            lastRoom.DungeonRoom.OnRoomPassed.AddListener(() => OpenNextRoom(finalRoom));
                            finalRoom.DungeonRoom.OnRoomEntered.AddListener(() => finalRoom.Entrance.Close());
                        }

                        if (finalRoom.Exit != null)
                        {
                            finalRoom.DungeonRoom.OnRoomPassed.AddListener(() => finalRoom.Exit.Open());

                            finalRoom.Exit.PlayerEvents.PlayerEnteredTrigger.AddListener(() => OnPlayerExitsDungeon.Invoke());

                            LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
                            if (levelLoader != null)
                                finalRoom.Exit.PlayerEvents.PlayerEnteredTrigger.AddListener(levelLoader.LevelFinished);
                            else
                                Debug.LogWarning($"{name} (Level Assembler): Level loader wasn't founded!");

                            GameScores gameScores = FindObjectOfType<GameScores>();
                            if (gameScores != null)
                                finalRoom.Exit.PlayerEvents.PlayerEnteredTrigger.AddListener(gameScores.SaveLevelScore);
                            else
                                Debug.LogWarning($"{name} (Level Assembler): Level loader wasn't founded!");
                        }

                        lastRoom = finalRoom;
                    }
                    else
                    {
                        if (finalRoom.Entrance != null)
                        {
                            lastRoom.DungeonRoom.OnRoomPassed.AddListener(() => finalRoom.Entrance.Open());
                            finalRoom.Entrance.PlayerEvents.PlayerEnteredTrigger.AddListener(() => OnPlayerExitsDungeon.Invoke());

                            LevelLoader levelLoader = FindObjectOfType<LevelLoader>();
                            if (levelLoader != null)
                                finalRoom.Entrance.PlayerEvents.PlayerEnteredTrigger.AddListener(levelLoader.LevelFinished);
                            else
                                Debug.LogWarning($"{name} (Level Assembler): Level loader wasn't founded!");

                            GameScores gameScores = FindObjectOfType<GameScores>();
                            if (gameScores != null)
                                finalRoom.Entrance.PlayerEvents.PlayerEnteredTrigger.AddListener(gameScores.SaveLevelScore);
                            else
                                Debug.LogWarning($"{name} (Level Assembler): Level loader wasn't founded!");

                            lastRoom = finalRoom;
                        }
                    }
                }

                // StartCoroutine(Extensions.SkipFrames(50, () => lastRoom.EnemiesRoot.SetActive(true)));
                lastRoom.DungeonRoom.EnemiesRoot.SetActive(true);
            }
        }
    }
}
