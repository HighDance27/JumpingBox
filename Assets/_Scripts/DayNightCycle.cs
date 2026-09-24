using System.Collections;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Day duration in seconds")]
    public float dayDuration = 60f;

    public Light directionalLight;
    public Camera mainCamera;

    public Color dayColor = new Color(0.53f, 0.81f, 0.98f);     // Blue Sky
    public Color sunsetColor = new Color(0.95f, 0.51f, 0.25f);     // Orange
    public Color nightColor = new Color(0.05f, 0.05f, 0.12f);   //deep blue

    public Color dayLightColor = new Color(1f, 0.95f, 0.84f);
    public Color nightLightColor = new Color(0.18f, 0.24f, 0.35f);
    public float maxLightIntensity = 1.2f;
    public float minLightIntensity = 0.1f;

    private float rotationSpeed;
    private bool isRunning = false;

    [Header("Biome Transitions")]
    public float biomeTransitionDuration = 2.0f;
    private Coroutine colorTransitionCoroutine;

    private Quaternion initialLightRotation;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }

        if (directionalLight == null)
        {
            directionalLight = GetComponent<Light>();
        }

        if (directionalLight != null)
        {
            initialLightRotation = directionalLight.transform.rotation;
        }
    }

    private void Start()
    {
        UpdateSkyAndLight();
    }

    private void Update()
    {
        if (!isRunning) return;
        rotationSpeed = 360f / dayDuration;
        directionalLight.transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        UpdateSkyAndLight();
    }

    public void StartCycle()
    {
        isRunning = true;
    }

    public void StopCycle()
    {
        isRunning = false;
    }

    public void ResetCycle()
    {
        isRunning = false;
        directionalLight.transform.rotation = initialLightRotation;
        UpdateSkyAndLight();
    }

    private void UpdateSkyAndLight()
    {
        float sunElevation = Vector3.Dot(directionalLight.transform.forward, Vector3.down);

        float cycleProgress = Mathf.InverseLerp(-1f, 1f, sunElevation);

        if (sunElevation > 0.2f)
        {
            // Day to Sunset blend
            float t = Mathf.InverseLerp(0.2f, 1f, sunElevation);
            mainCamera.backgroundColor = Color.Lerp(sunsetColor, dayColor, t);
        }
        else if (sunElevation > -0.1f)
        {
            // Sunset to Night blend
            float t = Mathf.InverseLerp(-0.1f, 0.2f, sunElevation);
            mainCamera.backgroundColor = Color.Lerp(nightColor, sunsetColor, t);
        }
        else
        {
            mainCamera.backgroundColor = nightColor;
        }

        directionalLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, cycleProgress);
        directionalLight.color = Color.Lerp(nightLightColor, dayLightColor, cycleProgress);
    }

    public void ApplyBiomeColors(BiomeData biome, bool instant = false)
    {
        if (biome == null) return;

        if (colorTransitionCoroutine != null) StopCoroutine(colorTransitionCoroutine);

        if (instant)
        {
            dayColor = biome.dayColor;
            sunsetColor = biome.sunsetColor;
            nightColor = biome.nightColor;

            dayLightColor = biome.dayLightColor;
            nightLightColor = biome.nightLightColor;

            UpdateSkyAndLight();
        }
        else
        {
            colorTransitionCoroutine = StartCoroutine(TransitionColorsCoroutine(biome, biomeTransitionDuration));
        }
    }

    private IEnumerator TransitionColorsCoroutine(BiomeData targetBiome, float duration)
    {
        Color startDay = dayColor;
        Color startSunset = sunsetColor;
        Color startNight = nightColor;
        Color startDayLight = dayLightColor;
        Color startNightLight = nightLightColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            dayColor = Color.Lerp(startDay, targetBiome.dayColor, t);
            sunsetColor = Color.Lerp(startSunset, targetBiome.sunsetColor, t);
            nightColor = Color.Lerp(startNight, targetBiome.nightColor, t);

            dayLightColor = Color.Lerp(dayLightColor, targetBiome.dayLightColor, t);
            nightLightColor = Color.Lerp(nightLightColor, targetBiome.nightLightColor, t);

            UpdateSkyAndLight();
            yield return null;
        }

        //Snap value at the end of the timer
        dayColor = targetBiome.dayColor;
        sunsetColor = targetBiome.sunsetColor;
        nightColor = targetBiome.nightColor;
        dayLightColor = targetBiome.dayLightColor;
        nightLightColor = targetBiome.nightLightColor;

        UpdateSkyAndLight();
    }
}
