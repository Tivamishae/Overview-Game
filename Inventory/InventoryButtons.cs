using UnityEngine;
using UnityEngine.UI;

public class InventoryButtons : MonoBehaviour
{
    [Header("UI Buttons (Assign in Inspector)")]
    public Button[] buttons = new Button[6];

    [Header("Target GameObjects (Assign in Inspector)")]
    public GameObject[] targets = new GameObject[6];

    public GameObject CraftButton;

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Capture the current index for the closure
            buttons[i].onClick.AddListener(() => ActivateOnly(index));
        }

        // Optionally activate the first target at start
        ActivateOnly(0);
    }

    private void ActivateOnly(int activeIndex)
    {
        WorkableInventoryManager.Instance.DeactiveAllRecipes();
        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
                targets[i].SetActive(i == activeIndex);
        }
        if (activeIndex < 4)
        {
            CraftButton.SetActive(true);
        } else {
            CraftButton.SetActive(false);
        }
    }
}
