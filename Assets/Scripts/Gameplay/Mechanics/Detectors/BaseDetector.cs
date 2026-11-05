using System.Collections.Generic;
using UnityEngine;

public abstract class BaseDetector: BaseChecker
{
    public List<GameObject> DetectedObjects { get; protected set; } = new List<GameObject>();
}
