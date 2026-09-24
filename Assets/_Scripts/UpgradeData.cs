using System;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Runner/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public static event Action<UpgradeData> OnUpgradePurchased;

    public string upgradeID;
    public string title;
    public Sprite image;
    [TextArea] public string description;

    public int maxLevel = 5;
    public int baseCost = 50;
    public float costMultiplier = 1.5f;

    public float baseValue = 1f;
    public float valueIncrementPerLevel = 0.2f;

    public int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt($"Upgrade_{upgradeID}_Level", 0);
    }

    public int GetCostForNextLevel()
    {
        int currentLevel = GetCurrentLevel();
        if (currentLevel >= maxLevel) return -1; //Maxed out
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
    }

    public float GetCurrentValue()
    {
        return baseValue + (GetCurrentLevel() * valueIncrementPerLevel);
    }

    public bool TryUpgrade()
    {
        int cost = GetCostForNextLevel();
        if (cost > 0 && CoinManager.Instance.TrySpendCoins(cost))
        {
            int nextLevel = GetCurrentLevel() + 1;
            PlayerPrefs.SetInt($"Upgrade_{upgradeID}_Level", nextLevel);
            PlayerPrefs.Save();
            OnUpgradePurchased?.Invoke(this);
            return true;
        }
        return false;
    }
}
