using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioClip[] audioClips;
    public Action<int> PlayCallback;

    private AudioSource audioSource;
    private void Awake()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();

        PlayCallback += PlayAudio;
    }

    public void PlayAudio(int index)
    {
        audioSource.PlayOneShot(audioClips[index]);
    }
}
