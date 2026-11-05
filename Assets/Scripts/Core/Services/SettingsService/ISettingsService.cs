using System;
using UtilityAI;

public interface ISettingsService
{
    GameSettingsData Current { get; }
    public static event Action OnSettingsChanged;
    void Save();
    void Load();
    void ApplyNewSettings(GameSettingsData newSettings);
}