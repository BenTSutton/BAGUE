using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    public AudioSource audioSource;

    [Header("SFX Clips")]
    public AudioClip buttonClick;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClick);
    }
}