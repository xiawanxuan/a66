using UnityEngine;

[System.Serializable]
public class CoilData
{
    public Vector3 position;
    public Vector3 axisDirection;
    public float radius;
    public float current;
    public int turns;

    public CoilData(Vector3 position, Vector3 axis, float radius, float current, int turns)
    {
        this.position = position;
        this.axisDirection = axis.normalized;
        this.radius = radius;
        this.current = current;
        this.turns = turns;
    }
}
