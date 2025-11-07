using UnityEngine;

public class FaceCameraAndScaleSize : MonoBehaviour
{
    [Header("Camera Search Settings")]
    public string waypointCameraName = "WayPointCamera"; // Name of your special camera

    [Header("Scaling Settings")]
    public float desiredSizeInWorldUnits = 1f; // How big it should appear at 1m distance

    private Camera targetCamera;

    private void Start()
    {
        // Try to find the special camera by name
        GameObject camObj = GameObject.Find(waypointCameraName);
        if (camObj != null)
        {
            targetCamera = camObj.GetComponent<Camera>();
        }

        if (targetCamera == null)
        {
            Debug.LogWarning($"FaceCameraAndScaleSize: No camera found named '{waypointCameraName}'.");
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // 1. Face the camera
        transform.rotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);

        // 2. Keep same size visually regardless of distance
        float distance = Vector3.Distance(transform.position, targetCamera.transform.position);
        transform.localScale = Vector3.one * distance * 0.01f * desiredSizeInWorldUnits;
    }
}
