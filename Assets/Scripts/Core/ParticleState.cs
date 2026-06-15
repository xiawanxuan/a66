using UnityEngine;

public class ParticleState
{
    public Vector3 position;
    public Vector3 velocity;
    public float temperature;
    public float lifetime;
    public int charge;
    public bool alive;

    public void Reset()
    {
        position = Vector3.zero;
        velocity = Vector3.zero;
        temperature = 300f;
        lifetime = 0f;
        charge = 0;
        alive = false;
    }
}
