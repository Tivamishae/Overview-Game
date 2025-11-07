using UnityEngine;
using System.Collections;

public class GameAmbience : MonoBehaviour
{
    public static GameAmbience Instance { get; private set; }

    [Header("Ambience Settings")]
    public string ambiencePath = "Sounds/Environment/Ambience/ForestAmbience";
    public float ambienceVolume = 0.3f;

    [Header("Music Settings")]
    public string musicPath = "Sounds/Environment/Music/ExplorationMusic";
    public string combatMusicPath = "Sounds/Environment/Combat/CombatMusic";
    public float musicVolume = 0.3f;

    [Header("State Flags")]
    public bool isInCombat = false;
    public bool isInBossFight = false;

    private AudioSource ambienceSource;
    private AudioSource musicSource;
    private Coroutine musicRoutine;
    private Coroutine fadeRoutine;

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

    void Update()
    {
        if (BossSystem.Instance.currentBoss != null)
        {
            SetBossFightState(true);
        }
        else if (PlayerStats.Instance.isInCombat)
        {
            SetCombatState(true);
        }
        else
        {
            SetCombatState(false);
            SetBossFightState(false);
        }
    }

    private void Start()
    {
        PlayAmbience();
        StartMusicLoopRoutine();
    }

    // ------------------ AMBIENCE ------------------ //
    public void PlayAmbience()
    {
        if (ambienceSource != null && ambienceSource.isPlaying) return;

        AudioClip clip = AudioPreloader.Instance.GetClip(ambiencePath);
       
        if (clip == null)
        {
            Debug.LogError($"GameAmbience: Could not find ambience at Resources/{ambiencePath}");
            return;
        }

        Transform followTarget = GetPlayerOrCamera();
        if (followTarget == null) return;

        ambienceSource = AudioSystem.Instance.PlayAudioLoop(clip, followTarget, ambienceVolume);
    }

    // ------------------ MUSIC ------------------ //
    private void StartMusicLoopRoutine()
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(MusicLoopRoutine());
    }

    private IEnumerator MusicLoopRoutine()
    {
        while (true)
        {
            // Wait until no music is playing
            while (musicSource != null && musicSource.isPlaying)
                yield return null;

            // Wait random cooldown AFTER music has finished
            float waitTime = Random.Range(1f, 2f); // 3–6 minutes
            yield return new WaitForSeconds(waitTime);

            // If boss fight or combat -> skip this round
            if (isInBossFight || isInCombat)
                continue;

            PlayExplorationMusic();
        }
    }


    private void PlayExplorationMusic()
    {
        PlayMusic(musicPath, musicVolume, loop: true);
    }

    private void PlayCombatMusic()
    {
        PlayMusic(combatMusicPath, musicVolume, loop: true);
    }

    private void PlayBossMusic()
    {
        PlayMusic(BossSystem.Instance.currentBoss.bossTheme, musicVolume, loop: true);
    }

    private void PlayMusic(string path, float volume, bool loop)
    {
        AudioClip clip = AudioPreloader.Instance.GetClip(path);
        if (clip == null)
        {
            Debug.LogError($"GameAmbience: Could not find music at Resources/{path}");
            return;
        }

        Transform followTarget = GetPlayerOrCamera();
        if (followTarget == null) return;

        // Stop current music
        if (musicSource != null) StopMusicImmediate();

        musicSource = AudioSystem.Instance.PlayAudioLoop(clip, followTarget, volume);
        musicSource.loop = loop;
    }

    private void StopMusicImmediate()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource = null;
        }
    }

    private IEnumerator FadeOutMusic(float fadeTime = 0.5f)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            if (musicSource != null)
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        StopMusicImmediate();
    }

    // ------------------ STATE CHANGES ------------------ //
    public void SetCombatState(bool active)
    {
        if (isInCombat == active) return;
        isInCombat = active;

        if (active)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutMusic(0.5f));
            StartCoroutine(WaitAndPlayCombatMusic(0.5f));
        }
        else
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutMusic(0.5f));
            // Resume cooldown
            StartMusicLoopRoutine();
        }
    }

    private IEnumerator WaitAndPlayCombatMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isInCombat && !isInBossFight)
            PlayCombatMusic();
    }

    public void SetBossFightState(bool active)
    {
        if (isInBossFight == active) return;
        isInBossFight = active;

        if (active)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutMusic(0.5f));
            StartCoroutine(WaitAndPlayBossMusic(0.5f));
        }
        else
        {
            // Resume cooldown when boss fight ends
            StartMusicLoopRoutine();
        }
    }

    private IEnumerator WaitAndPlayBossMusic(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isInBossFight)
            PlayBossMusic();
    }

    // ------------------ HELPERS ------------------ //
    private Transform GetPlayerOrCamera()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : Camera.main?.transform;
    }
}
