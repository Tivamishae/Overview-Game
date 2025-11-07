using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CraftingIngredient
{
    public int itemID;
    public int amount;
}

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
    public int resultItemID;
    public int resultAmount = 1;
}