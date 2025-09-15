
using UnityEngine;

namespace DefaultNamespace.Background
{
    public class BalloonModel
    {
        public Vector3 StartPosition { get; }
        public Vector3 TargetPosition { get; }
        public float MoveDuration { get; }
        public AnimationCurve AnimationCurve { get; }
        public Vector3 AmplitudeAxis { get; }
        public float Amplitude { get; }

        public BalloonModel(
            Vector3 startPosition,
            Vector3 targetPosition,
            float moveDuration,
            AnimationCurve animationCurve,
            Vector3 amplitudeAxis,
            float amplitude)
        {
            StartPosition = startPosition;
            TargetPosition = targetPosition;
            MoveDuration = moveDuration;
            AnimationCurve = animationCurve;
            AmplitudeAxis = amplitudeAxis.normalized;
            Amplitude = amplitude;
        }
    }
}