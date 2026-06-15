using UnityEngine;

public class ArcSimSceneSetup : MonoBehaviour
{
    public Material cathodeMaterial;
    public Material anodeMaterial;

    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        GameObject simRoot = new GameObject("ArcSimulation");

        GameObject fieldSolverObj = new GameObject("FieldSolver");
        fieldSolverObj.transform.SetParent(simRoot.transform);
        FieldSolver fs = fieldSolverObj.AddComponent<FieldSolver>();

        GameObject bFieldSolverObj = new GameObject("MagneticFieldSolver");
        bFieldSolverObj.transform.SetParent(simRoot.transform);
        MagneticFieldSolver bfs = bFieldSolverObj.AddComponent<MagneticFieldSolver>();
        fs.magneticFieldSolver = bfs;

        GameObject plasmaObj = new GameObject("PlasmaSystem");
        plasmaObj.transform.SetParent(simRoot.transform);
        PlasmaParticleSystem ps = plasmaObj.AddComponent<PlasmaParticleSystem>();

        GameObject plasmaRendererObj = new GameObject("PlasmaRenderer");
        plasmaRendererObj.transform.SetParent(simRoot.transform);
        PlasmaParticleRenderer pr = plasmaRendererObj.AddComponent<PlasmaParticleRenderer>();
        plasmaRendererObj.AddComponent<ParticleSystem>();

        GameObject arcTriggerObj = new GameObject("ArcBreakdownTrigger");
        arcTriggerObj.transform.SetParent(simRoot.transform);
        ArcBreakdownTrigger at = arcTriggerObj.AddComponent<ArcBreakdownTrigger>();

        GameObject cathodeObj = new GameObject("Cathode");
        cathodeObj.transform.SetParent(simRoot.transform);
        cathodeObj.transform.position = new Vector3(-0.05f, 0f, 0f);
        ElectrodeGeometry cathode = cathodeObj.AddComponent<ElectrodeGeometry>();
        cathode.electrodeType = ElectrodeType.Cathode;
        if (cathodeMaterial != null)
            cathodeObj.GetComponent<MeshRenderer>().material = cathodeMaterial;

        GameObject anodeObj = new GameObject("Anode");
        anodeObj.transform.SetParent(simRoot.transform);
        anodeObj.transform.position = new Vector3(0.05f, 0f, 0f);
        ElectrodeGeometry anode = anodeObj.AddComponent<ElectrodeGeometry>();
        anode.electrodeType = ElectrodeType.Anode;
        if (anodeMaterial != null)
            anodeObj.GetComponent<MeshRenderer>().material = anodeMaterial;

        GameObject coilObj = new GameObject("MagneticCoil_01");
        coilObj.transform.SetParent(simRoot.transform);
        coilObj.transform.position = new Vector3(0f, 0f, 0.15f);
        coilObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        MagneticCoil coil = coilObj.AddComponent<MagneticCoil>();
        coil.current = 100f;
        coil.turns = 10;
        coil.radius = 0.1f;

        GameObject dragObj = new GameObject("DragInteraction");
        dragObj.transform.SetParent(simRoot.transform);
        ElectrodeDragInteraction drag = dragObj.AddComponent<ElectrodeDragInteraction>();

        GameObject snapshotObj = new GameObject("SnapshotSystem");
        snapshotObj.transform.SetParent(simRoot.transform);
        SimulationSnapshot snap = snapshotObj.AddComponent<SimulationSnapshot>();

        GameObject domainPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        domainPlane.transform.SetParent(simRoot.transform);
        domainPlane.transform.position = Vector3.zero;
        domainPlane.transform.localScale = Vector3.one;
        domainPlane.name = "DomainPlane";

        GameObject managerObj = new GameObject("SimulationManager");
        managerObj.transform.SetParent(simRoot.transform);
        SimulationManager mgr = managerObj.AddComponent<SimulationManager>();
        mgr.cathode = cathode;
        mgr.anode = anode;
        mgr.magneticCoils = new MagneticCoil[] { coil };
        mgr.fieldSolver = fs;
        mgr.magneticFieldSolver = bfs;
        mgr.plasmaSystem = ps;
        mgr.plasmaRenderer = pr;
        mgr.arcTrigger = at;
        mgr.dragInteraction = drag;
        mgr.snapshotSystem = snap;
        mgr.thermalOverlayRenderer = domainPlane.GetComponent<MeshRenderer>();

        Debug.Log("Arc Simulation scene setup complete. Use ArcSim > Generate All Resources to create materials.");
    }
}
