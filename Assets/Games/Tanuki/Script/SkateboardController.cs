using nostra.booboogames.Tanuki;
using UnityEngine;

namespace nostra.booboogames.slapcastle
{
    
    public class SkateboardController : MonoBehaviour
    {
      
        public float m_alignSpeed = 5;
        public float m_rayDistance = 5f;
        public float m_FwdForce = 10;
        public float accelerationTime = 2f;
        private float currentForce = 0f;
        // private float forceTimer = 0f;
     //   public float maxForce = 10f;
    
        [SerializeField] float maxSpeed;
        private Vector3 m_surfaceNormal = new Vector3();
        private Vector3 m_collisionPoint = new Vector3();
        public bool m_useRaycast = true;
        [SerializeField] bool m_onSurface;
        private Collision m_surfaceCollisionInfo;
        private Rigidbody m_rigidbody;
    
        [Header("--- Animation Controller ---")]
        [SerializeField] AnimationController animationController;
    
    
        [Header("--- JoyStick ---")]
        public FloatingJoystick joystick;
        public float maxTurnAngle = 90f;
        public float turnSmoothTime = 0.05f;
        public float rotationSpeed = 10f;
        private float turnSmoothVelocity;
    
        [Header("--- SkateBoard Object ---")]
        public Transform m_skateboard;
        [SerializeField] private Transform RotaionPivot;
        [SerializeField] private float RotateSkateBoard = 20f;
        [SerializeField] private float pivotMoveFactor = 20f;
    
        [SerializeField] ParticleSystem MagicNovaParticle;
        [SerializeField] ParticleSystem CoinCollectParicle;
    
    
        [Header("--- Manager Script ---")]
        [SerializeField] GameManager gameManager;
        [SerializeField] GearBox gearBox;
    
        // Start is called before the first frame update
        void Start()
        {
            m_rigidbody = GetComponent<Rigidbody>();
        }
    
        // Update is called once per frame
        void Update()
        {
            if (m_onSurface)
                RotateSkateBoardTilt(joystick.Horizontal, joystick.Vertical);
    
            UpdateSpeedometer();
        }
    
        private void FixedUpdate()
        {
            float hInput = joystick.Horizontal;
            float vInput = joystick.Vertical;
            float inputMagnitude = Mathf.Clamp01(new Vector2(hInput, vInput).magnitude);
            if (m_onSurface)
            {
                HandlePivotAndSpark(hInput, inputMagnitude);
                ApplyRotation(hInput);
                ProcessForce(hInput);
                //ReduceDrift();
            }
            else
            {
                AddForceDown();
            }
                
                AlignToSurface();
           
        }
    
        private void HandlePivotAndSpark(float hInput, float inputMag)
        {
            Vector3 pivotPos = RotaionPivot.localPosition;
    
            if (inputMag > 0.8f)
            {
                if (hInput > 0.8f)
                {
                    animationController.RightDrift();
                }
                else if (hInput < -0.8f)
                {
                    animationController.LeftDrift();
                }
    
                pivotPos.x = Mathf.Lerp(pivotPos.x, hInput * pivotMoveFactor, 10f * Time.fixedDeltaTime);
              //  HandleSparkEffect(hInput);
            }
            else if (inputMag > 0.1f)
            {
                if (hInput > 0.1f && hInput < 0.8f)
                {
                    animationController.RightMove();
                }
                else if (hInput < -0.1f && hInput > -0.8f)
                {
                    animationController.LeftMove();
                }
    
                pivotPos.x = Mathf.Lerp(pivotPos.x, hInput * pivotMoveFactor, 5f * Time.fixedDeltaTime);
                // rb.MovePosition(rb.position + transform.right * hInput * maxSpeed * Time.fixedDeltaTime);
             //   HandleSparkEffect(0f);
            }
            else
            {
                animationController.IdleAnimation();
                pivotPos.x = Mathf.Lerp(pivotPos.x, 0f, 4f * Time.fixedDeltaTime);
             //   HandleSparkEffect(0f);
            }
    
            RotaionPivot.localPosition = pivotPos;
        }



        private void ProcessForce(float hInput)
        {
            if (!m_onSurface)
            {
                AddForceDown();
                return;
            }

            if (m_rigidbody.linearVelocity.magnitude >= maxSpeed * 1.5f) return; // Increase max speed limit

            m_FwdForce += Time.fixedDeltaTime * 2f; // Faster acceleration buildup
            float forceFactor = Mathf.Lerp(1f, 2f, Mathf.Abs(hInput)); // Stronger influence from input
            float t = Mathf.Clamp01(m_FwdForce / accelerationTime);
            currentForce = Mathf.Lerp(0, m_FwdForce * forceFactor, t);

            m_rigidbody.AddForce(transform.forward * currentForce, ForceMode.Force);
        }


        public void AddForceDown()
        { 
            m_rigidbody.AddForce(-transform.up * currentForce, ForceMode.Force);
        }
    
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.COINS))
            {
                Destroy(other.gameObject);
                CoinCollectParicle.Play();
                gearBox.IncreaseGear();
            }
        }
    
    
        private void OnCollisionStay(Collision other)
        {
            if (!m_onSurface)
            {
                MagicNovaParticle.Play();
            }
         
    
            m_onSurface = true;
            m_surfaceCollisionInfo = other;
            m_surfaceNormal = other.GetContact(0).normal;
            m_collisionPoint = other.GetContact(0).point;
        }
    
        private void OnCollisionExit(Collision other)
        {
            m_surfaceCollisionInfo = null;
            m_onSurface = false;
        }
    
    
        void AlignToSurface()
        {
            if (m_useRaycast)
            {
                var hit = new RaycastHit();
                var onSurface = Physics.Raycast(transform.position, Vector3.down, out hit, m_rayDistance);
                if (onSurface)
                {
                    var localRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                    var euler = localRot.eulerAngles;
                    euler.y = 0;
                    localRot.eulerAngles = euler;
                    m_skateboard.localRotation = Quaternion.LerpUnclamped(m_skateboard.localRotation, localRot, m_alignSpeed * Time.fixedDeltaTime);
                }
            }
            else
            {
                if (m_onSurface)
                {
                    var localRot = Quaternion.FromToRotation(transform.up, m_surfaceNormal) * transform.rotation;
                    var euler = localRot.eulerAngles;
                    euler.y = 0;
                    localRot.eulerAngles = euler;
                    m_skateboard.localRotation = Quaternion.LerpUnclamped(m_skateboard.localRotation, localRot, m_alignSpeed * Time.fixedDeltaTime);
                }
            }
        }
    
        private void RotateSkateBoardTilt(float horizontal, float vertical)
        {
            float inputMag = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);
            float targetAngle = inputMag > 0.8f ? -horizontal * RotateSkateBoard : 0f;
    
            Quaternion targetRot = Quaternion.Euler(0f, -targetAngle, 0f);
            m_skateboard.localRotation = Quaternion.Slerp(m_skateboard.localRotation, targetRot, Time.deltaTime * 5f);
        }


        public float GetCurrentSpeed()
        {
            return m_rigidbody != null ? m_rigidbody.linearVelocity.magnitude : 0f;
        }



        private void ApplyRotation(float hInput)
        {
            if (Mathf.Abs(hInput) < 0.05f) return;
    
            float targetYAngle = transform.eulerAngles.y + hInput * maxTurnAngle * 0.5f;
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYAngle, ref turnSmoothVelocity, turnSmoothTime * 0.2f);
            Quaternion targetRotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            m_rigidbody.MoveRotation(Quaternion.Slerp(m_rigidbody.rotation, targetRotation, rotationSpeed * 2f * Time.fixedDeltaTime));
    
    
            Vector3 currentVelocity = m_rigidbody.linearVelocity;
            float speed = currentVelocity.magnitude;
            Vector3 newForward = (m_rigidbody.rotation * Vector3.forward).normalized;
    
            m_rigidbody.linearVelocity = newForward * speed;
        }
    
        /* private void ApplyRotation(float hInput)     
         {
             if (Mathf.Abs(hInput) < 0.05f) return;
    
             // 1. Calculate new rotation based on input
             float rotationAmount = hInput * maxTurnAngle * Time.fixedDeltaTime;
             Quaternion deltaRotation = Quaternion.Euler(0f, rotationAmount, 0f);
    
             // 2. Apply the rotation
             m_rigidbody.MoveRotation(m_rigidbody.rotation * deltaRotation);
    
             // 3. Redirect velocity to match the new forward direction
             Vector3 currentVelocity = m_rigidbody.linearVelocity;
             float speed = currentVelocity.magnitude;
             Vector3 newForward = (m_rigidbody.rotation * Vector3.forward).normalized;
    
             m_rigidbody.linearVelocity = newForward * speed;
         }*/
    
    
    
    
        private void UpdateSpeedometer()
        {
            if (m_rigidbody == null) return;
    
            float currentSpeed = m_rigidbody.linearVelocity.magnitude;
    
            // Show in m/s
            //   Debug.Log($"{currentSpeed:F1} m/s");
    
        }
    }
    
}