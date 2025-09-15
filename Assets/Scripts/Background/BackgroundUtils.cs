using UnityEngine;

namespace DefaultNamespace.Background
{
    public static class BackgroundUtils
    {
        public static (Vector3 firstPoint, Vector3 secondPoint) GetRandomBalloonStartAndTarget(Transform[] leftSide, Transform[] rightSide)
        {
            if (leftSide == null || leftSide.Length == 0 || rightSide == null || rightSide.Length == 0)
            {
                Debug.LogError("Один из массивов пустой или null");
                return (Vector3.zero, Vector3.zero);
            }

            Vector3 leftPoint = leftSide[Random.Range(0, leftSide.Length)].position;
            Vector3 rightPoint = rightSide[Random.Range(0, rightSide.Length)].position;
            bool leftFirst = Random.value > 0.5f;

            if (leftFirst)
                return (leftPoint, rightPoint);
            else
                return (rightPoint, leftPoint);
        }
        public static (float duration, BalloonView balloon, AnimationCurve animationCurve) GetRandomBalloonSetup(BalloonView[] balloonViews, AnimationCurve[] animationsCurves)
        {
            if (balloonViews == null || balloonViews.Length == 0)
            {
                Debug.LogError("Массив balloonViews пустой или null");
                return (0f, null, null);
            }

            if (animationsCurves == null || animationsCurves.Length == 0)
            {
                Debug.LogError("Массив animationsCurves пустой или null");
                return (0f, null, null);
            }

            float duration = Random.Range(5f, 35f);
            BalloonView balloon = balloonViews[Random.Range(0, balloonViews.Length)];
            AnimationCurve animationCurve = animationsCurves[Random.Range(0, animationsCurves.Length)];

            return (duration, balloon, animationCurve);
        }
    }
}