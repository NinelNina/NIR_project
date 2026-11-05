using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "Configs/AudioConfig")]
public class AudioConfig : ScriptableObject
{
    [Header("System settings")]
    [SerializeField] private float _masterVolume = 1f;
    [SerializeField] private float _musicVolume = 0.8f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField] private float _voiceVolume = 1f;

    [Header("Technical settings")]
    [SerializeField] private int _maxConcurrentSfx = 10;
    [SerializeField] private float _audioCleanupInterval = 20f;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Header("UI universal sounds")]
    [SerializeField] private AudioClip _buttonClick;
    [SerializeField] private AudioClip _menuOpen;
    [SerializeField] private AudioClip _menuClose;

    public float MasterVolume
    {
        get => _masterVolume;
        set => _masterVolume = Mathf.Clamp01(value);
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set => _musicVolume = Mathf.Clamp01(value);
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set => _sfxVolume = Mathf.Clamp01(value);
    }
    public float VoiceVolume
    {
        get => _voiceVolume;
        set => _voiceVolume = Mathf.Clamp01(value);
    }

    public int MaxConcurrentSfx => _maxConcurrentSfx;
    public float AudioCleanupInterval => _audioCleanupInterval;

    public AudioMixer AudioMixer => _audioMixer;

    public AudioClip ButtonClick => _buttonClick;
    public AudioClip MenuOpen => _menuOpen;

    public AudioClip MenuClose => _menuClose;
}