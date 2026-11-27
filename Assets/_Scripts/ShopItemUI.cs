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

    [Header("Level Selection UI")]
    public Button plusButton;
    public Button minusButton;

    [Header("Audio Feedback")]
    public AudioClip purchaseSuccessSFX;
    public AudioClip purchaseFailedSFX;

    private void Start()
    {
        if (plusButton != null) plusButton.onClick.AddListener(IncreaseActiveLevel);
        if (minusButton != null) minusButton.onClick.AddListener(DecreaseActiveLevel);
    }

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

    public void IncreaseActiveLevel()
    {
        if (upgrade == null) return;
        int activeLvl = upgrade.GetActiveLevel();
        int maxLvl = upgrade.GetCurrentLevel();

        if (activeLvl < maxLvl)
        {
            upgrade.SetActiveLevel(activeLvl + 1);
            RefreshDisplay();
        }
    }

    public void DecreaseActiveLevel()
    {
        if (upgrade == null) return;
        int activeLvl = upgrade.GetActiveLevel();

        if (activeLvl > 0)
        {
            upgrade.SetActiveLevel(activeLvl - 1);
            RefreshDisplay();
        }
    }

    public void RefreshDisplay()
    {
        if (upgrade == null) return;

        image.sprite = upgrade.image;
        titleText.text = upgrade.title;
        descriptionText.text = upgrade.description;

        int maxLvl = upgrade.GetCurrentLevel();
        int activeLvl = upgrade.GetActiveLevel();

        levelText.text = $"Lvl: {activeLvl} (Max: {maxLvl}/{upgrade.maxLevel})";

        if (plusButton != null) plusButton.interactable = (activeLvl < maxLvl);
        if (minusButton != null) minusButton.interactable = (activeLvl > 0);

        if (maxLvl >= upgrade.maxLevel)
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
