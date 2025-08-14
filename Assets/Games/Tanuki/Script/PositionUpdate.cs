using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    
    public class PositionUpdate : MonoBehaviour
    {
        [SerializeField] Transform Targetpos;
        [SerializeField] bool IsFollowPos = true;
        [SerializeField] bool IsRotate = false;

        private void Start()
        {
            IsFollow();
        }

        public void IsFollow()
        { 
            IsFollowPos = true;
        }

        public void IsUnfollow()
        { 
            IsFollowPos = false;
        }


        void LateUpdate()
        {
            if (IsFollowPos)
            {
                transform.position = Targetpos.position;
               
                if(IsRotate)
                 transform.rotation = Targetpos.rotation * Quaternion.Euler(0f, -90f, 0f);
            }
        }

    }

}