using UnityEngine;
using UnityEngine.UIElements;

namespace nostra.booboogames.Tanuki
{
    
    public class AnimationController : MonoBehaviour
    {

        public Animator animator;
        public float animationValue;
        [SerializeField] float Speed = 1f;

        public void IdleAnimation()
        {
            animationValue = 0;
        }

        public void RightMove()
        {
            animationValue = 0.5f;
        }

        public void LeftMove() 
        {
            animationValue = -0.5f;
        }

        public void RightDrift()
        {
            animationValue = 1;
        }

        public void LeftDrift()
        {
            animationValue = -1;
        }

        float tempvalue = 0;
/*
        private void Update()
        {
            tempvalue = Mathf.Lerp(tempvalue, animationValue, Speed * Time.deltaTime);
            animator.SetFloat("Horizontal", tempvalue);
        }*/

        public void SetFloat(float H_value, float V_value)
        {
            //if (value > 0.9)
            //    value = 1;
            //else if (value < -0.9)
            //    value = -1;

                animator.SetFloat("Horizontal", H_value);
                animator.SetFloat("Vertical", V_value);
        }

        public void OnLanding()
        {
            animator.Play("Landing");
        }

    }
    
}