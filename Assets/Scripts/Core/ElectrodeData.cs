using UnityEngine;

public enum ElectrodeType
{
    Cathode,
    Anode
}

public class ElectrodeData
{
    public ElectrodeType type;
    public Vector3 position;
    public float radius;
    public float potential;

    public ElectrodeData(ElectrodeType type, Vector3 position, float radius, float potential)
    {
        this.type = type;
        this.position = position;
        this.radius = radius;
        this.potential = potential;
    }
}
