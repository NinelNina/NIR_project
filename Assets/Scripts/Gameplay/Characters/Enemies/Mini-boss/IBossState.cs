using UnityEngine;

public interface IBossState
{
    public bool Evaluate();

    public void Tick();

    public void Execute();

    public StateType GetStateType();
}

public enum StateType
{
    Soft, //Soft is when State can be changed whenever without any extreme consequences
    Strict // Strict is when Stat can be changed ONLY if it has ended its' execution
}
