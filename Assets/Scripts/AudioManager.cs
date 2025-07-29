using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public enum KEY
    {
        // BGM đánh số từ 1 đến 20
        BGM_1 = 1, BGM_2 = 2, BGM_Character = 3, BGM_Evolution = 4,
        // SFX  đánh số từ 21 trở lên
        SFX_ClickDefault = 21, SFX_ClickBack = 22, SFX_ClickPlayGame = 23, SFX_ClickFood = 24, SFX_ClickHarvest = 25,
        SFX_LevelUp = 26, SFX_Pray = 27
    }

    [Header("Audio Sources")]
    public AudioSource BGM;
    public AudioSource SFX;

    [System.Serializable]
    public struct DictionaryEntry
    {
        public KEY key;
        public AudioClip value;
    }
    [Header("Audio Clips")]
    public List<DictionaryEntry> BGMAudioClipEntries;
    public List<DictionaryEntry> SFXAudioClipEntries;
    private Dictionary<KEY, AudioClip> BGMAudioClip;
    private Dictionary<KEY, AudioClip> SFXAudioClip;

    private KEY[] bgmKeys = { KEY.BGM_1, KEY.BGM_2 };
    private int currentBGMIndex = 0;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize dictionaries
            BGMAudioClip = BGMAudioClipEntries.ToDictionary(entry => entry.key, entry => entry.value);
            SFXAudioClip = SFXAudioClipEntries.ToDictionary(entry => entry.key, entry => entry.value);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayCurrentBGM();
    }

    private void Update()
    {
        if (!BGM.isPlaying && BGMAudioClip.Count > 0)
        {
            PlayNextBGM();
        }
    }

    public void SetVolumeBGM(float volume)
    {
        BGM.volume = volume;
        GameData.bgmVolume = volume;
    }

    public void SetVolumeSFX(float volume)
    {
        SFX.volume = volume;
        GameData.sfxVolume = volume;
    }

    public void PlayBGM(KEY key)
    {
        if (BGMAudioClip.TryGetValue(key, out AudioClip clip))
        {
            BGM.clip = clip;
            BGM.Play();
        }
    }

    public void PlaySFX(KEY key)
    {
        // if (SFX.isPlaying) return;
        if (SFXAudioClip.TryGetValue(key, out AudioClip clip))
        {
            // SFX.pitch = Random.Range(0.9f, 1.1f); // Random pitch variation
            SFX.PlayOneShot(clip);
        }
    }

    private void PlayNextBGM()
    {
        currentBGMIndex = (currentBGMIndex + 1) % bgmKeys.Length;
        PlayBGM(bgmKeys[currentBGMIndex]);
    }

    public void PlayCurrentBGM()
    {
        PlayBGM(bgmKeys[currentBGMIndex]);
        BGM.loop = false;
    }
}
