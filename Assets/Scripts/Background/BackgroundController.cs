using DG.Tweening;
using UnityEngine;

namespace DefaultNamespace.Background
{
    public class BackgroundController : MonoBehaviour
    {
        [SerializeField] private AnimationCurve[] curves;
        [SerializeField] private Transform[] leftPoints;
        [SerializeField] private Transform[] rightPoints;
        [SerializeField] private BalloonView[] balloonViews;

        private float _amplitude = 1f;
        private Vector3 _amplitudeAxis = Vector3.up;

        private int _minBalloons = 1;
        private int _maxBalloons = 3;
        private int _activeBalloonsCount = 0;

        private void Start()
        {
            int balloonsCount = Random.Range(_minBalloons, _maxBalloons + 1);
            for (int i = 0; i < balloonsCount; i++)
            {
                SpawnAndMove();
            }
        }

        private void SpawnAndMove()
        {
            _activeBalloonsCount++;

            var route = BackgroundUtils.GetRandomBalloonStartAndTarget(leftPoints, rightPoints);
            var config = BackgroundUtils.GetRandomBalloonSetup(balloonViews, curves);

            Vector3 start = route.firstPoint;
            Vector3 target = route.secondPoint;

            float duration = config.duration;
            BalloonView prefab = config.balloon;
            AnimationCurve curve = config.animationCurve;

            BalloonModel model = new BalloonModel(
                start,
                target,
                duration,
                curve,
                _amplitudeAxis,
                _amplitude
            );

            BalloonView view = Instantiate(prefab, transform);
            view.Initialize(model, () =>
            {
                _activeBalloonsCount--;

                if (_activeBalloonsCount < _minBalloons)
                {
                    SpawnAndMove();
                }

                var balloonsCount = _maxBalloons - _activeBalloonsCount;
                for (int i = 0; i < balloonsCount; i++)
                {
                    if (Random.value > 0.5f)
                    {
                        SpawnAndMove();
                    }                    
                }
            });
        }
    }
}