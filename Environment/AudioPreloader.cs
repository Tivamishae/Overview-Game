#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CachedAudioClip
{
    public string path;     // e.g. "Sounds/Bosses/Boss1/Shockwave"
    public AudioClip clip;
}

public class AudioPreloader : MonoBehaviour
{
    public static AudioPreloader Instance { get; private set; }

    [SerializeField] private List<CachedAudioClip> cachedClips = new();
    private Dictionary<string, AudioClip> clipCache = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildCache();
        PreWarmAllClips(); //  prewarm at startup
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Audio Cache")]
    void RebuildAudioCacheEditor()
    {
        cachedClips.Clear();

        // find all AudioClips in Assets/Resources/Sounds
        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Resources/Sounds" });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip == null) continue;

            // strip Assets/Resources/ and extension -> get correct Resources.Load() path
            string path = assetPath.Replace("Assets/Resources/", "");
            path = System.IO.Path.ChangeExtension(path, null); // removes .wav/.mp3/.ogg

            cachedClips.Add(new CachedAudioClip { path = path, clip = clip });
        }

        EditorUtility.SetDirty(this);
        Debug.Log($"[AudioPreloader] Cached {cachedClips.Count} audio clips with full paths.");
    }
#endif

    void BuildCache()
    {
        clipCache.Clear();
        foreach (var c in cachedClips)
        {
            if (c.clip != null && !clipCache.ContainsKey(c.path))
                clipCache.Add(c.path, c.clip);
        }
    }

    /// <summary>
    /// Force Unity to fully load/decompress all clips into memory.
    /// Prevents stutter on first play.
    /// </summary>
    void PreWarmAllClips()
    {
        foreach (var c in cachedClips)
        {
            if (c.clip != null && !c.clip.loadState.Equals(AudioDataLoadState.Loaded))
            {
                c.clip.LoadAudioData();
            }
        }

        Debug.Log($"[AudioPreloader] Pre-warmed {cachedClips.Count} audio clips.");
    }

    public AudioClip GetClip(string path)
    {
        if (clipCache.TryGetValue(path, out var clip))
            return clip;

        Debug.LogWarning($"[AudioPreloader] Clip not found: {path}");
        return null;
    }
}
