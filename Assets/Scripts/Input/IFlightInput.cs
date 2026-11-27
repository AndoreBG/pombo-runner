using UnityEngine;

namespace PigeonRooftopRush
{


    public interface IFlightInput
    {

        Vector2 Steer { get; }


        bool ConsumeFlap();


        bool GlideHeld { get; }


        Vector3 RawSensor { get; }


        string DeviceLabel { get; }


        void Tick();


        void Calibrate();


        void InvertAxis(bool horizontal);


        void SwapAxes();
    }
}
