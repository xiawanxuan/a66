using UnityEngine;
using System.Collections.Generic;

public class ElectrodeDragInteraction : MonoBehaviour
{
    public LayerMask electrodeLayer;
    public Camera mainCamera;
    public float dragSmoothing = 10f;
    public float planeY = 0f;

    private ElectrodeGeometry draggedElectrode;
    private Vector3 dragOffset;
    private bool isDragging;

    public System.Action OnElectrodeMoved;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        HandleDrag();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, electrodeLayer))
            {
                ElectrodeGeometry electrode = hit.collider.GetComponent<ElectrodeGeometry>();
                if (electrode != null)
                {
                    draggedElectrode = electrode;
                    dragOffset = electrode.transform.position - hit.point;
                    isDragging = true;
                }
            }
        }

        if (isDragging && draggedElectrode != null)
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                Plane dragPlane = new Plane(Vector3.up, new Vector3(0f, planeY, 0f));

                if (dragPlane.Raycast(ray, out float distance))
                {
                    Vector3 targetPos = ray.GetPoint(distance) + dragOffset;
                    targetPos.y = planeY;
                    draggedElectrode.transform.position = Vector3.Lerp(
                        draggedElectrode.transform.position,
                        targetPos,
                        Time.deltaTime * dragSmoothing
                    );
                    draggedElectrode.SyncPosition();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                draggedElectrode.SyncPosition();
                OnElectrodeMoved?.Invoke();
                draggedElectrode = null;
            }
        }
    }

    public List<ElectrodeData> GetElectrodePositions(ElectrodeGeometry[] electrodes)
    {
        List<ElectrodeData> positions = new List<ElectrodeData>();
        foreach (var e in electrodes)
        {
            e.SyncPosition();
            positions.Add(e.Data);
        }
        return positions;
    }
}
