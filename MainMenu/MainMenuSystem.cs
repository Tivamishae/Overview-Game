using UnityEngine;
using UnityEngine.SceneManagement; // needed for loading scenes

public class MainMenuSystem : MonoBehaviour
{
    public Animator playAnimator;

    [Header("Sound Paths (match AudioPreloader paths)")]
    public string pressButtonSoundPath = "Sounds/UI/PressButton";

    void PlayUISound(string clipPath)
    {
        if (AudioPreloader.Instance == null || AudioSystem.Instance == null) return;

        var clip = AudioPreloader.Instance.GetClip(clipPath);
        if (clip == null) return;

        var cam = Camera.main;
        if (cam != null)
        {
            AudioSystem.Instance.PlayClipAtPoint(clip, cam.transform.position, 1f);
        }
    }

    public void PressPlayButton()
    {
        playAnimator.SetTrigger("Play");
        PlayUISound(pressButtonSoundPath);
    }

    public void PressPlayBackButton()
    {
        playAnimator.SetTrigger("Back");
        PlayUISound(pressButtonSoundPath);
    }

    public void PressNewGameButton()
    {
        // Play sound
        PlayUISound(pressButtonSoundPath);

        // Load GameScene (works in Editor without needing a build)
        SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }
}
