using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using DungeonShop.GameResources;
using DungeonShop.GameLogic;
using UnityEngine.Events;
using UnityEngine.UI;
using DungeonShop.Enemies;
using DungeonShop.UI;
using System;
using DungeonShop.Miscellaneous;

namespace DungeonShop.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Main Settings")]
        [SerializeField]
        private Animator citizenAnimator;
        [SerializeField]
        private Animator stickmanAnimator;

        [Header("Weapon Settings")]
        [Space(10.0f)]
        [SerializeField]
        private GameObject bow;
        [SerializeField]
        private GameObject arrow;
        [SerializeField]
        private GameObject axe;
        [SerializeField]
        private Transform projectileSpawnPoint;

        [Header("Tools Settings")]
        [Space(10.0f)]
        [SerializeField]
        private GameObject toolAxe;
        [SerializeField]
        private GameObject toolPickaxe;

        [Header("Bag Settings")]
        [Space(10.0f)]
        [SerializeField]
        private BagController bag;

        [Header("UI Settings")]
        [Space(10.0f)]
        [SerializeField]
        private Joystick joystick;
        [SerializeField]
        private HealthBar healthBar;
        [SerializeField]
        private bool showHealthBar = true;
        [SerializeField]
        private GameObject iconLootWood;
        [SerializeField]
        private GameObject iconLootCrystal;
        [SerializeField]
        private GameObject iconLootCoin;
        [SerializeField]
        private RectTransform fullBagNotificationHolder;
        [SerializeField]
        private Text fullBagNotification;
        [SerializeField]
        private Transform resIntakePoint;

        [Header("FX")]
        [Space(10.0f)]
        [SerializeField]
        private ParticleSystem damageFX;
        [SerializeField]
        private ParticleSystem damage2xFX;

        [Header("Events")]
        [Space(15.0f)]
        public Extensions.IntEvent OnWoodTaken;
        public Extensions.IntEvent OnCrystalTaken;
        public Extensions.IntEvent OnCoinsTaken;
        public UnityEvent OnFockenDeath;
#pragma warning restore 0649

        private GameConfiguration gameConfiguration;
        private Animator[] animators;
        private Transform thisTransform;
        private PlayerType playerType = PlayerType.Citizen;
        private PlayerAttackType attackType = PlayerAttackType.Always;
        private PlayerAttackWeapon attackWeapon = PlayerAttackWeapon.Bow;
        private CharacterController characterController;
        private float lastResourceHitTime;
        private float resourceHitDelay = 1.0f;
        private float maxSpeed = 1.0f;
        private float movementSmoothFactor = 5.0f;
        private float attackDelay = 1.0f;
        private float arrowSpeed = 10.0f;
        private float lastAttackTime;
        private float hitRadius = 2.0f;
        private float joystickSensitivity = 1.0f;
        private float resourcesRayLength = 2.0f;
        private float criticalHitChance = 0.05f;
        private float axeAngularSpeed = 360.0f;
        private float animationMoveSpeed = 0.0f;
        private int maxHealth = 100;
        private int currentHealth;
        private int attackStrength = 20;
        private bool hideHandProjectile = false;
        private RectTransform healtBarRect;
        private Camera activeCamera;
        private Enemy currentEnemy;

        public bool Dead { get; private set; } = false;
        public bool Attacking { get; set; } = false;
        public bool ShowHealthBar { get => showHealthBar; set => showHealthBar = value; }
        public Transform ResourceIntakePoint { get => resIntakePoint; }

        public enum PlayerType
        {
            Citizen,
            Stickman
        }

        public enum PlayerAttackType
        {
            OnStay,
            Always
        }

        public enum PlayerAttackWeapon
        {
            Axe,
            Bow
        }

        private void Awake()
        {
            thisTransform = transform;
            animators = new Animator[] { citizenAnimator, stickmanAnimator };
            characterController = GetComponent<CharacterController>();
            healtBarRect = healthBar.transform as RectTransform;
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

            activeCamera = Camera.main;

            SetMaxHealth(maxHealth);
        }

        private void Update()
        {
            joystick.Sensitivity = joystickSensitivity;

            if (!Dead)
            {
                DeltaMove();

                // Enemies
                Enemy[] enemies = FindObjectsOfType<Enemy>();
                if (Attacking && enemies.Length > 0)
                {
                    switch (attackWeapon)
                    {
                        case PlayerAttackWeapon.Axe:
                            bow.SetActive(false);
                            axe.SetActive(!hideHandProjectile);
                            break;
                        case PlayerAttackWeapon.Bow:
                            axe.SetActive(false);
                            bow.SetActive(true);
                            arrow.SetActive(!hideHandProjectile);
                            break;
                    }

                    Enemy closest = null;
                    for (int i = 0; i < enemies.Length; i++)
                    {
                        if (enemies[i].Dead)
                            continue;

                        if (closest != null)
                        {
                            bool closer = Vector3.Distance(thisTransform.position, enemies[i].transform.position) < Vector3.Distance(thisTransform.position, closest.transform.position);
                            if (closer)
                                closest = enemies[i];
                        }
                        else
                            closest = enemies[i];
                    }

                    bool canAttack = attackType == PlayerAttackType.OnStay && joystick.Direction.magnitude != 0.0f ? false : true;
                    canAttack = Attacking ? canAttack : false;
                    if (closest != null && Time.time - lastAttackTime > attackDelay && canAttack)
                    {
                        currentEnemy = closest;
                        Attack();

                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    bow.SetActive(false);
                    axe.SetActive(false);
                }

                if(currentEnemy != null && !currentEnemy.Dead)
                {
                    Vector3 direction = (currentEnemy.transform.position - thisTransform.position);
                    direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
                    Quaternion orientationToEnemy = Quaternion.LookRotation(direction, Vector3.up);
                    thisTransform.rotation = orientationToEnemy;
                }

                healthBar.gameObject.SetActive(showHealthBar);
            }
            else
            {
                SetAnimatorsFloat("Speed", 0.0f);
            }

            // animationMoveSpeed += Mathf.Sign(joystick.Direction.magnitude - animationMoveSpeed) * 2.0f * Time.deltaTime;
            // animationMoveSpeed = Mathf.Clamp01(animationMoveSpeed);
            animationMoveSpeed = joystick.Direction.magnitude;

            ApplySettings();
        }

        private void FixedUpdate()
        {
            Ray ray = new Ray(thisTransform.position + Vector3.up * 0.5f, thisTransform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, resourcesRayLength))
            {
                GameResource res = hit.collider.GetComponent<GameResource>();

                if (!Attacking && res != null && (res.GetType() == typeof(GameResourceRock) && bag.FreeCrystalSpace <= 0) ||
                    !Attacking && res != null && (res.GetType() == typeof(GameResourceWood) && bag.FreeWoodSpace <= 0))
                {
                    IncreaseNotificationOpacity(fullBagNotification, 2.0f * Time.fixedDeltaTime);
                }

                if (!Attacking && res != null && joystick.Direction.magnitude == 0.0f && Time.time - lastResourceHitTime > resourceHitDelay)
                {
                    if (res.GetType() == typeof(GameResourceRock))
                    {
                        int resourceCount = res.ResourceCount;
                        int bagRemainder = bag.FreeCrystalSpace - resourceCount;

                        if (bagRemainder < 0)
                            resourceCount += bagRemainder;

                        if (resourceCount > 0)
                            HitWithPickaxe();
                    }
                    else
                    if (res.GetType() == typeof(GameResourceWood))
                    {
                        int resourceCount = res.ResourceCount;
                        int bagRemainder = bag.FreeWoodSpace - resourceCount;

                        if (bagRemainder < 0)
                            resourceCount += bagRemainder;

                        if (resourceCount > 0)
                            HitWithAxe();
                    }
                    else
                    if (res.GetType() == typeof(GameResourceChest))
                    {
                        List<Transform> debris = new List<Transform>();
                        int takenResources = res.GetResource(ref debris);
                        StartCoroutine(Extensions.ScaledTimeDelay(2.25f, () => SpawnIcon(iconLootCoin, res.transform, takenResources, null)));

                        if (takenResources != 0)
                            OnCoinsTaken.Invoke(takenResources);
                    }

                    lastResourceHitTime = Time.time;
                }

                Debug.DrawRay(ray.origin, ray.direction * resourcesRayLength, Color.green);
            }
            else
            {
                DecreaseNotificationOpacity(fullBagNotification, 2.0f * Time.fixedDeltaTime);

                Debug.DrawRay(ray.origin, ray.direction * resourcesRayLength, Color.black);
            }
        }

        public void HitResource(PlayerType playerType)
        {
            if (playerType != this.playerType)
                return;

            Ray ray = new Ray(thisTransform.position + Vector3.up * 0.5f, thisTransform.forward);

            RaycastHit[] hits = Physics.RaycastAll(ray, resourcesRayLength);
            RaycastHit hit = hits.FirstOrDefault(x => x.collider != null && x.collider.GetComponent<GameResource>() != null);
            if (hit.collider != null)
            {
                GameResource res = hit.collider.GetComponent<GameResource>();

                if (hit.collider == null || res == null)
                    return;

                List<Transform> debris = new List<Transform>();

                if (res.GetType() == typeof(GameResourceRock))
                {
                    int takenResources = res.GetResource(ref debris);
                    int bagRemainder = bag.FreeCrystalSpace - takenResources;

                    if (bagRemainder < 0)
                        takenResources += bagRemainder;

                    if (takenResources > 0)
                    {
                        StartCoroutine(Extensions.ScaledTimeDelay(0.5f, () =>
                        {
                            SpawnIcon(iconLootCrystal, resIntakePoint, takenResources, null);
                        }));

                        OnCrystalTaken.Invoke(takenResources);
                    }
                }
                else
                if (res.GetType() == typeof(GameResourceWood))
                {
                    int takenResources = res.GetResource(ref debris);
                    int bagRemainder = bag.FreeWoodSpace - takenResources;

                    if (bagRemainder < 0)
                        takenResources += bagRemainder;

                    if (takenResources > 0)
                    {
                        StartCoroutine(Extensions.ScaledTimeDelay(0.5f, () =>
                        {
                            SpawnIcon(iconLootWood, resIntakePoint, takenResources, null);
                        }));

                        OnWoodTaken.Invoke(takenResources);
                    }
                }
            }
        }

        private void SpawnIcon(GameObject prefab, Transform resourceTransform, int resourcesCount, Action endCallback)
        {
            if (resourcesCount != 0)
            {
                RectTransform spawnedIcon = SpawnUILootIcon(prefab, resourcesCount);
                if (spawnedIcon != null)
                {
                    DisposableUI disposableUI = spawnedIcon.GetComponent<DisposableUI>();
                    if (disposableUI != null)
                    {
                        void PlaceUI()
                        {
                            Vector3 position = activeCamera.WorldToScreenPoint(resIntakePoint.position);
                            disposableUI.SetInitialData(position, Vector3.zero, Vector3.one);
                        }

                        StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, disposableUI.Duration, (t) => PlaceUI(), endCallback));
                    }
                    else
                    {
                        void PlaceUI(RectTransform _transform, CanvasGroup _canvasGroup, Vector3 outTakePoint, float t)
                        {
                            Vector3 startPosition = activeCamera.WorldToScreenPoint(outTakePoint);
                            Vector3 targetPosition = activeCamera.WorldToScreenPoint(resIntakePoint.position);
                            Vector3 position = Vector3.Lerp(startPosition, targetPosition, Mathf.Pow(t, 4.0f));
                            _transform.position = position;

                            if (_canvasGroup != null)
                                _canvasGroup.alpha = 1.0f - Mathf.Pow(2.0f * t - 1.0f, 4.0f);
                        }

                        CanvasGroup canvasGroup = spawnedIcon.GetComponent<CanvasGroup>();

                        Vector3 start = resourceTransform.position;
                        StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 0.8f, (t) => PlaceUI(spawnedIcon, canvasGroup, start, t), endCallback));
                    }
                }
            }
        }

        private void IncreaseNotificationOpacity(Text notification, float value)
        {
            Color startColor = notification.color;
            notification.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(startColor.a + value));
        }

        private void DecreaseNotificationOpacity(Text notification, float value)
        {
            Color startColor = notification.color;
            notification.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(startColor.a - value));
        }

        private void LateUpdate()
        {
            Vector3 screenPosition = activeCamera.WorldToScreenPoint(thisTransform.position);

            // Health Bar
            healtBarRect.position = screenPosition;
            // Bag notification
            fullBagNotificationHolder.position = screenPosition;
        }

        private RectTransform SpawnUILootIcon(GameObject prefab, int count)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No focken Canvas!");

                return null;
            }

            GameObject lootUIInstance = Instantiate(prefab);
            RectTransform iconTransform = lootUIInstance.transform as RectTransform;
            iconTransform.SetParent(canvas.transform, false);
            iconTransform.position = Vector3.zero;

            NumberToText numberToText = lootUIInstance.GetComponentInChildren<NumberToText>();
            if (numberToText != null)
                numberToText.Number = count;

            return iconTransform;
        }

        private void HitWithAxe()
        {
            void ActivateAxe() { toolPickaxe.SetActive(false); toolAxe.SetActive(true); }
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 1.5f, (x) => ActivateAxe(), () => toolAxe.SetActive(false)));
            StartCoroutine(Extensions.SkipFrames(1, () => SetAnimatorsBool("Loot", true)));
            StartCoroutine(Extensions.SkipFrames(2, () => SetAnimatorsBool("Loot", false)));
        }

        private void HitWithPickaxe()
        {
            void ActivatePickaxe() { toolAxe.SetActive(false); toolPickaxe.SetActive(true); }
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 1.5f, (x) => ActivatePickaxe(), () => toolPickaxe.SetActive(false)));
            StartCoroutine(Extensions.SkipFrames(1, () => SetAnimatorsBool("Loot", true)));
            StartCoroutine(Extensions.SkipFrames(2, () => SetAnimatorsBool("Loot", false)));
        }

        private void DeltaMove()
        {
            // Orient player with moving direction
            Vector3 direction = new Vector3(joystick.Direction.x, 0.0f, joystick.Direction.y);
            Quaternion orientation = direction.magnitude > 1e-3f ? Quaternion.LookRotation(direction, Vector3.up) : thisTransform.rotation;
            thisTransform.rotation = Quaternion.Slerp(thisTransform.rotation, orientation, movementSmoothFactor * Time.deltaTime);

            // Move player
            Vector3 deltaMove = (direction * maxSpeed + Vector3.down * 10.0f) * Time.deltaTime;
            characterController.Move(deltaMove);

            // Play move animation
            SetAnimatorsFloat("Speed", animationMoveSpeed); // * maxSpeed / referenceWalkSpeed);
        }

        public void Attack()
        {
            if (currentEnemy == null)
                return;

            void SpawnAndShoot()
            {
                if (!Attacking)
                    return;

                Vector3 direction = (currentEnemy.transform.position - projectileSpawnPoint.position).normalized;

                Ray ray = new Ray(projectileSpawnPoint.position, direction);
                List<RaycastHit> hits = Physics.RaycastAll(ray, Vector3.Distance(projectileSpawnPoint.position, currentEnemy.transform.position)).ToList();
                hits = hits.OrderBy(x => Vector3.Distance(projectileSpawnPoint.position, x.point)).ToList();
                RaycastHit? hit = null;

                for (int i = 0; i < hits.Count; i++)
                {
                    if (hits[i].collider.GetComponent<GameResource>() != null)
                        continue;

                    hit = hits[i];
                    break;
                }

                GameObject original = attackWeapon == PlayerAttackWeapon.Axe ? axe : arrow;

                GameObject projectile = Instantiate(original);
                Transform projectileTransform = projectile.transform;
                TrailRenderer trail = projectile.GetComponentInChildren<TrailRenderer>();

                if (trail != null)
                {
                    trail.emitting = true;
                }

                hideHandProjectile = true;
                StartCoroutine(Extensions.ScaledTimeDelay(Mathf.Clamp(attackDelay * 0.5f, 0.0f, 1.0f), () => hideHandProjectile = false));

                projectileTransform.position = projectileSpawnPoint.position;
                Vector3 destination = currentEnemy.transform.position + Vector3.up;

                float distance = Vector3.Distance(thisTransform.position, destination);
                float flyingTime = distance / arrowSpeed;

                if (hit != null)
                {
                    Enemy enemy = hit.Value.collider.GetComponent<Enemy>();
                    if (hit.Value.collider != null && (enemy == null || enemy != currentEnemy))
                    {
                        destination = hit.Value.point;
                        distance = Vector3.Distance(thisTransform.position, destination);
                        flyingTime = distance / arrowSpeed;

                        if (enemy == null)
                        {
                            ParticleSystem hitFX = Instantiate(damageFX.gameObject).GetComponent<ParticleSystem>();
                            hitFX.transform.position = hit.Value.point;
                            StartCoroutine(Extensions.ScaledTimeDelay(flyingTime, () => hitFX.Play()));
                            StartCoroutine(Extensions.ScaledTimeDelay(flyingTime + 1.0f, () => Destroy(hitFX.gameObject)));
                        }
                        else
                        if (enemy != currentEnemy)
                        {
                            currentEnemy = enemy;
                            destination = currentEnemy.transform.position + Vector3.up;
                            direction = (currentEnemy.transform.position - projectileSpawnPoint.position).normalized;
                            distance = Vector3.Distance(thisTransform.position, destination);
                            flyingTime = distance / arrowSpeed;
                        }
                    }
                }

                void ProjectileFly(float t)
                {
                    if (projectileTransform == null)
                        return;

                    switch (attackWeapon)
                    {
                        case PlayerAttackWeapon.Axe:
                            MoveAxe(projectileTransform, destination, t);
                            break;
                        case PlayerAttackWeapon.Bow:
                            MoveArrow(projectileTransform, destination, t);
                            break;
                    }

                    if (Vector3.Distance(currentEnemy.transform.position, projectileTransform.position) < hitRadius)
                    {
                        HitEnemy(projectileTransform);
                        Destroy(projectile);
                    }
                }

                StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, flyingTime, ProjectileFly, () =>
                {
                    if (projectileTransform != null)
                    {
                        HitEnemy(projectileTransform);
                        Destroy(projectile);
                    }
                }));

                // SHOOT DEBUG
                Debug.DrawLine(thisTransform.position, destination, Color.red, flyingTime);
            }

            void MoveArrow(Transform arrow, Vector3 destination, float t)
            {
                arrow.position = Vector3.Lerp(thisTransform.position, destination, t);
                arrow.LookAt(destination);
            }

            void MoveAxe(Transform axe, Vector3 destination, float t)
            {
                axe.position = Vector3.Lerp(thisTransform.position, destination, t);
                axe.rotation *= Quaternion.Euler(axeAngularSpeed * Time.deltaTime, 0.0f, 0.0f);
            }

            void HitEnemy(Transform ball)
            {
                if (Vector3.Distance(currentEnemy.transform.position, ball.position) < hitRadius)
                {
                    bool critical = criticalHitChance != 0.0f && UnityEngine.Random.value <= criticalHitChance;
                    currentEnemy.TakeDamage(critical ? attackStrength * 2 : attackStrength, critical);

                    CameraShaking.ShakeCamera(critical);
                }
            }

            currentEnemy.EnableArea(true);

            string animatorBoolName = attackWeapon == PlayerAttackWeapon.Axe ? "Attack Axe" : "Attack Bow";
            StartCoroutine(Extensions.SkipFrames(1, () => SetAnimatorsBool(animatorBoolName, true)));
            StartCoroutine(Extensions.SkipFrames(2, () => SetAnimatorsBool(animatorBoolName, false)));
            StartCoroutine(Extensions.ScaledTimeDelay(0.3f, SpawnAndShoot));
        }

        public void TakeDamage(int value, bool critical)
        {
            if (Dead)
                return;

            currentHealth -= value;

            if (healthBar != null)
            {
                healthBar.NormalizedValue = (float)currentHealth / maxHealth;
            }

            if (currentHealth > 0)
            {
                SetAnimatorsBool("Get Hit", true);
                StartCoroutine(Extensions.SkipFrames(1, () => SetAnimatorsBool("Get Hit", false)));
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

        public int GetCrystalFreeSpace()
        {
            if (bag != null)
            {
                return bag.FreeCrystalSpace;
            }

            return 0;
        }

        public int GetWoodFreeSpace()
        {
            if (bag != null)
            {
                return bag.FreeWoodSpace;
            }

            return 0;
        }

        public void SetMaxHealth(int value)
        {
            maxHealth = value;
            currentHealth = maxHealth;
            healthBar.MaxHealth = maxHealth;
            healthBar.NormalizedValue = (float)currentHealth / maxHealth;
        }

        public void SetMaxWeaponPower(int value)
        {
            attackStrength = value;
        }

        private void SetAnimatorsFloat(string floatName, float value)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].SetFloat(floatName, value);
            }
        }

        private void SetAnimatorsBool(string boolName, bool value)
        {
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].SetBool(boolName, value);
            }
        }

        private void Die()
        {
            if (Dead)
                return;

            SetAnimatorsBool("Death", true);
            currentHealth = 0;
            healthBar.gameObject.SetActive(false);
            Dead = true;

            OnFockenDeath.Invoke();
        }

        private void ApplySettings()
        {
            if (gameConfiguration != null)
            {
                switch (gameConfiguration.style)
                {
                    case Visuals.Styles.Regular:
                        playerType = PlayerType.Citizen;
                        break;
                    case Visuals.Styles.Simple:
                        playerType = PlayerType.Stickman;
                        break;
                    default:
                        break;
                }

                attackType = gameConfiguration.playerAttackType;
                maxSpeed = gameConfiguration.playerMaxSpeed;
                movementSmoothFactor = gameConfiguration.playerMovementSmoothFactor;
                attackDelay = gameConfiguration.playerAttackDelay;
                arrowSpeed = gameConfiguration.playerProjectileSpeed;
                criticalHitChance = Mathf.Clamp01(gameConfiguration.criticalHitChance);
                attackWeapon = gameConfiguration.playerAttackWeapon;
                axeAngularSpeed = gameConfiguration.axeAngularSpeed;
                joystickSensitivity = Mathf.Clamp(gameConfiguration.joystickSensitivity, 0.0f, float.PositiveInfinity);
            }
        }
    }
}
