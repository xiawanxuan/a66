using UnityEngine;
using System.Collections.Generic;

public class SimulationManager : MonoBehaviour
{
    public ElectrodeGeometry cathode;
    public ElectrodeGeometry anode;
    public MagneticCoil[] magneticCoils;
    public FieldSolver fieldSolver;
    public MagneticFieldSolver magneticFieldSolver;
    public PlasmaParticleSystem plasmaSystem;
    public PlasmaParticleRenderer plasmaRenderer;
    public ArcBreakdownTrigger arcTrigger;
    public ElectrodeDragInteraction dragInteraction;
    public SimParameterController paramController;
    public SimulationSnapshot snapshotSystem;

    public MeshRenderer thermalOverlayRenderer;

    public bool autoStart = true;
    public float fieldSolveInterval = 0.1f;

    private SimConfig config;
    private float simulationTime;
    private float lastFieldSolveTime;
    private bool isRunning;
    private Texture2D thermalTexture;

    public bool IsRunning => isRunning;
    public float SimulationTime => simulationTime;
    public int ParticleCount => plasmaSystem != null ? plasmaSystem.AliveCount : 0;

    void Start()
    {
        LoadConfig();
        InitializeSystems();

        if (autoStart)
            StartSimulation();
    }

    void LoadConfig()
    {
        SimConfigAsset asset = SimConfigAsset.LoadFromRuntime();
        config = asset.config;
    }

    void InitializeSystems()
    {
        if (fieldSolver != null)
        {
            fieldSolver.gridResolution = config.fieldGridResolution;
            fieldSolver.domainSize = config.domainSize;
            fieldSolver.InitializeGrids();

            if (magneticFieldSolver != null)
            {
                magneticFieldSolver.gridResolution = config.fieldGridResolution;
                magneticFieldSolver.domainSize = config.domainSize;
                magneticFieldSolver.InitializeGrid();
                fieldSolver.magneticFieldSolver = magneticFieldSolver;
            }
        }

        if (plasmaSystem != null)
        {
            plasmaSystem.maxParticles = config.maxParticleCount;
            plasmaSystem.emitRate = config.particleEmitRate;
            plasmaSystem.particleLifetime = config.particleLifetime;
            plasmaSystem.electronMobility = config.electronMobility;
            plasmaSystem.ionMobility = config.ionMobility;
            plasmaSystem.recombinationCoeff = config.recombinationCoeff;
            plasmaSystem.domainSize = config.domainSize;
            plasmaSystem.InitializePool();
        }

        if (plasmaRenderer != null && plasmaSystem != null)
        {
            plasmaRenderer.SetPlasmaSystem(plasmaSystem);
        }

        if (arcTrigger != null)
        {
            arcTrigger.breakdownThreshold = config.breakdownFieldThreshold;
        }

        if (paramController != null)
        {
            paramController.LoadFromConfig(config);
            paramController.OnParametersChanged += OnParametersChanged;
        }

        if (dragInteraction != null)
        {
            dragInteraction.OnElectrodeMoved += OnElectrodeMoved;
        }

        if (cathode != null)
        {
            cathode.electrodeType = ElectrodeType.Cathode;
            cathode.radius = config.electrodeRadius;
        }

        if (anode != null)
        {
            anode.electrodeType = ElectrodeType.Anode;
            anode.radius = config.electrodeRadius;
        }

        InitializeThermalTexture();
    }

    void InitializeThermalTexture()
    {
        int res = config.thermalGridResolution;
        thermalTexture = new Texture2D(res, res, TextureFormat.RFloat, false);
        thermalTexture.wrapMode = TextureWrapMode.Clamp;
        thermalTexture.filterMode = FilterMode.Bilinear;

        if (thermalOverlayRenderer != null)
        {
            thermalOverlayRenderer.material.SetTexture("_HeatTex", thermalTexture);
        }
    }

    public void StartSimulation()
    {
        isRunning = true;
        simulationTime = 0f;
        lastFieldSolveTime = 0f;
    }

    public void PauseSimulation()
    {
        isRunning = false;
    }

    public void ResumeSimulation()
    {
        isRunning = true;
    }

    public void ResetSimulation()
    {
        isRunning = false;
        simulationTime = 0f;
        lastFieldSolveTime = 0f;

        if (plasmaSystem != null) plasmaSystem.ClearAll();
        if (fieldSolver != null) fieldSolver.InitializeGrids();
    }

    void Update()
    {
        if (!isRunning) return;

        float dt = Time.deltaTime * config.simulationSpeed;
        simulationTime += dt;

        if (simulationTime - lastFieldSolveTime >= fieldSolveInterval)
        {
            SolveFields(dt);
            lastFieldSolveTime = simulationTime;
        }

        UpdateArcState();

        if (arcTrigger != null && arcTrigger.ArcActive)
        {
            EmitPlasmaParticles();
        }

        if (plasmaSystem != null)
        {
            plasmaSystem.Simulate(dt, fieldSolver, paramController != null ? paramController.CoolingWindSpeed : 0f);
        }

        UpdateThermalOverlay();

        HandleInput();
    }

    void SolveFields(float dt)
    {
        if (fieldSolver == null) return;

        List<ElectrodeData> electrodes = new List<ElectrodeData>();
        if (cathode != null)
        {
            cathode.SyncPosition();
            cathode.UpdatePotential(paramController != null ? paramController.Voltage : config.voltage);
            electrodes.Add(cathode.Data);
        }
        if (anode != null)
        {
            anode.SyncPosition();
            anode.UpdatePotential(paramController != null ? paramController.Voltage : config.voltage);
            electrodes.Add(anode.Data);
        }

        float voltage = paramController != null ? paramController.Voltage : config.voltage;
        fieldSolver.SolveElectricField(electrodes, voltage);

        if (magneticFieldSolver != null && magneticCoils != null)
        {
            List<CoilData> coils = new List<CoilData>();
            foreach (var coil in magneticCoils)
            {
                if (coil == null) continue;
                coil.SyncTransform();
                coils.Add(coil.Data);
            }
            magneticFieldSolver.SolveMagneticField(coils);
        }

        float coolingWind = paramController != null ? paramController.CoolingWindSpeed : config.coolingWindSpeed;
        float ambientTemp = config.gasTemperatureAmbient;

        bool arcActive = arcTrigger != null && arcTrigger.ArcActive;
        Vector3 arcCenter = arcTrigger != null ? arcTrigger.ArcCenter : Vector3.zero;
        float arcRadius = arcTrigger != null ? arcTrigger.arcRadius : 0f;

        fieldSolver.SolveThermalField(
            dt,
            coolingWind,
            ambientTemp,
            config.thermalDiffusivity,
            arcActive,
            arcCenter,
            arcRadius,
            config.arcTemperaturePeak
        );
    }

    void UpdateArcState()
    {
        if (arcTrigger == null) return;
        if (cathode == null || anode == null) return;

        arcTrigger.breakdownThreshold = paramController != null ? paramController.BreakdownThreshold : config.breakdownFieldThreshold;
        arcTrigger.CheckBreakdown(fieldSolver, cathode.Data, anode.Data);
    }

    void EmitPlasmaParticles()
    {
        if (plasmaSystem == null || cathode == null || anode == null) return;

        Vector3 cathodePos = cathode.transform.position;
        Vector3 anodePos = anode.transform.position;
        Vector3 direction = (anodePos - cathodePos).normalized;
        Vector3 midPoint = (cathodePos + anodePos) * 0.5f;

        int emitCount = Mathf.CeilToInt(config.particleEmitRate * Time.deltaTime);

        plasmaSystem.Emit(cathodePos, direction, emitCount / 2, -1, cathode.radius, config.arcTemperaturePeak * 0.7f);
        plasmaSystem.Emit(anodePos, -direction, emitCount / 2, 1, anode.radius, config.arcTemperaturePeak * 0.5f);
        plasmaSystem.Emit(midPoint, Random.insideUnitSphere, emitCount / 4, -1, arcTrigger.arcRadius * 2f, config.arcTemperaturePeak);
    }

    void UpdateThermalOverlay()
    {
        if (thermalTexture == null || fieldSolver == null) return;

        float[] temp = fieldSolver.Temperature;
        int n = fieldSolver.CellSize > 0 ? Mathf.RoundToInt(Mathf.Sqrt(temp.Length)) : 0;

        if (n == 0) return;

        Color[] pixels = new Color[n * n];
        float maxTemp = config.arcTemperaturePeak;

        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                int idx = j * n + i;
                float t = temp[idx] / maxTemp;
                pixels[idx] = new Color(t, t, t, 1f);
            }
        }

        thermalTexture.SetPixels(pixels);
        thermalTexture.Apply();
    }

    void OnParametersChanged()
    {
        if (paramController != null)
        {
            paramController.ApplyToConfig(config);
        }

        if (cathode != null) cathode.UpdatePotential(paramController != null ? paramController.Voltage : config.voltage);
        if (anode != null) anode.UpdatePotential(paramController != null ? paramController.Voltage : config.voltage);

        if (magneticCoils != null && paramController != null)
        {
            foreach (var coil in magneticCoils)
            {
                if (coil == null) continue;
                coil.current = paramController.CoilCurrent;
                coil.turns = paramController.CoilTurns;
                coil.radius = paramController.CoilRadius;
                coil.UpdateCurrent(paramController.CoilCurrent);
                coil.UpdateTurns(paramController.CoilTurns);
                coil.GenerateTorusMesh();
            }
        }

        if (plasmaSystem != null && paramController != null)
        {
            plasmaSystem.lorentzForceScale = paramController.LorentzForceScale;
        }
    }

    void OnElectrodeMoved()
    {
        if (fieldSolver != null)
        {
            SolveFields(Time.deltaTime);
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveSnapshot();
        }

        if (Input.GetKeyDown(KeyCode.F9))
        {
            LoadSnapshot();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isRunning) PauseSimulation();
            else ResumeSimulation();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetSimulation();
        }
    }

    public void SaveSnapshot()
    {
        if (snapshotSystem == null) return;
        snapshotSystem.SaveSnapshot(fieldSolver, plasmaSystem, paramController, cathode, anode,
            magneticFieldSolver, magneticCoils, simulationTime);
    }

    public void LoadSnapshot()
    {
        if (snapshotSystem == null) return;
        if (snapshotSystem.LoadLatestSnapshot(fieldSolver, plasmaSystem, paramController, cathode, anode,
            magneticFieldSolver, magneticCoils, out float time))
        {
            simulationTime = time;
        }
    }

    void OnDestroy()
    {
        if (paramController != null)
            paramController.OnParametersChanged -= OnParametersChanged;

        if (dragInteraction != null)
            dragInteraction.OnElectrodeMoved -= OnElectrodeMoved;

        if (thermalTexture != null)
            Destroy(thermalTexture);
    }
}
