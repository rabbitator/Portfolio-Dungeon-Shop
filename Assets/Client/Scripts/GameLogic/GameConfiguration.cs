using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using DungeonShop.Enemies;
using DungeonShop.Player;
using DungeonShop.Visuals;

namespace DungeonShop.GameLogic
{
    [CreateAssetMenu(fileName = "GameConfiguration", menuName = "Game Configuration File", order = 1)]
    public class GameConfiguration : ScriptableObject
    {
        [Header("Joystick Settings")]
        [Range(0.0f, 1.0f)]
        public float joystickVisibility = 0.0f;
        public float joystickSensitivity = 1.0f;

        [Header("Player Settings")]
        [Space(10.0f)]
        public Styles style = Styles.Regular;
        public PlayerController.PlayerAttackType playerAttackType = PlayerController.PlayerAttackType.Always;
        public PlayerController.PlayerAttackWeapon playerAttackWeapon = PlayerController.PlayerAttackWeapon.Bow;
        public float playerProjectileSpeed = 10.0f;
        public float axeAngularSpeed = 360.0f;
        public float playerAttackDelay = 0.5f;
        [Range(0.0f, 1.0f)]
        public float criticalHitChance = 0.1f;
        public float playerMaxSpeed = 1.0f;
        public float playerMovementSmoothFactor = 5.0f;

        [Header("Level Assembler Settings")]
        [Space(20.0f)]
        [Range(0.0f, 1.0f)]
        public float bossRoomSpawnProbability = 0.2f;

        [Header("Level Up System")]
        [Space(20.0f)]
        [Range(0.0f, 1.0f)]
        public float levelUpPercent = 0.25f;

        [Header("Level Up: Capacity")]
        [Space(5.0f)]
        public int level1Capacity = 20;
        public int startPriceCapacity = 20;

        [Header("Level Up: Health")]
        [Space(5.0f)]
        public int level1Health = 100;
        public int startPriceHealth = 20;

        [Header("Level Up: Weapon")]
        [Space(5.0f)]
        public float level1Weapon = 10.0f;
        public int startPriceWeapon = 20;

        [Header("Camera Settings")]
        [Space(20.0f)]
        public float hitCameraShakingDuration = 0.2f;
        public float hitCameraAmplitude = 0.01f;
        public float hitCameraShakingSpeed = 50.0f;
        public float hitCameraCriticalAmplitudeMultiplier = 3.0f;

        [Header("Enemies Common Settings")]
        [Space(10.0f)]
        public float closeAttackDistance = 1.0f;
        public float farAttackDistance = 15.0f;
        public float ballSpeed = 20.0f;
        [Range(0.0f, 1.0f)]
        public float enemiesLevelUpHealthPercent = 0.1f;
        [Range(0.0f, 1.0f)]
        public float enemiesLevelUpAttackPercent = 0.1f;

        [Header("Enemy: Bee")]
        [Space(10.0f)]
        public Enemy.EnemyAttackType beeAttackType = Enemy.EnemyAttackType.Close;
        public float beeMaxSpeed = 5.0f;
        public float beeAttackDelay = 1.0f;
        public int beeMaxHealth = 100;
        public int beeAttackStrength = 10;

        [Header("Enemy: Mashroom")]
        // [Space(5.0f)]
        public Enemy.EnemyAttackType mashAttackType = Enemy.EnemyAttackType.Close;
        public float mashMaxSpeed = 5.0f;
        public float mashAttackDelay = 1.0f;
        public int mashMaxHealth = 100;
        public int mashAttackStrength = 10;

        [Header("Enemy: Spider")]
        // [Space(5.0f)]
        public Enemy.EnemyAttackType spiderAttackType = Enemy.EnemyAttackType.Close;
        public float spiderMaxSpeed = 5.0f;
        public float spiderAttackDelay = 1.0f;
        public int spiderMaxHealth = 100;
        public int spiderAttackStrength = 10;

        [Header("Enemy: Ghost")]
        // [Space(5.0f)]
        public Enemy.EnemyAttackType ghostAttackType = Enemy.EnemyAttackType.Close;
        public float ghostMaxSpeed = 5.0f;
        public float ghostAttackDelay = 1.0f;
        public int ghostMaxHealth = 100;
        public int ghostAttackStrength = 10;

        [Header("Enemy: Seed")]
        // [Space(5.0f)]
        public Enemy.EnemyAttackType seedAttackType = Enemy.EnemyAttackType.Close;
        public float seedMaxSpeed = 5.0f;
        public float seedAttackDelay = 1.0f;
        public int seedMaxHealth = 100;
        public int seedAttackStrength = 10;

        [Header("Resource Generators")]
        [Space(20.0f)]
        [Range(0.0f, 1.0f)]
        public float generatorsLevelUpPricePercent = 0.25f;
        public int generatorsLevel1Price = 20;
        public int generatorsStartProductionPerHour = 15;
        [Range(0.0f, 1.0f)]
        public float generatorsLevelUpProductionPercent = 0.15f;
        public int generatorsStartCapacity = 90;
        [Range(0.0f, 1.0f)]
        public float generatorsLevelUpCapacityPercent = 0.15f;

        [Header("Levels cycle")]
        [Space(20.0f)]
        public LevelsIteration[] iterations;

        private void OnValidate()
        {
            if (iterations != null)
            {
                for (int i = 0; i < iterations.Length; i++)
                {
                    if (iterations[i].levels == null)
                        continue;

                    for (int j = 0; j < iterations[i].levels.Length; j++)
                    {
                        if (!iterations[i].levels[j].scene.ToString().Contains("(UnityEngine.SceneAsset)"))
                        {
                            iterations[i].levels[j].scene = null;
                            iterations[i].levels[j].sceneName = "";
                        }
                        else
                        {
                            iterations[i].levels[j].sceneName = iterations[i].levels[j].scene.name;
                        }
                    }
                }
            }

            StyleSwitcher[] styleSwitchers = FindObjectsOfType<StyleSwitcher>();
            for (int i = 0; i < styleSwitchers.Length; i++)
            {
                styleSwitchers[i].ChangeStyle(style);
            }
            
            SkinSwitcher[] skinSwitchers = FindObjectsOfType<SkinSwitcher>();
            for (int i = 0; i < skinSwitchers.Length; i++)
            {
                skinSwitchers[i].ChangeStyle(style);
            }

            // Debug.Log($"Styles switchers: {styleSwitchers.Length}, skin switchers: {skinSwitchers.Length}");
        }
    }

    [Serializable]
    public class LevelsIteration
    {
        public bool loopForever = false;
        public Level[] levels;
    }

    [Serializable]
    public class Level
    {
        [Space(5.0f)]
        public UnityEngine.Object scene;
        [HideInInspector]
        public string sceneName;
    }
}