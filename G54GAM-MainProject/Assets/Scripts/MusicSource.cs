using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    public enum Fade {FadeIn, FadeOut, NoFade}

    public AudioClip music;
    public float volume;
    public bool loop;
    public float fadeRate = 0.25f;
    public AudioSource audioSource;
    
    public Fade fade;

    public void init(AudioSource aSource)
    {
        audioSource = aSource;
        if (fade != Fade.FadeOut)
        {
            
            audioSource.clip = music;
            audioSource.loop = loop;
            audioSource.volume = 0;
            audioSource.Play();
        }
    }

    void Update()
    {
        if (audioSource != null)
        {
            if (fade == Fade.FadeIn)
            {
                audioSource.volume += volume * fadeRate * Time.deltaTime;
                if(audioSource.volume >= volume)
                {
                    audioSource.volume = volume;
                    audioSource = null;
                }
            }
            else if(fade == Fade.FadeOut && audioSource.clip != null)
            {
                audioSource.volume -= volume * fadeRate * Time.deltaTime;
                Debug.Log(audioSource.volume);
                if (audioSource.volume <= 0)
                {
                    audioSource.volume = 0;
                    audioSource = null;
                }
            }
            else
            {
                audioSource.volume = volume;
            }

        }
    }
}
