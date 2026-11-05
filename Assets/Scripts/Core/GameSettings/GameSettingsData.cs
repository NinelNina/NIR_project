using UnityEngine;
using System;
using Core.Graphics;

[Serializable]
public class GameSettingsData
{
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float MusicVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;

    public GraphicsSettingsData Graphics { get; set; } = new GraphicsSettingsData();

    public string LanguageCode = "ru";
}
