using UnityEngine.Audio;
using UnityEngine;
using System;

public class audioManager : MonoBehaviour
{
    public Sound[] sounds;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.Loop;
        }
    }

    
    // Update is called once per frame
    public void Play(string name)
    {
       Sound s = Array.Find(sounds, Sound => Sound.name == name);
        s.source.Play();
    }
    public void stop(string name)
    {
        Sound s = Array.Find(sounds, Sound => Sound.name == name);
        s.source.Stop();
    }
}
