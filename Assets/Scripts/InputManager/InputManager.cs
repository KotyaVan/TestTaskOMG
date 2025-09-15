using DefaultNamespace;
using DefaultNamespace.InputManager;
using DG.Tweening;
using ElementsGame.Views;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class InputManager : MonoBehaviour
{
    [SerializeField] private ElementsGameController gameController;

    private const float SwipeThreshold = 0.4f;

    private PieceView _selectedCellView;

    private BaseInputHandler _inputHandler;
    private Vector3 _touchBeginPosition;

    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            _inputHandler = gameObject.AddComponent<MobileInputHandler>();
        }
        else
        {
            _inputHandler = gameObject.AddComponent<PCInputHandler>();
        }

        _inputHandler.OnPointerDown += PointerDown;
        _inputHandler.OnPointerDrag += PointerDrag;
        _inputHandler.OnPointerUp += PointerUp;
    }

    private void OnDestroy()
    {
        if (_inputHandler != null)
        {
            _inputHandler.OnPointerDown -= PointerDown;
            _inputHandler.OnPointerDrag -= PointerDrag;
            _inputHandler.OnPointerUp -= PointerUp;
        }
    }


    private void PointerDown()
    {
        _selectedCellView = null;

        RaycastHit2D hit;
        var ray = Camera.main.ScreenPointToRay(_inputHandler.Position);
        hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null)
        {
            _selectedCellView = hit.collider.GetComponent<PieceView>();
            _touchBeginPosition = ray.origin;
        }
    }

    private void PointerDrag()
    {
        if (_selectedCellView != null)
        {
            var ray = Camera.main.ScreenPointToRay(_inputHandler.Position);
            var currentTouchPosition = ray.origin;

            var xDir = _touchBeginPosition.x - currentTouchPosition.x;
            var yDir = _touchBeginPosition.y - currentTouchPosition.y;
            if (Mathf.Abs(xDir) > SwipeThreshold)
            {
                if (xDir < 0)
                {
                    gameController.OnPieceSwiped(_selectedCellView, SwipeDirection.SwipeRight);
                }
                else
                {
                    gameController.OnPieceSwiped(_selectedCellView, SwipeDirection.SwipeLeft);
                }

                _selectedCellView = null;
            }
            else if (Mathf.Abs(yDir) > SwipeThreshold)
            {
                if (yDir < 0)
                {
                    gameController.OnPieceSwiped(_selectedCellView, SwipeDirection.SwipeUp);
                }
                else
                {
                    gameController.OnPieceSwiped(_selectedCellView, SwipeDirection.SwipeDown);
                }

                _selectedCellView = null;
            }
        }
    }

    private void PointerUp()
    {
        if (_selectedCellView != null)
        {
            _selectedCellView = null;
        }
    }
}