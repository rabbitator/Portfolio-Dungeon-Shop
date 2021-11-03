using DungeonShop.Miscellaneous;
using UnityEngine;

namespace DungeonShop.GameLogic
{
    public class GameScores : MonoBehaviour
    {
        [Header("Resources: Wood")]
        public Extensions.IntEvent SetCurrentWood;
        public Extensions.IntEvent SetTotalWood;

        [Header("Resources: Crystal")]
        [Space(5.0f)]
        public Extensions.IntEvent SetCurrentCrystal;
        public Extensions.IntEvent SetTotalCrystal;

        [Header("Resources: Coin")]
        [Space(5.0f)]
        public Extensions.IntEvent SetCurrentCoins;
        public Extensions.IntEvent SetTotalCoins;

        [Header("Player Level: Capacity")]
        [Space(15.0f)]
        public Extensions.IntEvent SetPlayerCapacityLevel;
        public Extensions.IntEvent SetPlayerCapacity;

        [Header("Player Level: Health")]
        [Space(5.0f)]
        public Extensions.IntEvent SetPlayerHealthLevel;
        public Extensions.IntEvent SetPlayerHealth;

        [Header("Player Level: Weapon")]
        [Space(5.0f)]
        public Extensions.IntEvent SetPlayerWeaponLevel;
        public Extensions.IntEvent SetPlayerWeapon;

        private int currentCoins = 0;
        private int currentWood = 0;
        private int currentCrystal = 0;
        private GameConfiguration gameConfiguration;

        public static readonly string totalCoinsPropertyName = "Total_Coins";
        public static readonly string totalWoodPropertyName = "Total_Wood";
        public static readonly string totalCrystalPropertyName = "Total_Crystal";
        public static readonly string capacityLevelPropertyName = "Capacity_Level";
        public static readonly string healthLevelPropertyName = "Health_Level";
        public static readonly string weaponLevelPropertyName = "Weapon_Level";

        private void Start()
        {
            gameConfiguration = ConfigurationObject.GameConfiguration;
            if (gameConfiguration == null)
                Debug.LogError("Can't get Game Configuration!");

            SetCurrentCoins.Invoke(currentCoins);
            SetCurrentWood.Invoke(currentWood);
            SetCurrentCrystal.Invoke(currentCrystal);

            SetTotalCoins.Invoke(GetTotalCoins());
            SetTotalWood.Invoke(GetTotalWood());
            SetTotalCrystal.Invoke(GetTotalCrystal());

            SetPlayerCapacity.Invoke(GetMaxCapacity());
            SetPlayerCapacityLevel.Invoke(GetPlayerLevelCapacity());

            SetPlayerHealth.Invoke(GetMaxHealth());
            SetPlayerHealthLevel.Invoke(GetPlayerLevelHealth());

            SetPlayerWeapon.Invoke(GetMaxWeapon());
            SetPlayerWeaponLevel.Invoke(GetPlayerLevelWeapon());
        }

        private int GetMaxCapacity()
        {
            float playerMaxCapacity = gameConfiguration.level1Capacity;
            for (int i = 0; i < PlayerPrefs.GetInt(capacityLevelPropertyName, 1) - 1; i++)
                playerMaxCapacity += playerMaxCapacity * gameConfiguration.levelUpPercent;

            return Mathf.RoundToInt(playerMaxCapacity);
        }

        private int GetMaxHealth()
        {
            float playerMaxHealth = gameConfiguration.level1Health;
            for (int i = 0; i < PlayerPrefs.GetInt(healthLevelPropertyName, 1) - 1; i++)
                playerMaxHealth += playerMaxHealth * gameConfiguration.levelUpPercent;

            return Mathf.RoundToInt(playerMaxHealth);
        }

        private int GetMaxWeapon()
        {
            float playerMaxWeapon = gameConfiguration.level1Weapon;
            for (int i = 0; i < PlayerPrefs.GetInt(weaponLevelPropertyName, 1) - 1; i++)
                playerMaxWeapon += playerMaxWeapon * gameConfiguration.levelUpPercent;

            return Mathf.RoundToInt(playerMaxWeapon);
        }

        public int GetPlayerLevelCapacity()
        {
            return PlayerPrefs.GetInt(capacityLevelPropertyName, 1);
        }

        public int GetPlayerLevelHealth()
        {
            return PlayerPrefs.GetInt(healthLevelPropertyName, 1);
        }

        public int GetPlayerLevelWeapon()
        {
            return PlayerPrefs.GetInt(weaponLevelPropertyName, 1);
        }

        private int GetTotalCoins()
        {
            int rawValue = PlayerPrefs.GetInt(totalCoinsPropertyName, 0) + currentCoins;
            return Mathf.Clamp(rawValue, 0, 9999);
        }

        private int GetTotalWood()
        {
            int rawValue = PlayerPrefs.GetInt(totalWoodPropertyName, 0) + currentWood;
            return Mathf.Clamp(rawValue, 0, GetMaxCapacity());
        }

        private int GetTotalCrystal()
        {
            int rawValue = PlayerPrefs.GetInt(totalCrystalPropertyName, 0) + currentCrystal;
            return Mathf.Clamp(rawValue, 0, GetMaxCapacity());
        }

        public void AddCurrentCoins(int count)
        {
            int totalCoins = PlayerPrefs.GetInt(totalCoinsPropertyName, 0);

            currentCoins = AddCurrentValue(totalCoins, currentCoins, count, 9999);

            SetCurrentCoins.Invoke(currentCoins);
            SetTotalCoins.Invoke(totalCoins + currentCoins);
        }

        public void AddCurrentWood(int count)
        {
            int capacity = Mathf.RoundToInt(GetMaxCapacity());
            int totalWood = PlayerPrefs.GetInt(totalWoodPropertyName, 0);

            currentWood = AddCurrentValue(totalWood, currentWood, count, capacity);

            SetCurrentWood.Invoke(currentWood);
            SetTotalWood.Invoke(totalWood + currentWood);
        }

        public void AddCurrentCrystal(int count)
        {
            int capacity = Mathf.RoundToInt(GetMaxCapacity());
            int totalCrystal = PlayerPrefs.GetInt(totalCrystalPropertyName, 0);

            currentCrystal = AddCurrentValue(totalCrystal, currentCrystal, count, capacity);

            SetCurrentCrystal.Invoke(currentCrystal);
            SetTotalCrystal.Invoke(totalCrystal + currentCrystal);
        }

        public void AddTotalCoins(int count)
        {
            int totalCoins = PlayerPrefs.GetInt(totalCoinsPropertyName, 0);

            totalCoins = AddTotalValue(totalCoins, currentCoins, count, 9999);
            PlayerPrefs.SetInt(totalCoinsPropertyName, totalCoins);

            SetTotalCoins.Invoke(totalCoins);
        }

        public void AddTotalWood(int count)
        {
            int capacity = Mathf.RoundToInt(GetMaxCapacity());
            int totalWood = PlayerPrefs.GetInt(totalWoodPropertyName, 0);

            totalWood = AddTotalValue(totalWood, currentWood, count, capacity);
            PlayerPrefs.SetInt(totalWoodPropertyName, totalWood);

            SetTotalWood.Invoke(totalWood);
        }

        public void AddTotalCrystal(int count)
        {
            int capacity = Mathf.RoundToInt(GetMaxCapacity());
            int totalCrystal = PlayerPrefs.GetInt(totalCrystalPropertyName, 0);

            totalCrystal = AddTotalValue(totalCrystal, currentCrystal, count, capacity);
            PlayerPrefs.SetInt(totalCrystalPropertyName, totalCrystal);

            SetTotalCrystal.Invoke(totalCrystal);
        }

        private int AddCurrentValue(int totalValue, int currentValue, int addValue, int capacity)
        {
            currentValue += addValue;
            currentValue = Mathf.Clamp(currentValue, 0, int.MaxValue);
            totalValue += currentValue;
            currentValue += Mathf.Clamp((capacity - totalValue), -currentValue, 0);

            return currentValue;
        }

        private int AddTotalValue(int totalValue, int currentValue, int addValue, int capacity)
        {
            totalValue += currentValue + addValue;
            totalValue = Mathf.Clamp(totalValue, 0, capacity);

            return totalValue;
        }

        public void AddCapacityLevel()
        {
            int currentCapacityLevel = PlayerPrefs.GetInt(capacityLevelPropertyName, 1);
            PlayerPrefs.SetInt(capacityLevelPropertyName, ++currentCapacityLevel);

            SetPlayerCapacity.Invoke(GetMaxCapacity());
            SetPlayerCapacityLevel.Invoke(currentCapacityLevel);
        }

        public void AddHealthLevel()
        {
            int currentHealthLevel = PlayerPrefs.GetInt(healthLevelPropertyName, 1);
            PlayerPrefs.SetInt(healthLevelPropertyName, ++currentHealthLevel);

            SetPlayerHealth.Invoke(GetMaxHealth());
            SetPlayerHealthLevel.Invoke(currentHealthLevel);
        }

        public void AddWeaponLevel()
        {
            int currentWeaponLevel = PlayerPrefs.GetInt(weaponLevelPropertyName, 1);
            PlayerPrefs.SetInt(weaponLevelPropertyName, ++currentWeaponLevel);

            SetPlayerWeapon.Invoke(GetMaxWeapon());
            SetPlayerWeaponLevel.Invoke(currentWeaponLevel);

        }

        public void SaveLevelScore()
        {
            PlayerPrefs.SetInt(totalCoinsPropertyName, PlayerPrefs.GetInt(totalCoinsPropertyName, 0) + currentCoins);
            PlayerPrefs.SetInt(totalWoodPropertyName, PlayerPrefs.GetInt(totalWoodPropertyName, 0) + currentWood);
            PlayerPrefs.SetInt(totalCrystalPropertyName, PlayerPrefs.GetInt(totalCrystalPropertyName, 0) + currentCrystal);
        }
    }
}