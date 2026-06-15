using UnityEngine;
using UnityEngine.UI;

public class SimParameterController : MonoBehaviour
{
    public Slider voltageSlider;
    public Slider pressureSlider;
    public Slider windSpeedSlider;
    public Slider breakdownSlider;
    public Text voltageLabel;
    public Text pressureLabel;
    public Text windSpeedLabel;
    public Text breakdownLabel;

    public float Voltage { get; private set; } = 10000f;
    public float GasPressure { get; private set; } = 101325f;
    public float CoolingWindSpeed { get; private set; } = 0f;
    public float BreakdownThreshold { get; private set; } = 3.0e6f;

    public System.Action OnParametersChanged;

    void Start()
    {
        InitializeSliders();
    }

    void InitializeSliders()
    {
        if (voltageSlider != null)
        {
            voltageSlider.minValue = 0f;
            voltageSlider.maxValue = 50000f;
            voltageSlider.value = Voltage;
            voltageSlider.onValueChanged.AddListener(OnVoltageChanged);
        }

        if (pressureSlider != null)
        {
            pressureSlider.minValue = 1000f;
            pressureSlider.maxValue = 500000f;
            pressureSlider.value = GasPressure;
            pressureSlider.onValueChanged.AddListener(OnPressureChanged);
        }

        if (windSpeedSlider != null)
        {
            windSpeedSlider.minValue = 0f;
            windSpeedSlider.maxValue = 50f;
            windSpeedSlider.value = CoolingWindSpeed;
            windSpeedSlider.onValueChanged.AddListener(OnWindSpeedChanged);
        }

        if (breakdownSlider != null)
        {
            breakdownSlider.minValue = 1e5f;
            breakdownSlider.maxValue = 1e7f;
            breakdownSlider.value = BreakdownThreshold;
            breakdownSlider.onValueChanged.AddListener(OnBreakdownChanged);
        }

        UpdateLabels();
    }

    void OnVoltageChanged(float val)
    {
        Voltage = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnPressureChanged(float val)
    {
        GasPressure = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnWindSpeedChanged(float val)
    {
        CoolingWindSpeed = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnBreakdownChanged(float val)
    {
        BreakdownThreshold = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void UpdateLabels()
    {
        if (voltageLabel != null) voltageLabel.text = $"Voltage: {Voltage:F0} V";
        if (pressureLabel != null) pressureLabel.text = $"Pressure: {GasPressure:F0} Pa";
        if (windSpeedLabel != null) windSpeedLabel.text = $"Wind: {CoolingWindSpeed:F1} m/s";
        if (breakdownLabel != null) breakdownLabel.text = $"Breakdown: {BreakdownThreshold:F0} V/m";
    }

    public void LoadFromConfig(SimConfig config)
    {
        Voltage = config.voltage;
        GasPressure = config.gasPressure;
        CoolingWindSpeed = config.coolingWindSpeed;
        BreakdownThreshold = config.breakdownFieldThreshold;

        if (voltageSlider != null) voltageSlider.value = Voltage;
        if (pressureSlider != null) pressureSlider.value = GasPressure;
        if (windSpeedSlider != null) windSpeedSlider.value = CoolingWindSpeed;
        if (breakdownSlider != null) breakdownSlider.value = BreakdownThreshold;

        UpdateLabels();
    }

    public void ApplyToConfig(SimConfig config)
    {
        config.voltage = Voltage;
        config.gasPressure = GasPressure;
        config.coolingWindSpeed = CoolingWindSpeed;
        config.breakdownFieldThreshold = BreakdownThreshold;
    }
}
