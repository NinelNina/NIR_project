using UnityEngine;

    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Configs/Player")]
    public class PlayerConfig : ScriptableObject
    {
        public GameObject PlayerPrefab;
        public Vector3 DefaultSpawnPosition = Vector3.zero;
        public int StartLives = 3;
    }
