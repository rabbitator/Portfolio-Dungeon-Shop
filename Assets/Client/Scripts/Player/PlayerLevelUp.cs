using DungeonShop.GameLogic;
using DungeonShop.Miscellaneous;
using DungeonShop.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DungeonShop.Player
{
    public class PlayerLevelUp : MonoBehaviour
    {
#pragma warning disable 0649
        [Header("Main Elements")]
        [SerializeField]
        private Joystick joystick;
        [SerializeField]
        private GameObject levelUpWindow;

        [Header("Level Up Items")]
        [Space(10.0f)]
        [SerializeField]
        private LevelUpUiItem capacityItem;
        [SerializeField]
        private LevelUpUiItem healthItem;
        [SerializeField]
        private LevelUpUiItem weaponItem;
#pragma warning restore 0649

        [Space(20.0f)]
        public Extensions.IntEvent PurchaseAddCoins;
        public UnityEvent OnLevelUpCapacity;
        public UnityEvent OnLevelUpHealth;
        public UnityEvent OnLevelUpWeapon;

        private GameConfiguration gameConfiguration;
        private int playerResourceCoinsCount;
        private int playerCapacityLevel;
        private int playerHealthLevel;
        private int playerWeaponLevel;
        // private bool capacityPurchased;
        // private bool healthPurchased;
        // private bool weaponPurchased;

        [Serializable]
        private class LevelUpUiItem
        {
#pragma warning disable 0649
            [SerializeField]
            private NumberToText levelOutput;
            [SerializeField]
            private Button purchaseButton;
            [SerializeField]
            private NumberToText priceOutput;
#pragma warning restore 0649

            public NumberToText PriceOutput { get => priceOutput; }
            public Button PurchaseButton { get => purchaseButton; }
            public NumberToText LevelOutput { get => levelOutput; }
        }

        private void Start()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration == null)
                Debug.LogError("Can't get Game Configuration!");
        }

        private void OnEnable()
        {
            capacityItem.PurchaseButton.onClick.AddListener(PurchaseCapacity);
            healthItem.PurchaseButton.onClick.AddListener(PurchaseHealth);
            weaponItem.PurchaseButton.onClick.AddListener(PurchaseWeapon);
        }

        private void OnDisable()
        {
            capacityItem.PurchaseButton.onClick.RemoveListener(PurchaseCapacity);
            healthItem.PurchaseButton.onClick.RemoveListener(PurchaseHealth);
            weaponItem.PurchaseButton.onClick.RemoveListener(PurchaseWeapon);
        }

        private void InitializeWindow()
        {
            capacityItem.PriceOutput.Number = GetCurrentLevelPrice(gameConfiguration.startPriceCapacity, playerCapacityLevel);
            capacityItem.LevelOutput.Number = playerCapacityLevel;
            capacityItem.PurchaseButton.interactable = /*!capacityPurchased && */(playerResourceCoinsCount >= capacityItem.PriceOutput.Number);

            healthItem.PriceOutput.Number = GetCurrentLevelPrice(gameConfiguration.startPriceHealth, playerHealthLevel);
            healthItem.LevelOutput.Number = playerHealthLevel;
            healthItem.PurchaseButton.interactable = /*!healthPurchased && */(playerResourceCoinsCount >= healthItem.PriceOutput.Number);

            weaponItem.PriceOutput.Number = GetCurrentLevelPrice(gameConfiguration.startPriceWeapon, playerWeaponLevel);
            weaponItem.LevelOutput.Number = playerWeaponLevel;
            weaponItem.PurchaseButton.interactable = /*!weaponPurchased && */(playerResourceCoinsCount >= weaponItem.PriceOutput.Number);
        }

        private int GetCurrentLevelPrice(int startPrice, int level)
        {
            if (gameConfiguration == null)
                Debug.LogError("No Game Configuration!");

            float price = startPrice;
            for (int i = 0; i < level - 1; i++)
                price += price * gameConfiguration.levelUpPercent;

            float remainder = price % 5.0f;
            if (remainder >= 2.5f)
                price += 5.0f - remainder;
            else
                price -= remainder;

            return Mathf.RoundToInt(price);
        }

        public void OpenLevelUpWindow()
        {
            Start();

            joystick.gameObject.SetActive(false);
            levelUpWindow.SetActive(true);

            InitializeWindow();
        }

        public void CloselevelUpWindow()
        {
            joystick.gameObject.SetActive(true);
            levelUpWindow.SetActive(false);
        }

        public void PurchaseCapacity()
        {
            if (playerResourceCoinsCount < capacityItem.PriceOutput.Number)
                return;

            // capacityItem.PurchaseButton.interactable = false;
            PurchaseAddCoins.Invoke(-capacityItem.PriceOutput.Number);
            // capacityPurchased = true;
            OnLevelUpCapacity.Invoke();

            InitializeWindow();
        }

        public void PurchaseHealth()
        {
            if (playerResourceCoinsCount < healthItem.PriceOutput.Number)
                return;

            // healthItem.PurchaseButton.interactable = false;
            PurchaseAddCoins.Invoke(-healthItem.PriceOutput.Number);
            // healthPurchased = true;
            OnLevelUpHealth.Invoke();

            InitializeWindow();
        }

        public void PurchaseWeapon()
        {
            if (playerResourceCoinsCount < weaponItem.PriceOutput.Number)
                return;

            // weaponItem.PurchaseButton.interactable = false;
            PurchaseAddCoins.Invoke(-weaponItem.PriceOutput.Number);
            // weaponPurchased = true;
            OnLevelUpWeapon.Invoke();

            InitializeWindow();
        }

        public void SetPlayerCoinsResourceCount(int count)
        {
            playerResourceCoinsCount = count;
        }

        public void SetPlayerCapacityLevel(int level)
        {
            playerCapacityLevel = level;
        }

        public void SetPlayerHealthLevel(int level)
        {
            playerHealthLevel = level;
        }

        public void SetPlayerWeaponLevel(int level)
        {
            playerWeaponLevel = level;
        }
    }
}