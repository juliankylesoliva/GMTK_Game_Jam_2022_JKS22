using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour
{
    [SerializeField] AudioClip[] clipList;

    private static Dictionary<string, AudioClip> clipDict = null;

    void Awake()
    {
        if (clipDict == null)
        {
            clipDict = new Dictionary<string, AudioClip>();
            foreach (AudioClip clip in clipList)
            {
                string clipString = clip.ToString();
                clipString = clipString.Substring(0, clipString.IndexOf(' ', 0, clipString.Length - 1));
                clipDict.Add(clipString, clip);
            }
        }
    }

    public static AudioClip GetAudioClip(string name)
    {
        if (clipDict.ContainsKey(name))
        {
            return clipDict[name];
        }
        return null;
    }
}
