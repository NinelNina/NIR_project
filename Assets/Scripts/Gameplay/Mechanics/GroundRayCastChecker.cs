using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class GroundRayCastChecker : BaseChecker
{
    [SerializeField] private List<CustomRayCast> _origins = new List<CustomRayCast>();

    public List<bool> CheckGroundCollision()
    {
        int n = _origins.Count;
        bool[] ans = new bool[n];
        for (int i = 0; i < n; i++)
        {
            ans[i] = _origins[i].CheckCollision(out var point);
        }

        return ans.ToList();
    }

    public override void UpdateDetection()
    {
        IsDetecting = CheckGroundCollision().Any(a => a == true);
    }
}
