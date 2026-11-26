using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // PigeonFlightController — voo arcade 3D do MVP.
    // O mundo desliza em -Z (SpawnManager/GameManager); o pombo fica em z=0.
    //   tilt X  -> velocidade lateral
    //   tilt Y  -> aceleração vertical (com gravidade constante)
    //   tap     -> impulso de subida (consome energia)
    //   hold    -> planar (gravidade reduzida)
    // Correntes de ar alteram a gravidade (azul x0.3 / vermelha x1.6).
    // ---------------------------------------------------------------------
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PigeonFlightController : MonoBehaviour
    {
        [Header("Movel")]
        public float strafeSpeed = 6.5f;
        public float tiltLiftAccel = 26f;
        public float gravityNormal = 9.5f;
        public float gravityGlide = 2.4f;
        public float flapBoost = 6.5f;
        public float maxClimb = 9f;
        public float maxDive = 12f;
        public float boundsX = 7.5f;
        public float minY = 0.6f;
        public float maxY = 22f;

        [Header("Visual")]
        public float visualSmoothing = 12f;
        public float rollDegrees = 30f;
        public float pitchDegrees = 25f;

        [Header("Referencias (bootstrap)")]
        public Transform ModelRoot;
        public Transform WingLeft;
        public Transform WingRight;

        public bool ControlsEnabled { get; set; }
        public bool IsGliding { get; private set; }
        public bool GroundTouched { get; private set; }
        public float VerticalVelocity => _vy;

        public IFlightInput InputSource { get; set; }
        public EnergySystem Energy { get; set; }

        private Rigidbody _rb;
        private float _vy;
        private float _flapAnimTimer;
        private Vector2 _visualSteer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.mass = 1f;
            _rb.linearDamping = 0f;
            _rb.angularDamping = 0f;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void ResetRun()
        {
            _vy = 0f;
            _flapAnimTimer = 0f;
            GroundTouched = false;
            IsGliding = false;
            _visualSteer = Vector2.zero;
            _rb.linearVelocity = Vector3.zero;
            var pos = _rb.position;
            pos.y = 6f;
            pos.z = 0f;
            pos.x = Mathf.Clamp(pos.x, -boundsX, boundsX);
            _rb.position = pos;
        }

        private void FixedUpdate()
        {
            if (!ControlsEnabled || InputSource == null)
            {
                _rb.linearVelocity = Vector3.zero;
                return;
            }

            float dt = Time.fixedDeltaTime;
            Vector2 steer = InputSource.Steer;

            // ---- gravidade com modificadores de corrente de ar ----
            float gravity = gravityNormal;
            bool inAscending = Energy != null && Energy.InWind && Energy.Wind == WindType.Ascending;
            bool inTurbulence = Energy != null && Energy.InWind && Energy.Wind == WindType.Turbulence;
            if (inAscending) gravity *= 0.3f;
            else if (inTurbulence) gravity *= 1.6f;

            IsGliding = InputSource.GlideHeld;
            if (IsGliding) gravity = gravityGlide * (inAscending ? 0.3f : inTurbulence ? 1.6f : 1f);

            // turbulência sacode o controle
            float steerX = steer.x;
            if (inTurbulence)
                steerX += Mathf.Sin(Time.time * 13.1f) * 0.16f;

            _vy -= gravity * dt;
            _vy += steer.y * tiltLiftAccel * dt;

            // ---- tap: bater asas ----
            if (InputSource.FlapPressed)
            {
                if (Energy != null && Energy.TryConsumeFlap())
                {
                    _vy = Mathf.Min(_vy + flapBoost, maxClimb);
                    _flapAnimTimer = 0.30f;
                    EventBus.Raise(new FlapEvent { EnergyAfter = Energy.Current });
                }
                else
                {
                    EventBus.Raise(new FlapBlockedEvent());
                }
            }

            _vy = Mathf.Clamp(_vy, -maxDive, maxClimb);

            // ---- integração ----
            Vector3 pos = _rb.position;
            float vx = Mathf.Clamp(steerX, -1.5f, 1.5f) * strafeSpeed;
            pos.x += vx * dt;
            pos.y += _vy * dt;
            pos.z = 0f;

            if (pos.x < -boundsX) { pos.x = -boundsX; }
            else if (pos.x > boundsX) { pos.x = boundsX; }

            if (pos.y <= minY)
            {
                pos.y = minY;
                if (_vy < 0f) GroundTouched = true;
                _vy = 0f;
            }
            else if (pos.y >= maxY)
            {
                pos.y = maxY;
                if (_vy > 0f) _vy = 0f;
            }

            // MovePosition (sem velocity): integração determinística p/ arcade,
            // colisões de trigger disparam normalmente com o RB dinâmico.
            _rb.MovePosition(pos);
        }

        private void Update()
        {
            if (ModelRoot == null) return;

            Vector2 steer = ControlsEnabled && InputSource != null
                ? InputSource.Steer
                : Vector2.zero;
            _visualSteer = Vector2.Lerp(_visualSteer, steer,
                1f - Mathf.Exp(-visualSmoothing * Time.deltaTime));

            // roll inclina para o lado do steer; pitch segue a velocidade
            // vertical (subir = nariz para cima, mergulhar = nariz p/ baixo,
            // com menos intensidade para nao parecer um capotamento)
            float roll = -_visualSteer.x * rollDegrees;
            float pitchRaw = _vy >= 0f ? -_vy * 2.2f : -_vy * 1.2f;
            float pitch = Mathf.Clamp(pitchRaw, -pitchDegrees, pitchDegrees);

            var target = Quaternion.Euler(pitch, 0f, roll);
            ModelRoot.localRotation = Quaternion.Slerp(
                ModelRoot.localRotation, target,
                1f - Mathf.Exp(-visualSmoothing * Time.deltaTime));

            AnimateWings();
        }

        private void AnimateWings()
        {
            if (WingLeft == null || WingRight == null) return;

            float angle;
            if (_flapAnimTimer > 0f)
            {
                _flapAnimTimer -= Time.deltaTime;
                float t = Mathf.Clamp01(_flapAnimTimer / 0.30f);
                angle = Mathf.Sin(t * Mathf.PI) * 50f;
            }
            else if (IsGliding)
            {
                angle = Mathf.Sin(Time.time * 5f) * 12f;
            }
            else
            {
                angle = Mathf.Sin(Time.time * 16f) * 7f;
            }

            WingRight.localRotation = Quaternion.Euler(0f, 0f, angle);
            WingLeft.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
