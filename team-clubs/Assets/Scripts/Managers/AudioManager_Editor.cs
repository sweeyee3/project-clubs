#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AudioManager))]
public class AudioManager_Editor : Editor
{    
    private string m_audioName;
    private AudioClip m_audioClip;
    private AudioManager.EAudioType m_audioType;
    private bool m_loop;

    SerializedObject m_so;
    SerializedProperty m_bgmList;
    SerializedProperty m_sfxList;

    private void OnEnable()
    {
        m_so = serializedObject;
        m_bgmList = m_so.FindProperty("m_bgmList");
        m_sfxList = m_so.FindProperty("m_sfxList");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var audioManager = m_so.targetObject as AudioManager;

        m_so.Update();
        using (new GUILayout.HorizontalScope())
        {
            m_audioName = GUILayout.TextField(m_audioName, GUILayout.MinWidth(100));
            m_audioClip = EditorGUILayout.ObjectField(m_audioClip, typeof(AudioClip)) as AudioClip;
            m_audioType = (AudioManager.EAudioType)EditorGUILayout.EnumPopup(m_audioType);            

            if (GUILayout.Button(m_loop ? "loop" : "no loop"))
            {                
                m_loop = !m_loop;
            }
            if (GUILayout.Button("+"))
            {
                GameObject source = new GameObject(m_audioName);
                var audioSrc = source.AddComponent<AudioSource>();
                audioSrc.clip = m_audioClip;
                audioSrc.loop = m_loop;
                audioSrc.playOnAwake = false;

                audioSrc.transform.parent = audioManager.transform;

                switch (m_audioType)
                {
                    case AudioManager.EAudioType.BGM:
                        m_bgmList.InsertArrayElementAtIndex(m_bgmList.arraySize);
                        m_bgmList.GetArrayElementAtIndex(m_bgmList.arraySize - 1).objectReferenceValue = audioSrc;                        
                        break;
                    case AudioManager.EAudioType.SFX:                        
                        m_sfxList.InsertArrayElementAtIndex(m_sfxList.arraySize);                        
                        m_sfxList.GetArrayElementAtIndex(m_sfxList.arraySize - 1).objectReferenceValue = audioSrc;
                        break;
                }              
            }
            if (GUILayout.Button("Update"))
            {                
                switch (m_audioType)
                {
                    case AudioManager.EAudioType.BGM:
                        for (var i=0; i<m_bgmList.arraySize; i++)
                        {
                            var audioSource = m_bgmList.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                            if (audioSource.name == m_audioName)
                            {
                                audioSource.clip = m_audioClip;
                                audioSource.loop = m_loop;
                                m_bgmList.GetArrayElementAtIndex(i).objectReferenceValue = audioSource;
                            }
                        }                       
                        break;
                    case AudioManager.EAudioType.SFX:
                        for (var i = 0; i < m_sfxList.arraySize; i++)
                        {
                            var audioSource = m_sfxList.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                            if (audioSource.name == m_audioName)
                            {
                                audioSource.clip = m_audioClip;
                                audioSource.loop = m_loop;
                                m_sfxList.GetArrayElementAtIndex(i).objectReferenceValue = audioSource;
                            }
                        }
                        break;
                }                
            }
            if (GUILayout.Button("-"))
            {
                int removedIdx = -1;
                switch (m_audioType)
                {
                    case AudioManager.EAudioType.BGM:
                        for (var i = 0; i < m_bgmList.arraySize; i++)
                        {
                            var audioSource = m_bgmList.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                            if (audioSource.name == m_audioName)
                            {
                                removedIdx = i;
                            }
                        }
                        if (removedIdx >= 0)
                        {
                            var asrc = m_bgmList.GetArrayElementAtIndex(removedIdx).objectReferenceValue as AudioSource;
                            DestroyImmediate(asrc.gameObject);
                            m_bgmList.DeleteArrayElementAtIndex(removedIdx);
                        }
                        break;
                    case AudioManager.EAudioType.SFX:
                        for (var i = 0; i < m_sfxList.arraySize; i++)
                        {
                            var audioSource = m_sfxList.GetArrayElementAtIndex(i).objectReferenceValue as AudioSource;
                            if (audioSource.name == m_audioName)
                            {
                                removedIdx = i;
                            }
                        }
                        if (removedIdx >= 0)
                        {
                            var asrc = m_sfxList.GetArrayElementAtIndex(removedIdx).objectReferenceValue as AudioSource;
                            DestroyImmediate(asrc.gameObject);
                            m_sfxList.DeleteArrayElementAtIndex(removedIdx);
                        }
                        break;
                }
            }            
        }        

        m_so.ApplyModifiedProperties();            
    }
}

#endif