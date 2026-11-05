using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New BossInfo", menuName = "BossInfo")]
public class BossInfo : ScriptableObject
{
    [Header("Base Info")]
    public string bossName;
    public int bossHp;
}
