using UnityEngine;
using UnityEngine.UI;

public class CraftableObjectButton : MonoBehaviour
{
    private GameObject activeImage;
    private GameObject inactiveImage;

    private static CraftableObjectButton currentlySelected;

    private void Awake()
    {
        // Automatically find child images by name
        activeImage = transform.Find("Active")?.gameObject;
        inactiveImage = transform.Find("Inactive")?.gameObject;

        if (activeImage == null || inactiveImage == null)
        {
            Debug.LogWarning($"CraftableObjectButton on {gameObject.name} is missing child images named 'ActiveImage' and/or 'InactiveImage'.");
        }
    }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        SetActive(false); // Start inactive
    }

    private void OnButtonClicked()
    {
        if (currentlySelected != null && currentlySelected != this)
        {
            currentlySelected.SetActive(false);
        }

        SetActive(true);
        currentlySelected = this;
    }

    public void SetActive(bool isActive)
    {
        if (activeImage != null) activeImage.SetActive(isActive);
        if (inactiveImage != null) inactiveImage.SetActive(!isActive);
    }
}
