using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private const string AUDIO_MUTED_KEY = "IsAudioMuted";
    public bool IsMuted { get; private set; }

    public Toggle audioToggle;

    public Image toggleIconImage;
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        IsMuted = PlayerPrefs.GetInt(AUDIO_MUTED_KEY, 0) == 1;
        ApplyAudioState();

        audioToggle.isOn = !IsMuted;
        UpdateIcon(!IsMuted);
        audioToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void UpdateIcon(bool soundEnabled)
    {
        toggleIconImage.sprite = soundEnabled ? soundOnSprite : soundOffSprite;
    }

    private void OnToggleChanged(bool soundEnabled)
    {
        SetMute(!soundEnabled);
        UpdateIcon(soundEnabled);
    }

    public void SetMute(bool mute)
    {
        IsMuted = mute;
        ApplyAudioState();

        PlayerPrefs.SetInt(AUDIO_MUTED_KEY, IsMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyAudioState()
    {
        AudioListener.volume = IsMuted ? 0.0f : 1.0f;
    }
}
