using UnityEngine;

public class ResourcebarsSystem : MonoBehaviour
{
    // Singleton instance
    public static ResourcebarsSystem Instance { get; private set; }

    [Header("Assign Resource Bars")]
    public Resourcebar healthBar;
    public Resourcebar staminaBar;
    public Resourcebar hungerBar;

    [Header("Slider Min Settings")]
    public float healthMinSlider = 0f;
    public float staminaMinSlider = 0f;
    public float hungerMinSlider = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (PlayerStats.Instance == null) return;

        float playerMaxHealth = PlayerStats.Instance.MaxHealth;
        float playerMaxStamina = PlayerStats.Instance.MaxStamina;
        float playerMaxHunger = PlayerStats.Instance.MaxHunger;

        float currentHealth = PlayerStats.Instance.Health;
        float currentStamina = PlayerStats.Instance.Stamina;
        float currentHunger = PlayerStats.Instance.Hunger;

        // Set health bar
        healthBar.SetMaxResource(playerMaxHealth);
        healthBar.SetSliderRange(healthMinSlider, playerMaxHealth);
        healthBar.SetResource(currentHealth);

        // Set stamina bar
        staminaBar.SetMaxResource(playerMaxStamina);
        staminaBar.SetSliderRange(staminaMinSlider, playerMaxStamina);
        staminaBar.SetResource(currentStamina);

        // Set hunger bar
        hungerBar.SetMaxResource(playerMaxHunger);
        hungerBar.SetSliderRange(hungerMinSlider, playerMaxHunger);
        hungerBar.SetResource(currentHunger);
    }

    // Optional accessors
    public Resourcebar GetHealthBar() => healthBar;
    public Resourcebar GetStaminaBar() => staminaBar;
    public Resourcebar GetHungerBar() => hungerBar;
}
