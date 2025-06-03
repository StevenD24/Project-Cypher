using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Sound Effects")]
    public AudioSource[] soundEffects;

    [Header("Background Music GameObjects")]
    public GameObject normalBGMPrefab;
    public GameObject bossBGMPrefab;

    [Header("Audio Controls")]
    public bool isMusicMuted = false;

    public static AudioManager instance;

    private bool isBossMusicPlaying = false;
    private GameObject currentMusicObject;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Start playing normal background music
        if (normalBGMPrefab != null && !isMusicMuted)
        {
            PlayNormalBGM();
        }
    }

    public void PlaySFX(int soundToPlay)
    {
        if (soundToPlay >= 0 && soundToPlay < soundEffects.Length)
        {
            soundEffects[soundToPlay].Play();
        }
    }

    public void PlayBossBGM()
    {
        if (bossBGMPrefab != null && !isBossMusicPlaying && !isMusicMuted)
        {
            // Stop and destroy current music
            if (currentMusicObject != null)
            {
                Destroy(currentMusicObject);
            }

            // Instantiate and play boss music
            currentMusicObject = Instantiate(bossBGMPrefab);
            AudioSource bossAudioSource = currentMusicObject.GetComponent<AudioSource>();
            if (bossAudioSource != null)
            {
                bossAudioSource.loop = true;
                bossAudioSource.Play();
            }

            isBossMusicPlaying = true;
            Debug.Log("Boss music started!");
        }
    }

    public void PlayNormalBGM()
    {
        if (normalBGMPrefab != null && !isMusicMuted)
        {
            // Stop and destroy current music
            if (currentMusicObject != null)
            {
                Destroy(currentMusicObject);
            }

            // Instantiate and play normal music
            currentMusicObject = Instantiate(normalBGMPrefab);
            AudioSource normalAudioSource = currentMusicObject.GetComponent<AudioSource>();
            if (normalAudioSource != null)
            {
                normalAudioSource.loop = true;
                normalAudioSource.Play();
            }

            isBossMusicPlaying = false;
            Debug.Log(isBossMusicPlaying ? "Normal music resumed!" : "Normal music started!");
        }
    }

    // New methods for muting/unmuting music
    public void MuteMusic()
    {
        isMusicMuted = true;
        if (currentMusicObject != null)
        {
            AudioSource audioSource = currentMusicObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            Destroy(currentMusicObject);
            currentMusicObject = null;
        }
        Debug.Log("Background music muted!");
    }

    public void UnmuteMusic()
    {
        isMusicMuted = false;
        // Resume appropriate music based on current state
        if (isBossMusicPlaying)
        {
            PlayBossBGM();
        }
        else
        {
            PlayNormalBGM();
        }
        Debug.Log("Background music unmuted!");
    }

    public void ToggleMusic()
    {
        if (isMusicMuted)
        {
            UnmuteMusic();
        }
        else
        {
            MuteMusic();
        }
    }

    public bool IsBossMusicPlaying()
    {
        return isBossMusicPlaying;
    }

    public bool IsMusicMuted()
    {
        return isMusicMuted;
    }

    private void OnDestroy()
    {
        // Clean up music object when AudioManager is destroyed
        if (currentMusicObject != null)
        {
            Destroy(currentMusicObject);
        }
    }
}
