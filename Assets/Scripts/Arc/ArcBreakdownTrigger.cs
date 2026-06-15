using UnityEngine;

public class ArcBreakdownTrigger : MonoBehaviour
{
    public float breakdownThreshold = 3.0e6f;
    public float arcSustainField = 1.0e6f;
    public float arcRadius = 0.015f;

    private bool arcActive;
    private Vector3 arcCenter;
    private float currentFieldStrength;
    private float arcIntensity;

    public bool ArcActive => arcActive;
    public Vector3 ArcCenter => arcCenter;
    public float ArcIntensity => arcIntensity;

    public void CheckBreakdown(FieldSolver fieldSolver, ElectrodeData cathode, ElectrodeData anode)
    {
        currentFieldStrength = fieldSolver.GetMaxFieldStrength();

        if (!arcActive)
        {
            if (currentFieldStrength >= breakdownThreshold)
            {
                TriggerArc(cathode, anode);
            }
        }
        else
        {
            if (currentFieldStrength < arcSustainField)
            {
                ExtinguishArc();
            }
            else
            {
                UpdateArcCenter(cathode, anode);
                arcIntensity = Mathf.Clamp01(currentFieldStrength / breakdownThreshold);
            }
        }
    }

    void TriggerArc(ElectrodeData cathode, ElectrodeData anode)
    {
        arcActive = true;
        UpdateArcCenter(cathode, anode);
        arcIntensity = 1f;
    }

    void ExtinguishArc()
    {
        arcActive = false;
        arcIntensity = 0f;
    }

    void UpdateArcCenter(ElectrodeData cathode, ElectrodeData anode)
    {
        arcCenter = (cathode.position + anode.position) * 0.5f;
    }

    public float GetArcLength(ElectrodeData cathode, ElectrodeData anode)
    {
        return Vector3.Distance(cathode.position, anode.position);
    }

    public float GetArcColumnRadius(float gasPressure, float current)
    {
        float radius0 = 0.005f;
        float pressureNorm = gasPressure / 101325f;
        float currentNorm = current / 100f;
        return radius0 * Mathf.Sqrt(currentNorm) / Mathf.Sqrt(pressureNorm);
    }
}
