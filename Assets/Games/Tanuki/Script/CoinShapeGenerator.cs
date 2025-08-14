using EasyButtons;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    public class CoinShapeGenerator : MonoBehaviour
    {
        public enum ShapeType
        {
            S,
            HalfCircle,
            SineWave,
            Zigzag
        }

        [Header("General Settings")]
        public ShapeType shapeType;
        public GameObject coinPrefab;
        public int coinCount = 30;
        public float radius = 5f;
        public float gapBetweenCoins = 1f;

        [Header("Spiral Settings")]
        public float spiralTurns = 2f;

        [Header("Sine Wave Settings")]
        public float sineWaveWidth = 10f;
        public float sineWaveHeight = 2f;

        [Header("Zigzag Settings")]
        public float zigzagStepLength = 1f;
        public float zigzagAmplitude = 1f;


        private void Start()
        {
            coinCount = Random.Range(4, 10);
            GenerateRandomShape();
        }

        [Button]
        public void GenerateRandomShape()
        {
            shapeType = GetRandomShape();
            GenerateCoins();
        }

        private ShapeType GetRandomShape()
        {
            var values = System.Enum.GetValues(typeof(ShapeType));
            return (ShapeType)values.GetValue(Random.Range(0, values.Length));
        }




        [Button]
        public void GenerateCoins()
        {
            ClearCoins();

            switch (shapeType)
            {
                case ShapeType.S:
                    GenerateSShape();
                    break;
                case ShapeType.HalfCircle:
                    GenerateArc(180f);
                    break;
                case ShapeType.SineWave:
                    GenerateSineWave(sineWaveWidth, sineWaveHeight);
                    break;
                case ShapeType.Zigzag:
                    GenerateZigzag(zigzagStepLength, zigzagAmplitude);
                    break;
            }
        }

        private void GenerateSShape()
        {
            int halfCount = coinCount / 2;

            for (int i = 0; i < halfCount; i++)
            {
                float angle = Mathf.Lerp(0, Mathf.PI, i / (float)(halfCount - 1));
                float x = Mathf.Cos(angle) * radius * gapBetweenCoins;
                float z = Mathf.Sin(angle) * radius * gapBetweenCoins;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }

            for (int i = 0; i < halfCount; i++)
            {
                float angle = Mathf.Lerp(Mathf.PI, 0, i / (float)(halfCount - 1));
                float x = (Mathf.Cos(angle) * radius + (radius * 2f)) * gapBetweenCoins;
                float z = -Mathf.Sin(angle) * radius * gapBetweenCoins;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }
        }

        private void GenerateArc(float angleDegrees)
        {
            float angleRad = angleDegrees * Mathf.Deg2Rad;

            for (int i = 0; i < coinCount; i++)
            {
                float angle = Mathf.Lerp(0, angleRad, i / (float)(coinCount - 1));
                float x = Mathf.Cos(angle) * radius * gapBetweenCoins;
                float z = Mathf.Sin(angle) * radius * gapBetweenCoins;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }
        }

        private void GenerateSpiral(float turns)
        {
            for (int i = 0; i < coinCount; i++)
            {
                float angle = i * (Mathf.PI * 2f) * (turns / coinCount);
                float radiusStep = radius * (i / (float)coinCount) * gapBetweenCoins;
                float x = Mathf.Cos(angle) * radiusStep;
                float z = Mathf.Sin(angle) * radiusStep;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }
        }

        private void GenerateSineWave(float width, float height)
        {
            float effectiveWidth = sineWaveWidth * gapBetweenCoins;

            for (int i = 0; i < coinCount; i++)
            {
                float t = i / (float)(coinCount - 1);
                float x = Mathf.Lerp(-effectiveWidth / 2f, effectiveWidth / 2f, t);
                float z = Mathf.Sin(t * Mathf.PI * 2f) * sineWaveHeight * gapBetweenCoins;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }
        }

        private void GenerateZigzag(float stepLength, float amplitude)
        {
            float step = stepLength * gapBetweenCoins;
            float amp = amplitude * gapBetweenCoins;

            for (int i = 0; i < coinCount; i++)
            {
                float x = i * step;
                float z = (i % 2 == 0 ? 1 : -1) * amp;

                Vector3 localPos = new Vector3(x, 0, z);
                SpawnCoin(localPos);
            }
        }

        private void SpawnCoin(Vector3 localOffset)
        {
            Vector3 worldPos = transform.TransformPoint(localOffset);
            Instantiate(coinPrefab, worldPos, transform.rotation * Quaternion.Euler(0, 0, 0), transform);
        }

        [Button]
        private void ClearCoins()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
}
