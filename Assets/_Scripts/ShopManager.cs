using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private UpgradeData[] availableUpgrades;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform contentContainer;

    private readonly List<ShopItemUI> spawnedItem = new List<ShopItemUI>();

    private void Awake()
    {
        PopulateShop();
    }

    private void PopulateShop()
    {
        //delete old items
        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedItem.Clear();

        if (availableUpgrades == null || shopItemPrefab == null) return;

        //Initialize items according to upgrades list
        foreach (UpgradeData upgrade in availableUpgrades)
        {
            if (upgrade == null) return;

            GameObject itemObj = Instantiate(shopItemPrefab, contentContainer);
            ShopItemUI itemUI = itemObj.GetComponent<ShopItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(upgrade);
                spawnedItem.Add(itemUI);
            }
        }
    }

    public void RefreshAllItems()
    {
        foreach (ShopItemUI item in spawnedItem)
        {
            if (item != null)
            {
                item.RefreshDisplay();
            }
        }
    }
}
