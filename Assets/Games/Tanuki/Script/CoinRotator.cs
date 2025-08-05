using DG.Tweening;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    public class CoinRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        public float rotationSpeed = 180f; // degrees per second

        private Tween rotationTween;

        void Start()
        {
            StartRotating();
        }

        private void StartRotating()
        {
            rotationTween = transform.DORotate(
                    new Vector3(0, transform.eulerAngles.y + 360f, 0),
                    360f / rotationSpeed,
                    RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void OnDestroy()
        {
            if (rotationTween != null && rotationTween.IsActive())
                rotationTween.Kill();
        }
    }
}
