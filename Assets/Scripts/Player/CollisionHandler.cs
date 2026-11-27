using UnityEngine;

namespace PigeonRooftopRush
{


    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class CollisionHandler : MonoBehaviour
    {
        public EnergySystem Energy { get; set; }
        public bool Running { get; set; }

        private void OnTriggerEnter(Collider other)
        {
            if (!Running || other == null) return;

            if (other.GetComponent<ObstacleMarker>() != null)
            {
                Running = false;
                EventBus.Raise(new CrashEvent { Reason = CrashReason.Obstacle });
                return;
            }

            var wind = other.GetComponent<WindZone>();
            if (wind != null && wind.Type != WindType.None)
            {
                Energy?.SetWind(wind.Type, true);
                wind.SetPlayerInside(true);
                EventBus.Raise(new WindChangedEvent { Type = wind.Type, Entered = true });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other == null) return;

            var wind = other.GetComponent<WindZone>();
            if (wind != null && wind.Type != WindType.None)
            {

                if (Energy != null && Energy.InWind && Energy.Wind == wind.Type)
                {
                    Energy.SetWind(WindType.None, false);
                    EventBus.Raise(new WindChangedEvent { Type = wind.Type, Entered = false });
                }
                wind.SetPlayerInside(false);
            }
        }
    }
}
