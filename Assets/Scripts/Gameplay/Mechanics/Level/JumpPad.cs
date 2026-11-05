using UnityEngine;

public class JumpPad : MonoBehaviour, ITrap
{
    [SerializeField] float jumpPower;
    [SerializeField] Vector2 direction;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D player = collision.gameObject.GetComponent<Rigidbody2D>();

        if (player != null)
        {
            ActivateTrap(null, player);
        }
    }

    public void ActivateTrap(HealthSystem health = null, Rigidbody2D rb = null, Transform teleportPoint = null)
    {
        rb.AddForce(direction * jumpPower, ForceMode2D.Impulse);
    }
}
