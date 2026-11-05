using UnityEngine;

public class PlayerDetector : MonoBehaviour
{
    private Transform _player;

    private void Awake()
    {
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerController>().transform;
        }
    }

    public float GetDistanceToPlayer()
    {
        return Vector2.Distance(transform.position, _player.position);
    }

    public Transform GetPlayer()
    {
        return _player;
    }
}
