using DungeonShop.Enemies;
using DungeonShop.Miscellaneous;
using DungeonShop.Player;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DungeonShop.GameLogic
{
    public class DungeonRoom : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private float enemiesActivationDelay = 0.0f;
        [SerializeField]
        private GameObject enemiesRoot;
#pragma warning restore 0649

        [Space(20.0f)]
        public UnityEvent OnRoomEntered;
        public UnityEvent OnRoomPassed;

        private List<Enemy> roomEnemies = new List<Enemy>();
        private PlayerController player;
        private int enemiesLevel = 1;
        private bool roomPassed = false;

        public GameObject EnemiesRoot { get => enemiesRoot; }
        public float EnemiesActivationDelay { get => enemiesActivationDelay; set => enemiesActivationDelay = value; }

/*
        public void AddEnemyToRoom(Enemy enemy)
        {
            if (enemy == null)
                return;

            enemy.SetEnemyLevel(enemiesLevel);
            roomEnemies.Add(enemy);
        }
*/

        private void Awake()
        {
            player = FindObjectOfType<PlayerController>();
            
            GameScores gameScores = FindObjectOfType<GameScores>();
            if(gameScores != null)
            {
                enemiesLevel = gameScores.GetPlayerLevelHealth() + gameScores.GetPlayerLevelWeapon();
            }
            else
            {
                Debug.LogError($"Can't get GameScores object on {name} (DungeonRoom)");
            }

            for (int i = 0; i < enemiesRoot.transform.childCount; i++)
            {
                Enemy enemy = enemiesRoot.transform.GetChild(i).GetComponent<Enemy>();
                if (enemy == null)
                    continue;

                enemy.SetEnemyLevel(enemiesLevel);
                roomEnemies.Add(enemy);
            }
        }

        private void Update()
        {
            if (roomEnemies != null && roomEnemies.Count != 0)
            {
                List<Enemy> filteredEnemies = roomEnemies.Where(x => x != null && x.isActiveAndEnabled).ToList();
                if (!roomPassed && (filteredEnemies.Count != 0 && filteredEnemies.All(x => x.Dead)))
                {
                    FinishRoom();
                    roomPassed = true;
                }
            }
        }

        private void ActivateEnemies()
        {
            if (roomEnemies != null && roomEnemies.Count != 0)
            {
                for (int i = 0; i < roomEnemies.Count; i++)
                {
                    roomEnemies[i].Attacking = true;
                }
            }
        }

        private void FinishRoom()
        {
            if (player != null)
                player.Attacking = false;

            OnRoomPassed.Invoke();
        }

        public void GotPlayer()
        {
            if (roomPassed)
                return;

            player = FindObjectOfType<PlayerController>();
            player.Attacking = true;

            if (enemiesActivationDelay <= 0.0f)
                ActivateEnemies();
            else
                StartCoroutine(Extensions.ScaledTimeDelay(enemiesActivationDelay, ActivateEnemies));

            OnRoomEntered.Invoke();
        }
    }
}