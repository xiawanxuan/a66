using UnityEngine;

public class AdaptiveCamera : MonoBehaviour
{
    public float domainSize = 1f;
    public float padding = 0.3f;
    public bool maintainAspectRatio = true;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustCamera();
    }

    void Update()
    {
        AdjustCamera();
    }

    void AdjustCamera()
    {
        if (cam == null) return;

        if (!maintainAspectRatio) return;

        float targetAspect = 1f;
        float currentAspect = (float)Screen.width / Screen.height;

        float orthoSize = (domainSize + padding) * 0.5f;

        if (currentAspect < targetAspect)
        {
            orthoSize /= currentAspect;
        }

        cam.orthographicSize = orthoSize;
        cam.transform.position = new Vector3(0f, domainSize * 2f, 0f);
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
