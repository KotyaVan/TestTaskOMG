using System;
using UnityEngine;

namespace DefaultNamespace.InputManager
{
    public class PCInputHandler : BaseInputHandler
    {
        private void Update()
        {
            Position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown?.Invoke();
            }

            if (Input.GetMouseButton(0))
            {
                OnPointerDrag?.Invoke();
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp?.Invoke();
            }
        }
    }
}