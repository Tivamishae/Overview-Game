using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioSystem : MonoBehaviour
{
    public static AudioSystem Instance;
    [SerializeField] int initialPool = 8;

    readonly Queue<AudioSource> pool = new();
    readonly List<AudioSource> inUse = new();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < initialPool; i++) pool.Enqueue(CreateSource());
    }

    AudioSource CreateSource()
    {
        var go = new GameObject("PooledAudio");
        go.transform.parent = transform;
        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.dopplerLevel = 0f;
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.bypassEffects = true; // tweak per needs
        src.bypassListenerEffects = false;
        src.bypassReverbZones = true;
        go.SetActive(false);
        return src;
    }

    public AudioSource PlayAudioLoop(AudioClip clip, Transform followTransform, float volume)
    {
        if (!clip) return null;

        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();
        var go = src.gameObject;
        go.transform.SetParent(followTransform);  // Attach the audio to the boss
        go.transform.localPosition = Vector3.zero; // Ensure audio source stays at the boss's position
        go.SetActive(true);

        src.clip = clip;
        src.volume = volume;
        src.loop = true;  // Set to loop
        src.Play();

        inUse.Add(src);
        return src;  // Return the AudioSource so it can be stopped or modified later if needed
    }

    private IEnumerator ReturnWhenDone(AudioSource src, float seconds)
    {
        yield return new WaitForSeconds(seconds);

        if (src == null) yield break; // it was destroyed, bail out safely
        if (!src.gameObject) yield break;

        inUse.Remove(src);

        // Safely stop only if still valid
        if (src != null)
        {
            src.Stop();
            src.clip = null;
            src.gameObject.SetActive(false);
            pool.Enqueue(src);
        }
    }


    public void PlayClipAtPoint(AudioClip clip, Vector3 pos, float volume)
    {
        if (!clip) return;

        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();
        var go = src.gameObject;
        go.transform.position = pos;
        go.SetActive(true);

        src.clip = clip;
        src.volume = volume;
        src.Play();

        inUse.Add(src);
        StartCoroutine(ReturnWhenDone(src, clip.length / Mathf.Max(0.01f, src.pitch)));
    }

    public AudioSource PlayClipFollow(AudioClip clip, Transform followTransform, float volume)
    {
        if (!clip || followTransform == null) return null;

        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();
        var go = src.gameObject;

        // Parent to target initially
        go.transform.SetParent(followTransform);
        go.transform.localPosition = Vector3.zero;   // stick to NPC
        go.SetActive(true);

        src.clip = clip;
        src.volume = volume;
        src.loop = false;  // play once
        src.Play();

        inUse.Add(src);

        // Start coroutine that handles cleanup & detaching if parent destroyed
        StartCoroutine(FollowOrDetach(src, followTransform, clip.length / Mathf.Max(0.01f, src.pitch)));

        return src;
    }

    private IEnumerator FollowOrDetach(AudioSource src, Transform followTransform, float duration)
    {
        float timer = 0f;
        while (timer < duration && src != null)
        {
            // If parent got destroyed  detach to world
            if (followTransform == null && src.transform.parent != null)
            {
                src.transform.SetParent(null, true); // keep world position
            }

            timer += Time.deltaTime;
            yield return null;
        }

        if (src != null)
        {
            src.Stop();
            src.gameObject.SetActive(false);
            inUse.Remove(src);
            pool.Enqueue(src);
        }
    }

    public void PlayClipDetached(AudioClip clip, Vector3 position, float volume)
    {
        if (!clip) return;

        // Create or reuse a pooled AudioSource
        var src = pool.Count > 0 ? pool.Dequeue() : CreateSource();
        var go = src.gameObject;

        go.transform.SetParent(null);       // make sure it's fully unparented
        go.transform.position = position;   // world-space
        go.SetActive(true);

        src.clip = clip;
        src.volume = volume;
        src.loop = false;
        src.spatialBlend = 1f;
        src.Play();

        inUse.Add(src);

        // Let the clip play out independently, then recycle
        StartCoroutine(ReturnWhenDone(src, clip.length / Mathf.Max(0.01f, src.pitch)));
    }


}



