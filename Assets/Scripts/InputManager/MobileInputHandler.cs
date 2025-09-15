using System;
using UnityEngine;

namespace DefaultNamespace.InputManager
{
    public class MobileInputHandler : BaseInputHandler
    {
        private int _activeTouchIndex = -1;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                for (var index = 0; index < Input.touches.Length; index++)
                {
                    Touch touch = Input.touches[index];

                    if (touch.phase == TouchPhase.Began)
                    {
                        PointerUp();

                        _activeTouchIndex = index;
                        PointerDown();
                    }

                    if (_activeTouchIndex == index &&
                        (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                    {
                        PointerUp();
                    }
                }
                
                PointerDrag();
            }
            else
            {
                PointerUp();
            }
        }

        private void PointerDown()
        {
            if (_activeTouchIndex != -1)
            {
                if (Input.touches.Length > _activeTouchIndex)
                {
                    Position = Input.touches[_activeTouchIndex].position;
                }

                OnPointerDown?.Invoke();
            }
        }

        private void PointerDrag()
        {
            if (_activeTouchIndex != -1)
            {
                if (Input.touches.Length > _activeTouchIndex)
                {
                    Position = Input.touches[_activeTouchIndex].position;
                }

                OnPointerDrag?.Invoke();
            }
        }

        private void PointerUp()
        {
            if (_activeTouchIndex != -1)
            {
                if (Input.touches.Length > _activeTouchIndex)
                {
                    Position = Input.touches[_activeTouchIndex].position;
                }

                _activeTouchIndex = -1;
                OnPointerUp?.Invoke();
            }
        }
    }
}