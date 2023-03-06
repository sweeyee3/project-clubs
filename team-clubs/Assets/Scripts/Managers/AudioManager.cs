using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum EAudioType
    {
        BGM,
        SFX
    }   

    [SerializeField] private List<AudioSource> m_sfxList;
    [SerializeField] private List<AudioSource> m_bgmList;    
    
    private List<AudioSource> SFXList
    {
        get
        {
            return m_sfxList;
        }
    }

    private List<AudioSource> BGMList
    {
        get
        {
            return m_bgmList;
        }
    }

    private static AudioManager m_instance;
    public static AudioManager Instance
    {
        get
        {
            m_instance = GameObject.FindObjectOfType<AudioManager>();
            if (m_instance == null)
            {
                var gObj = new GameObject("AudioManager");
                m_instance = gObj.AddComponent<AudioManager>();
            }
            return m_instance;                        
        }
    }               

    public void Play(string name, EAudioType type, float startTime = 0)
    {
        var list = type == EAudioType.BGM ? m_bgmList : m_sfxList;

        AudioSource source = list.Find(x => x.name == name);        
        if (source == null)
        {
            Debug.LogError("Missing " + type.ToString() + " source: " + name);
        }
        else
        {
            if (source.clip == null) Debug.LogError("Missing " + type.ToString() + " clip: " + name);
            else
            {
                if (!source.isPlaying)
                {
                    source.time = startTime;
                    source.Play();
                }
            }
        }
    }

    public void Stop(string name, EAudioType type)
    {
        var list = type == EAudioType.BGM ? m_bgmList : m_sfxList;

        AudioSource source = list.Find(x => x.name == name);

        if (source == null)
        {
            Debug.LogError("Missing " + type.ToString() + " source: " + name);
        }
        else
        {
            if (source.clip == null) Debug.LogError("Missing " + type.ToString() + " clip: " + name);
            else source.Stop();
        }
    }
}
