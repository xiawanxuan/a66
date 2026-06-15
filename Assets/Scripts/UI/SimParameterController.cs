using UnityEngine;
using UnityEngine.UI;

public class SimParameterController : MonoBehaviour
{
    public Slider voltageSlider;
    public Slider pressureSlider;
    public Slider windSpeedSlider;
    public Slider breakdownSlider;
    public Slider coilCurrentSlider;
    public Slider coilTurnsSlider;
    public Slider coilRadiusSlider;
    public Slider lorentzScaleSlider;
    public Text voltageLabel;
    public Text pressureLabel;
    public Text windSpeedLabel;
    public Text breakdownLabel;
    public Text coilCurrentLabel;
    public Text coilTurnsLabel;
    public Text coilRadiusLabel;
    public Text lorentzScaleLabel;

    public float Voltage { get; private set; } = 10000f;
    public float GasPressure { get; private set; } = 101325f;
    public float CoolingWindSpeed { get; private set; } = 0f;
    public float BreakdownThreshold { get; private set; } = 3.0e6f;
    public float CoilCurrent { get; private set; } = 100f;
    public int CoilTurns { get; private set; } = 10;
    public float CoilRadius { get; private set; } = 0.1f;
    public float LorentzForceScale { get; private set; } = 1e6f;

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

        if (coilCurrentSlider != null)
        {
            coilCurrentSlider.minValue = 0f;
            coilCurrentSlider.maxValue = 1000f;
            coilCurrentSlider.value = CoilCurrent;
            coilCurrentSlider.onValueChanged.AddListener(OnCoilCurrentChanged);
        }

        if (coilTurnsSlider != null)
        {
            coilTurnsSlider.minValue = 1f;
            coilTurnsSlider.maxValue = 200f;
            coilTurnsSlider.wholeNumbers = true;
            coilTurnsSlider.value = CoilTurns;
            coilTurnsSlider.onValueChanged.AddListener(OnCoilTurnsChanged);
        }

        if (coilRadiusSlider != null)
        {
            coilRadiusSlider.minValue = 0.01f;
            coilRadiusSlider.maxValue = 0.5f;
            coilRadiusSlider.value = CoilRadius;
            coilRadiusSlider.onValueChanged.AddListener(OnCoilRadiusChanged);
        }

        if (lorentzScaleSlider != null)
        {
            lorentzScaleSlider.minValue = 1e3f;
            lorentzScaleSlider.maxValue = 1e9f;
            lorentzScaleSlider.value = LorentzForceScale;
            lorentzScaleSlider.onValueChanged.AddListener(OnLorentzScaleChanged);
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

    void OnCoilCurrentChanged(float val)
    {
        CoilCurrent = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnCoilTurnsChanged(float val)
    {
        CoilTurns = Mathf.RoundToInt(val);
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnCoilRadiusChanged(float val)
    {
        CoilRadius = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void OnLorentzScaleChanged(float val)
    {
        LorentzForceScale = val;
        UpdateLabels();
        OnParametersChanged?.Invoke();
    }

    void UpdateLabels()
    {
        if (voltageLabel != null) voltageLabel.text = $"Voltage: {Voltage:F0} V";
        if (pressureLabel != null) pressureLabel.text = $"Pressure: {GasPressure:F0} Pa";
        if (windSpeedLabel != null) windSpeedLabel.text = $"Wind: {CoolingWindSpeed:F1} m/s";
        if (breakdownLabel != null) breakdownLabel.text = $"Breakdown: {BreakdownThreshold:F0} V/m";
        if (coilCurrentLabel != null) coilCurrentLabel.text = $"Coil I: {CoilCurrent:F0} A";
        if (coilTurnsLabel != null) coilTurnsLabel.text = $"Coil N: {CoilTurns}";
        if (coilRadiusLabel != null) coilRadiusLabel.text = $"Coil R: {CoilRadius:F3} m";
        if (lorentzScaleLabel != null) lorentzScaleLabel.text = $"Lorentz: {LorentzForceScale:E1}";
    }

    public void LoadFromConfig(SimConfig config)
    {
        Voltage = config.voltage;
        GasPressure = config.gasPressure;
        CoolingWindSpeed = config.coolingWindSpeed;
        BreakdownThreshold = config.breakdownFieldThreshold;
        CoilCurrent = config.coilCurrent;
        CoilTurns = config.coilTurns;
        CoilRadius = config.coilRadius;
        LorentzForceScale = config.lorentzForceScale;

        if (voltageSlider != null) voltageSlider.value = Voltage;
        if (pressureSlider != null) pressureSlider.value = GasPressure;
        if (windSpeedSlider != null) windSpeedSlider.value = CoolingWindSpeed;
        if (breakdownSlider != null) breakdownSlider.value = BreakdownThreshold;
        if (coilCurrentSlider != null) coilCurrentSlider.value = CoilCurrent;
        if (coilTurnsSlider != null) coilTurnsSlider.value = CoilTurns;
        if (coilRadiusSlider != null) coilRadiusSlider.value = CoilRadius;
        if (lorentzScaleSlider != null) lorentzScaleSlider.value = LorentzForceScale;

        UpdateLabels();
    }

    public void ApplyToConfig(SimConfig config)
    {
        config.voltage = Voltage;
        config.gasPressure = GasPressure;
        config.coolingWindSpeed = CoolingWindSpeed;
        config.breakdownFieldThreshold = BreakdownThreshold;
        config.coilCurrent = CoilCurrent;
        config.coilTurns = CoilTurns;
        config.coilRadius = CoilRadius;
        config.lorentzForceScale = LorentzForceScale;
    }
}
