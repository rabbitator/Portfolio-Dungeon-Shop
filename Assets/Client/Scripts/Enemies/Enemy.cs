using System.Collections.Generic;
using UnityEngine;
using DungeonShop.UI;
using DungeonShop.GameLogic;
using DungeonShop.Player;
using DungeonShop.Miscellaneous;

namespace DungeonShop.Enemies
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    public class Enemy : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        private EnemyType enemyType = EnemyType.Bee;
        [SerializeField]
        private GameObject healthBarPrefab;
        [SerializeField]
        private GameObject enemyArea;
        [SerializeField]
        private bool attacking = true;

        [Header("FX")]
        [Space(10.0f)]
        [SerializeField]
        private ParticleSystem damageFX;
        [SerializeField]
        private ParticleSystem damage2xFX;
#pragma warning disable 0649

        private GameConfiguration gameConfiguration;
        private EnemyAttackType attackType = EnemyAttackType.Close;
        private Canvas activeCanvas;
        private HealthBar healthBar;
        private RectTransform healthBarRect;
        private Camera activeCamera;
        private CharacterController characterController;
        private Transform thisTransform;
        private Animator thisAnimator;
        private int maxHealth = 100;
        private int currentHealth;
        private int attackStrength = 10;
        private PlayerController player;
        private float movementSmoothFactor = 5.0f;
        private float closeDistance = 3.0f;
        private float farDistance = 15.0f;
        private float maxSpeed = 3.0f;
        private float accelerationMultiplier = 0.0f;
        private float ballSpeed = 20.0f;
        private float lastAttackTime;
        private float attackDelay = 2.0f;
        private float hitRadius = 2.0f;
        private int level = 1;
        private bool dead = false;

        private static List<Enemy> enemies;

        public bool Dead { get => dead; }
        public bool Attacking { get => attacking; set => attacking = value; }

        public enum EnemyType
        {
            Bee,
            Mashroom,
            Spider,
            Ghost,
            Seed
        }

        public enum EnemyAttackType
        {
            Close,
            Far
        }

        private void Awake()
        {
            if (enemies == null)
                enemies = new List<Enemy>();

            enemies.Add(this);

            thisTransform = transform;
            thisAnimator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration != null)
            {
                ApplySettings();
            }
            else
            {
                Debug.LogError("Can't get Game Configuration!");
            }

            currentHealth = maxHealth;
            lastAttackTime = attackDelay * Random.value;

            activeCanvas = FindObjectOfType<Canvas>();
            if(activeCanvas == null)
                Debug.LogError($"{name}: Can't get Canvas!");

            activeCamera = Camera.main;
            if (activeCamera == null)
                Debug.LogError($"{name}: Can't get Camera!");

            healthBar = Instantiate(healthBarPrefab).GetComponent<HealthBar>();
            if (healthBar == null)
            {
                Debug.LogError($"{name}: Can't get HealthBar!");
            }
            else
            {
                healthBarRect = healthBar.transform as RectTransform;
                healthBarRect.SetParent(activeCanvas.transform, false);
                healthBarRect.SetSiblingIndex(0);
                healthBar.MaxHealth = maxHealth;
                healthBar.NormalizedValue = 1.0f;
            }

            player = FindObjectOfType<PlayerController>();
            enemyArea.SetActive(false);
        }

        private void OnDestroy()
        {
            Destroy(healthBar);
            enemies.Remove(this);
        }

        private void Update()
        {
            if (!dead && attacking)
                DeltaMoveToPlayer();

            float distanceToPlayer = Vector3.Distance(thisTransform.position, player.transform.position);

            if (attacking && Time.time - lastAttackTime > attackDelay)
            {
                float attackDistance = -1.0f;
                switch (attackType)
                {
                    case EnemyAttackType.Close:
                        attackDistance = closeDistance;
                        if (distanceToPlayer < attackDistance)
                            Attack();
                        break;
                    case EnemyAttackType.Far:
                        attackDistance = farDistance;
                        // if (distanceToPlayer < attackDistance)
                        ThrowBall();
                        break;
                    default:
                        break;
                }

                lastAttackTime = Time.time;
            }

            healthBarRect.position = activeCamera.WorldToScreenPoint(thisTransform.position);

            ApplySettings();
        }

        private void DeltaMoveToPlayer()
        {
            // Orient character with moving direction
            Vector3 direction = Vector3.zero;
            if (player != null && !player.Dead)
                direction = Vector3.ProjectOnPlane((player.transform.position - thisTransform.position), Vector3.up).normalized;

            Quaternion orientation = direction.magnitude > 1e-3f ? Quaternion.LookRotation(direction, Vector3.up) : thisTransform.rotation;
            thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, orientation, movementSmoothFactor * Time.deltaTime);

            bool playerOk = player != null && !player.Dead;
            float distanceToPlayer = -1.0f;
            if (playerOk)
                distanceToPlayer = Vector3.Distance(thisTransform.position, player.transform.position);

            switch (attackType)
            {
                case EnemyAttackType.Close:
                    accelerationMultiplier += playerOk && distanceToPlayer > closeDistance ? 2.0f * Time.deltaTime : -2.0f * Time.deltaTime;
                    break;
                case EnemyAttackType.Far:
                    accelerationMultiplier += playerOk && distanceToPlayer > farDistance ? 2.0f * Time.deltaTime : -2.0f * Time.deltaTime;
                    break;
                default:
                    break;
            }

            accelerationMultiplier = Mathf.Clamp01(accelerationMultiplier);

            Vector3 smoothMovementFactor = direction * maxSpeed * accelerationMultiplier;

            // Move character
            Vector3 deltaMove = (smoothMovementFactor + Vector3.down * 10.0f) * Time.deltaTime;
            characterController.Move(deltaMove);

            // Play move animation
            thisAnimator.SetFloat("Speed", accelerationMultiplier);
        }

        public void Attack()
        {
            if (player == null || player.Dead || dead)
                return;

            if (Vector3.Distance(player.transform.position, thisTransform.position) - closeDistance > hitRadius)
                return;

            StartCoroutine(Extensions.SkipFrames(1, () => thisAnimator.SetBool("Attack", true)));
            StartCoroutine(Extensions.SkipFrames(2, () => thisAnimator.SetBool("Attack", false)));
            StartCoroutine(Extensions.ScaledTimeDelay(0.5f, () => player.TakeDamage(attackStrength, false)));
        }

        public void ThrowBall()
        {
            if (player == null || player.Dead || dead)
                return;

            void CreateBallAndShoot()
            {
                GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Transform ballTransform = ball.transform;
                Destroy(ball.GetComponent<Collider>());
                ballTransform.localScale = Vector3.one * 0.5f;

                ballTransform.position = thisTransform.position + Vector3.up;
                Vector3 destination = player.transform.position + Vector3.up;
                Vector3 direction = (player.transform.position - ballTransform.position).normalized;

                float distance = Vector3.Distance(thisTransform.position, destination);
                float flyingTime = distance / ballSpeed;

                Ray ray = new Ray(ballTransform.position, direction);
                if (Physics.Raycast(ray, out RaycastHit hit, Vector3.Distance(ballTransform.position, player.transform.position)))
                {
                    PlayerController hitObjectPlayer = hit.collider.GetComponent<PlayerController>();
                    if (hit.collider != null && (hitObjectPlayer == null))
                    {
                        destination = hit.point;
                        distance = Vector3.Distance(thisTransform.position, destination);
                        flyingTime = distance / ballSpeed;

                        if (hitObjectPlayer == null)
                        {
                            ParticleSystem hitFX = Instantiate(damageFX.gameObject).GetComponent<ParticleSystem>();
                            hitFX.transform.position = hit.point;
                            StartCoroutine(Extensions.ScaledTimeDelay(flyingTime, () => hitFX.Play()));
                            StartCoroutine(Extensions.ScaledTimeDelay(flyingTime + 1.0f, () => Destroy(hitFX.gameObject)));
                        }
                    }
                }

                void BallFly(float x) => ballTransform.position = Vector3.Lerp(thisTransform.position, destination, x);
                StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, flyingTime, BallFly, () => { HitPlayer(ballTransform); Destroy(ball); }));

                // SHOOT DEBUG
                StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 1.0f, (x) => Debug.DrawLine(thisTransform.position, destination, Color.red), null));
            }

            void HitPlayer(Transform ball)
            {
                if(Vector3.Distance(player.transform.position, ball.position) < hitRadius)
                {
                    player.TakeDamage(attackStrength, false);
                }
            }

            StartCoroutine(Extensions.SkipFrames(1, () => thisAnimator.SetBool("Attack", true)));
            StartCoroutine(Extensions.SkipFrames(2, () => thisAnimator.SetBool("Attack", false)));
            StartCoroutine(Extensions.ScaledTimeDelay(0.3f, CreateBallAndShoot));
        }

        public void TakeDamage(int value, bool critical)
        {
            if (dead)
                return;

            currentHealth -= value;

            if (healthBar != null)
            {
                healthBar.NormalizedValue = (float)currentHealth / maxHealth;

                if (critical)
                    healthBar.SetSubstractionTextColor(Color.red);
                else
                    healthBar.SetSubstractionTextColor(Color.white);
            }

            if (currentHealth > 0)
            {
                thisAnimator.SetBool("Get Hit", true);
                StartCoroutine(Extensions.SkipFrames(1, () => thisAnimator.SetBool("Get Hit", false)));
            }
            else
            {
                Die();
            }

            if (critical)
            {
                if (damage2xFX != null)
                    damage2xFX.Play();
            }
            else
            {
                if (damageFX != null)
                    damageFX.Play();
            }
        }

        public void EnableArea(bool condition)
        {
            for (int i = 0; i < enemies.Count; i++) enemies[i].enemyArea.SetActive(false);
            enemyArea.SetActive(condition);
        }

        public void SetEnemyLevel(int level)
        {
            this.level = level;

            ApplySettings();
            currentHealth = maxHealth;
            ResetHealthBar();
        }

        private void Die()
        {
            thisAnimator.SetBool("Death", true);
            currentHealth = 0;
            healthBar.gameObject.SetActive(false);
            enemyArea.SetActive(false);
            dead = true;

            Vector3 startPosition = thisTransform.position;
            Vector3 targetPosition = startPosition + Vector3.down * 2.0f;
            void DiveUnderground(float t) => thisTransform.position = Vector3.Lerp(startPosition, targetPosition, t);
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.5f, 3.0f, DiveUnderground, () => Destroy(gameObject)));
        }

        private void ResetHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.MaxHealth = maxHealth;
                healthBar.NormalizedValue = currentHealth / maxHealth;
            }
        }

        private void ApplySettings()
        {
            if(gameConfiguration != null)
            {
                closeDistance = gameConfiguration.closeAttackDistance;
                farDistance = gameConfiguration.farAttackDistance;
                ballSpeed = gameConfiguration.ballSpeed;

                float maxHealth;
                float attackStrength;

                switch (enemyType)
                {
                    case EnemyType.Bee:
                        attackType = gameConfiguration.beeAttackType;
                        maxSpeed = gameConfiguration.beeMaxSpeed;
                        maxHealth = gameConfiguration.beeMaxHealth;
                        attackDelay = gameConfiguration.beeAttackDelay;
                        attackStrength = gameConfiguration.beeAttackStrength;
                        break;
                    case EnemyType.Mashroom:
                        attackType = gameConfiguration.mashAttackType;
                        maxSpeed = gameConfiguration.mashMaxSpeed;
                        maxHealth = gameConfiguration.mashMaxHealth;
                        attackDelay = gameConfiguration.mashAttackDelay;
                        attackStrength = gameConfiguration.mashAttackStrength;
                        break;
                    case EnemyType.Spider:
                        attackType = gameConfiguration.spiderAttackType;
                        maxSpeed = gameConfiguration.spiderMaxSpeed;
                        maxHealth = gameConfiguration.spiderMaxHealth;
                        attackDelay = gameConfiguration.spiderAttackDelay;
                        attackStrength = gameConfiguration.spiderAttackStrength;
                        break;
                    case EnemyType.Ghost:
                        attackType = gameConfiguration.ghostAttackType;
                        maxSpeed = gameConfiguration.ghostMaxSpeed;
                        maxHealth = gameConfiguration.ghostMaxHealth;
                        attackDelay = gameConfiguration.ghostAttackDelay;
                        attackStrength = gameConfiguration.ghostAttackStrength;
                        break;
                    case EnemyType.Seed:
                        attackType = gameConfiguration.seedAttackType;
                        maxSpeed = gameConfiguration.seedMaxSpeed;
                        maxHealth = gameConfiguration.seedMaxHealth;
                        attackDelay = gameConfiguration.seedAttackDelay;
                        attackStrength = gameConfiguration.seedAttackStrength;
                        break;
                    default:
                        attackType = gameConfiguration.beeAttackType;
                        maxSpeed = gameConfiguration.beeMaxSpeed;
                        maxHealth = gameConfiguration.beeMaxHealth;
                        attackDelay = gameConfiguration.beeAttackDelay;
                        attackStrength = gameConfiguration.beeAttackStrength;
                        break;
                }

                for (int i = 0; i < level - 1; i++)
                    maxHealth += maxHealth * gameConfiguration.enemiesLevelUpHealthPercent;

                if (this.maxHealth != (int)maxHealth)
                {
                    this.maxHealth = (int)maxHealth;
                    ResetHealthBar();
                }

                for (int i = 0; i < level - 1; i++)
                    attackStrength += attackStrength * gameConfiguration.enemiesLevelUpAttackPercent;

                if (this.attackStrength != (int)attackStrength)
                {
                    this.attackStrength = (int)attackStrength;
                }
            }
        }
    }
}
