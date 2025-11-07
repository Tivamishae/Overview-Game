using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingButtonUI : MonoBehaviour
{
    private Button button;
    private CraftingRecipe myRecipe;
    private Transform requirementsContainer;
    private Image backgroundImage;

    private Color normalColor = Color.white;
    private Color selectedColor = new Color(0.7f, 0.7f, 0.7f); // Slightly darker

    void Awake()
    {
        button = GetComponent<Button>();
        backgroundImage = GetComponent<Image>();

        if (backgroundImage == null)
        {
            Debug.LogWarning("No Image component found on CraftingButtonUI.");
        }

        requirementsContainer = transform.Find("RequirementsContainer");

        if (requirementsContainer == null)
        {
            Debug.LogWarning("RequirementsContainer not found as a child of " + gameObject.name);
        }
    }

    public void Setup(CraftingRecipe recipe)
    {
        myRecipe = recipe;

        // Set button label
        TextMeshProUGUI label = GetComponentInChildren<TextMeshProUGUI>();
        if (label) label.text = recipe.recipeName + " x" + recipe.resultAmount;

        // Assign icon image
        Image icon = transform.Find("Icon")?.GetComponent<Image>();
        if (icon != null)
        {
            Sprite sprite = ItemDatabase.Instance.GetItemSpriteByID(recipe.resultItemID);
            if (sprite != null)
                icon.sprite = sprite;
            else
                Debug.LogWarning($"No sprite found for item ID {recipe.resultItemID}");
        }
        else
        {
            Debug.LogWarning("Icon Image component not found as child of CraftingButtonUI.");
        }

        button.onClick.AddListener(() => WorkableInventoryManager.Instance.SelectRecipe(recipe, this));
        SetHighlight(false);
        PopulateRequirements();
    }

    void PopulateRequirements()
    {
        if (requirementsContainer == null) return;

        foreach (Transform child in requirementsContainer)
            Destroy(child.gameObject);

        GameObject requirementPrefab = Resources.Load<GameObject>("2D/InventoryDisplays/Requirement");
        if (!requirementPrefab)
        {
            Debug.LogError("Missing Requirement prefab at Resources/2D/InventoryDisplays/Requirement");
            return;
        }

        foreach (var ingredient in myRecipe.ingredients)
        {
            GameObject reqGO = Instantiate(requirementPrefab, requirementsContainer);

            // Set amount text
            TextMeshProUGUI reqLabel = reqGO.GetComponentInChildren<TextMeshProUGUI>();
            if (reqLabel != null)
            {
                reqLabel.text = ingredient.amount.ToString() + "x";
            }

            // Set image icon
            Image reqImage = reqGO.transform.Find("Image")?.GetComponent<Image>();
            if (reqImage != null)
            {
                Sprite sprite = ItemDatabase.Instance.GetItemSpriteByID(ingredient.itemID);
                if (sprite != null)
                {
                    reqImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"No sprite found for itemID {ingredient.itemID}");
                }
            }
            else
            {
                Debug.LogWarning("Requirement prefab is missing an Image child.");
            }
        }
    }


    public void SetHighlight(bool active)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = active ? selectedColor : normalColor;
        }
    }
}
