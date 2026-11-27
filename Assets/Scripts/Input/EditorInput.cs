using UnityEngine;
using UnityEngine.InputSystem;

namespace PigeonRooftopRush
{
    public sealed class EditorInput : IFlightInput
    {
        private float _steerX;
        private float _steerY;
        private int _pendingFlaps;
        private bool _glideHeld;

        public Vector2 Steer => new Vector2(_steerX, _steerY);
        public bool GlideHeld => _glideHeld;
        public Vector3 RawSensor => new Vector3(_steerX, _steerY, -1f);
        public string DeviceLabel => "Teclado (editor)";

        public bool ConsumeFlap()
        {
            if (_pendingFlaps == 0) return false;
            _pendingFlaps--;
            return true;
        }

        public void Tick()
        {
            Keyboard keyboard = Keyboard.current;
            float x = 0f;
            float y = 0f;
            _glideHeld = false;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) y -= 1f;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) y += 1f;
                if (keyboard.spaceKey.wasPressedThisFrame) _pendingFlaps = Mathf.Min(_pendingFlaps + 1, 4);
                _glideHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
            }
            _steerX = x;
            _steerY = y;
        }

        public void Calibrate()
        {
            _pendingFlaps = 0;
            _steerX = 0f;
            _steerY = 0f;
        }

        public void InvertAxis(bool horizontal) { }
        public void SwapAxes() { }
    }
}
