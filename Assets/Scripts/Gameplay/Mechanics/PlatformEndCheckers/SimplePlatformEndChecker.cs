using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class SimplePlatformEndChecker : BasePlatformEndChecker
{
    [SerializeField] private CustomRayCast _leftRay;
    [SerializeField] private CustomRayCast _rightRay;

    public override bool CheckRight()
    {
        return _rightRay.CheckCollision(out var _);
    }

    public override bool CheckLeft()
    {
        return _leftRay.CheckCollision(out var _);
    }
}
