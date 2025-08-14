using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    public class PathManager : MonoBehaviour
    {
        public GameObject[] pathPrefabs;
        public int initialSegments = 5;
        public Transform startPoint;
        public SkateboardController skateboardController;

        private Vector3 nextSpawnPoint;
        private Quaternion nextSpawnRotation = Quaternion.identity;
        private GameObject previousPath;

        private int curveBalance = 0;
        private PathType lastCurveType = PathType.HighWay;
        private PathType lastPathType = PathType.HighWay;
        private bool lastWasCurve = false;

        [SerializeField] AudioSource audiosource;

        public enum PathType
        {
            HighWay,
            SingleRoad,
            SingleToDouble, DoubleToSingle,
            DoubleRoad,
            LeftCurve, RightCurve,
            LeftSCruve, RightSCruve,
        }

        public enum DifficultyLevel
        {
            Easy,
            Hard
        }

        public DifficultyLevel currentDifficulty = DifficultyLevel.Easy;

        void Start()
        {
            GameObject firstSegment = Instantiate(pathPrefabs[0], startPoint.position, startPoint.rotation, this.transform);
            PathSegment segment = firstSegment.GetComponent<PathSegment>();
            segment.pathManager = this;

            if (segment != null)
            {
                nextSpawnPoint = segment.GetEndPoint().position;
                nextSpawnRotation = segment.GetEndPoint().rotation;
            }

            previousPath = firstSegment;
            lastPathType = segment.RoadType;

            for (int i = 1; i < initialSegments; i++)
            {
                SpawnPath();
            }
        }

        public void SpawnPath(GameObject prefabOverride = null, bool RemoveBoxCollider = false)
        {
            GameObject pathInstance;
            PathSegment segment;

            int attempt = 0;
            const int maxAttempts = 10;
            int maxBalance = GetMaxCurveBalance();
            float curveChance = GetCurveProbability();

            while (attempt < maxAttempts)
            {
                GameObject selectedPrefab;

                if (prefabOverride != null)
                {
                    selectedPrefab = prefabOverride;
                }
                else
                {
                    List<GameObject> validPrefabs = new List<GameObject>();

                    foreach (var prefab in pathPrefabs)
                    {
                        var tempSegment = prefab.GetComponent<PathSegment>();
                        if (tempSegment == null) continue;

                        var type = tempSegment.RoadType;

                        // No two HighWays in a row
                        if (type == PathType.HighWay && lastPathType == PathType.HighWay)
                            continue;

                        // Check curve balance limits
                        if (IsLeftCurve(type) && curveBalance >= maxBalance) continue;
                        if (IsRightCurve(type) && curveBalance <= -maxBalance) continue;

                        // Avoid same-direction curves in Easy mode
                        if (currentDifficulty == DifficultyLevel.Easy && lastWasCurve)
                        {
                            bool isSameDirection =
                                (IsLeftCurve(type) && IsLeftCurve(lastCurveType)) ||
                                (IsRightCurve(type) && IsRightCurve(lastCurveType));
                            if (isSameDirection) continue;
                        }

                        // Force alternating curve directions in Hard mode
                        if (currentDifficulty == DifficultyLevel.Hard && lastWasCurve)
                        {
                            if (IsLeftCurve(lastCurveType) && IsLeftCurve(type)) continue;
                            if (IsRightCurve(lastCurveType) && IsRightCurve(type)) continue;
                        }

                        // Probability check for curves (Easy only)
                        bool isCurve = IsLeftCurve(type) || IsRightCurve(type);
                        if (currentDifficulty == DifficultyLevel.Easy && isCurve && Random.value > curveChance)
                            continue;

                        validPrefabs.Add(prefab);
                    }

                    // If no valid prefabs found, fallback to straight road
                    if (validPrefabs.Count == 0)
                    {
                        selectedPrefab = pathPrefabs.First(p =>
                            p.GetComponent<PathSegment>().RoadType == PathType.SingleRoad ||
                            p.GetComponent<PathSegment>().RoadType == PathType.HighWay);
                    }
                    else
                    {
                        selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                    }
                }

                pathInstance = Instantiate(selectedPrefab, nextSpawnPoint, nextSpawnRotation, this.transform);

                segment = pathInstance.GetComponent<PathSegment>();
                if (segment == null)
                {
                    attempt++;
                    continue;
                }

                segment.pathManager = this;
                segment.Previouspath = previousPath;
                previousPath = pathInstance;

                nextSpawnPoint = segment.GetEndPoint().position;
                nextSpawnRotation = segment.GetEndPoint().rotation;

                UpdateCurveBalance(segment.RoadType);

                lastWasCurve = IsLeftCurve(segment.RoadType) || IsRightCurve(segment.RoadType);
                if (lastWasCurve)
                    lastCurveType = segment.RoadType;

                // Track last path for "no 2 HighWays" rule
                lastPathType = segment.RoadType;

                break;
            }
        }

        private int GetMaxCurveBalance()
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: return 1;
                case DifficultyLevel.Hard: return 3;
                default: return 2;
            }
        }

        private float GetCurveProbability()
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: return 0.2f;
                case DifficultyLevel.Hard: return 0.8f;
                default: return 0.4f;
            }
        }

        private void UpdateCurveBalance(PathType type)
        {
            if (IsLeftCurve(type))
                curveBalance++;
            else if (IsRightCurve(type))
                curveBalance--;
            else
            {
                // Neutral roads gradually normalize balance
                if (curveBalance > 0) curveBalance--;
                else if (curveBalance < 0) curveBalance++;
            }
        }

        private bool IsLeftCurve(PathType type)
        {
            return type == PathType.LeftCurve || type == PathType.LeftSCruve;
        }

        private bool IsRightCurve(PathType type)
        {
            return type == PathType.RightCurve || type == PathType.RightSCruve;
        }
    }
}
