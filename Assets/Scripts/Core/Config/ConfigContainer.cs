using System;
using UnityEngine;

public class ConfigContainer : MonoBehaviour
{
    public static ConfigContainer Instance;

    [SerializeField] PlayerConfig playerConfig;
    [SerializeField] AudioConfig audioConfig;
    [SerializeField] SceneLoaderConfig sceneLoaderConfig;
    [SerializeField] SaveConfig saveConfig;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        DontDestroyOnLoad(this);
    }

    public PlayerConfig GetPlayerConfig()
    {
        return playerConfig;
    }
    public AudioConfig GetAudioConfig()
    {
        return audioConfig;
    }

    public SceneLoaderConfig GetSceneLoaderConfig()
    {
        return sceneLoaderConfig;
    }

    public SaveConfig GetSaveConfig()
    {
        return saveConfig;
    }
}
