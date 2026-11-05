using NUnit.Framework;
using UnityEngine;

using System.Collections.Generic;

public class StaticDetector : BaseDetector
{
    [SerializeField] private List<CustomRayCast> _rayCasts;

    [SerializeField] private string _targetTag;

    public override void UpdateDetection()
    {
        List<GameObject> detected = new List<GameObject>();

        foreach (var el in _rayCasts)
        {
            if (el.CheckCollision(out var collisionPoint) && collisionPoint.CompareTag(_targetTag))
            {
                detected.Add(collisionPoint.gameObject);
            }
        }

        DetectedObjects = new List<GameObject>(detected);
        IsDetecting = (DetectedObjects.Count > 0);
    }
}
