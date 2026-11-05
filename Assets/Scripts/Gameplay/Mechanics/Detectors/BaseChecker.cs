using UnityEngine;
using UnityEngine.Events;

public abstract class BaseChecker : MonoBehaviour
{
    public UnityEvent<bool> StateChangedEvent;

    public bool IsAutomated = true;

    public virtual bool IsDetecting
    {
        get => _isDetecting;
        protected set
        {
            var temp = _isDetecting;
            _isDetecting = value;
            if (temp != _isDetecting)
            {
                StateChangedEvent.Invoke(_isDetecting);
            }
        }
    }

    private bool _isDetecting;

    public virtual void UpdateDetection()
    {

    }

    protected virtual void Update()
    {
        if (IsAutomated)
        {
            UpdateDetection();
        }
    }
}
