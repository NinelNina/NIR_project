using UnityEngine;
using System;
using UtilityAI;

public class GameEventsManager : MonoBehaviour
{
    /// <summary>
    /// Чтобы подписаться/отписаться от эвента в нужных нам скриптах пишем
    /// GameEventsManager.НазваниеЭвента += (или -=) НазваниеМетода 
    /// </summary>

    public static Action<EntityType> OnEntityDeath;

    public static Action OnPlayerRespawned;

    public static Action OnPlayerControlsOn;

    public static Action OnPlayerControlsOff;

    //public static Action OnItemCollected;

    public static Action<int> OnFragmentCollected;

    public static Action OnStoryRecieved; // когда все фрагменты собраны, игрок получает историю

    public static Action<int> OnFoodCollected;

    public static Action<int> OnHealthChanged;

    //Boss

    public static Action<int> OnBossHealthChanged;

    public static Action<string, int> OnBossSetActive;

    //Level

    public static Action OnLevelComplete;

    public static Action OnLevelFailed;


    //Cохранения
    public static Action OnSaveRequested;

    public static Action OnSaveChanged;

    public enum EntityType
    {
        None,
        Player,
        BaseEnemy,
        Boss
    }
}