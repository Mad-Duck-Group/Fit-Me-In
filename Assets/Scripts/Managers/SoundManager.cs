using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundFXTypes
{
    ButtonHover,
    ButtonClick,
    VolumeSlider,
    SceneTransition,
    CountOff,
    BlockHover,
    BlockRotate,
    BlockPlaced,
    BlockCancel,
    Score,
    ScoreFitMe,
    BonusTime,
    BonusTimeFitMe,
    BombAnnounce,
    BombExplode,
    FitMeAnnounce,
    FitMeExplode,
    TenSecondsLeft,
    TimeOut,
    Pause
}

public enum BGMTypes
{
    MainMenu,
    Game,
    Leaderboard
}
public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("Sound Manager is null");
            }
            return _instance;
        }
    }

    [Header("Sound Settings")]
    [SerializeField] private int poolSize = 10;
    [SerializeField] AudioMixerGroup audioMixerGroup;
    
    [Header("General FX")]
    [SerializeField] private AudioClip buttonHover;
    [SerializeField] private AudioClip buttonClick;
    [SerializeField] private AudioClip volumeSlider;
    [SerializeField] private AudioClip sceneTransition;
    
    [Header("Game FX")]
    [SerializeField] private AudioClip countOff;
    [SerializeField] private AudioClip blockHover;
    [SerializeField] private AudioClip blockRotate;
    [SerializeField] private AudioClip blockPlaced;
    [SerializeField] private AudioClip blockCancel;
    [SerializeField] private AudioClip score;
    [SerializeField] private AudioClip scoreFitMe;
    [SerializeField] private AudioClip bonusTime;
    [SerializeField] private AudioClip bonusTimeFitMe;
    [SerializeField] private AudioClip bombAnnounce;
    [SerializeField] private AudioClip bombExplode;
    [SerializeField] private AudioClip fitMeAnnounce;
    [SerializeField] private AudioClip fitMeExplode;
    [SerializeField] private AudioClip tenSecondsLeft;
    [SerializeField] private AudioClip timeOut;
    [SerializeField] private AudioClip pause;
    
    [Header("BGM")]
    [SerializeField] private AudioClip mainMenuBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private AudioClip leaderboardBGM;
    
    private List<AudioSource> _audioSources = new List<AudioSource>();

    private void Awake()
    {
        List<SoundManager> soundManagers = FindObjectsOfType<SoundManager>().ToList();
        if (soundManagers.Count > 1)
        {
            foreach (SoundManager soundManager in soundManagers)
            {
                if (soundManager != _instance)
                {
                    Destroy(soundManager.gameObject);
                }
            }
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateAudioSource();
        }
    }
    
    private void CreateAudioSource()
    {
        GameObject soundGameObject = new GameObject($"AudioSource{_audioSources.Count}");
        soundGameObject.transform.SetParent(transform);
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        _audioSources.Add(audioSource);
        audioSource.playOnAwake = false;
    }
    
    private bool FindFreeAudioSource(out AudioSource audioSource)
    {
        audioSource = _audioSources.Find(source => !source.isPlaying && !source.loop);
        return audioSource;
    }
    
    public void PlaySound(AudioClip clip, out AudioSource audioSource, bool loop, AudioSource preset = null)
    {
        audioSource = null;
        if (!clip) return;
        if (!FindFreeAudioSource(out AudioSource source))
        {
            Debug.LogWarning("No free audio source found, creating a new one, consider increasing the pool size");
            CreateAudioSource();
            source = _audioSources.Last();
        }
        source.loop = loop;
        source.clip = clip;
        if (preset) ApplyPreset(source, preset);
        source.Play();
        audioSource = source;
    }
    
    public void PauseSound(AudioSource audioSource)
    {
        if (!audioSource) return;
        audioSource.Pause();
    }
    
    public void ResumeSound(AudioSource audioSource)
    {
        if (!audioSource) return;
        audioSource.UnPause();
    }
    
    public void StopSound(AudioSource audioSource)
    {
        if (!audioSource) return;
        audioSource.Stop();
    }
    
    public void ResumeAllSounds()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            PauseSound(audioSource);
        }
    }
    
    public void PauseAllSounds()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            ResumeSound(audioSource);
        }
    }

    public void StopAllSounds()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            StopSound(audioSource);
        }
    }
    
    private void ApplyPreset(AudioSource audioSource, AudioSource preset)
    {
        audioSource.volume = preset.volume;
        audioSource.pitch = preset.pitch;
        audioSource.loop = preset.loop;
        audioSource.outputAudioMixerGroup = preset.outputAudioMixerGroup;
        audioSource.spatialBlend = preset.spatialBlend;
        audioSource.panStereo = preset.panStereo;
        audioSource.reverbZoneMix = preset.reverbZoneMix;
        audioSource.bypassEffects = preset.bypassEffects;
        audioSource.bypassListenerEffects = preset.bypassListenerEffects;
        audioSource.bypassReverbZones = preset.bypassReverbZones;
        audioSource.dopplerLevel = preset.dopplerLevel;
        audioSource.spread = preset.spread;
        audioSource.rolloffMode = preset.rolloffMode;
        audioSource.minDistance = preset.minDistance;
        audioSource.maxDistance = preset.maxDistance;
        audioSource.ignoreListenerVolume = preset.ignoreListenerVolume;
        audioSource.ignoreListenerPause = preset.ignoreListenerPause;
        audioSource.priority = preset.priority;
        audioSource.mute = preset.mute;
    }
    
    public void PlaySoundFX(SoundFXTypes soundFXType, out AudioSource audioSource, bool loop = false, AudioSource preset = null)
    {
        AudioClip clip = null;
        switch (soundFXType)
        {
            case SoundFXTypes.ButtonHover:
                clip = buttonHover;
                break;
            case SoundFXTypes.ButtonClick:
                clip = buttonClick;
                break;
            case SoundFXTypes.VolumeSlider:
                clip = volumeSlider;
                break;
            case SoundFXTypes.SceneTransition:
                clip = sceneTransition;
                break;
            case SoundFXTypes.CountOff:
                clip = countOff;
                break;
            case SoundFXTypes.BlockHover:
                clip = blockHover;
                break;
            case SoundFXTypes.BlockRotate:
                clip = blockRotate;
                break;
            case SoundFXTypes.BlockPlaced:
                clip = blockPlaced;
                break;
            case SoundFXTypes.BlockCancel:
                clip = blockCancel;
                break;
            case SoundFXTypes.Score:
                clip = score;
                break;
            case SoundFXTypes.ScoreFitMe:
                clip = scoreFitMe;
                break;
            case SoundFXTypes.BonusTime:
                clip = bonusTime;
                break;
            case SoundFXTypes.BonusTimeFitMe:
                clip = bonusTimeFitMe;
                break;
            case SoundFXTypes.BombAnnounce:
                clip = bombAnnounce;
                break;
            case SoundFXTypes.BombExplode:
                clip = bombExplode;
                break;
            case SoundFXTypes.FitMeAnnounce:
                clip = fitMeAnnounce;
                break;
            case SoundFXTypes.FitMeExplode:
                clip = fitMeExplode;
                break;
            case SoundFXTypes.TenSecondsLeft:
                clip = tenSecondsLeft;
                break;
            case SoundFXTypes.TimeOut:
                clip = timeOut;
                break;
            case SoundFXTypes.Pause:
                clip = pause;
                break;
        }
        PlaySound(clip, out AudioSource source, loop, preset);
        audioSource = source;
    }
    
    public void PlayBGM(BGMTypes bgmType, out AudioSource audioSource, bool loop = true, AudioSource preset = null)
    {
        AudioClip clip = null;
        switch (bgmType)
        {
            case BGMTypes.MainMenu:
                clip = mainMenuBGM;
                break;
            case BGMTypes.Game:
                clip = gameBGM;
                break;
            case BGMTypes.Leaderboard:
                clip = leaderboardBGM;
                break;
        }
        PlaySound(clip, out AudioSource source, loop, preset);
        audioSource = source;
    }
}
