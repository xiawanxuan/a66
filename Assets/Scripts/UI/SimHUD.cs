using UnityEngine;
using UnityEngine.UI;

public class SimHUD : MonoBehaviour
{
    public SimulationManager simManager;
    public Text statusText;
    public Text fpsText;
    public Text particleCountText;
    public Text fieldStrengthText;
    public Text arcStatusText;
    public Text simTimeText;

    private float fpsTimer;
    private int frameCount;
    private float currentFPS;

    void Update()
    {
        UpdateFPS();
        UpdateHUD();
    }

    void UpdateFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 0.5f)
        {
            currentFPS = frameCount / fpsTimer;
            frameCount = 0;
            fpsTimer = 0f;
        }
    }

    void UpdateHUD()
    {
        if (simManager == null) return;

        if (fpsText != null)
            fpsText.text = $"FPS: {currentFPS:F0}";

        if (particleCountText != null)
            particleCountText.text = $"Particles: {simManager.ParticleCount}";

        if (statusText != null)
            statusText.text = simManager.IsRunning ? "Running" : "Paused";

        if (simTimeText != null)
            simTimeText.text = $"Time: {simManager.SimulationTime:F2}s";

        if (arcStatusText != null)
        {
            if (simManager.arcTrigger != null)
            {
                arcStatusText.text = simManager.arcTrigger.ArcActive
                    ? $"<color=yellow>Arc Active ({simManager.arcTrigger.ArcIntensity:P0})</color>"
                    : "<color=gray>Arc Off</color>";
            }
        }

        if (fieldStrengthText != null && simManager.fieldSolver != null)
        {
            float maxE = simManager.fieldSolver.GetMaxFieldStrength();
            fieldStrengthText.text = $"E_max: {maxE:E2} V/m";
        }
    }
}
