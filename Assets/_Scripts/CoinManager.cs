using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }
    public static event Action<int> OnCoinsUpdated;

    private const string COIN_KEY = "TotalCoinsCollected";
    public int TotalCoins { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        TotalCoins = PlayerPrefs.GetInt(COIN_KEY, 0);
    }

    private void Start()
    {
        OnCoinsUpdated?.Invoke(TotalCoins);
    }

    public void AddCoins(int amount)
    {
        TotalCoins += amount;
        SaveCoins();
    }

    public bool TrySpendCoins(int amount)
    {
        if (TotalCoins >= amount)
        {
            TotalCoins -= amount;
            SaveCoins();
            return true;
        }
        return false;
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_KEY, TotalCoins);
        PlayerPrefs.Save();
        OnCoinsUpdated?.Invoke(TotalCoins);
    }
}
