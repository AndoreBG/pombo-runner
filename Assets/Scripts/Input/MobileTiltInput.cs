using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // MobileTiltInput — entrada por inclinação do aparelho (acelerômetro).
    //
    // Input.compensatingSensors = true faz o Unity reportar o acelerômetro
    // já compensado na orientação atual da tela (landscape), eliminando o
    // mapeamento manual de eixos entre LandscapeLeft/LandscapeRight.
    //
    // Touch: tap = bater asas; segurar > GlideThreshold = planar.
    // ---------------------------------------------------------------------
    public sealed class MobileTiltInput : IFlightInput
    {
        private const float GlideThreshold = 0.22f;
        private const float NeutralRefineSeconds = 0.5f;

        public readonly SensorCalibration Calibration = new SensorCalibration();

        private float _steerX;
        private float _steerY;
        private bool _flapPressed;
        private bool _glideHeld;
        private bool _touchActive;
        private float _touchTimer;
        private float _neutralRefineUntil;

        public Vector2 Steer => new Vector2(_steerX, _steerY);
        public bool FlapPressed => _flapPressed;
        public bool GlideHeld => _glideHeld;
        public Vector3 RawSensor => Input.acceleration;
        public string DeviceLabel => "Acelerômetro + touch";

        public MobileTiltInput()
        {
            // necessário para a compensação de orientação funcionar
            Input.compensatingSensors = true;
        }

        public void Tick()
        {
            Vector3 acc = Input.acceleration;

            // refinamento pós-calibragem: usuário ainda está com o aparelho
            // na pose neutra logo após tocar em COMEÇAR
            if (Time.time < _neutralRefineUntil)
            {
                if ((acc - Calibration.Neutral).magnitude < 0.15f)
                    Calibration.RefineNeutral(acc, 0.25f);
                else
                    _neutralRefineUntil = 0f; // usuario ja comecou a inclinar
            }

            Vector2 steer = Calibration.ToSteer(acc);
            _steerX = steer.x;
            _steerY = steer.y;

            // ---- touch: tap = flap, hold = glide ----
            _flapPressed = false;
            _glideHeld = false;

            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    _flapPressed = true;
                    _touchActive = true;
                    _touchTimer = 0f;
                }

                if (_touchActive)
                {
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        _touchActive = false;
                    }
                    else
                    {
                        _touchTimer += Time.deltaTime;
                        if (_touchTimer >= GlideThreshold)
                            _glideHeld = true;
                    }
                }
            }
            else
            {
                _touchActive = false;
            }
        }

        public void Calibrate()
        {
            Calibration.SetNeutral(Input.acceleration);
            _neutralRefineUntil = Time.time + NeutralRefineSeconds;
        }

        public void InvertAxis(bool horizontal)
        {
            if (horizontal) Calibration.invertX = !Calibration.invertX;
            else Calibration.invertY = !Calibration.invertY;
        }

        public void SwapAxes()
        {
            Calibration.swapXY = !Calibration.swapXY;
        }
    }
}
