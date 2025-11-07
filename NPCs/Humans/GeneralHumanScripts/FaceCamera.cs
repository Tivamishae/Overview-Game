using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
        }
    }
}