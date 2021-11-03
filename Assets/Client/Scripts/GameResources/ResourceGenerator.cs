using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DungeonShop.UI;
using DungeonShop.GameLogic;
using DungeonShop.Miscellaneous;
using System.Linq;
using DungeonShop.Player;

namespace DungeonShop.GameResources
{
    public abstract class ResourceGenerator : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("UI: Window")]
        [SerializeField]
        private GameObject joystick;
        [SerializeField]
        private GameObject generatorWindow;
        [SerializeField]
        private GameObject buildContent;
        [SerializeField]
        private GameObject levelUpContent;
        [SerializeField]
        private NumberToText levelNumber;
        [SerializeField]
        private NumberToText perHourCount;
        [SerializeField]
        private Button buttonClose;
        [SerializeField]
        private ProgressBar windowProgressBar;
        [SerializeField]
        private Text windowProgressText;

        [Header("UI: Purchase button")]
        [Space(10.0f)]
        [SerializeField]
        private Button purchaseButton;
        [SerializeField]
        private Text buttonText;
        [SerializeField]
        private NumberToText buttonPrice;
        [SerializeField]
        private GameObject buttonProductIcon;
        [SerializeField]
        private GameObject buttonCoinIcon;

        [Header("UI: Status cloud")]
        [Space(10.0f)]
        [SerializeField]
        private Transform cloudScenePoint;
        [SerializeField]
        private RectTransform cloudUiRoot;
        [SerializeField]
        private ProgressBar cloudProgressBar;
        [SerializeField]
        private Text cloudProgressText;

        [Header("Issue Data")]
        [Space(10.0f)]
        [SerializeField]
        private GameObject productPrefab;
        [SerializeField]
        private Parabola[] parabolas;

        [Header("Scene Objects")]
        [Space(10.0f)]
        [SerializeField]
        private GameObject buildIcon;
        [SerializeField]
        private GameObject buildingRoot;
#pragma warning restore 0649

        [Space(20.0f)]
        public Extensions.IntEvent PurchaseAddCoins;
        public Extensions.IntEvent PurchaseAddProduct;
        public Extensions.IntEvent GiveResources;

        protected GameConfiguration gameConfiguration;
        protected string levelPropertyName;
        protected string lastCheckTimePropertyName;
        protected string debugHoursPropertName;
        protected int buildPrice = 45;
        private Coroutine delayedWindowCoroutine;
        private int playerMoney;
        private int playerProduct;
        private bool initialized = false;

        private class IssuedProduct
        {
            public Transform product;
            public Parabola parabola;
        }

        private void Initialize()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration == null)
                Debug.LogError($"{name} (ResourceGenerator): Can't get Game Configuration!");

            initialized = true;

            if (buildIcon != null && buildingRoot != null)
            {
                if (GetCurrentLevel() > 0)
                {
                    buildIcon.SetActive(false);
                    buildingRoot.SetActive(true);
                }
                else
                {
                    buildIcon.SetActive(true);
                    buildingRoot.SetActive(false);
                }
            }
        }

        private void OnEnable()
        {
            Initialize();

            buttonClose.onClick.AddListener(CloseWindow);
            purchaseButton.onClick.AddListener(() => Purchase());
        }

        private void OnDisable()
        {
            buttonClose.onClick.RemoveListener(CloseWindow);
            purchaseButton.onClick.RemoveListener(() => Purchase());
        }

        private void LateUpdate()
        {
            if (cloudScenePoint != null && cloudUiRoot != null)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    cloudUiRoot.position = mainCamera.WorldToScreenPoint(cloudScenePoint.position);
                    cloudUiRoot.gameObject.SetActive(GetCurrentLevel() > 0);
                }
            }
        }

        public virtual void OpenWindow()
        {
            joystick.SetActive(false);
            generatorWindow.SetActive(true);

            if (!initialized)
                Initialize();

            InitializeWindow();
        }

        public void CloseWindow()
        {
            joystick.SetActive(true);
            generatorWindow.SetActive(false);
        }

        private int GetNextLevelPrice(int level1Price, int currentLevel)
        {
            float price = currentLevel > 0 ? level1Price : buildPrice;
            for (int i = 0; i < currentLevel; i++)
                price += price * gameConfiguration.generatorsLevelUpPricePercent;

            float remainder = price % 5.0f;
            if (remainder >= 2.5f)
                price += 5.0f - remainder;
            else
                price -= remainder;

            return Mathf.RoundToInt(price);
        }

        protected void InitializeWindow()
        {
            int currentLevel = GetCurrentLevel();
            int price = GetNextLevelPrice(gameConfiguration.generatorsLevel1Price, currentLevel);

            if (currentLevel == 0)
            {
                buildContent.SetActive(true);
                levelUpContent.SetActive(false);

                levelNumber.gameObject.SetActive(false);

                buttonProductIcon.SetActive(true);
                buttonCoinIcon.SetActive(false);
                buttonText.text = "BUILD";
                buttonPrice.Number = price;
            }
            else
            {
                buildContent.SetActive(false);
                levelUpContent.SetActive(true);

                levelNumber.gameObject.SetActive(true);
                levelNumber.Number = currentLevel;

                buttonProductIcon.SetActive(false);
                buttonCoinIcon.SetActive(true);
                buttonText.text = "UPGRADE";
                buttonPrice.Number = price;

                perHourCount.Number = GetProduction(currentLevel);
            }

            if ((currentLevel == 0 ? playerProduct : playerMoney) < price)
                purchaseButton.interactable = false;
            else
                purchaseButton.interactable = true;
        }

        private void Purchase()
        {
            int purchasingLevel = GetCurrentLevel() + 1;
            int price = GetNextLevelPrice(gameConfiguration.generatorsLevel1Price, purchasingLevel - 1);

            if (price > (purchasingLevel == 1 ? playerProduct : playerMoney))
            {
                Debug.LogWarning($"Not enough {(purchasingLevel == 1 ? "product" : "money")}, price: {price}");

                return;
            }

            PlayerPrefs.SetInt(levelPropertyName, purchasingLevel);
            buildingRoot.SetActive(true);
            buildIcon.SetActive(false);

            if (purchasingLevel == 1)
            {
                UpdateLastCheckTime();

                PurchaseAddProduct.Invoke(-price);
            }
            else
                PurchaseAddCoins.Invoke(-price);

            SetProgressBarsValues(GetGeneratedResourceCount(), GetCapacity(purchasingLevel));
            InitializeWindow();
        }

        private void SetProgressBarsValues(int generatedResources, int capacity)
        {
            windowProgressBar.NormalizedValue = generatedResources / (float)capacity;
            windowProgressText.text = $"{generatedResources}/{capacity}";

            cloudProgressBar.NormalizedValue = generatedResources / (float)capacity;
            cloudProgressText.text = $"{generatedResources}/{capacity}";
        }

        public virtual void SetPlayerCoinsResourceCount(int value)
        {
            playerMoney = value;
        }

        public virtual void SetPlayerProductCount(int value)
        {
            playerProduct = value;
        }

        public void PlayerAppears()
        {
            int currentlevel = GetCurrentLevel();
            int generatedCount = GetGeneratedResourceCount();
            int capacity = GetCapacity(currentlevel);            
            int playerFreeSpace = GetPlayerFreeSpace();

            int clampedCount = Mathf.Clamp(generatedCount, 0, capacity);
            clampedCount = Mathf.Clamp(clampedCount, 0, playerFreeSpace);

            if (clampedCount > 0 && clampedCount <= playerFreeSpace)
            {
                ProductIssue(clampedCount);
                UpdateLastCheckTime();

                GiveResources.Invoke(clampedCount);
            }
            else
                OpenWindow();
        }

        public void PlayerLeaves()
        {
            if (delayedWindowCoroutine != null)
                StopCoroutine(delayedWindowCoroutine);

            CloseWindow();
        }

        public void AddDebugHour()
        {
            PlayerPrefs.SetInt(debugHoursPropertName, PlayerPrefs.GetInt(debugHoursPropertName, 0) + 1);
        }

        protected abstract int GetPlayerFreeSpace();

        private void ProductIssue(int count)
        {
            Parabola GetParabola()
            {
                return parabolas[UnityEngine.Random.Range(0, parabolas.Length)];
            }

            int currentlevel = GetCurrentLevel();
            int capacity = GetCapacity(currentlevel);
            int clampedCount = Mathf.Clamp(count, 0, capacity);
            List<IssuedProduct> products = new List<IssuedProduct>();

            for (int i = 0; i < Mathf.Clamp(parabolas.Length, 0, count); i++)
            {
                Transform product = Instantiate(productPrefab).transform;
                Parabola parabola = GetParabola();

                int tries = 10;
                while (tries > 0 && products.FirstOrDefault(x => x.parabola == parabola) != null)
                {
                    GetParabola();
                    tries--;
                }

                product.position = parabola.GetParabolaPoint(0.0f);
                products.Add(new IssuedProduct() { product = product, parabola = parabola });
            }

            PlayerController player = FindObjectOfType<PlayerController>();
            Transform intakePoint = player == null ? null : player.ResourceIntakePoint;

            if (intakePoint == null)
                Debug.LogWarning("Intake point not found!");

            delayedWindowCoroutine = StartCoroutine(Extensions.ScaledTimeDelay(2.0f, OpenWindow));
            StartCoroutine(Extensions.UniversalScaledTimeCoroutine(0.0f, 1.5f,

            (t) =>
            {
                /* Выдать ведьмаку его монету */
                SetProgressBarsValues(Mathf.RoundToInt((1.0f - t) * clampedCount), capacity);
                for (int i = 0; i < products.Count; i++)
                {
                    float issueT = Mathf.Clamp01(t * 3.0f);
                    products[i].product.position = products[i].parabola.GetParabolaPoint(issueT);
                    products[i].product.rotation = Quaternion.Lerp(Quaternion.LookRotation(Vector3.right), Quaternion.LookRotation(Vector3.up), issueT);

                    float intakeT = Mathf.Clamp01((t - 0.9f) * 10.0f);
                    if (intakePoint != null && intakeT > 0.0f)
                    {
                        Vector3 startPoint = products[i].parabola.GetParabolaPoint(1.0f);
                        Vector3 startScale = products[i].product.localScale;
                        products[i].product.position = Vector3.Lerp(startPoint, intakePoint.position, intakeT * intakeT);
                        products[i].product.localScale = Vector3.Lerp(startScale, Vector3.zero, intakeT * intakeT);
                    }
                }
            },

            () =>
            {
                // PlayerPrefs.SetInt(givenResourceCountPropertyName, PlayerPrefs.GetInt(givenResourceCountPropertyName, 0) + generatedCount);
                for (int i = 0; i < products.Count; i++)
                {
                    Destroy(products[i].product.gameObject);
                }
            }));
        }

        private int GetCapacity(int level)
        {
            float currentCapacity = -1.0f;

            if (gameConfiguration != null)
            {
                currentCapacity = gameConfiguration.generatorsStartCapacity;

                for (int i = 0; i < level - 1; i++)
                {
                    currentCapacity += currentCapacity * gameConfiguration.generatorsLevelUpCapacityPercent;
                }
            }
            else
            {
                Debug.LogError($"{name} (ResourceGenerator): can't get Game Configuration!");
            }

            return Mathf.RoundToInt(currentCapacity);
        }

        private int GetProduction(int level)
        {
            float currentProduction = 0.0f;

            if (gameConfiguration != null)
            {
                currentProduction = gameConfiguration.generatorsStartProductionPerHour;

                for (int i = 0; i < level - 1; i++)
                {
                    currentProduction += currentProduction * gameConfiguration.generatorsLevelUpProductionPercent;
                }
            }
            else
            {
                Debug.LogError($"{name} (ResourceGenerator): can't get Game Configuration!");
            }

            return Mathf.RoundToInt(currentProduction);
        }

        private int GetCurrentLevel()
        {
            return PlayerPrefs.GetInt(levelPropertyName, 0);
        }

        private int GetGeneratedResourceCount()
        {
            int debugHours = PlayerPrefs.GetInt(debugHoursPropertName, 0);
            int currentLevel = GetCurrentLevel();
            int generatedCount = 0;
            string lastCheckTimeString = PlayerPrefs.GetString(lastCheckTimePropertyName, DateTime.Now.AddHours(debugHours).Ticks.ToString());
            int currentProduction = GetProduction(currentLevel);

            long.TryParse(lastCheckTimeString, out long lastLogoutTime);
            if(lastLogoutTime > 0)
            {
                long nowTime = DateTime.Now.AddHours(debugHours).Ticks;
                generatedCount = Mathf.RoundToInt((float)(((nowTime - lastLogoutTime) / (double)TimeSpan.TicksPerHour) * currentProduction));
            }

            return Mathf.Clamp(generatedCount, 0, GetCapacity(currentLevel));
        }

        private void UpdateLastCheckTime()
        {
            int debugHours = PlayerPrefs.GetInt(debugHoursPropertName, 0);
            PlayerPrefs.SetString(lastCheckTimePropertyName, DateTime.Now.AddHours(debugHours).Ticks.ToString());
        }

        public void Check()
        {
            int currentCapacity = GetCapacity(GetCurrentLevel());
            int generatedResources = GetGeneratedResourceCount();
            generatedResources = Mathf.Clamp(generatedResources, 0, currentCapacity);

            SetProgressBarsValues(generatedResources, currentCapacity);
        }
    }
}
