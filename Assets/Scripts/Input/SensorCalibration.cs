using UnityEngine;

namespace PigeonRooftopRush
{


    [System.Serializable]
    public sealed class SensorCalibration
    {


        public bool invertX = false;
        public bool invertY = true;
        public bool swapXY = false;

        [Range(0.5f, 4f)] public float sensitivity = 1.2f;
        [Range(0f, 0.3f)] public float deadZone = 0.14f;

        public Vector3 Neutral = Vector3.zero;
        public bool IsSet { get; private set; }

        public void SetNeutral(Vector3 raw)
        {
            Neutral = raw;
            IsSet = true;
        }

        public void RefineNeutral(Vector3 raw, float t)
        {
            if (!IsSet) { SetNeutral(raw); return; }
            Neutral = Vector3.Lerp(Neutral, raw, t);
        }

        public void Reset()
        {
            Neutral = Vector3.zero;
            IsSet = false;
        }

        public Vector2 ToSteer(Vector3 raw)
        {
            Vector3 d = raw - Neutral;
            float x = d.x;
            float y = d.y;

            if (swapXY)
            {
                float tmp = x;
                x = y;
                y = tmp;
            }
            if (invertX) x = -x;
            if (invertY) y = -y;

            x = Mathf.Clamp(x * sensitivity, -1f, 1f);
            y = Mathf.Clamp(y * sensitivity, -1f, 1f);
            if (Mathf.Abs(x) < deadZone) x = 0f;
            if (Mathf.Abs(y) < deadZone) y = 0f;
            return new Vector2(x, y);
        }
    }
}
