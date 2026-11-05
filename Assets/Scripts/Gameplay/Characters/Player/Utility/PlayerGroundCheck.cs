using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRaycastsCheck : MonoBehaviour
{
    [SerializeField] List<Transform> _groundCheckPoints;
    [SerializeField] LayerMask _layerMask;
    [SerializeField] float _rayCastDistance;

    [Header("Ceiling")]
    [SerializeField] Transform[] _ceilingRayCastsInner;
    [SerializeField] Transform[] _ceilingRayCastsOuter;
    bool _isCeilingHit;

    [Header("Ledge Climb")]
    [SerializeField] Transform _ledgeCheckRight;
    [SerializeField] Transform _ledgeCheckLeft;
    [SerializeField] float _ledgeCheckRadius;
    bool _isRightLedge;
    bool _canDetectLedge;
    PlayerLedgeCheck _ledgeCheck;

    private void Start()
    {
        _ledgeCheck = GetComponentInChildren<PlayerLedgeCheck>();
        _ledgeCheck.OnLedgeDetectAbility += OnLedgeCheck;
    }

    private void FixedUpdate()
    {
        CheckAllCollisions();
    }

    private void CheckAllCollisions()
    {
        GroundCheck();
        CeilingCheck();
        LedgeClimbingCheck();
    }

    private void GroundCheck()
    {
        //GroundChecks

        RaycastHit2D groundHit = default;

        foreach (Transform raycast in _groundCheckPoints)
        {
            groundHit = Physics2D.Raycast(raycast.position, Vector2.down, _rayCastDistance, _layerMask);

            if (groundHit)
            {
                break;
            }
        }

        if (groundHit)
        {
            PlatformEffector2D platform = groundHit.transform.GetComponent<PlatformEffector2D>();
            if (platform != null)
            {
                OnGroundHitChanged?.Invoke(true);
                OnPlatformStandingChanged?.Invoke(true, platform);
            }
            else
            {
                OnGroundHitChanged?.Invoke(true);
                OnPlatformStandingChanged?.Invoke(false, null);
            }
        }
        else
        {
            OnGroundHitChanged?.Invoke(false);
            OnPlatformStandingChanged?.Invoke(false, null);
        }
    }

    private void CeilingCheck()
    {
        RaycastHit2D ceilingHitInner = default;
        RaycastHit2D ceilingHitOuter = default;
        int ceilingHitInnerCounter = 0;
        int ceilingHitOuterCounter = 0;

        foreach (Transform raycast in _ceilingRayCastsInner)
        {
            ceilingHitInner = Physics2D.Raycast(raycast.position, Vector2.up, _rayCastDistance, _layerMask);

            if (ceilingHitInner)
            {
                ceilingHitInnerCounter++;
            }
        }

        foreach (Transform raycast in _ceilingRayCastsOuter)
        {
            ceilingHitOuter = Physics2D.Raycast(raycast.position, Vector2.up, _rayCastDistance, _layerMask);

            if (ceilingHitOuter)
            {
                ceilingHitOuterCounter++;
            }
        }

        //If Inner and Outer raycasts hit the same amount and player doesn't moves, it hits the ceiling and should be stopped
        if ((ceilingHitInnerCounter > 0 || ceilingHitOuterCounter > 0) && (!CheckForPlatform(ceilingHitInner) || !CheckForPlatform(ceilingHitOuter)))
        {
            OnCeilingHitChanged?.Invoke(true);
            _isCeilingHit = true;
        }
        else
        {
            OnCeilingHitChanged?.Invoke(false);
            _isCeilingHit = false;
        }
    }

    private void OnLedgeCheck(bool canDetectLedge)
    {
        if (canDetectLedge)
        {
            _canDetectLedge = true;
        }
        else
        {
            _canDetectLedge = false;
        }
    }

    private void LedgeClimbingCheck()
    {
        Collider2D ledgeHitRight = Physics2D.OverlapCircle(_ledgeCheckRight.position, _ledgeCheckRadius, _layerMask);
        Collider2D ledgeHitLeft = Physics2D.OverlapCircle(_ledgeCheckLeft.position, _ledgeCheckRadius, _layerMask);

        if (!_canDetectLedge)
        {
            return;
        }

        if (ledgeHitRight != null && ledgeHitRight.gameObject.GetComponent<PlatformEffector2D>() != null)
        {
            return;
        }

        if (ledgeHitLeft != null && ledgeHitLeft.gameObject.GetComponent<PlatformEffector2D>() != null)
        {
            return;
        }

        if (ledgeHitRight != null && !_isCeilingHit)
        {
            OnLedgeDetected?.Invoke(true, true);
        }
        else if (ledgeHitLeft != null && !_isCeilingHit)
        {
            OnLedgeDetected?.Invoke(true, false);
        }
        else
        {
            OnLedgeDetected?.Invoke(false, false);
        }
    }

    private bool CheckForPlatform(RaycastHit2D hit)
    {
        if (hit.transform != null && hit.transform.GetComponent<PlatformEffector2D>() != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Action<bool> OnGroundHitChanged;
    public Action<bool, PlatformEffector2D> OnPlatformStandingChanged;
    public Action<bool> OnCeilingHitChanged;
    public Action<bool, bool> OnLedgeDetected;
}
