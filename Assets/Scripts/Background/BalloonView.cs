using System;
using DG.Tweening;
using UnityEngine;

namespace DefaultNamespace.Background
{
    public class BalloonView : MonoBehaviour
    {
        private BalloonModel _model;
        private Action _onComplete;

        public void Initialize(BalloonModel model, Action onComplete)
        {
            _model = model;
            _onComplete = onComplete;

            transform.position = model.StartPosition;
            StartMovement();
        }

        private void StartMovement()
        {
            DOVirtual.Float(0, 1, _model.MoveDuration, t =>
                {
                    Vector3 basePosition = Vector3.Lerp(_model.StartPosition, _model.TargetPosition, t);
                    float curveValue = _model.AnimationCurve.Evaluate(t);
                    Vector3 offset = _model.AmplitudeAxis * curveValue * _model.Amplitude;

                    transform.position = basePosition + offset;
                })
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _onComplete?.Invoke();
                    Destroy(gameObject);
                });
        }
    }
}