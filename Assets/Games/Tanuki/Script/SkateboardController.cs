using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace nostra.booboogames.Tanuki
{
    public class SkateboardController : MonoBehaviour
    {

        public float m_alignSpeed = 5;
        public float m_rayDistance = 1f;
        public float m_FwdForce = 10;
        public float accelerationTime = 2f;
        public float currentForce = 0f;
        // private float forceTimer = 0f;
        // public float maxForce = 10f;

        [SerializeField] float maxSpeed;
        private Vector3 m_surfaceNormal = new Vector3();
        private Vector3 m_collisionPoint = new Vector3();
        // public bool m_useRaycast = true;
        // [SerializeField] bool m_onSurface;
        private Collision m_surfaceCollisionInfo;
        private Rigidbody m_rigidbody;

        [SerializeField] bool IsJumper = false;

        [Header("--- Character Obj ---")]
        [SerializeField] GameObject CharacterObj;

        [Header("Speed Adjustment")]
        [SerializeField] private float baseMaxSpeed = 40f;
        [SerializeField] private float maxSpeedCap = 100f;
        [SerializeField] private float speedIncreaseRate = 30f;
        [SerializeField] private float speedDecreaseRate = 20f;

        [Header("--- Particle System ---")]
        [SerializeField] ParticleSystem WindEffect;
        [SerializeField] ParticleSystem MagicNovaParticle;
        [SerializeField] ParticleSystem CoinCollectParicle;

        [SerializeField] private List<ParticleSystem> LeftSideSpark;
        [SerializeField] private List<ParticleSystem> RightSideSpark;
        private bool leftSparkActive = false;
        bool IsWindOn = false;
        bool IsplayParticle = false;

        [Header("--- Trail Render ---")]
        [SerializeField] TrailRenderer RightHandTrail;
        [SerializeField] TrailRenderer LeftHandTrail;


        [Header("--- Animation Controller ---")]
        [SerializeField] AnimationController animationController;

        [Header("--- JoyStick ---")]
        public Joystick joystick;
        public float maxTurnAngle = 90f;
        public float turnSmoothTime = 0.05f;
        public float rotationSpeed = 10f;
        private float turnSmoothVelocity;

        [Header("--- SkateBoard Object ---")]
        public Transform m_skateboard;
        [SerializeField] private Transform RotaionPivot;
        [SerializeField] private float RotateSkateBoard = 20f;
        [SerializeField] private float pivotMoveFactor = 20f;


        private bool rightSparkActive = false;

        [Header("--- Manager Script ---")]
        [SerializeField] GameManager gameManager;
        [SerializeField] GearBox gearBox;

        [Header("--- Cinemachine Camera ---")]
        [SerializeField] CinemachineCamera cinemachinecamera;
        CinemachineBasicMultiChannelPerlin noise;
        [SerializeField] private float amplitudeSmoothSpeed = 5f; // How fast it smooths
        private float targetAmplitude = 0f; // Where we want to go
        private float currentAmplitude = 0f; // Current smooth value


        [Header("--- Player State ---")]
        [SerializeField] PlayerState playerState;


        [Header("--- References ---")]
        [SerializeField] PositionUpdate positionUpdate;
        [SerializeField] TextMeshProUGUI DriftText;

        public enum PlayerState
        {
            IsPlay,
            IsDeath
        }

        // Start is called before the first frame update
        void Start()
        {
            m_rigidbody = GetComponent<Rigidbody>();

            if (cinemachinecamera != null)
            {
                // Get Perlin noise component from the camera
                noise = cinemachinecamera.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            }
        }

        // Update is called once per frame
        void Update()
        {
            /*if (onSurface)
                RotateSkateBoardTilt(joystick.Horizontal,joystick.Vertical);*/

            if (noise != null)
            {
                currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, Time.deltaTime * amplitudeSmoothSpeed);
                noise.AmplitudeGain = currentAmplitude;
            }

            UpdateSpeedometer();
        }

        private void FixedUpdate()
        {
            if (playerState == PlayerState.IsDeath)
                return;

            float hInput, vInput;

            hInput = joystick.Horizontal;
            vInput = joystick.Vertical;

            float inputMagnitude = Mathf.Clamp01(new Vector2(hInput, vInput).magnitude);


            if (onSurface)
            {
                HandlePivotAndSpark(vInput, hInput, inputMagnitude);

                if (!IsJumper)
                {
                    ApplyRotation(hInput);
                    RotateSkateBoardTilt(hInput, vInput);
                }

                AdjustMaxSpeed(vInput);
                ProcessForce(hInput);
            }
            else
            {
                HandleSparkEffect(0);
                AddForceDown();
            }

            AlignToSurface();
        }


        [SerializeField] private float driftIncreaseRate = 5f; // 1 per second
        private float driftTimer = 0f;

        private int driftScore = 0; [SerializeField] private float driftThreshold = 0.8f; // Horizontal input required to count drift

        private bool wasDrifting = false;

        float TempHVal = 0;
        float TempVVal = 0;
        private void  HandlePivotAndSpark(float vInput, float hInput, float inputMag)
        {
            Vector3 pivotPos = RotaionPivot.localPosition;
            TempHVal = Mathf.Lerp(TempHVal, hInput, 2f * Time.deltaTime);
            TempVVal = Mathf.Lerp(TempVVal, vInput,2f * Time.deltaTime);

            animationController.SetFloat(TempHVal, TempVVal);

            bool isDrifting = Mathf.Abs(hInput) > driftThreshold;

            if (isDrifting)
            {
                driftTimer += Time.fixedDeltaTime;

                if (driftTimer >= 5f / driftIncreaseRate)
                {
                    DriftText.DOFade(1, 0.2f);
                    driftScore += 1;
                    driftTimer = 0f;
                    DriftText.text = "Drift :- " + driftScore.ToString();
                    //   Debug.Log($"Drift Score: {driftScore}");
                }
                wasDrifting = true;
            }
            else
            {
                // Drift just ended
                if (wasDrifting)
                {
                    DriftText.DOFade(0, 0.2f);
                    //     Debug.Log($"Final Drift Score: {driftScore}");
                    gameManager.IncreaseCoinDrift(driftScore);
                    driftScore = 0;
                    driftTimer = 0f;

                    wasDrifting = false;
                }
            }


            if (inputMag > 0.8f)
            {
                if (hInput > driftThreshold)
                {
                    // animationController.RightDrift();
                }
                else if (hInput < -driftThreshold)
                {
                    // animationController.LeftDrift();
                }

                pivotPos.x = Mathf.Lerp(pivotPos.x, hInput * pivotMoveFactor, 10f * Time.fixedDeltaTime);
                HandleSparkEffect(hInput);
            }
            else if (inputMag > 0.1f)
            {
                if (hInput > 0.1f && hInput < driftThreshold)
                {
                    // animationController.RightMove();
                }
                else if (hInput < -0.1f && hInput > -driftThreshold)
                {
                    // animationController.LeftMove();
                }

                pivotPos.x = Mathf.Lerp(pivotPos.x, hInput * pivotMoveFactor, 5f * Time.fixedDeltaTime);
                HandleSparkEffect(0f);
            }
            else
            {
                // animationController.IdleAnimation();
                pivotPos.x = Mathf.Lerp(pivotPos.x, 0f, 4f * Time.fixedDeltaTime);
                HandleSparkEffect(0f);
            }

            RotaionPivot.localPosition = pivotPos;
        }

        private void AdjustMaxSpeed(float vInput)
        {
            float targetSpeed = baseMaxSpeed;

            if (vInput > 0.1f)
            {
                SetAmplitude(vInput * 2.5f);
                // Increase target based on input strength
                float increaseAmount = vInput * speedIncreaseRate * Time.fixedDeltaTime;
                maxSpeed = Mathf.Min(maxSpeed + increaseAmount, maxSpeedCap);
            }
            else if (vInput <= 0.1f)
            {
                if(vInput <= 0)
                 SetAmplitude(1.2f);
                
                // Gradually decrease back to base
                maxSpeed = Mathf.Max(maxSpeed - speedDecreaseRate * Time.fixedDeltaTime, baseMaxSpeed);
                currentForce = m_FwdForce;
            }
        }

        public void SetAmplitude(float amplitude)
        {
           targetAmplitude = amplitude;
        }


        private float accelTimer = 25f;
        private float targetForce = 0f;
        private float smoothedForce = 0f; // <-- this smooths per-frame changes
        private float smoothVelocity = 0f; // for SmoothDamp

        private void ProcessForce(float hInput)
        {
            // Limit top speed
            if (m_rigidbody.linearVelocity.magnitude >= maxSpeed * 1.5f)
                return;

            // Acceleration buildup
            accelTimer += Time.fixedDeltaTime;
            accelTimer = Mathf.Clamp(accelTimer, 0f, accelerationTime);

            // Ease-in curve for buildup
            float buildup = accelTimer / accelerationTime;
            buildup = buildup * buildup;

            // Calculate desired target force
            targetForce = Mathf.Lerp(0f, maxSpeed, buildup) * Mathf.Lerp(1f, 2f, Mathf.Abs(hInput));

            // Smooth the force change over time to kill jerk
            smoothedForce = Mathf.SmoothDamp(smoothedForce, targetForce, ref smoothVelocity, 0.1f);

            // Apply smoothed force
            m_rigidbody.AddForce(transform.forward * smoothedForce, ForceMode.Acceleration);
        }

        private void ResetForce()
        {
            accelTimer = 0f;
            targetForce = 0f;
            smoothedForce = 0f;
            smoothVelocity = 0f;
        }


        [SerializeField] float addforcedownside;

        public void AddForceDown()
        {
            // A small, controlled deceleration factor
            float slowDownForce = currentForce * addforcedownside; // 20% of current force
            slowDownForce = Mathf.Clamp(slowDownForce, 5f, 50f); // prevent it from being too small or too large

            m_rigidbody.AddForce(-transform.up * slowDownForce, ForceMode.Acceleration);
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.COINS))
            {
                Destroy(other.gameObject);
                CoinCollectParicle.Play();
                gearBox.IncreaseGear();
            }

            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.DEAD) && playerState != PlayerState.IsDeath)
            {
                positionUpdate.IsUnfollow();
                playerState = PlayerState.IsDeath;

                cinemachinecamera.Follow = null;
                cinemachinecamera.LookAt = null;

                m_skateboard.transform.SetParent(transform.parent);
                m_skateboard.transform.DOLocalMove(m_skateboard.transform.localPosition - m_skateboard.transform.forward * 20f, 1f);

                CharacterObj.transform.SetParent(this.transform.parent);
                
                CharacterObj.transform.DOLocalMove(CharacterObj.transform.localPosition + CharacterObj.transform.forward * 200f, 5f);

                CharacterObj.transform
                    .DOScale(Vector3.zero, 1f);

                gameManager.OpenGameoverUI();
            }

        }


        private void OnCollisionStay(Collision other)
        {
            if (IsplayParticle)
            {
                MagicNovaParticle.Play();
                animationController.OnLanding();
                IsplayParticle = false;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.JUMP) && !IsJumper)
                IsJumper = true;

            if (other.gameObject.layer == LayerMask.NameToLayer(MyLayers.PATH) && IsJumper)
                IsJumper = false;

        }

        private void OnCollisionExit(Collision collision)
        {

            if (collision.gameObject.layer == LayerMask.NameToLayer(MyLayers.JUMP))
            {
                IsplayParticle = true;
            }

        }


        [SerializeField] bool onSurface = false;
        [SerializeField] private LayerMask surfaceLayerMask; // Assign allowed layers in Inspector


        [SerializeField] private float jumpIncreaseRate = 5f; // Points per second
        private float jumpTimer = 0f;
        private int jumpScore = 0;
        private bool wasJumping = false;

        [SerializeField] private TextMeshProUGUI JumpText;


        void AlignToSurface()
        {
            var hit = new RaycastHit();
            onSurface = Physics.Raycast(transform.position, Vector3.down, out hit, m_rayDistance, surfaceLayerMask);
            if (onSurface)
            {
                var localRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                var euler = localRot.eulerAngles;
                euler.y = 0;
                localRot.eulerAngles = euler;
                m_skateboard.localRotation = Quaternion.LerpUnclamped(m_skateboard.localRotation, localRot, m_alignSpeed * Time.fixedDeltaTime);
            }

            // Jump scoring logic
            if (IsJumper)
            {
                jumpTimer += Time.fixedDeltaTime;

                if (jumpTimer >= 1f / jumpIncreaseRate)
                {
                    JumpText.DOFade(1, 0.2f);
                    jumpScore += 1;
                    jumpTimer = 0f;
                    JumpText.text = "Jump :- " + jumpScore.ToString();
                }
                wasJumping = true;
            }
            else
            {
                if (wasJumping) // Jump just ended
                {
                    JumpText.DOFade(0, 0.2f);
                    gameManager.IncreaseCoinDrift(jumpScore); // Custom function to add coins for jump
                    jumpScore = 0;
                    jumpTimer = 0f;
                    wasJumping = false;
                }
            }

        }



        private void RotateSkateBoardTilt(float horizontal, float vertical)
        {
            float inputMag = Mathf.Clamp01(new Vector2(horizontal, vertical).magnitude);

            // Bigger tilt multiplier
            float maxTilt = RotateSkateBoard * 2.5f; // 50% more tilt than before

            // Start tilting as soon as input is detected
            float targetAngle = -horizontal * maxTilt * inputMag;

            // Smooth tilt
            Quaternion targetRot = Quaternion.Euler(0f, -targetAngle, 0f);
            m_skateboard.localRotation = Quaternion.Lerp(m_skateboard.localRotation, targetRot, Time.deltaTime * 7f);
        }


        public float GetCurrentSpeed()
        {
            return m_rigidbody != null ? m_rigidbody.linearVelocity.magnitude : 0f;
        }



        private void ApplyRotation(float hInput)
        {
            if (Mathf.Abs(hInput) < 0.05f) 
                return;

            float targetYAngle = transform.eulerAngles.y + hInput * maxTurnAngle * 0.5f;
            float smoothedAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYAngle, ref turnSmoothVelocity, turnSmoothTime * 0.2f);
            Quaternion targetRotation = Quaternion.Euler(0f, smoothedAngle, 0f);
            m_rigidbody.MoveRotation(Quaternion.Slerp(m_rigidbody.rotation, targetRotation, rotationSpeed * 2f * Time.fixedDeltaTime));

            Vector3 currentVelocity = m_rigidbody.linearVelocity;
            float speed = currentVelocity.magnitude;
            Vector3 newForward = (m_rigidbody.rotation * Vector3.forward).normalized;

            m_rigidbody.linearVelocity = newForward * speed;
        }

        private void HandleSparkEffect(float horizontalInput)
        {
            ToggleSparkList(LeftSideSpark, horizontalInput < -0.8f, ref leftSparkActive);
            ToggleSparkList(RightSideSpark, horizontalInput > 0.8f, ref rightSparkActive);
        }

        private void ToggleSparkList(List<ParticleSystem> sparks, bool isActive, ref bool currentState)
        {
            if (currentState == isActive) return;

            currentState = isActive;
            foreach (var ps in sparks)
            {
                var emission = ps.emission;
                emission.enabled = isActive;
            }
        }

        private void UpdateSpeedometer()
        {
            if (m_rigidbody == null) return;

            float currentSpeed = m_rigidbody.linearVelocity.magnitude;


            if (maxSpeed > 70 && !IsWindOn)
            {
                IsWindOn = true;
                var emission = WindEffect.emission;
                emission.rateOverTime = 12;
                emission.rateOverDistance = 1;
            }
            else if (maxSpeed <= 70 && IsWindOn)
            {
                IsWindOn = false;
                var emission = WindEffect.emission;
                emission.rateOverTime = 0;
                emission.rateOverDistance = 0;
            }


            // Show in m/s
            //  Debug.Log($"{currentSpeed:F1} m/s");

        }


    }

}