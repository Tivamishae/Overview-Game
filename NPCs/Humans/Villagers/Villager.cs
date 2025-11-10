using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Villager : NPC
{
    [Header("Clothes")]
    public bool hasSetGender = false;
    public bool isMale = true;
    public bool needClothes = true;
    public List<GameObject> UpperBodyClothes = new();
    public List<GameObject> LowerBodyClothes = new();
    public List<GameObject> Hats = new();
    public List<GameObject> Hairs = new();
    public GameObject UpperAndLowerContainer;
    public GameObject HatContainer;
    public GameObject HairContainer;

    [Header("Interaction")]
    public bool isBeingInteractedWith = false;

    #region Clothing Logic
    void ClotheNPC()
    {
        // Upper
        if (UpperBodyClothes.Count > 0)
        {
            var prefab = UpperBodyClothes[Random.Range(0, UpperBodyClothes.Count)];
            AttachClothing(prefab, UpperAndLowerContainer.transform);
        }

        // Lower
        if (LowerBodyClothes.Count > 0)
        {
            var prefab = LowerBodyClothes[Random.Range(0, LowerBodyClothes.Count)];
            AttachClothing(prefab, UpperAndLowerContainer.transform);
        }

        // Hat
        if (Hats.Count > 0)
        {
            var prefab = Hats[Random.Range(0, Hats.Count)];
            AttachClothing(prefab, HatContainer.transform);
        }

        // Hair
        if (Hairs.Count > 0)
        {
            var prefab = Hairs[Random.Range(0, Hairs.Count)];
            AttachClothing(prefab, HairContainer.transform);
        }
    }

    private void AttachClothing(GameObject prefab, Transform container)
    {
        // Instantiate as a child of the container
        GameObject instance = Instantiate(prefab, container, false);

        // Do NOT reset localPosition, localRotation, or localScale
        // They will remain exactly as defined in the prefab
    }

    private void LoadClothes()
    {
        string genderFolder = isMale ? "Male" : "Female";

        UpperBodyClothes = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Upper");
        LowerBodyClothes = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Lower");
        Hats = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Hats");
        Hairs = LoadPrefabs($"3D/NPCAddOns/{genderFolder}/Hairs");
    }

    private List<GameObject> LoadPrefabs(string path)
    {
        List<GameObject> loaded = new();

        GameObject[] resources = Resources.LoadAll<GameObject>(path);
        foreach (var prefab in resources)
        {
            loaded.Add(prefab);
        }

        return loaded;
    }

    #endregion

    #region Interaction Logic

    public void Interact()
    {
        if (currentState == NPCState.Dead)
            return;

        var talkClip = Resources.Load<AudioClip>("Sounds/NPC/Talk");
        if (talkClip != null)
            AudioSystem.Instance.PlayClipFollow(talkClip, transform, 1f);

        GetComponent<HumanInteraction>()?.Interact();

        animator.SetTrigger("Talk");
    }

    public void SetInteractionState(bool state)
    {
        isBeingInteractedWith = state;
    }

    #endregion
}
