using System.Collections.Generic;
using System.Linq;
using nostra.booboogames.slapcastle;
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

        private int leftCurveBalance = 0;  // Positive = more lefts, Negative = more rights

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
            Medium,
            Hard
        }

        public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;


        void Start()
        {
            GameObject firstSegment = Instantiate(pathPrefabs[0], startPoint.position, startPoint.rotation);
            PathSegment segment = firstSegment.GetComponent<PathSegment>();
            segment.pathManager = this;

            if (segment != null)
            {
                nextSpawnPoint = segment.GetEndPoint().position;
                nextSpawnRotation = segment.GetEndPoint().rotation;
            }

            previousPath = firstSegment;

            for (int i = 1; i < initialSegments; i++)
            {
                SpawnPath();
            }
        }

        /*  public void SpawnPath(GameObject prefabOverride = null, bool RemoveBoxCollider = false)
          {
              GameObject pathInstance;
              PathSegment segment;

              const int maxBalance = 2;
              const int maxAttempts = 10;
              int attempt = 0;

              while (attempt < maxAttempts)
              {
                  GameObject selectedPrefab;

                  if (prefabOverride != null)
                  {
                      selectedPrefab = prefabOverride;
                  }
                  else
                  {
                      // Filter valid prefabs
                      List<GameObject> validPrefabs = new List<GameObject>();

                      foreach (var prefab in pathPrefabs)
                      {
                          var tempSegment = prefab.GetComponent<PathSegment>();
                          if (tempSegment == null) continue;

                          var type = tempSegment.RoadType;

                          // Check curve balance rules
                          if ((type == PathType.LeftCurve || type == PathType.LeftSCruve) && curveBalance >= maxBalance)
                              continue;
                          if ((type == PathType.RightCurve || type == PathType.RightSCruve) && curveBalance <= -maxBalance)
                              continue;

                          validPrefabs.Add(prefab);
                      }

                      // If no valid prefabs found, fallback to straight road
                      if (validPrefabs.Count == 0)
                      {
                          selectedPrefab = pathPrefabs.First(p => p.GetComponent<PathSegment>().RoadType == PathType.SingleRoad);
                      }
                      else
                      {
                          selectedPrefab = validPrefabs[Random.Range(0, validPrefabs.Count)];
                      }
                  }

                  pathInstance = Instantiate(selectedPrefab, nextSpawnPoint, nextSpawnRotation);
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

                  break;
              }
          }*/


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

                        // Check curve balance limits
                        if ((type == PathType.LeftCurve || type == PathType.LeftSCruve) && curveBalance >= maxBalance)
                            continue;
                        if ((type == PathType.RightCurve || type == PathType.RightSCruve) && curveBalance <= -maxBalance)
                            continue;

                        // Probability check: skip curve if random chance fails
                        bool isCurve = type == PathType.LeftCurve || type == PathType.RightCurve ||
                                       type == PathType.LeftSCruve || type == PathType.RightSCruve;

                        if (isCurve && Random.value > curveChance)
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

                pathInstance = Instantiate(selectedPrefab, nextSpawnPoint, nextSpawnRotation);
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

                // Optional debug
                Debug.Log($"Spawned: {segment.RoadType} | CurveBalance: {curveBalance}");

                break;
            }
        }


        private int curveBalance = 0;

        private int GetMaxCurveBalance()
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: return 1;
                case DifficultyLevel.Medium: return 2;
                case DifficultyLevel.Hard: return 3;
                default: return 2;
            }
        }

        private float GetCurveProbability()
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: return 0.2f;
                case DifficultyLevel.Medium: return 0.4f;
                case DifficultyLevel.Hard: return 0.7f;
                default: return 0.4f;
            }
        }

        private void UpdateCurveBalance(PathType type)
        {
            switch (type)
            {
                case PathType.LeftCurve:
                case PathType.LeftSCruve:
                    curveBalance++;
                    break;
                case PathType.RightCurve:
                case PathType.RightSCruve:
                    curveBalance--;
                    break;
                default:
                    // Neutral roads gradually normalize balance
                    if (curveBalance > 0) curveBalance--;
                    else if (curveBalance < 0) curveBalance++;
                    break;
            }
        }


    }

}
