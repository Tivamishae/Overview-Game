using UnityEngine;
using TMPro;
using System.Collections;

public class BossSystem : MonoBehaviour
{
    public static BossSystem Instance { get; private set; }

    [Header("UI")]
    public GameObject bossDisplayArea;        // Assign in Inspector (panel/root)
    public TextMeshProUGUI bossNameText;      // TMP text for boss name
    public Resourcebar bossHealthBar;         // Assign HealthbarContainer (with Resourcebar)
    public TextMeshProUGUI bossHealthNumber;

    public GeneralBossScript currentBoss;

    [Header("Audio")]
    [Tooltip("Resource path to the boss start sound, without the 'Resources/' prefix or extension.")]
    public string bossStartSfxPath = "Sounds/Bosses/Boss1/BossStartSound";
    [Range(0f, 1f)] public float bossStartSfxVolume = 0.2f;
    private AudioClip bossStartSfx;
    private AudioSource bossThemeSource;  // AudioSource to play the boss theme

    private Vector3 playerReturnPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (bossDisplayArea != null) bossDisplayArea.SetActive(false);

        // Preload boss start sound
        bossStartSfx = Resources.Load<AudioClip>(bossStartSfxPath);
        if (bossStartSfx == null)
            Debug.LogWarning($"BossSystem: Could not find boss start sound at Resources/{bossStartSfxPath}");
    }

    void Update()
    {
        if (IsBossActive())
        {
            bossHealthNumber.text = currentBoss.health + "/" + currentBoss.MaxHealth;
        }
    }

    public void RegisterBoss(GeneralBossScript boss)
    {
        // Clean up prior (just in case)
        if (currentBoss != null) currentBoss.OnHealthChanged -= OnBossHealthChanged;

        currentBoss = boss;

        if (bossDisplayArea != null) bossDisplayArea.SetActive(true);
        if (bossNameText != null && currentBoss != null) bossNameText.text = currentBoss.name;

        // Initialize the health bar once
        if (bossHealthBar != null && currentBoss != null)
        {
            bossHealthBar.SetSliderRange(0f, currentBoss.MaxHealth);
            bossHealthBar.SetMaxResource(currentBoss.MaxHealth);
            bossHealthBar.SetResource(currentBoss.health);
        }

        // Subscribe for live updates
        if (currentBoss != null)
            currentBoss.OnHealthChanged += OnBossHealthChanged;

        //  Play start sound
        if (bossStartSfx != null && AudioSystem.Instance != null)
            AudioSystem.Instance.PlayClipAtPoint(bossStartSfx, boss.transform.position, bossStartSfxVolume);

        // Play boss theme on loop

        Debug.Log("Boss registered in BossSystem.");
    }

    public void UnregisterBoss()
    {
        // Stop playing the boss theme with fade-out effect
        if (bossThemeSource != null && bossThemeSource.isPlaying)
        {
            StartCoroutine(FadeOutBossTheme(1f));  // 1 second fade-out duration
        }

        if (currentBoss != null)
        {
            currentBoss.OnHealthChanged -= OnBossHealthChanged;
            currentBoss = null;

            if (bossDisplayArea != null) bossDisplayArea.SetActive(false);
        }

        // --- NEW: Teleport player back when boss is gone ---
        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player != null)
        {
            player.position = playerReturnPoint;
            Debug.Log("Player returned to saved point: " + playerReturnPoint);
        }
    }

    public void GiveReturnPoint(Vector3 returnPoint)
    {
        playerReturnPoint = returnPoint;
        Debug.Log("Saved return point: " + playerReturnPoint);
    }

    public bool IsBossActive() => currentBoss != null;

    // Event handler from the boss
    private void OnBossHealthChanged(float current, float max)
    {
        if (bossHealthBar == null) return;

        // Ensure range matches (in case of buffs)
        if (!Mathf.Approximately(bossHealthBar.GetMaxResource(), max))
        {
            bossHealthBar.SetSliderRange(0f, max);
            bossHealthBar.SetMaxResource(max);
        }

        bossHealthBar.SetResource(current);
    }

    // Fade-out the boss theme music
    private IEnumerator FadeOutBossTheme(float duration)
    {
        float startVolume = bossThemeSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            bossThemeSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bossThemeSource.volume = 0f;
        bossThemeSource.Stop();
    }
}
