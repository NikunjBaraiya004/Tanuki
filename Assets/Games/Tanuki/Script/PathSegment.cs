using System.Net;
using DG.Tweening;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    
    public class PathSegment : MonoBehaviour
    {
        [SerializeField] private Transform endPoint;   // Assign in prefab
        [SerializeField] bool isHalfCurve;
        [SerializeField] BoxCollider boxC;
        public PathManager pathManager;
        public GameObject Previouspath;
        public PathManager.PathType RoadType;

        public Transform GetEndPoint() => endPoint;
        
        public bool IsHalfCurve() => isHalfCurve;

        public void DestroyPreviousPath()
        { 
            Destroy(Previouspath);
        }

    }
    
}