using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using Gyroscope = UnityEngine.InputSystem.Gyroscope;

namespace PigeonRooftopRush
{
    public sealed class MobileTiltInput : IFlightInput, IDisposable
    {
        private enum SensorSource { None, Gravity, Accelerometer }

        private const float GlideThreshold = 0.22f;
        private const float NeutralRefineSeconds = 0.5f;
        private const float StillAngularSpeedSquared = 0.04f;

        public readonly SensorCalibration Calibration = new SensorCalibration();
        private readonly HashSet<InputDevice> _enabledSensors = new HashSet<InputDevice>();
        private SensorSource _source;
        private Vector3 _rawSensor;
        private float _steerX;
        private float _steerY;
        private int _pendingFlaps;
        private bool _glideHeld;
        private bool _touchActive;
        private bool _waitingForTouchRelease;
        private int _touchId;
        private float _touchStartedAt;
        private float _neutralRefineUntil;
        private bool _disposed;

        public Vector2 Steer => new Vector2(_steerX, _steerY);
        public bool GlideHeld => _glideHeld;
        public Vector3 RawSensor => _rawSensor;
        public string DeviceLabel
        {
            get
            {
                string sensor = _source == SensorSource.Gravity ? "Gravidade" :
                    _source == SensorSource.Accelerometer ? "Acelerômetro" : "Aguardando sensor";
                return Gyroscope.current != null && Gyroscope.current.enabled
                    ? sensor + " + giroscópio"
                    : sensor;
            }
        }

        public MobileTiltInput()
        {
            InputSystem.settings.compensateForScreenOrientation = true;
            EnhancedTouchSupport.Enable();
            EnsureSensors();
        }

        private void EnsureSensors()
        {
            EnableSensor(GravitySensor.current);
            EnableSensor(Accelerometer.current);
            EnableSensor(Gyroscope.current);
        }

        private void EnableSensor(InputDevice sensor)
        {
            if (sensor == null || sensor.enabled) return;
            InputSystem.EnableDevice(sensor);
            if (sensor.enabled) _enabledSensors.Add(sensor);
        }

        private static bool TryReadSensor(SensorSource source, out Vector3 reading)
        {
            reading = Vector3.zero;
            if (source == SensorSource.Gravity && GravitySensor.current != null && GravitySensor.current.enabled)
                reading = GravitySensor.current.gravity.ReadValue();
            else if (source == SensorSource.Accelerometer && Accelerometer.current != null && Accelerometer.current.enabled)
                reading = Accelerometer.current.acceleration.ReadValue();
            else
                return false;

            float magnitude = reading.magnitude;
            if (float.IsNaN(magnitude) || float.IsInfinity(magnitude) || magnitude < 0.1f)
                return false;
            reading /= magnitude;
            return true;
        }

        private bool SelectSensor(out Vector3 reading)
        {
            if (TryReadSensor(SensorSource.Gravity, out reading))
            {
                _source = SensorSource.Gravity;
                return true;
            }
            if (TryReadSensor(SensorSource.Accelerometer, out reading))
            {
                _source = SensorSource.Accelerometer;
                return true;
            }
            _source = SensorSource.None;
            reading = Vector3.zero;
            return false;
        }

        public void Tick()
        {
            EnsureSensors();
            Vector3 reading;
            if (!TryReadSensor(_source, out reading))
            {
                if (SelectSensor(out reading)) Calibration.SetNeutral(reading);
                else Calibration.Reset();
            }

            _rawSensor = reading;
            if (Calibration.IsSet)
            {
                bool gyroStill = Gyroscope.current == null || !Gyroscope.current.enabled ||
                    Gyroscope.current.angularVelocity.ReadValue().sqrMagnitude < StillAngularSpeedSquared;
                if (Time.time < _neutralRefineUntil)
                {
                    if (gyroStill && (reading - Calibration.Neutral).magnitude < 0.15f)
                        Calibration.RefineNeutral(reading, 0.25f);
                    else
                        _neutralRefineUntil = 0f;
                }

                Vector2 steer = Calibration.ToSteer(reading);
                _steerX = steer.x;
                _steerY = steer.y;
            }
            else
            {
                _steerX = 0f;
                _steerY = 0f;
            }

            _glideHeld = false;
            var touches = Touch.activeTouches;
            if (_waitingForTouchRelease)
            {
                if (touches.Count == 0) _waitingForTouchRelease = false;
                return;
            }

            if (!_touchActive)
            {
                for (int i = 0; i < touches.Count; i++)
                {
                    Touch touch = touches[i];
                    if (touch.phase != TouchPhase.Began) continue;
                    _touchId = touch.touchId;
                    _touchStartedAt = Time.unscaledTime;
                    _touchActive = true;
                    break;
                }
            }

            if (_touchActive)
            {
                bool found = false;
                for (int i = 0; i < touches.Count; i++)
                {
                    Touch touch = touches[i];
                    if (touch.touchId != _touchId) continue;
                    found = true;
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        double duration = touch.time - touch.startTime;
                        if (touch.phase == TouchPhase.Ended && duration >= 0 && duration < GlideThreshold)
                            _pendingFlaps = Mathf.Min(_pendingFlaps + 1, 4);
                        _touchActive = false;
                    }
                    else
                    {
                        _glideHeld = Time.unscaledTime - _touchStartedAt >= GlideThreshold;
                    }
                    break;
                }
                if (!found) _touchActive = false;
            }
        }

        public bool ConsumeFlap()
        {
            if (_pendingFlaps == 0) return false;
            _pendingFlaps--;
            return true;
        }

        public void Calibrate()
        {
            EnsureSensors();
            Vector3 reading;
            if (SelectSensor(out reading)) Calibration.SetNeutral(reading);
            else Calibration.Reset();
            _rawSensor = reading;
            _steerX = 0f;
            _steerY = 0f;
            _neutralRefineUntil = Time.time + NeutralRefineSeconds;
            _pendingFlaps = 0;
            _touchActive = false;
            _glideHeld = false;
            _waitingForTouchRelease = Touch.activeTouches.Count > 0;
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            EnhancedTouchSupport.Disable();
            foreach (InputDevice sensor in _enabledSensors)
                if (sensor != null && sensor.added && sensor.enabled) InputSystem.DisableDevice(sensor);
            _enabledSensors.Clear();
        }
    }
}
