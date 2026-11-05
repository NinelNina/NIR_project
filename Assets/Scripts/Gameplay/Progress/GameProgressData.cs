using System;
using System.Collections.Generic;

[Serializable]
public class GameProgressData
{
    // Игрок
    public string PlayerName = "Player";
    public int PlayerLevel = 1;
    public int Experience = 0;
    public int Food = 0;
    public InventoryData Inventory = new();

    // Прогресс
    public int CurrentLevel = 1;
    public HashSet<string> UnlockedLevels = new() { "level_1" };
    public Dictionary<string, QuestProgress> Quests = new();

    // Время
    public float TotalPlayTime;
    public DateTime LastPlayed;

    // Настройки (копия из Settings)
    public GameSettingsData Settings = new();
}

[Serializable]
public class InventoryData
{
    public List<ItemData> Items = new();
    public Dictionary<string, int> ItemCounts = new();
}

[Serializable]
public class ItemData
{
    public string Id;
    public int Quantity;
    public DateTime AcquiredAt;
}

[Serializable]
public class QuestProgress
{
    public string QuestId;
    public int CurrentStep;
    public bool IsCompleted;
    public DateTime StartedAt;
}