using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using Core.Utility;
using System;
using UtilityAI;

public class BossController : MonoBehaviour
{
    [SerializeField] BossInfo _bossInfo;

    [Header("Boss States")]
    [SerializeField] float _stateTimer;
    private float _currentStateTimer;

    [SerializeField] MonoBehaviour _baseState;

    [SerializeField] MonoBehaviour _vulnarableState;

    [SerializeField] List<MonoBehaviour> _states;

    [Header("Hitbox")]
    [SerializeField] HitBoxCheck _hitBoxCheck;

    //For public states (to allow other states transfer to these states if necessary)
    [HideInInspector] public MonoBehaviour BaseState => _baseState;
    [HideInInspector] public MonoBehaviour VulnarableState => _vulnarableState;

    //Boss states
    private List<IBossState> _ApprovedStates = new List<IBossState>();
    private IBossState _currentState;
    private StateType _currentStateType;
    private bool _CanChangeStates = false;

    //Other
    private HealthSystem _healthSystem;

    //Event
    public static Action OnBaseStateRequest;
    public static Action OnVulnerableStateRequest;

    private void Start()
    {
        OnBaseStateRequest += ChangeToBaseState;
        OnVulnerableStateRequest += ChangeToVulnerableState;

        CheckStates();

        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.SetMaxHp(_bossInfo.bossHp);

        GameEventsManager.OnBossSetActive?.Invoke(_bossInfo.bossName, _bossInfo.bossHp);

        _currentStateTimer = _stateTimer;
    }

    private void Update()
    {
        if (_currentStateType == StateType.Soft)
        {
            _currentState.Execute();
        }

        if (!_CanChangeStates)
        {
            StateTimer();
        }
        

        if (_CanChangeStates && _currentStateType != StateType.Strict)
        {
            EvaluateStates();
        }

        var hit = _hitBoxCheck.GetHitBoxHit();
        if (hit)
        {
            var playerRb = hit.GetComponentInParent<Rigidbody2D>();
            GameplayUtilities.OnPlayerKickOutRequest?.Invoke(playerRb, transform, 3, default);
        }

        _currentState.Tick();
    }

    private void StateTimer()
    {
        _currentStateTimer -= Time.deltaTime;

        if (_currentStateTimer <= 0)
        {
            _currentStateTimer = _stateTimer;

            if (_currentStateType == StateType.Soft)
            {
                _CanChangeStates = true;
            }
        }
    }

    private void EvaluateStates()
    {
        List<IBossState> nextPossibleStates = new List<IBossState>();
        
        foreach (IBossState state in _states)
        {
            if (state.Evaluate())
            {
                nextPossibleStates.Add(state);
            }
        }

        nextPossibleStates = ShuffleList(nextPossibleStates);

        int randomIndex = Random.Range(0, nextPossibleStates.Count);

        StateChange(nextPossibleStates[randomIndex]);
    }

    private List<IBossState> ShuffleList(List<IBossState> list)
    {
        return list;
    }

    private void ChangeToBaseState()
    {
        StateChange((IBossState)_baseState);
    }

    private void ChangeToVulnerableState()
    {
        if (_vulnarableState != null)
        {
            StateChange((IBossState)_vulnarableState);
        }
    }

    public void StateChange(IBossState nextState)
    {
        _currentState = nextState;

        _currentStateType = nextState.GetStateType();

        _CanChangeStates = false;

        if (_currentStateType == StateType.Strict)
        {
            _currentState.Execute();
        }
    }

    private void FixedUpdate()
    {
        //=================[BOSS HP CHECK]=======================
        //RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 10, 12);

        //if (hit)
        //{
        //    _healthSystem.TakeDamage(1);
        //    Debug.Log("Игрок ударил");
        //}
    }

    private void CheckStates()
    {
        if (_baseState is IBossState)
        {
            _currentState = (IBossState)_baseState;
        }
        else
        {
            Debug.LogError("Базовое состояние не является наследником IBossState!!!");
        }

        foreach (var state in _states)
        {
            if (state != null && state is IState)
            {
                _ApprovedStates.Add(state as IBossState);
            }
        }
    }
}
