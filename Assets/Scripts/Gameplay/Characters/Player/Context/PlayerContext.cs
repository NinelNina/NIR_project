using Characters;
using System;
using UnityEngine;

namespace Characters.Player
{
    public class PlayerContext : CharacterContext
    {
        public Rigidbody2D Rb;
        public Animator Animator;
        public Renderer Renderer;
        public DefaultMovement Movement;

        public float HorizontalDirection = 0;

        public bool IsJumpPressed = false;
        public bool IsCrouchPressed = false;
        public bool IsGrounded = false;
    }
}