using UnityEngine;

public interface IState
{
    public void EnterState();

    public void ExitState();

    public void Execute();
}
