using UnityEngine;
using UnityEngine.Rendering;

public class TimeCycle : MonoBehaviour
{
    public static TimeCycle Instance { get; private set; }

    [Header("Time Settings")]
    [Range(0, 23)] public int hours = 9;
    [Range(0, 59)] public int minutes;
    [Range(0, 59)] public int seconds;
    public float timeMultiplier = 60f;

    [Header("Shader Settings")]
    public Material targetMaterial;
    public float transitionDuration = 5f;

    [Header("Fog Settings")]
    public Color dayFogColor = new(0.8f, 0.9f, 1.0f);
    public Color nightFogColor = new(0.05f, 0.07f, 0.15f);
    [Range(0f, 1f)] public float dayFogDensity = 0.002f;
    [Range(0f, 1f)] public float nightFogDensity = 0.008f;

    [Header("Lighting Settings")]
    public Color dayAmbientColor = new(1f, 0.95f, 0.9f);
    public Color nightAmbientColor = new(0.05f, 0.1f, 0.25f);
    public Light mainDirectionalLight;
    public Color dayLightColor = new(1f, 0.95f, 0.8f);
    public Color nightLightColor = new(0.2f, 0.3f, 0.6f);
    public float dayLightIntensity = 1.2f;
    public float nightLightIntensity = 0.05f;

    [Header("Sun Movement Settings")]
    [Tooltip("Rotation at 00:00 (midnight)")]
    public float midnightRotationX = -90f;
    [Tooltip("Rotation at 12:00 (noon)")]
    public float noonRotationX = 90f;
    public float sunRotationY = 0f;

    [Header("Player & Audio Settings")]
    public Transform player; // Player position to attach audio to
    [Tooltip("Path inside Resources/Sounds/Environment/DayAndNight/")]
    public string daybreakPath = "Sounds/Environment/DayAndNight/Daybreak";
    public string nightfallPath = "Sounds/Environment/DayAndNight/Night";

    private float timeAccumulator;
    private bool isTransitioning = false;
    private float transitionTimer = 0f;
    private float transitionStart = 0.5f;
    private float transitionTarget = 0.5f;
    public float currentTransition = 0.5f;
    public bool isDay = true;
    private float previousTime = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (targetMaterial != null)
            targetMaterial.SetFloat("_Transition", currentTransition);

        previousTime = hours * 3600f + minutes * 60f + seconds;
        ApplyEnvironmentSettings(currentTransition);
    }

    private void Update()
    {
        AdvanceTime();
        float currentTime = hours * 3600f + minutes * 60f + seconds;

        bool crossedMorning = previousTime < 8 * 3600f && currentTime >= 8 * 3600f;
        bool crossedEvening = previousTime < 20 * 3600f && currentTime >= 20 * 3600f;

        if (!isTransitioning)
        {
            if (crossedMorning && !isDay)
            {
                BeginTransition(0f, 0.5f);
                isDay = true;
                PlayDaybreakSound();
                TimeTransitionUI.Instance?.ShowMessage("Daybreak");
            }

            if (crossedEvening && isDay)
            {
                BeginTransition(0.5f, 0f);
                isDay = false;
                PlayNightSound();
                TimeTransitionUI.Instance?.ShowMessage("Nightfall");
            }
        }

        previousTime = currentTime;

        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            currentTransition = Mathf.Lerp(transitionStart, transitionTarget, t);

            if (t >= 1f)
                isTransitioning = false;
        }

        ApplyEnvironmentSettings(currentTransition);
        RotateSun(currentTime);

        if (targetMaterial != null)
            targetMaterial.SetFloat("_Transition", currentTransition);
    }

    private void BeginTransition(float from, float to)
    {
        isTransitioning = true;
        transitionTimer = 0f;
        transitionStart = from;
        transitionTarget = to;
    }

    private void ApplyEnvironmentSettings(float transitionValue)
    {
        float normalized = transitionValue * 2f;

        Color fogColor = Color.Lerp(nightFogColor, dayFogColor, normalized);
        float fogDensity = Mathf.Lerp(nightFogDensity, dayFogDensity, normalized);
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, normalized);

        if (mainDirectionalLight != null)
        {
            mainDirectionalLight.color = Color.Lerp(nightLightColor, dayLightColor, normalized);
            mainDirectionalLight.intensity = Mathf.Lerp(nightLightIntensity, dayLightIntensity, normalized);
        }
    }

    private void RotateSun(float currentTime)
    {
        if (mainDirectionalLight == null) return;

        // 0–1 fraction of the day (24 hours)
        float dayProgress = currentTime / 86400f;

        // Sun moves in a gentle arc from 30 (sunrise) to 150 (sunset)
        // Using a sine curve so it's smooth and slower near dawn/dusk
        float angleRangeMin = 30f;
        float angleRangeMax = 150f;

        // Smooth sine-based movement (0–1 mapped to 0–)
        float normalizedSin = Mathf.Sin(dayProgress * Mathf.PI);

        float sunAngleX = Mathf.Lerp(angleRangeMin, angleRangeMax, normalizedSin);
        mainDirectionalLight.transform.rotation = Quaternion.Euler(sunAngleX, sunRotationY, 0f);
    }


    private void AdvanceTime()
    {
        timeAccumulator += Time.deltaTime * timeMultiplier;
        while (timeAccumulator >= 1f)
        {
            timeAccumulator -= 1f;
            seconds++;
            if (seconds >= 60) { seconds = 0; minutes++; }
            if (minutes >= 60) { minutes = 0; hours++; }
            if (hours >= 24) { hours = 0; }
        }
    }

    //  Plays the daybreak audio
    private void PlayDaybreakSound()
    {
        if (player == null) return;
        var clip = AudioPreloader.Instance?.GetClip(daybreakPath);
        if (clip != null)
            AudioSystem.Instance.PlayClipFollow(clip, player, 1f);
    }

    //  Plays the nightfall audio
    private void PlayNightSound()
    {
        if (player == null) return;
        var clip = AudioPreloader.Instance?.GetClip(nightfallPath);
        if (clip != null)
            AudioSystem.Instance.PlayClipFollow(clip, player, 1f);
    }
}
