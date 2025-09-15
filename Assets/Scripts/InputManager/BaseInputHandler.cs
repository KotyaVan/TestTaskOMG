using System;
using UnityEngine;

namespace DefaultNamespace.InputManager
{
    public abstract class BaseInputHandler : MonoBehaviour
    {
        public Vector3 Position { get; set; }
        public Action OnPointerDown { get; set; }
        public Action OnPointerDrag { get; set; }
        public Action OnPointerUp { get; set; }
    }
}