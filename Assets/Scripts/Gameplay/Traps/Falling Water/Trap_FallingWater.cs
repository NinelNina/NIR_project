using UnityEngine;

public class Trap_FallingWater : MonoBehaviour, ITrap
{
    [SerializeField] Trap_WaterDroplet droplet;
    [SerializeField] Transform dropletSpawnPoint;
    [SerializeField] float timeToSpawnDroplet;
    private float currentTimer;

    private void Start()
    {
        currentTimer = timeToSpawnDroplet;
    }

    private void Update()
    {
        currentTimer -= Time.deltaTime;
        if (currentTimer <= 0)
        {
            ActivateTrap();
            currentTimer = timeToSpawnDroplet;
        }
    }

    public void ActivateTrap(HealthSystem health = null, Rigidbody2D rb = null, Transform teleportPoint = null)
    {
        var newDroplet = Instantiate(droplet, dropletSpawnPoint.position, droplet.transform.rotation);
    }
}
