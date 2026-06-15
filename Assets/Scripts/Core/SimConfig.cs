using UnityEngine;

[System.Serializable]
public class SimConfig
{
    public float voltage = 10000f;
    public float gasPressure = 101325f;
    public float coolingWindSpeed = 0f;
    public float breakdownFieldThreshold = 3.0e6f;
    public int maxParticleCount = 80000;
    public int fieldGridResolution = 128;
    public int thermalGridResolution = 128;
    public int particleEmitRate = 2000;
    public float particleLifetime = 2f;
    public float ionMobility = 1.5e-4f;
    public float electronMobility = 0.03f;
    public float recombinationCoeff = 2.0e-13f;
    public float gasTemperatureAmbient = 300f;
    public float arcTemperaturePeak = 20000f;
    public float thermalConductivity = 0.025f;
    public float thermalDiffusivity = 2.2e-5f;
    public float electrodeRadius = 0.02f;
    public float electrodeGap = 0.1f;
    public float domainSize = 1f;
    public float simulationSpeed = 1f;
    public float coilCurrent = 100f;
    public int coilTurns = 10;
    public float coilRadius = 0.1f;
    public float lorentzForceScale = 1e6f;
}
