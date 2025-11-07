using UnityEngine;

public class MapSystem : MonoBehaviour
{
    public static MapSystem Instance { get; private set; }

    [Header("Assign the map camera GameObject here")]
    public GameObject mapCameraObject;
    public GameObject Map;

    public bool renderMap = false;
    public Camera cam;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicate instances
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keep across scenes
    }

    void Update()
    {
        // Skip the very first frame
        if (!renderMap)
        {
            cam.enabled = false;
        }

        else
        {
            cam.enabled = true;
        }
    }
}
