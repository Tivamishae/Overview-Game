using System.Linq;
using UnityEngine;

public class FirstPersonAudio : MonoBehaviour
{
    public FirstPersonMovement character;
    public GroundCheck groundCheck;

    [Header("Step")]
    public AudioSource stepAudio;
    public AudioSource runningAudio;
    [Tooltip("Minimum velocity for moving audio to play")]
    public float velocityThreshold = 0.01f;
    Vector2 lastCharacterPosition;
    Vector2 CurrentCharacterPosition => new Vector2(character.transform.position.x, character.transform.position.z);

    [Header("Landing")]
    public AudioSource landingAudio;
    public AudioClip[] landingSFX;

    [Header("Jump")]
    public AudioSource jumpAudio;
    public AudioClip[] jumpSFX;

    [Header("Crouch")]
    public AudioSource crouchStartAudio, crouchedAudio, crouchEndAudio;
    public AudioClip[] crouchStartSFX, crouchEndSFX;

    AudioSource[] MovingAudios => new AudioSource[] { stepAudio, runningAudio, crouchedAudio };

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>();
        groundCheck = (transform.parent ?? transform).GetComponentInChildren<GroundCheck>();

        stepAudio = GetOrCreateAudioSource("Step Audio");
        runningAudio = GetOrCreateAudioSource("Running Audio");
        crouchedAudio = GetOrCreateAudioSource("Crouched Audio");
        landingAudio = GetOrCreateAudioSource("Landing Audio");

        jumpAudio = GetOrCreateAudioSource("Jump Audio");
        crouchStartAudio = GetOrCreateAudioSource("Crouch Start Audio");
        crouchEndAudio = GetOrCreateAudioSource("Crouch End Audio");
    }

    void OnEnable() => SubscribeToEvents();
    void OnDisable() => UnsubscribeToEvents();

    void FixedUpdate()
    {
        float velocity = Vector3.Distance(CurrentCharacterPosition, lastCharacterPosition);
        if (velocity >= velocityThreshold && groundCheck && groundCheck.isGrounded)
        {
            if (character.IsCrouched)
            {
                SetPlayingMovingAudio(crouchedAudio);
            }
            else if (character.IsRunning)
            {
                SetPlayingMovingAudio(runningAudio);
            }
            else
            {
                SetPlayingMovingAudio(stepAudio);
            }
        }
        else
        {
            SetPlayingMovingAudio(null);
        }

        lastCharacterPosition = CurrentCharacterPosition;
    }

    void SetPlayingMovingAudio(AudioSource audioToPlay)
    {
        foreach (var audio in MovingAudios.Where(a => a != audioToPlay && a != null))
        {
            audio.Pause();
        }

        if (audioToPlay && !audioToPlay.isPlaying)
        {
            audioToPlay.Play();
        }
    }

    void PlayLandingAudio() => PlayRandomClip(landingAudio, landingSFX);
    void PlayJumpAudio() => PlayRandomClip(jumpAudio, jumpSFX);
    void PlayCrouchStartAudio() => PlayRandomClip(crouchStartAudio, crouchStartSFX);
    void PlayCrouchEndAudio() => PlayRandomClip(crouchEndAudio, crouchEndSFX);

    void SubscribeToEvents()
    {
        if (groundCheck) groundCheck.Grounded += PlayLandingAudio;
        if (character)
        {
            character.Jumped += PlayJumpAudio;
            character.CrouchStart += PlayCrouchStartAudio;
            character.CrouchEnd += PlayCrouchEndAudio;
        }
    }

    void UnsubscribeToEvents()
    {
        if (groundCheck) groundCheck.Grounded -= PlayLandingAudio;
        if (character)
        {
            character.Jumped -= PlayJumpAudio;
            character.CrouchStart -= PlayCrouchStartAudio;
            character.CrouchEnd -= PlayCrouchEndAudio;
        }
    }

    AudioSource GetOrCreateAudioSource(string name)
    {
        AudioSource result = GetComponentsInChildren<AudioSource>().FirstOrDefault(a => a.name == name);
        if (result) return result;

        GameObject go = new GameObject(name);
        result = go.AddComponent<AudioSource>();
        result.spatialBlend = 1;
        result.playOnAwake = false;
        go.transform.SetParent(transform, false);
        return result;
    }

    static void PlayRandomClip(AudioSource audio, AudioClip[] clips)
    {
        if (!audio || clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clips.Length > 1)
        {
            while (clip == audio.clip)
            {
                clip = clips[Random.Range(0, clips.Length)];
            }
        }

        audio.clip = clip;
        audio.Play();
    }
}
