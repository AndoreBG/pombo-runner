using UnityEngine;

namespace PigeonRooftopRush
{
    // ---------------------------------------------------------------------
    // IFlightInput — Strategy de entrada de voo (padrão do PDF).
    // MobileTiltInput  : acelerômetro + calibragem (aparelho)
    // EditorInput      : teclado WASD/setas (Unity Editor)
    // ---------------------------------------------------------------------
    public interface IFlightInput
    {
        /// <summary>Steer calibrado, -1..1 em X (virar) e Y (subir/mergulhar).</summary>
        Vector2 Steer { get; }

        /// <summary>Borda de "bater asas" (tap) neste frame.</summary>
        bool FlapPressed { get; }

        /// <summary>Segurar pressionado = planar.</summary>
        bool GlideHeld { get; }

        /// <summary>Leitura crua do sensor (para exibição no CalibrationUI).</summary>
        Vector3 RawSensor { get; }

        /// <summary>Nome do dispositivo para a UI.</summary>
        string DeviceLabel { get; }

        /// <summary>Chamado uma vez por frame pelo GameManager.</summary>
        void Tick();

        /// <summary>Captura a posição neutra (calibragem).</summary>
        void Calibrate();

        /// <summary>Inverte um eixo do steer (ajuste em dispositivo).</summary>
        void InvertAxis(bool horizontal);

        /// <summary>Troca X/Y do steer (ajuste em dispositivo).</summary>
        void SwapAxes();
    }
}
