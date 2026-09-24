using TMPro;
using UnityEngine;

public class CoinBalanceUI : MonoBehaviour
{
    private TextMeshProUGUI balanceText;

    private void Awake()
    {
        balanceText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        CoinManager.OnCoinsUpdated += HandleCoinsUpdated;
        if (CoinManager.Instance != null)
        {
            HandleCoinsUpdated(CoinManager.Instance.TotalCoins);
        }
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsUpdated -= HandleCoinsUpdated;
    }

    private void HandleCoinsUpdated(int newTotal)
    {
        balanceText.text = newTotal.ToString();
    }
}
