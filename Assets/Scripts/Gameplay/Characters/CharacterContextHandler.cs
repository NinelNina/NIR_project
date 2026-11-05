using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Characters
{
    public abstract class CharacterContextHandler : MonoBehaviour
    {
        public abstract CharacterContext Context { get; }

        protected void Awake()
        {
            Context.CharacterObject = gameObject;
            SetupContext();
        }

        protected abstract void SetupContext();
    }
}
