using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip menuBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private List<AudioClipData> sfxClips = new List<AudioClipData>();

    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
    }

    [Header("Volume Settings")]
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField] [Range(0f, 1f)] private float sfxVolume = 1f;

    private Dictionary<string, AudioClip> sfxClipDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bgmSource == null)
        {
            GameObject bgmObj = new GameObject("BGM");
            bgmObj.transform.SetParent(transform);
            bgmSource = bgmObj.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = true;
        }

        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFX");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
        }

        UpdateVolume();
    }

    private void Start()
    {
        BuildSFXDictionary();
        if (menuBGM != null && bgmSource != null)
            PlayBGM(menuBGM);
    }

    private void BuildSFXDictionary()
    {
        sfxClipDictionary = new Dictionary<string, AudioClip>();
        foreach (AudioClipData data in sfxClips)
        {
            if (data.clip != null && !string.IsNullOrEmpty(data.name))
                sfxClipDictionary[data.name] = data.clip;
        }
    }

    private void OnEnable()
    {
        GameManager.OnGameStart += OnGameStart;
        GameManager.OnGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= OnGameStart;
        GameManager.OnGameOver -= OnGameOver;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null)
            return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void PlaySFX(string clipName)
    {
        if (sfxSource == null || string.IsNullOrEmpty(clipName))
            return;

        if (sfxClipDictionary != null && sfxClipDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            if (clip != null)
                sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolume();
    }

    public float GetMusicVolume() => musicVolume;

    public float GetSFXVolume() => sfxVolume;

    private void UpdateVolume()
    {
        if (bgmSource != null)
            bgmSource.volume = musicVolume;
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }

    private void OnGameStart()
    {
        if (gameBGM != null)
            PlayBGM(gameBGM);
    }

    private void OnGameOver()
    {
        PlaySFX("GameOver");
    }
}
