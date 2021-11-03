using DungeonShop.Miscellaneous;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonShop.GameResources
{
    public class GameResourcesShop : MonoBehaviour
    {
#pragma warning disable 0649
        // [SerializeField]
        // private Customer[] customers = new Customer[] { };
        
        [Header("Customers")]
        [SerializeField]
        private CustomerSkin[] customerSkins = new CustomerSkin[] { };
        [SerializeField]
        private Transform[] customersAwayPoints;

        [Header("UI Elements")]
        [Space(10.0f)]
        [SerializeField]
        private Joystick joystick;
        [SerializeField]
        private GameObject shopWindow;
        [SerializeField]
        private GameObject customersWindow;
        [SerializeField]
        private GameObject shopClosedWindow;
        [SerializeField]
        private Image customerPhoto;
        [SerializeField]
        private Text customerNameField;
        [SerializeField]
        private GameObject customerBadLabel;
        [SerializeField]
        private GameObject customerGoodLabel;
        [SerializeField]
        private GameObject customerPerfectLabel;
        [SerializeField]
        private GameObject itemWood;
        [SerializeField]
        private GameObject itemCrystal;
        [SerializeField]
        private Text leftItemTotalCountField;
        [SerializeField]
        private Text rightItemTotalCountField;
        [SerializeField]
        private Button buttonNext;
        [SerializeField]
        private Button buttonSell;
        [SerializeField]
        private GameObject perPieceWoodItem;
        [SerializeField]
        private GameObject perPieceCrystalItem;
        [SerializeField]
        private Text perPieceCountField;
        [SerializeField]
        private Text perPiecePriceField;

        [Header("Events")]
        [Space(10.0f)]
        public Extensions.IntEvent DealAddWood;
        public Extensions.IntEvent DealAddCrystal;
        public Extensions.IntEvent DealAddCoins;
        public Extensions.IntEvent OnTotalSold;

        private Customer currentCustomer;
        private int playerResourceWoodCount;
        private int playerResourceCrystalCount;
        private int sessionCoins = 0;
#pragma warning restore 0649

        private string[] randomNames = new string[] { "Stanley Reyes",
                                                      "Earl Garraway",
                                                      "Jamie Rogers",
                                                      "Leo Richards",
                                                      "Raymond Marshman",
                                                      "Lovell Clayton",
                                                      "Gardner Greer",
                                                      "Hector Gildon",
                                                      "Dudley Burrows" };

        [Serializable]
        private class Customer
        {
#pragma warning disable 0649
            [SerializeField]
            private string name;
            [SerializeField]
            private CustomerSkin customerSkin;
            [SerializeField]
            private CustomerOffer offer;
            [SerializeField]
            [HideInInspector]
            private bool perfect = false;
            [SerializeField]
            [HideInInspector]
            private bool served = false;
#pragma warning restore 0649

            public string Name { get => name; set => name = value; }
            public CustomerOffer Offer { get => offer; set => offer = value; }
            public bool Perfect { get => perfect; set => perfect = value; }
            public bool Served { get => served; set => served = value; }
            public CustomerSkin CustomerSkin { get => customerSkin; set => customerSkin = value; }
        }

        [Serializable]
        private class CustomerSkin
        {
#pragma warning disable 0649
            [SerializeField]
            private GameObject customerObject;
            [SerializeField]
            private Sprite customerPhotoSprite;
#pragma warning restore 0649

            public Sprite CustomerPhotoSprite { get => customerPhotoSprite; }
            public GameObject CustomerObject { get => customerObject; }
        }

        [Serializable]
        private class CustomerOffer
        {
#pragma warning disable 0649
            [SerializeField]
            private ResourceType takingResource;
            [SerializeField]
            private int totalCount;
            [SerializeField]
            private int costPerPiece;
#pragma warning restore 0649

            public ResourceType TakingResource { get => takingResource; set => takingResource = value; }
            public int TotalCount { get => totalCount; set => totalCount = value; }
            public int CostPerPiece { get => costPerPiece; set => costPerPiece = value; }
        }

        public enum ResourceType
        {
            Wood,
            Crystal
        }

        private void OnEnable()
        {
            buttonNext.onClick.AddListener(NextCustomer);
            buttonSell.onClick.AddListener(Sell);
        }

        private void OnDisable()
        {
            buttonNext.onClick.RemoveListener(NextCustomer);
            buttonSell.onClick.RemoveListener(Sell);
        }

        public void OpenShopWindow()
        {
            joystick.gameObject.SetActive(false);
            shopWindow.SetActive(true);

            ChangeCustomer();

            if (playerResourceWoodCount > 0 || playerResourceCrystalCount > 0)
                ShowCustomersWindow();
            else
                HideCustomersWindow();
        }

        public void CloseShopWindow()
        {
            joystick.gameObject.SetActive(true);
            shopWindow.SetActive(false);
        }

        public void DisperseCrowd()
        {
            for (int i = 0; i < customerSkins.Length; i++)
            {
                Citizen walker = customerSkins[i].CustomerObject.GetComponent<Citizen>();

                if (walker != null)
                {
                    walker.AwayPoint = customersAwayPoints[i % customersAwayPoints.Length];
                    walker.Awake();
                    walker.WalkAway(i / customersAwayPoints.Length, 3.0f);
                }
                else
                {
                    Debug.LogWarning(customerSkins[i].CustomerObject.name + " doesn't have Citizen component!");
                }
            }
        }

        public void NextCustomer()
        {
            if (playerResourceWoodCount > 0 || playerResourceCrystalCount > 0)
            {
                ChangeCustomer();
            }
            else
            {
                HideCustomersWindow();
            }
        }

        private void ShowCustomersWindow()
        {
            customersWindow.SetActive(true);
            shopClosedWindow.SetActive(false);

            buttonSell.interactable = CanSell();
        }

        private bool CanSell()
        {
            bool enoughResources;

            switch (currentCustomer.Offer.TakingResource)
            {
                case ResourceType.Wood:
                    enoughResources = currentCustomer.Offer.TotalCount <= playerResourceWoodCount;
                    break;
                case ResourceType.Crystal:
                    enoughResources = currentCustomer.Offer.TotalCount <= playerResourceCrystalCount;
                    break;
                default:
                    enoughResources = false;
                    break;
            }

            return enoughResources;
        }

        private void HideCustomersWindow()
        {
            customersWindow.SetActive(false);
            shopClosedWindow.SetActive(true);
        }

        private void ChangeCustomer()
        {
            int GetCostPerPiece(int lowestPrice, int perfectPrice)
            {
                return UnityEngine.Random.value < 0.05f ? perfectPrice : UnityEngine.Random.Range(lowestPrice, perfectPrice);
            }

            ResourceType resourceType = (ResourceType)UnityEngine.Random.Range(0, 2);

            int _lowestPrice = 1;
            int _perfectPrice = 4;

            if(resourceType == ResourceType.Crystal)
            {
                _lowestPrice = 2;
                _perfectPrice = 5;
            }

            int costPerPiece = GetCostPerPiece(_lowestPrice, _perfectPrice);
            int totalCount = UnityEngine.Random.Range(1, 21);

            currentCustomer = new Customer()
            {
                CustomerSkin = customerSkins[UnityEngine.Random.Range(0, customerSkins.Length)],
                Offer = new CustomerOffer()
                {
                    TakingResource = resourceType,
                    CostPerPiece = costPerPiece,
                    TotalCount = totalCount
                },
                Name = randomNames[UnityEngine.Random.Range(0, randomNames.Length)],
                Perfect = false,
                Served = false
            };

            if (currentCustomer.Offer.CostPerPiece >= 4)
                currentCustomer.Perfect = true;

            customerPhoto.sprite = currentCustomer.CustomerSkin.CustomerPhotoSprite;

            customerNameField.text = currentCustomer.Name;

            switch (currentCustomer.Offer.TakingResource)
            {
                case ResourceType.Wood:
                    itemCrystal.SetActive(false);
                    perPieceCrystalItem.SetActive(false);
                    itemWood.SetActive(true);
                    perPieceWoodItem.SetActive(true);
                    break;
                case ResourceType.Crystal:
                    itemWood.SetActive(false);
                    perPieceWoodItem.SetActive(false);
                    itemCrystal.SetActive(true);
                    perPieceCrystalItem.SetActive(true);
                    break;
                default:
                    break;
            }

            buttonSell.interactable = CanSell();

            leftItemTotalCountField.text = totalCount.ToString();
            rightItemTotalCountField.text = (totalCount * costPerPiece).ToString();

            perPieceCountField.text = "1";
            perPiecePriceField.text = costPerPiece.ToString();

            customerBadLabel.SetActive(costPerPiece == _lowestPrice);
            customerGoodLabel.SetActive(costPerPiece != _lowestPrice && costPerPiece != _perfectPrice);
            customerPerfectLabel.SetActive(costPerPiece == _perfectPrice);
        }

        public void SetPlayerWoodResourceCount(int count)
        {
            playerResourceWoodCount = count;
        }

        public void SetPlayerCrystalResourceCount(int count)
        {
            playerResourceCrystalCount = count;
        }

        public void Sell()
        {
            int totalCount = currentCustomer.Offer.TotalCount;
            int costPerPiece = currentCustomer.Offer.CostPerPiece;

            DealAddCoins.Invoke(totalCount * costPerPiece);

            switch (currentCustomer.Offer.TakingResource)
            {
                case ResourceType.Wood:
                    DealAddWood.Invoke(-totalCount);
                    break;
                case ResourceType.Crystal:
                    DealAddCrystal.Invoke(-totalCount);
                    break;
                default:
                    break;
            }

            currentCustomer.Served = true;
            buttonSell.interactable = false;

            sessionCoins += totalCount * costPerPiece;

            NextCustomer();
        }

        public void InvokeTotalSoldEvent()
        {
            if (sessionCoins <= 0)
            {
                sessionCoins = 0;
                return;
            }

            OnTotalSold.Invoke(sessionCoins);
            sessionCoins = 0;
        }
    }
}
