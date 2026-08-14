using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public AudioSource audioSource;
    public AudioClip mapMusic;
    public AudioClip combatMusic;
    public AudioClip tarotMusic;
    public AudioClip outpostMusic;
    public AudioClip menuMusic;
    public AudioClip defeatMusic;
    public AudioClip victoryMusic;

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

    public void PlayMapMusic()
    {
        PlayMusic(mapMusic);
    }

    public void PlayCombatMusic()
    {
        PlayMusic(combatMusic);
    }

    public void PlayTarotMusic()
    {
        PlayMusic(tarotMusic);
    }

    public void PlayOutpostMusic()
    {
        PlayMusic(outpostMusic);
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayDefeatMusic()
    {
        PlayMusic(defeatMusic);
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (audioSource.clip == clip && audioSource.isPlaying)
            return;

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }
}