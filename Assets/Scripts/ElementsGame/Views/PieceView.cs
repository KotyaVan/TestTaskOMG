using System;
using System.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.InputManager;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace ElementsGame.Views
{
    public class PieceView : MonoBehaviour
    {
        [SerializeField] public SpriteRenderer pieceSprite;
        [SerializeField] private Animator animator;

        public PieceModel PieceModel;
        private Action _onAnimationComplete;

        public void Initialize(PieceModel model)
        {
            PieceModel = model;
        }

        public void Fall(Vector3 targetPos, TaskCompletionSource<bool> taskCompletionSource)
        {
            MovePiece(targetPos, taskCompletionSource, () =>
            {
                PieceModel.FallingInFieldProcess = 0;
                PieceModel.IsFalling = false;
            });
        }

        public void Swap(Vector3 targetPos, TaskCompletionSource<bool> taskCompletionSource)
        {
            MovePiece(targetPos, taskCompletionSource, () => { PieceModel.IsSwapping = false; });
        }

        public void SetSpriteOrder(int x, int y, int maxHeight)
        {
            pieceSprite.sortingOrder = (maxHeight - y * 10) + x;
        }

        public void PlayDestroyAnimation(Action completeHandler)
        {
            animator.SetBool("Destroyed", true);
            _onAnimationComplete = completeHandler;
        }

        public void OnDestroyAnimationComplete()
        {
            _onAnimationComplete.Invoke();
            gameObject.SetActive(false);
        }

        private void MovePiece(Vector3 targetPos, TaskCompletionSource<bool> taskCompletionSource,
            Action onCompleteAction)
        {
            transform.DOMove(targetPos, 0.2f)
                .OnComplete(() =>
                {
                    onCompleteAction?.Invoke();
                    taskCompletionSource.SetResult(true);
                });
        }
    }
}