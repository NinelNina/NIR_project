using UnityEngine;

public class HitBoxCheck : MonoBehaviour
{
    private Collider2D hit;

    private bool _isHitRecently;
    private float timer = 0.7f;
    private float _currentTimer;

    private void Update()
    {
        if (_isHitRecently)
        {
            _currentTimer += Time.deltaTime;

            if (_currentTimer >= timer)
            {
                _isHitRecently = false;
                _currentTimer = 0;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_isHitRecently)
        {
            hit = collision;
            _isHitRecently = true;
        }
        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        hit = null;
    }

    public Collider2D GetHitBoxHit()
    {
        return hit;
    }
}
