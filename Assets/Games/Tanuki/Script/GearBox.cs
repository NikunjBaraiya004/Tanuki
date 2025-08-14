using DG.Tweening;
using EasyButtons;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace nostra.booboogames.Tanuki
{
    public class GearBox : MonoBehaviour
    {
        [SerializeField] Image GearImg;
        float gearcount = 0;

        [SerializeField] Transform GreenGear,SpinParent;
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
        [SerializeField] float increasevalue;

        // For tracking last method call
        private float lastIncreaseTime = 0f;
        private float resetDelay = 1f; // How long to wait before calling Resetpos

        /*  [Button]
          public void IncreaseGear()
          {
              lastIncreaseTime = Time.time;

              // Stop only movement tweens, not UI tweens
              transform.DOKill();

              // Increase first, so it’s never skipped
              gearcount += increasevalue;
              gearcount = Mathf.Clamp01(gearcount);

              // Move UI
              transform.DOLocalMoveX(punchInX, moveDuration).SetEase(Ease.InCubic).OnComplete(() =>
              {
                  GearImg.DOFillAmount(gearcount, 0.3f).SetEase(Ease.OutQuad).OnComplete(() =>
                  {
                      if (gearcount >= 1f)
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
          }*/

        [Button]
        public void IncreaseGear()
        {
            lastIncreaseTime = Time.time;

            gearcount = Mathf.Clamp01(gearcount + increasevalue);
          

            PlayPunchEffect();

           
        }

        private void PlayPunchEffect()
        {
            // Kill only position tween
            DOTween.Kill(transform, false);

            // Smooth punch motion
            transform.DOLocalMoveX(punchInX, moveDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    GearImg.DOFillAmount(gearcount, 0.15f).SetEase(Ease.OutQuad);

                    if (gearcount >= 1f && !SpinWheelOn)
                    {
                        CompleteGear();
                       
                        SpinWheelOn = true;
                    }

                    GreenGear.DOLocalRotate(
                        GreenGear.localEulerAngles + new Vector3(0, 0, -rotateStep),
                        rotateDuration,
                        RotateMode.FastBeyond360
                    ).SetEase(Ease.OutSine).OnComplete(() =>
                    {
                        DOVirtual.DelayedCall(resetDelay, () =>
                        {
                            if (!SpinWheelOn && Time.time - lastIncreaseTime >= resetDelay)
                                Resetpos();
                        });
                    });
                });
        }

        public void SetGearImgFillAMount()
        {
            GearImg.DOFillAmount(gearcount, 0.5f);
        }



        public void CompleteGear()
        {
            SpinParent.gameObject.transform.DOScale(Vector3.one * scalespin, 0.5f)
                .SetEase(Ease.OutBounce)
                .OnComplete(() =>
                {
                    SpinWheel.Spin();
                });

            gearcount = 0;
          // GearImg.fillAmount = 0;
        }

        public void Resetpos()
        {
            if (SpinWheelOn)
            {
                GearImg.DOFillAmount(0,0);
                return;
            }
            transform.DOKill(); // Stop any other tween before moving
            transform.DOLocalMoveX(moveOutX, moveDuration).SetEase(Ease.OutCubic);
        }
    }
}
