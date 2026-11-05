using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ITrap
{
    public void ActivateTrap(HealthSystem health = default, Rigidbody2D rb = default, Transform teleportPoint = default);
}
