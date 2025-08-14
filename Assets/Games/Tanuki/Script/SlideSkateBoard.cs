using DG.Tweening;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    public class SlideSkateBoard : MonoBehaviour
    {

        [SerializeField] Transform StartingPos;
        [SerializeField] Transform Endpos;
        [SerializeField] float JumpHeight = 2.5f;
        [SerializeField] float JumpDuration = 0.4f;
        [SerializeField] float SlideDuration = 0.5f;

        [Header("Final Small Bump Settings")]
        [SerializeField] float FinalHopHeight = 0.8f;         // Smaller hop
        [SerializeField] float FinalHopDistance = 3f;
        [SerializeField] float FinalHopDuration = 0.3f;        // Shorter duration
        [SerializeField] float FinalHopForce = 100f;           // Lower impulse for realism

        public void JumpOnStartPos(SkateboardController skateBoard, Transform ChildSkateBoard, Animator characterAni)
        {
            var controller = skateBoard.GetComponent<SkateboardController>();
            Rigidbody rb = skateBoard.GetComponent<Rigidbody>();

            //if (controller != null) controller.SetJumpingState(true);
            if (rb != null) rb.constraints = RigidbodyConstraints.FreezeRotation;

            Vector3 startPos = skateBoard.transform.position;
            Vector3 targetPos = StartingPos.position;

          

            

            // Step 3: Move Z
                skateBoard.transform.DOMoveZ(targetPos.z, JumpDuration).SetEase(Ease.Linear);

                // Step 4: Jump up and down (Y)
                skateBoard.transform.DOLocalMoveY(startPos.y + JumpHeight, JumpDuration * 0.4f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    skateBoard.transform.DOLocalMoveY(targetPos.y, JumpDuration * 0.6f).SetEase(Ease.InQuad).OnComplete(() =>
                    {
                        characterAni.Play("Balancing");
                        ChildSkateBoard.transform.DOLocalRotate(new Vector3(0, -90f, 0), 0.2f);
                        // Step 5: Slide to end
                        skateBoard.transform.DOMove(Endpos.position, SlideDuration).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            // Step 6: Final hop forward
                            Vector3 hopTarget = skateBoard.transform.position + skateBoard.transform.forward * FinalHopDistance;

                            // Reset child rotation

                            characterAni.Play("Movment");
                            ChildSkateBoard.transform.DOLocalRotate(new Vector3(0, 0f, 0), 0.2f);
                            skateBoard.transform.DOJump(hopTarget, FinalHopHeight, 1, FinalHopDuration)
                                .SetEase(Ease.OutQuad)
                                .OnComplete(() =>
                                {
                                 //   ChildSkateBoard.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutQuad);
                                   // if (controller != null) controller.SetJumpingState(false);
                                    if (rb != null)
                                    {
                                        rb.constraints = RigidbodyConstraints.None;
                                        rb.AddForce(skateBoard.transform.forward * FinalHopForce, ForceMode.Impulse);
                                    }
                                });
                        });
                    });
                });
           // });
        }
    }

}