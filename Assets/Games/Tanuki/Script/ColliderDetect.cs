using nostra.booboogames.Tanuki;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    
    public class ColliderDetect : MonoBehaviour
    {
        [SerializeField] PathSegment pathSegment;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.PLAYER))
            {
                if (pathSegment.Previouspath != null)
                {
                    pathSegment.pathManager.SpawnPath();
                    pathSegment.DestroyPreviousPath();
                }
            }
        }
    }
    
}