using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private List<AudioFile> audioFiles;
    private Dictionary<AudioType, AudioClip> audioDictionary;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource BGMSource;
    [SerializeField] private AudioSource SFXSource;

    private void Awake()
    {
        audioDictionary = new Dictionary<AudioType, AudioClip>();
        foreach (var audioFile in audioFiles)
        {
            if (!audioDictionary.ContainsKey(audioFile.audioType))
            {
                audioDictionary.Add(audioFile.audioType, audioFile.clip);
            }
        }
    }
    public void PlayAudio(AudioType audioType)
    {
        //get audio file from dectionary
        AudioFile audioFile = audioDictionary.TryGetValue(audioType, out AudioClip clip) ? new AudioFile { clip = clip } : null;
        if (audioFile != null && audioFile.clip != null)
        {
            SFXSource.PlayOneShot(audioFile.clip);
        }
        else
        {
            Debug.LogWarning($"Audio clip for {audioType} not found.");
        }
    }

    public void PlayMusic(AudioType audioType)
    {
        AudioFile audioFile = audioDictionary.TryGetValue(audioType, out AudioClip clip) ? new AudioFile { clip = clip } : null;
        if (audioFile != null && audioFile.clip != null)
        {
            BGMSource.clip = audioFile.clip;
            BGMSource.Play();
        }
        else
        {
            Debug.LogWarning($"Audio clip for {audioType} not found.");
        }
    }
}

[System.Serializable]
public class AudioFile
{
    public AudioType audioType;
    public AudioClip clip;
}

public enum AudioType
{
    BGM_1,
    ButtonClick,
    DiamondCollect,
    GameOver,
    PlayerJump,
    PlayerLand,
    SpikeHit
}