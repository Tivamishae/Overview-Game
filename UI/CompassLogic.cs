using UnityEngine;

public class CompassLogic : MonoBehaviour
{
    public GameObject compassContainer;
    void Update()
    {
        compassContainer.SetActive(true);
    }
}