using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // EditorInput — entrada por teclado para testar no Unity Editor
    // (mesmo papel do "EditorKeyboardInput" do diagrama do PDF).
    //   WASD / setas : inclinar (virar / subir / mergulhar)
    //   Espaço       : tap -> bater asas
    //   Shift        : hold -> planar
    // ---------------------------------------------------------------------
    public sealed class EditorInput : IFlightInput
    {
        private float _steerX;
        private float _steerY;
        private bool _flapPressed;
        private bool _glideHeld;

        public Vector2 Steer => new Vector2(_steerX, _steerY);
        public bool FlapPressed => _flapPressed;
        public bool GlideHeld => _glideHeld;
        public Vector3 RawSensor => new Vector3(_steerX, _steerY, -1f);
        public string DeviceLabel => "Teclado (editor)";

        public void Tick()
        {
            float x = 0f;
            float y = 0f;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;

            Vector2 steer = Vector2.ClampMagnitude(new Vector2(x, y), 1f);
            _steerX = steer.x;
            _steerY = steer.y;

            _flapPressed = Input.GetKeyDown(KeyCode.Space);
            _glideHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public void Calibrate()
        {
            // teclado não tem neutro — steer é absoluto
        }

        public void InvertAxis(bool horizontal)
        {
            // sem efeitos no teclado
        }

        public void SwapAxes()
        {
            // sem efeitos no teclado
        }
    }
}
