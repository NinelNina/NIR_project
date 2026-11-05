using System.Collections;
using UnityEngine;

public class DeleteOnTime : MonoBehaviour
{
    [SerializeField] private float _lifetime;

    private void Start()
    {
        StartCoroutine(Wait(_lifetime));
    }

    private void OnDelete()
    {
        Destroy(gameObject);
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        OnDelete();
    }
}
