using UnityEngine;

 [CreateAssetMenu(menuName = "Configs/Save")]
 public class SaveConfig : ScriptableObject
 {
    public int MaxSaveSlots = 3;
    public bool AutoSaveEnabled = true;
    public float AutoSaveInterval = 30f;
    public bool EnableBackups = true;
    public int MaxBackupsPerSlot = 5;
    public string BackupFormat = "yyyy-MM-dd_HH-mm-ss";
 }


