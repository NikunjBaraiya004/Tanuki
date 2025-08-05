using UnityEngine;
using DG.Tweening;
using EasyButtons;

namespace nostra.booboogames.Tanuki
{
    public class SpinWheel : MonoBehaviour
    {
        [Header("Spin Settings")]
        public float spinDuration = 4f;
        public int minRounds = 3;
        public int maxRounds = 6;
        private int numberOfSegments = 6;

        [SerializeField] GameManager gameManager;
     

        [Header("Rewards for Segments")]
        public int[] segmentRewards = new int[6]
        {
            100, 2000, 200 ,  500, 10000, 3000
        };

        private bool isSpinning = false;

        [Button]
        public void Spin()
        {
            if (isSpinning) return;

            isSpinning = true;

            int fullRounds = Random.Range(minRounds, maxRounds);
            float randomAngle = Random.Range(0f, 360f);
            float totalRotation = fullRounds * 360f + randomAngle;

            transform.DORotate(new Vector3(0, 0, -totalRotation), spinDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.OutQuart)
                .OnComplete(() =>
                {
                    isSpinning = false;

                    float finalZ = transform.eulerAngles.z;
                    int segment = GetSegmentFromAngleCustom(finalZ);

                    Debug.Log($"Final Z Angle: {finalZ} | Inverted: {360f - finalZ % 360f} | Won Segment: {segment}");

                    if (segmentRewards != null && segment < segmentRewards.Length)
                        Debug.Log("Reward: " + segmentRewards[segment]);

                    transform.rotation = Quaternion.identity;
                    transform.DOScale(Vector3.zero, 0.25f);
                    gameManager.IncreaseCoins(segmentRewards[segment]);
                });

        }

        private int GetSegmentFromAngleCustom(float angle)
        {
            // Normalize angle between 0 - 360
            float z = angle % 360f;
            if (z < 0) z += 360f;

            // NOTE: Unity rotates clockwise, so we flip to match your defined segments
            float inverted = 360f - z;

            // Use your manual ranges
            if (inverted >= 0f && inverted < 60f) return 5;            
            else if (inverted >= 60f && inverted < 155f) return 4;     
            else if (inverted >= 155f && inverted < 180f) return 3;    
            else if (inverted >= 180f && inverted < 245f) return 2;    
            else if (inverted >= 245f && inverted < 300f) return 1;    
            else return 0;                                             
        }




    }
}
