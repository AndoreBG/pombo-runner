using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // SensorCalibration — neutro do acelerômetro, dead-zone e mapeamento
    // de eixos. Os flags públicos permitem correção em dispositivo pelo
    // CalibrationUI (Inverter X / Inverter Y / Trocar eixos) e como
    // padrão no código.
    // ---------------------------------------------------------------------
    [System.Serializable]
    public sealed class SensorCalibration
    {
        // Padrões derivados da derivação do frame da tela (landscape):
        // inclinar p/ direita -> accel.x negativo; inclinar topo p/ cima ->
        // accel.y negativo. Se o aparelho do usuário divergir, o CalibrationUI
        // permite inverter/trocar ao vivo.
        public bool invertX = true;
        public bool invertY = true;
        public bool swapXY = false;

        [Range(0.5f, 4f)] public float sensitivity = 1.7f;
        [Range(0f, 0.3f)] public float deadZone = 0.09f;

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

            var steer = new Vector2(x, y) * sensitivity;
            if (steer.magnitude < deadZone) return Vector2.zero;
            return Vector2.ClampMagnitude(steer, 1f);
        }
    }
}
