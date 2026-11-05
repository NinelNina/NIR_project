using UnityEngine;
using Reflex.Attributes;
using Core.Services;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class SecretPassage : MonoBehaviour
{
    [Inject] private AudioService _audioService;
    private List<SpriteRenderer> _sprites = new List<SpriteRenderer>();

    private void Start()
    {
        foreach (Transform obj in transform)
        {
            var sprteRenderer = obj.gameObject.GetComponent<SpriteRenderer>();

            if (sprteRenderer!= null )
            {
                _sprites.Add(sprteRenderer);
            }
           
        }
    }

    private async void OnTriggerEnter2D(Collider2D collision)
    {
        //Проигрываем звук найденного секрета (Потом поменять)
        //await _audioService.PlaySfx("СекретВАУ");

        //Спрайты магическим образом исчезают (потом сделать нормально)
        foreach (SpriteRenderer sprite in _sprites)
        {
            sprite.color = new Color(255, 255, 255, 0);

            await UniTask.Delay(25);

            sprite.gameObject.SetActive(false);
        }
    }
}
