using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float updateInterval = 0.5f;

    private float accumTime = 0f;
    private int framesCount = 0;
    private float timeLeft;

    private void Start()
    {
        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        accumTime += Time.unscaledDeltaTime;
        framesCount++;

        if (timeLeft <= 0.0f)
        {
            float fps = framesCount / accumTime;

            if (fpsText != null)
            {
                fpsText.text = $"{Mathf.RoundToInt(fps)} FPS";
            }

            timeLeft = updateInterval;
            accumTime = 0f;
            framesCount = 0;
        }
    }
}