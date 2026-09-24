using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public UpgradeData upgrade;

    [Header("UI Bindings")]
    public Image image;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button buyButton;

    [Header("Audio Feedback")]
    public AudioClip purchaseSuccessSFX;
    public AudioClip purchaseFailedSFX;

    private void OnEnable()
    {
        CoinManager.OnCoinsUpdated += HandleCoinsUpdated;
        RefreshDisplay();
    }

    private void OnDisable()
    {
        CoinManager.OnCoinsUpdated -= HandleCoinsUpdated;
    }

    public void Setup(UpgradeData data)
    {
        upgrade = data;
        RefreshDisplay();
    }

    public void Buy()
    {
        if (upgrade.TryUpgrade())
        {
            AudioSource.PlayClipAtPoint(purchaseSuccessSFX, Camera.main.transform.position);
            RefreshDisplay();
        }
        else
        {
            AudioSource.PlayClipAtPoint(purchaseFailedSFX, Camera.main.transform.position);
        }
    }

    private void HandleCoinsUpdated(int totalCoins)
    {
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (upgrade == null) return;

        image.sprite = upgrade.image;
        titleText.text = upgrade.title;
        descriptionText.text = upgrade.description;

        int currentLvl = upgrade.GetCurrentLevel();
        levelText.text = $"Lvl: {currentLvl} / {upgrade.maxLevel}";

        if (currentLvl >= upgrade.maxLevel)
        {
            costText.text = "MAX";
            buyButton.interactable = false;
        }
        else
        {
            int cost = upgrade.GetCostForNextLevel();
            costText.text = $"{cost} Coins";
            buyButton.interactable = CoinManager.Instance.TotalCoins >= cost;
        }
    }
}
