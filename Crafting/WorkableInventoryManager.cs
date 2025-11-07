using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WorkableInventoryManager : MonoBehaviour
{
    public static WorkableInventoryManager Instance { get; private set; }

    public Button craftButton;

    [HideInInspector] public CraftingRecipe selectedRecipe;
    [HideInInspector] public List<CraftingButtonUI> allCraftingButtons = new List<CraftingButtonUI>();

    [Header("Missing Materials Popup")]
    public GameObject missingMaterialsPopup;
    public CanvasGroup popupCanvasGroup; // CanvasGroup must be attached to the popup GameObject

    private Coroutine popupRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (craftButton != null)
            craftButton.onClick.AddListener(OnCraftPressed);

        if (missingMaterialsPopup != null)
        {
            popupCanvasGroup = missingMaterialsPopup.GetComponent<CanvasGroup>();
            if (popupCanvasGroup == null)
            {
                popupCanvasGroup = missingMaterialsPopup.AddComponent<CanvasGroup>();
            }

            popupCanvasGroup.alpha = 0f;
            missingMaterialsPopup.SetActive(false);
        }
    }

    public void SelectRecipe(CraftingRecipe recipe, CraftingButtonUI selectedButton)
    {
        selectedRecipe = recipe;
        foreach (var button in allCraftingButtons)
        {
            button.SetHighlight(button == selectedButton);
        }
    }

    public void DeactiveAllRecipes()
    {
        foreach (var button in allCraftingButtons)
        {
            button.SetHighlight(false);
            selectedRecipe = null;
        }
    }

    private void OnCraftPressed()
    {
        if (selectedRecipe == null || Inventory.Instance == null) return;

        if (CanCraft(selectedRecipe))
        {
            foreach (var ingredient in selectedRecipe.ingredients)
                Inventory.Instance.RemoveItem(ingredient.itemID, ingredient.amount);

            Inventory.Instance.AddItem(selectedRecipe.resultItemID, selectedRecipe.resultAmount);
            Debug.Log("Crafted: " + selectedRecipe.recipeName);
        }
        else
        {
            Debug.Log("Missing materials!");
            ShowMissingMaterialsPopup();
        }
    }

    private bool CanCraft(CraftingRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!Inventory.Instance.HasItem(ingredient.itemID, ingredient.amount))
                return false;
        }
        return true;
    }

    private void ShowMissingMaterialsPopup()
    {
        if (popupRoutine != null)
        {
            StopCoroutine(popupRoutine);
        }
        popupRoutine = StartCoroutine(HandlePopup());
    }

    private IEnumerator HandlePopup()
    {
        // Show popup
        if (missingMaterialsPopup != null)
        {
            missingMaterialsPopup.SetActive(true);
            popupCanvasGroup.alpha = 1f;
        }

        // Wait for 3 seconds fully visible
        yield return new WaitForSeconds(3f);

        // Fade over 1 second
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            popupCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        // Fully hidden
        popupCanvasGroup.alpha = 0f;
        missingMaterialsPopup.SetActive(false);
        popupRoutine = null;
    }
}
