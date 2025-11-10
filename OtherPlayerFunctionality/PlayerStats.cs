using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public enum DebuffStat
{
    AttackDamage,
    MaxHealth,
    MaxStamina,
    MaxHunger,
    Toughness,
    Speed,
    Strength,
    Luck
}

public class StatDebuff
{
    public float percentage;   // e.g. 0.40f for -40%
    public float remaining;    // seconds left
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    public string PlayerName;
    public float Health;
    public float MaxHealth;
    public float Stamina;
    public float MaxStamina;
    public float Speed;
    public float Luck;
    public float XP;
    public int Level;
    public int Money;
    public float Strength;
    public float Toughness;
    public float Armor;
    public float AttackDamage;
    public float Hunger;
    public float MaxHunger;

    public float baseAttackDamage;
    public float baseHealth;
    public float baseStamina;
    public float baseHunger;
    public float baseToughness;
    public float baseSpeed;
    public float baseStrength;
    public float baseLuck;

    [Header("PlayerState")]
    public bool isInCombat;
    public Collider detectionCollider;

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI StaminaText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI LuckText;
    public TextMeshProUGUI LevelText;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI StrengthText;
    public TextMeshProUGUI TougnessText;
    public TextMeshProUGUI ArmorText;
    public TextMeshProUGUI AttackDamageText;
    public TextMeshProUGUI HungerText;

    public TextMeshProUGUI DamageUIText;
    public TextMeshProUGUI ArmorUIText;
    public TextMeshProUGUI HealthbarNumber;
    public TextMeshProUGUI StaminbarNumber;
    public TextMeshProUGUI HungerbarNumber;

    void Awake()
    {
        // If an instance already exists and it's not this one, destroy this
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;

        }
    }

    void Start()
    {
        NameText.text = "Name: " + PlayerName;
        HealthText.text = "Health: " + Health.ToString() + "/" + MaxHealth.ToString();
        StaminaText.text = "Stamina: " + Stamina.ToString() + "/" + MaxStamina.ToString();
        HungerText.text = "Hunger: " + Hunger.ToString() + "/" + MaxHunger.ToString();

        SpeedText.text = "Speed: " + Speed.ToString();
        LuckText.text = "Luck: " + Luck.ToString();
        LevelText.text = "Level: " + Level;
        MoneyText.text = "Money: " + Money;
        StrengthText.text = "Strength: " + Strength.ToString();
        TougnessText.text = "Toughness: " + Toughness.ToString();
        ArmorText.text = "Armor: " + Armor.ToString();
        ArmorUIText.text = Armor.ToString();
        AttackDamageText.text = "AttackDamage: " + AttackDamage.ToString();
        DamageUIText.text = AttackDamage.ToString();

        // Initialize stat values
        MaxHealth = baseHealth;
        MaxStamina = baseStamina;
        MaxHunger = baseHunger;
        AttackDamage = baseAttackDamage;
        Toughness = baseToughness;
        Speed = baseSpeed;
        Strength = baseStrength;
        Luck = baseLuck;

        Health = baseHealth;
        Stamina = baseStamina;
        Hunger = baseHunger;
    }


    // Update is called once per frame
    void Update()
    {

        CheckEquipmentSlots();

        DebuffTickAndApply();

        ClampResources();
        UpdateUI();

        if (Input.GetKeyDown(KeyCode.U))
        {
            Health -= 10f;
        }
        CheckIfInCombat();
    }

    #region Debuff System
    // -------------------------
    // Timed Debuff System
    // -------------------------

    private readonly Dictionary<DebuffStat, StatDebuff> activeDebuffs = new();
    private readonly Dictionary<DebuffStat, StatDebuff> queuedDebuffs = new();

    /// <summary>
    /// Apply a temporary percentage reduction to a stat.
    /// Example: ApplyTemporaryReduction(DebuffStat.Speed, 0.40f, 5f) for -40% Speed for 5 seconds.
    /// Rules:
    /// - Same % while active: refresh duration
    /// - Stronger %: overrides immediately (weaker is discarded)
    /// - Weaker %: queued; its time ticks down while waiting; starts if/when active ends and it still has time left
    /// </summary>
    public void ApplyTemporaryReduction(DebuffStat stat, float percent, float durationSeconds)
    {
        percent = Mathf.Clamp01(percent); // 0..1

        // No-op if zero or duration <= 0
        if (percent <= 0f || durationSeconds <= 0f) return;

        // If nothing active, make this active
        if (!activeDebuffs.TryGetValue(stat, out var active))
        {
            activeDebuffs[stat] = new StatDebuff { percentage = percent, remaining = durationSeconds };
            // Clear any queued weaker; new active supersedes
            if (queuedDebuffs.ContainsKey(stat)) queuedDebuffs.Remove(stat);
            return;
        }

        // Same strength -> refresh active time
        if (Mathf.Approximately(active.percentage, percent))
        {
            active.remaining = durationSeconds;
            return;
        }

        // Stronger than active -> override now (discard old active and any queued)
        if (percent > active.percentage)
        {
            activeDebuffs[stat] = new StatDebuff { percentage = percent, remaining = durationSeconds };
            if (queuedDebuffs.ContainsKey(stat)) queuedDebuffs.Remove(stat);
            return;
        }

        // Weaker than active -> queue it (time counts down while waiting)
        if (queuedDebuffs.TryGetValue(stat, out var queued))
        {
            // Keep the stronger of queued debuffs
            if (percent > queued.percentage)
            {
                queuedDebuffs[stat] = new StatDebuff { percentage = percent, remaining = durationSeconds };
            }
            else if (Mathf.Approximately(percent, queued.percentage))
            {
                // Keep whichever has MORE remaining time (so we don't accidentally shorten it)
                queued.remaining = Mathf.Max(queued.remaining, durationSeconds);
            }
            // If new is weaker than queued, ignore it.
        }
        else
        {
            queuedDebuffs[stat] = new StatDebuff { percentage = percent, remaining = durationSeconds };
        }
    }

    /// <summary>
    /// Call this ONCE PER FRAME, AFTER buffs are calculated (i.e., after CheckEquipmentSlots()).
    /// This ticks timers and applies current debuffs to your working fields (Max* and others).
    /// </summary>
    private void DebuffTickAndApply()
    {
        float dt = Time.deltaTime;

        // Tick active and queued timers
        // (queued timers tick down while waiting; if they expire before activation, they are dropped)
        var toClearActive = new List<DebuffStat>();
        foreach (var kvp in activeDebuffs)
        {
            kvp.Value.remaining -= dt;
            if (kvp.Value.remaining <= 0f)
                toClearActive.Add(kvp.Key);
        }
        foreach (var stat in toClearActive)
            activeDebuffs.Remove(stat);

        var toClearQueued = new List<DebuffStat>();
        foreach (var kvp in queuedDebuffs)
        {
            kvp.Value.remaining -= dt;
            if (kvp.Value.remaining <= 0f)
                toClearQueued.Add(kvp.Key);
        }
        foreach (var stat in toClearQueued)
            queuedDebuffs.Remove(stat);

        // Promote queued -> active where needed (pick the queued if active missing)
        foreach (DebuffStat stat in System.Enum.GetValues(typeof(DebuffStat)))
        {
            if (!activeDebuffs.ContainsKey(stat) && queuedDebuffs.TryGetValue(stat, out var q))
            {
                // Only promote if it still has time left
                if (q.remaining > 0f)
                {
                    activeDebuffs[stat] = new StatDebuff { percentage = q.percentage, remaining = q.remaining };
                }
                queuedDebuffs.Remove(stat);
            }
        }

        // Now apply active debuffs to current (already-buffed) values.
        // IMPORTANT: This runs AFTER CheckEquipmentSlots(), so fields include buffs already.
        ApplyActiveDebuffsToFields();
    }

    /// <summary>
    /// Applies the currently active debuffs to the working fields.
    /// For MaxHealth/MaxStamina/MaxHunger: we scale Max* and preserve current percentage.
    /// For others: we multiply the current (already-buffed) working value.
    /// </summary>
    private void ApplyActiveDebuffsToFields()
    {
        // Helper to get multiplier for a stat: (1 - percent) if debuffed, else 1
        float Mul(DebuffStat stat)
        {
            return activeDebuffs.TryGetValue(stat, out var d) ? (1f - Mathf.Clamp01(d.percentage)) : 1f;
        }

        // ------- MaxHealth (and Health proportion) -------
        {
            float maxBefore = MaxHealth;
            float m = Mul(DebuffStat.MaxHealth);
            float maxAfter = maxBefore * m;

            if (maxBefore > 0f)
            {
                float ratio = Health / maxBefore;
                MaxHealth = maxAfter;
                Health = Mathf.Clamp(maxAfter * ratio, 0f, MaxHealth);
            }
            else
            {
                MaxHealth = maxAfter;
            }
        }

        // ------- MaxStamina (and Stamina proportion) -------
        {
            float maxBefore = MaxStamina;
            float m = Mul(DebuffStat.MaxStamina);
            float maxAfter = maxBefore * m;

            if (maxBefore > 0f)
            {
                float ratio = Stamina / maxBefore;
                MaxStamina = maxAfter;
                Stamina = Mathf.Clamp(maxAfter * ratio, 0f, MaxStamina);
            }
            else
            {
                MaxStamina = maxAfter;
            }
        }

        // ------- MaxHunger (and Hunger proportion) -------
        {
            float maxBefore = MaxHunger;
            float m = Mul(DebuffStat.MaxHunger);
            float maxAfter = maxBefore * m;

            if (maxBefore > 0f)
            {
                float ratio = Hunger / maxBefore;
                MaxHunger = maxAfter;
                Hunger = Mathf.Clamp(maxAfter * ratio, 0f, MaxHunger);
            }
            else
            {
                MaxHunger = maxAfter;
            }
        }

        // ------- Other stats (already include buffs; multiply by debuff multiplier) -------
        AttackDamage *= Mul(DebuffStat.AttackDamage);
        Toughness *= Mul(DebuffStat.Toughness);
        Speed *= Mul(DebuffStat.Speed);
        Strength *= Mul(DebuffStat.Strength);
        Luck *= Mul(DebuffStat.Luck);
    }

    /// <summary>
    /// Returns the buff multiplier currently applied by your totem system for a given stat name,
    /// based on how you set them in CheckEquipmentSlots(). If you don't need per-stat buff
    /// multipliers here, you can simply return 1f and let CheckEquipmentSlots() set the
    /// buffed working values before this debuff pass.
    ///
    /// If you want tighter integration, you can store your last totem multipliers on fields
    /// when you compute them in CheckEquipmentSlots(), and read them here instead.
    /// </summary>


    #endregion


    void ClampResources()
    {
        Health = Mathf.Min(Health, MaxHealth);
        Stamina = Mathf.Min(Stamina, MaxStamina);
        Hunger = Mathf.Min(Hunger, MaxHunger);
    }

    void CheckIfInCombat()
    {
        isInCombat = false;

        // Get all colliders inside this trigger
        Collider[] hits = Physics.OverlapBox(
            detectionCollider.bounds.center,
            detectionCollider.bounds.extents,
            detectionCollider.transform.rotation
        );

        foreach (var col in hits)
        {
            if (col == null) continue;

            // Check InteractableNPC
            var npc = col.GetComponent<NPC>();
            if (npc != null && npc.angry)
            {
                isInCombat = true;
                return;
            }
        }
    }


    void UpdateUI()
    {
        NameText.text = "Name: " + PlayerName;
        HealthText.text = "Health: " + Mathf.RoundToInt(Health) + "/" + Mathf.RoundToInt(MaxHealth);
        StaminaText.text = "Stamina: " + Mathf.RoundToInt(Stamina) + "/" + Mathf.RoundToInt(MaxStamina);
        HungerText.text = "Hunger: " + Mathf.RoundToInt(Hunger) + "/" + Mathf.RoundToInt(MaxHunger);

        SpeedText.text = "Speed: " + Speed.ToString();
        LuckText.text = "Luck: " + Luck.ToString();
        LevelText.text = "Level: " + Level;
        MoneyText.text = "Money: " + Money;
        StrengthText.text = "Strength: " + Strength.ToString();
        TougnessText.text = "Toughness: " + Toughness.ToString();
        AttackDamageText.text = "AttackDamage: " + AttackDamage.ToString();

        DamageUIText.text = AttackDamage.ToString();
        HealthbarNumber.text = Mathf.RoundToInt(Health) + "/" + Mathf.RoundToInt(MaxHealth);
        StaminbarNumber.text = Mathf.RoundToInt(Stamina) + "/" + Mathf.RoundToInt(MaxStamina);
        HungerbarNumber.text = Mathf.RoundToInt(Hunger) + "/" + Mathf.RoundToInt(MaxHunger);
    }


    void CheckEquipmentSlots()
    {
        bool hasSpiritAnimal = Inventory.Instance.SpiritAnimalSlot.transform.childCount > 0;
        bool hasTotem = Inventory.Instance.TotemSlot.transform.childCount > 0;
        bool hasArmor = Inventory.Instance.ArmorSlot.transform.childCount > 0;

        if (hasArmor)
        {
            Transform armorItem = Inventory.Instance.ArmorSlot.transform.GetChild(0);
            Armor armor = armorItem.GetComponent<Armor>();

            ArmorText.text = "Armor: " + armor.ArmorValue;
            ArmorUIText.text = armor.ArmorValue.ToString();
        }
        else
        {
            ArmorText.text = "Armor: 0";
            ArmorUIText.text = "0";
        }

        if (hasTotem)
        {
            Transform totemItem = Inventory.Instance.TotemSlot.transform.GetChild(0);
            Totem totem = totemItem.GetComponent<Totem>();

            if (totem != null)
            {
                // Preserve current percentage
                float healthPercent = Health / MaxHealth;
                float staminaPercent = Stamina / MaxStamina;
                float hungerPercent = Hunger / MaxHunger;

                // Apply stat multipliers
                AttackDamage = UpdateAttackDamage() * totem.DamageBoost;
                Toughness = baseToughness * totem.TougnessBoost;
                Speed = baseSpeed * totem.SpeedBoost;
                Strength = baseStrength * totem.StrengthBoost;
                Luck = baseLuck * totem.LuckBoost;

                // Apply new max values
                MaxHealth = baseHealth * totem.HealthBoost;
                MaxStamina = baseStamina * totem.StaminaBoost;
                MaxHunger = baseHunger * totem.HungerBoost;

                // Recalculate current values based on old percentage
                Health = MaxHealth * healthPercent;
                Stamina = MaxStamina * staminaPercent;
                Hunger = MaxHunger * hungerPercent;
            }
        }
        else
        {
            // Preserve current percentage
            float healthPercent = Health / MaxHealth;
            float staminaPercent = Stamina / MaxStamina;
            float hungerPercent = Hunger / MaxHunger;

            // Reset stats
            AttackDamage = UpdateAttackDamage();
            Toughness = baseToughness;
            Speed = baseSpeed;
            Strength = baseStrength;
            Luck = baseLuck;

            // Reset max values
            MaxHealth = baseHealth;
            MaxStamina = baseStamina;
            MaxHunger = baseHunger;

            // Recalculate current values based on percentage
            Health = MaxHealth * healthPercent;
            Stamina = MaxStamina * staminaPercent;
            Hunger = MaxHunger * hungerPercent;
        }
    }

    public void TakeDamage(float damage)
    {
        float armorMultiplier = 1f - (Armor / (Armor + 100f));
        float afterArmor = damage * armorMultiplier;

        float toughnessMultiplier = 1f - (Toughness / (Toughness + 100f));
        float finalDamage = afterArmor * toughnessMultiplier;

        Health -= finalDamage;
    }

    public float UpdateAttackDamage()
    {
        if (HotbarLogic.Instance.currentEquipped != null)
        {
            AttackDamage = baseAttackDamage + HotbarLogic.Instance.currentEquipped.GetComponent<EquipmentItem>().Damage;
        }
        else
        {
            AttackDamage = baseAttackDamage;
        }
        return AttackDamage;
    }

}
