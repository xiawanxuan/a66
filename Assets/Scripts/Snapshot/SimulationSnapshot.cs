using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SnapshotData
{
    public float timestamp;
    public float voltage;
    public float gasPressure;
    public float coolingWindSpeed;
    public float[] potentialField;
    public float[] temperatureField;
    public float[] eFieldX;
    public float[] eFieldZ;
    public int particleCount;
    public float[] particlePosX;
    public float[] particlePosZ;
    public float[] particleVelX;
    public float[] particleVelZ;
    public float[] particleTemp;
    public float[] particleLife;
    public int[] particleCharge;
    public bool[] particleAlive;
    public float cathodeX;
    public float cathodeZ;
    public float anodeX;
    public float anodeZ;
    public float[] bFieldX;
    public float[] bFieldY;
    public float[] bFieldZ;
    public int coilCount;
    public float[] coilPosX;
    public float[] coilPosZ;
    public float[] coilAxisX;
    public float[] coilAxisZ;
    public float[] coilRadius;
    public float[] coilCurrent;
    public int[] coilTurns;
    public float coilCurrentParam;
    public int coilTurnsParam;
    public float coilRadiusParam;
    public float lorentzForceScale;
}

public class SimulationSnapshot : MonoBehaviour
{
    public string saveDirectory = "Snapshots";
    public int maxSnapshots = 20;

    private string savePath;
    private List<string> snapshotFiles = new List<string>();

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, saveDirectory);
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        LoadSnapshotList();
    }

    void LoadSnapshotList()
    {
        snapshotFiles.Clear();
        var files = Directory.GetFiles(savePath, "*.json");
        foreach (var f in files)
        {
            snapshotFiles.Add(Path.GetFileName(f));
        }
    }

    public void SaveSnapshot(
        FieldSolver fieldSolver,
        PlasmaParticleSystem plasmaSystem,
        SimParameterController paramController,
        ElectrodeGeometry cathode,
        ElectrodeGeometry anode,
        MagneticFieldSolver magneticFieldSolver,
        MagneticCoil[] coils,
        float simTime)
    {
        SnapshotData snap = new SnapshotData();
        snap.timestamp = simTime;
        snap.voltage = paramController.Voltage;
        snap.gasPressure = paramController.GasPressure;
        snap.coolingWindSpeed = paramController.CoolingWindSpeed;
        snap.coilCurrentParam = paramController.CoilCurrent;
        snap.coilTurnsParam = paramController.CoilTurns;
        snap.coilRadiusParam = paramController.CoilRadius;
        snap.lorentzForceScale = paramController.LorentzForceScale;

        fieldSolver.GetFieldData(out float[] potential, out Vector3[] eField, out float[] temperature);

        int gridSize = potential.Length;
        snap.potentialField = new float[gridSize];
        snap.temperatureField = new float[gridSize];
        snap.eFieldX = new float[gridSize];
        snap.eFieldZ = new float[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            snap.potentialField[i] = potential[i];
            snap.temperatureField[i] = temperature[i];
            snap.eFieldX[i] = eField[i].x;
            snap.eFieldZ[i] = eField[i].z;
        }

        if (magneticFieldSolver != null)
        {
            Vector3[] bField = magneticFieldSolver.MagneticField;
            if (bField != null && bField.Length == gridSize)
            {
                snap.bFieldX = new float[gridSize];
                snap.bFieldY = new float[gridSize];
                snap.bFieldZ = new float[gridSize];
                for (int i = 0; i < gridSize; i++)
                {
                    snap.bFieldX[i] = bField[i].x;
                    snap.bFieldY[i] = bField[i].y;
                    snap.bFieldZ[i] = bField[i].z;
                }
            }
        }

        if (coils != null)
        {
            List<MagneticCoil> validCoils = new List<MagneticCoil>();
            foreach (var c in coils) if (c != null) validCoils.Add(c);
            snap.coilCount = validCoils.Count;
            snap.coilPosX = new float[snap.coilCount];
            snap.coilPosZ = new float[snap.coilCount];
            snap.coilAxisX = new float[snap.coilCount];
            snap.coilAxisZ = new float[snap.coilCount];
            snap.coilRadius = new float[snap.coilCount];
            snap.coilCurrent = new float[snap.coilCount];
            snap.coilTurns = new int[snap.coilCount];

            for (int c = 0; c < validCoils.Count; c++)
            {
                var coil = validCoils[c];
                coil.SyncTransform();
                snap.coilPosX[c] = coil.transform.position.x;
                snap.coilPosZ[c] = coil.transform.position.z;
                snap.coilAxisX[c] = coil.transform.up.x;
                snap.coilAxisZ[c] = coil.transform.up.z;
                snap.coilRadius[c] = coil.radius;
                snap.coilCurrent[c] = coil.current;
                snap.coilTurns[c] = coil.turns;
            }
        }

        ParticleState[] particles = plasmaSystem.GetAllParticles();
        int maxP = particles.Length;

        List<int> aliveIndices = new List<int>();
        for (int i = 0; i < maxP; i++)
        {
            if (particles[i].alive) aliveIndices.Add(i);
        }

        snap.particleCount = aliveIndices.Count;
        snap.particlePosX = new float[aliveIndices.Count];
        snap.particlePosZ = new float[aliveIndices.Count];
        snap.particleVelX = new float[aliveIndices.Count];
        snap.particleVelZ = new float[aliveIndices.Count];
        snap.particleTemp = new float[aliveIndices.Count];
        snap.particleLife = new float[aliveIndices.Count];
        snap.particleCharge = new int[aliveIndices.Count];
        snap.particleAlive = new bool[aliveIndices.Count];

        for (int i = 0; i < aliveIndices.Count; i++)
        {
            ParticleState p = particles[aliveIndices[i]];
            snap.particlePosX[i] = p.position.x;
            snap.particlePosZ[i] = p.position.z;
            snap.particleVelX[i] = p.velocity.x;
            snap.particleVelZ[i] = p.velocity.z;
            snap.particleTemp[i] = p.temperature;
            snap.particleLife[i] = p.lifetime;
            snap.particleCharge[i] = p.charge;
            snap.particleAlive[i] = p.alive;
        }

        snap.cathodeX = cathode.transform.position.x;
        snap.cathodeZ = cathode.transform.position.z;
        snap.anodeX = anode.transform.position.x;
        snap.anodeZ = anode.transform.position.z;

        string filename = $"snapshot_{System.DateTime.Now:yyyyMMdd_HHmmss_fff}.json";
        string filepath = Path.Combine(savePath, filename);
        string json = JsonUtility.ToJson(snap);
        File.WriteAllText(filepath, json);

        snapshotFiles.Add(filename);

        while (snapshotFiles.Count > maxSnapshots)
        {
            string oldest = snapshotFiles[0];
            string oldestPath = Path.Combine(savePath, oldest);
            if (File.Exists(oldestPath)) File.Delete(oldestPath);
            snapshotFiles.RemoveAt(0);
        }
    }

    public bool LoadLatestSnapshot(
        FieldSolver fieldSolver,
        PlasmaParticleSystem plasmaSystem,
        SimParameterController paramController,
        ElectrodeGeometry cathode,
        ElectrodeGeometry anode,
        MagneticFieldSolver magneticFieldSolver,
        MagneticCoil[] coils,
        out float simTime)
    {
        simTime = 0f;

        if (snapshotFiles.Count == 0) return false;

        string latestFile = snapshotFiles[snapshotFiles.Count - 1];
        return LoadSnapshotByName(latestFile, fieldSolver, plasmaSystem, paramController, cathode, anode,
            magneticFieldSolver, coils, out simTime);
    }

    public bool LoadSnapshotByName(string filename,
        FieldSolver fieldSolver,
        PlasmaParticleSystem plasmaSystem,
        SimParameterController paramController,
        ElectrodeGeometry cathode,
        ElectrodeGeometry anode,
        MagneticFieldSolver magneticFieldSolver,
        MagneticCoil[] coils,
        out float simTime)
    {
        simTime = 0f;
        string filepath = Path.Combine(savePath, filename);
        if (!File.Exists(filepath)) return false;

        string json = File.ReadAllText(filepath);
        SnapshotData snap = JsonUtility.FromJson<SnapshotData>(json);
        if (snap == null) return false;

        simTime = snap.timestamp;

        int gridSize = snap.potentialField.Length;
        float[] potential = new float[gridSize];
        Vector3[] eField = new Vector3[gridSize];
        float[] temperature = new float[gridSize];

        for (int i = 0; i < gridSize; i++)
        {
            potential[i] = snap.potentialField[i];
            temperature[i] = snap.temperatureField[i];
            eField[i] = new Vector3(snap.eFieldX[i], 0f, snap.eFieldZ[i]);
        }

        fieldSolver.SetFieldData(potential, eField, temperature);

        if (magneticFieldSolver != null && snap.bFieldX != null && snap.bFieldX.Length == gridSize)
        {
            Vector3[] bField = new Vector3[gridSize];
            for (int i = 0; i < gridSize; i++)
            {
                bField[i] = new Vector3(snap.bFieldX[i], snap.bFieldY[i], snap.bFieldZ[i]);
            }
            magneticFieldSolver.SetFieldData(bField);
        }

        if (paramController != null)
        {
            paramController.CoilCurrent = snap.coilCurrentParam;
            paramController.CoilTurns = snap.coilTurnsParam;
            paramController.CoilRadius = snap.coilRadiusParam;
            paramController.LorentzForceScale = snap.lorentzForceScale;
        }

        if (coils != null && snap.coilCount > 0)
        {
            int n = Mathf.Min(snap.coilCount, coils.Length);
            for (int c = 0; c < n; c++)
            {
                var coil = coils[c];
                if (coil == null) continue;
                coil.transform.position = new Vector3(snap.coilPosX[c], 0f, snap.coilPosZ[c]);
                Vector3 axis = new Vector3(snap.coilAxisX[c], 0f, snap.coilAxisZ[c]);
                if (axis.sqrMagnitude > 1e-6f)
                    coil.transform.rotation = Quaternion.FromToRotation(Vector3.up, axis);
                coil.radius = snap.coilRadius[c];
                coil.current = snap.coilCurrent[c];
                coil.turns = snap.coilTurns[c];
                coil.GenerateTorusMesh();
            }
        }

        if (plasmaSystem != null)
            plasmaSystem.lorentzForceScale = snap.lorentzForceScale;

        plasmaSystem.ClearAll();
        for (int i = 0; i < snap.particleCount; i++)
        {
            Vector3 pos = new Vector3(snap.particlePosX[i], 0f, snap.particlePosZ[i]);
            Vector3 vel = new Vector3(snap.particleVelX[i], 0f, snap.particleVelZ[i]);
            plasmaSystem.SetParticleState(i, pos, vel, snap.particleTemp[i],
                snap.particleLife[i], snap.particleCharge[i], snap.particleAlive[i]);
        }

        cathode.transform.position = new Vector3(snap.cathodeX, 0f, snap.cathodeZ);
        anode.transform.position = new Vector3(snap.anodeX, 0f, snap.anodeZ);
        cathode.SyncPosition();
        anode.SyncPosition();

        return true;
    }

    public int GetSnapshotCount()
    {
        return snapshotFiles.Count;
    }

    public List<string> GetSnapshotNames()
    {
        return new List<string>(snapshotFiles);
    }

    public void ClearAllSnapshots()
    {
        foreach (var f in snapshotFiles)
        {
            string p = Path.Combine(savePath, f);
            if (File.Exists(p)) File.Delete(p);
        }
        snapshotFiles.Clear();
    }
}
