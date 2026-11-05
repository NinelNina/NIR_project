using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace Core.Utility
{
    public class GameplayUtilities
    {
        //Ёвенты
        public static Action<Rigidbody2D, Transform> OnPlayerTeleportRequest;
        public static Action<Rigidbody2D, Transform, float, Transform> OnPlayerKickOutRequest;
         
        public static Action<float> OnInvulnarableStateRequest;

        //—осто€ние неу€звимости
        private bool _invulInEffect = false;
        private float timerForInvul = 0f;

        public void Initialize()
        {
            OnPlayerTeleportRequest += TeleportPlayerTo;
            OnPlayerKickOutRequest += KickOutPlayer;
            OnInvulnarableStateRequest += SetInvulnarableState;
        }

        public void Tick()
        {
            if (_invulInEffect)
            {
                timerForInvul -= Time.deltaTime;

                if (timerForInvul <= 0)
                {
                    _invulInEffect = false;
                    Debug.Log($"—осто€ние неу€звимости: {_invulInEffect}");
                }
            }
        }

        public void TeleportPlayerTo(Rigidbody2D playerRb, Transform teleportPoint)
        {
            GameEventsManager.OnPlayerControlsOff?.Invoke();

            playerRb.linearVelocity = Vector2.zero;
            playerRb.position = (Vector2)teleportPoint.position + new Vector2(0, 0.5f);

            GameEventsManager.OnPlayerControlsOn?.Invoke();
        }

        public async void KickOutPlayer(Rigidbody2D playerRb, Transform originalPoint, float kickOutForce, Transform teleportPoint = default)
        {
            //ќтбрасываем игрока от ловушки
            float kickPosX = playerRb.position.x - originalPoint.position.x;

            Vector2 kickOutVector = new Vector2(kickPosX, 1);
            playerRb.AddForce(kickOutVector.normalized * kickOutForce, ForceMode2D.Impulse);

            //„уть-чуть ждЄм и включаем управление обратно
            await UniTask.Delay(350);
            GameEventsManager.OnPlayerControlsOn?.Invoke();

            //≈сли есть куда - то телепортируем игрока, чтобы он на эту же ловушку не упал
            if (teleportPoint != default)
            {
                TeleportPlayerTo(playerRb, teleportPoint);
            }
        }

        public void SetInvulnarableState(float invulTimer)
        {
            _invulInEffect = true;
            timerForInvul = invulTimer;
        }

        public bool GetInvulnarableState()
        {
            return _invulInEffect;
        }
    }
}

