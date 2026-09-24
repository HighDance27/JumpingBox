using UnityEngine;
using UnityEngine.UI;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; }

    private const string VIBRATION_KEY = "VibrationEnabled";
    public bool IsVibrationEnabled { get; private set; }

    public Toggle vibrationToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        IsVibrationEnabled = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;

        vibrationToggle.isOn = IsVibrationEnabled;
        vibrationToggle.onValueChanged.AddListener(SetVibrationEnabled);
    }

    public void SetVibrationEnabled(bool enabled)
    {
        IsVibrationEnabled = enabled;
        PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void TriggerDeathVibration()
    {
        if (!IsVibrationEnabled) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
