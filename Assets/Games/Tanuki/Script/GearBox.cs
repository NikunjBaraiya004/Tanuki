using DG.Tweening;
using EasyButtons;
using UnityEngine;
using UnityEngine.UI;

namespace nostra.booboogames.Tanuki
{
    public class GearBox : MonoBehaviour
    {
        [SerializeField] Image GearImg;
        float gearcount = 0;

        [SerializeField] Transform GreenGear;
        [SerializeField] SpinWheel SpinWheel;
        [SerializeField] float scalespin;

        [Header("Gear Rotation Settings")]
        [SerializeField] float rotateStep = 30f;
        [SerializeField] float rotateDuration = 0.3f;

        [Header("Movement Settings")]
        [SerializeField] float punchInX = -100f;
        [SerializeField] float moveOutX = 200f;
        [SerializeField] float moveDuration = 0.25f;

        public bool SpinWheelOn = false;

        // For tracking last method call
        private float lastIncreaseTime = 0f;
        private float resetDelay = 0.35f; // How long to wait before calling Resetpos

        [Button]
        public void IncreaseGear()
        {
            // 1. Record the time of this call
            lastIncreaseTime = Time.time;

            // 2. Stop any previous tweens
            transform.DOKill();

            // 3. Move to punch-in position
            transform.DOLocalMoveX(punchInX, moveDuration).SetEase(Ease.InCubic).OnComplete(() =>
            {
                // 4. Increase gear logic
                gearcount += 0.1f;
                gearcount = Mathf.Clamp01(gearcount);

                GearImg.DOFillAmount(gearcount, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    if (gearcount == 1)
                    {
                        CompleteGear();
                        SpinWheelOn = true;
                    }
                    else
                    {
                        GreenGear.DOLocalRotate(
                            GreenGear.localEulerAngles + new Vector3(0, 0, -rotateStep),
                            rotateDuration,
                            RotateMode.FastBeyond360
                        ).SetEase(Ease.OutSine).OnComplete(() =>
                        {
                            // Delay Resetpos check to allow any new IncreaseGear call to happen
                            DOVirtual.DelayedCall(resetDelay, () =>
                            {
                                if (!SpinWheelOn && Time.time - lastIncreaseTime >= resetDelay)
                                {
                                    Resetpos();
                                }
                            });
                        });
                    }
                });
            });
        }

        public void CompleteGear()
        {
            SpinWheel.gameObject.transform.DOScale(Vector3.one * scalespin, 0.5f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    SpinWheel.Spin();
                });

            gearcount = 0;
            GearImg.fillAmount = 0;
        }

        public void Resetpos()
        {
            if (SpinWheelOn)
                return;

            transform.DOKill(); // Stop any other tween before moving
            transform.DOLocalMoveX(moveOutX, moveDuration).SetEase(Ease.OutCubic);
        }
    }
}
