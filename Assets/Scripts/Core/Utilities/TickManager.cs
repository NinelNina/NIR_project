using UnityEngine;
using Reflex.Attributes;

namespace Core.Utility
{
    public class TickManager : MonoBehaviour
    {
        //Все инжекты
        [Inject] private GameplayUtilities _utilities;

        private void Start()
        {
            _utilities.Initialize();
        }

        private void Update()
        {
            _utilities.Tick();
        }
    }
}

