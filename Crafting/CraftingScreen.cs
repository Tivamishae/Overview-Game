using UnityEngine;
using System.Collections.Generic;

public class CraftingScreen : MonoBehaviour
{
    public string Name; // Folder name under Resources/CraftingRecipes
    public Transform buttonParent;

    void Start()
    {
        GameObject buttonPrefab = Resources.Load<GameObject>("2D/InventoryDisplays/CraftingButton");
        if (!buttonPrefab)
        {
            Debug.LogError("Missing prefab at Resources/2D/InventoryDisplays/CraftingButton");
            return;
        }

        // Load all recipes in Resources/CraftingRecipes/Name
        CraftingRecipe[] loadedRecipes = Resources.LoadAll<CraftingRecipe>($"CraftingRecipes/{Name}");

        foreach (CraftingRecipe recipe in loadedRecipes)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonParent);
            CraftingButtonUI buttonUI = buttonObj.GetComponent<CraftingButtonUI>();
            buttonUI.Setup(recipe);

            // Register to the global manager
            if (WorkableInventoryManager.Instance != null)
            {
                WorkableInventoryManager.Instance.allCraftingButtons.Add(buttonUI);
            }
        }
    }
}
