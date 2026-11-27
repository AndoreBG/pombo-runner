using UnityEngine;

namespace PigeonRooftopRush
{
    public enum WindType
    {
        None,
        Ascending,
        Turbulence
    }


    public sealed class WindZone : MonoBehaviour
    {
        public WindType Type { get; private set; }

        private Transform[] _stripes;
        private Vector3[] _stripeBase;
        private float[] _phase;
        private bool _playerInside;

        public void Setup(WindType type, Transform[] stripes)
        {
            Type = type;
            _stripes = stripes;
            _stripeBase = new Vector3[stripes.Length];
            _phase = new float[stripes.Length];
            for (int i = 0; i < stripes.Length; i++)
            {
                _stripeBase[i] = stripes[i].localPosition;
                _phase[i] = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            }
        }

        public void SetPlayerInside(bool inside)
        {
            _playerInside = inside;
        }

        private void Update()
        {
            if (_stripes == null) return;
            float dt = Time.deltaTime;

            if (Type == WindType.Ascending)
            {
                float speed = _playerInside ? 10f : 7f;
                for (int i = 0; i < _stripes.Length; i++)
                {
                    Vector3 p = _stripes[i].localPosition;
                    p.y += speed * dt;
                    if (p.y > 18f) p.y -= 18f;
                    _stripes[i].localPosition = p;
                }
            }
            else if (Type == WindType.Turbulence)
            {
                float t = Time.time;
                float amp = _playerInside ? 1.5f : 1.1f;
                for (int i = 0; i < _stripes.Length; i++)
                {
                    Vector3 b = _stripeBase[i];
                    float ph = _phase[i];
                    _stripes[i].localPosition = new Vector3(
                        b.x + Mathf.Sin(t * 6.5f + ph) * amp,
                        b.y + Mathf.Sin(t * 2.7f + ph * 1.7f) * 0.8f,
                        b.z);
                }
            }
        }
    }
}
