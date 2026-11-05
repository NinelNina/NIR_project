using System;
using UnityEngine;
using UtilityAI;

public class SettingsService : ISettingsService
{
    public GameSettingsData Current { get; private set; } = new();

    private const string KEY_PREFIX = "Settings_";

    public static event Action OnSettingsChanged;

    public void ApplyNewSettings(GameSettingsData newSettings)
    {
        Current = newSettings;
        Save();
        OnSettingsChanged?.Invoke();
    }

    public void Load()
    {
        Current.MasterVolume = PlayerPrefs.GetFloat(KEY_PREFIX + "Master", 1f);
        Current.MusicVolume = PlayerPrefs.GetFloat(KEY_PREFIX + "Music", 1f);
        Current.SFXVolume = PlayerPrefs.GetFloat(KEY_PREFIX + "SFX", 1f);

        Current.Graphics.QualityLevel = PlayerPrefs.GetInt(KEY_PREFIX + "Graphics", 3);
        Current.Graphics.Fullscreen = PlayerPrefs.GetInt(KEY_PREFIX + "Fullscreen", 1) == 1;
        Current.Graphics.VSync = PlayerPrefs.GetInt(KEY_PREFIX + "VSync", 1) == 1;
        Current.Graphics.ResolutionWidth = PlayerPrefs.GetInt(KEY_PREFIX + "ResW", 1920);
        Current.Graphics.ResolutionHeight = PlayerPrefs.GetInt(KEY_PREFIX + "ResH", 1080);
    }

    public void Save()
    {
        PlayerPrefs.SetFloat(KEY_PREFIX + "Master", Current.MasterVolume);
        PlayerPrefs.SetFloat(KEY_PREFIX + "Music", Current.MusicVolume);
        PlayerPrefs.SetFloat(KEY_PREFIX + "SFX", Current.SFXVolume);

        Current.Graphics.QualityLevel = PlayerPrefs.GetInt(KEY_PREFIX + "Graphics", 3);
        Current.Graphics.Fullscreen = PlayerPrefs.GetInt(KEY_PREFIX + "Fullscreen", 1) == 1;
        Current.Graphics.VSync = PlayerPrefs.GetInt(KEY_PREFIX + "VSync", 1) == 1;
        Current.Graphics.ResolutionWidth = PlayerPrefs.GetInt(KEY_PREFIX + "ResW", 1920);
        Current.Graphics.ResolutionHeight = PlayerPrefs.GetInt(KEY_PREFIX + "ResH", 1080);

        PlayerPrefs.Save();
    }
}