using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    [Header("Upgrade Reference")]
    [SerializeField] private UpgradeData radiusUpgrade;

    [Header("Visual Settings")]
    [SerializeField] private bool showInGameVisual = true;
    [SerializeField] private int circleSegments = 32;

    private SphereCollider triggerCollider;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        triggerCollider = GetComponent<SphereCollider>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void OnEnable()
    {
        UpgradeData.OnUpgradePurchased += HandleUpgradePurchased;
        UpdateCollectorRadius();
    }

    private void OnDisable()
    {
        UpgradeData.OnUpgradePurchased -= HandleUpgradePurchased;
    }

    private void HandleUpgradePurchased(UpgradeData upgradedData)
    {
        UpdateCollectorRadius();
    }

    public void UpdateCollectorRadius()
    {
        int currentLevel = (radiusUpgrade != null) ? radiusUpgrade.GetCurrentLevel() : 0;
        float currentRadius = (radiusUpgrade != null) ? radiusUpgrade.GetCurrentValue() : 0.5f;

        if (triggerCollider == null) triggerCollider = GetComponent<SphereCollider>();
        triggerCollider.radius = currentRadius;

        bool isPlaying = GameManager.Instance != null && GameManager.Instance.IsGamePlaying;
        bool shouldShowVisual = (currentLevel > 0) && isPlaying;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = shouldShowVisual;

            if (shouldShowVisual)
            {
                DrawVisualCircle(currentRadius);
            }
        }
    }

    private void DrawVisualCircle(float radius)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = circleSegments;

        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 point = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);

            lineRenderer.SetPosition(i, point);
        }
    }

    public void SetVisualEnabled(bool isVisible)
    {
        int currentLevel = radiusUpgrade.GetCurrentLevel();
        lineRenderer.enabled = isVisible && (currentLevel > 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Coin coin = other.GetComponent<Coin>();
            coin.Collect();
        }
    }
}
