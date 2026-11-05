using Cysharp.Threading.Tasks;
using UnityEngine;

public class RandomMoveState : MonoBehaviour, IBossState
{
    PlayerDetector playerDetector;
    Transform targetPlayer;
    float distanceToPlayer;
    float playerDirection;
    [SerializeField] bool isFacingLeft = true;

    [SerializeField] float moveSpeed;
    [SerializeField, Range(0.1f, 10)] float distanceThreshhold;
    [SerializeField] Transform visualBody;
    Rigidbody2D rb;
    int randomDirection = 0;
    private void Start()
    {
        playerDetector = GetComponent<PlayerDetector>();
        rb = GetComponent<Rigidbody2D>();

        targetPlayer = playerDetector.GetPlayer();
    }
    public bool Evaluate()
    {
        randomDirection = Random.Range(-1f, 1f) > 0 ? 1 : -1;
        return true;
    }

    public void Tick()
    {
        //nothing;
    }

    public void Execute()
    {
        distanceToPlayer = playerDetector.GetDistanceToPlayer();
        playerDirection = targetPlayer.position.x - transform.position.x > 0 ? 1 : -1;

        if (playerDirection < 0 && !isFacingLeft)
        {
            Flip(180, true);
        }

        if (playerDirection > 0 && isFacingLeft)
        {
            Flip(0, false);
        }


        if (distanceToPlayer > distanceThreshhold)
        {
            if (!Physics2D.Raycast(transform.position, Vector2.right * randomDirection, 3))
            {
                rb.AddForce(new Vector2(randomDirection, 0) * moveSpeed * 10, ForceMode2D.Force);
            }
            else
            {
                rb.AddForce(new Vector2(-randomDirection, 0) * moveSpeed * 10, ForceMode2D.Force);
            }
            
        }
        else
        {
            rb.AddForce(Vector2.right * -playerDirection * moveSpeed * 10, ForceMode2D.Force);
        }
    }
    private void Flip(float angleY, bool isLeft)
    {
        visualBody.rotation = new Quaternion(0, angleY, 0, 0);
        isFacingLeft = isLeft;
    }

    public StateType GetStateType()
    {
        return StateType.Soft;
    }
}
