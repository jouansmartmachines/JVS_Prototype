using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class MusicLoader : MonoBehaviour
{
    public static MusicLoader Instance;

    [SerializeField] private string folderLocation;
    [SerializeField] private List<AudioClip> backgroundMusics = new List<AudioClip>();
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        string appPath = Application.dataPath;

        string newPath = Path.GetFullPath(Path.Combine(appPath, @"..\..\"));
        string musicPath = newPath + folderLocation;

        if (musicPath != string.Empty)
        {

            DirectoryInfo file = new DirectoryInfo(musicPath);
            if( file != null )
            {
                Debug.LogWarning("file null "+ musicPath);
                return;
            }    
            FileInfo[] fInfosArray = file.GetFiles();
            if (fInfosArray.Length > 0)
            {
                StartCoroutine(GetAudioClip(fInfosArray));
            }
        }
    }

    private IEnumerator GetAudioClip(FileInfo[] infos)
    {
        UnityWebRequest[] unityWebRequest = new UnityWebRequest[infos.Length];
        AudioClip[] audios = new AudioClip[infos.Length];
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].Extension == ".wav")
            {
                unityWebRequest[i] = UnityWebRequestMultimedia.GetAudioClip(infos[i].FullName, AudioType.WAV);
            }
            else if (infos[i].Extension == ".mp3")
            {
                unityWebRequest[i] = UnityWebRequestMultimedia.GetAudioClip(infos[i].FullName, AudioType.MPEG);
            }

            if (unityWebRequest[i] != null)
            {
                yield return unityWebRequest[i].SendWebRequest();

                if (unityWebRequest[i].result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(unityWebRequest[i].error);
                }
                else if (unityWebRequest[i].result == UnityWebRequest.Result.Success)
                {
                    audios[i] = DownloadHandlerAudioClip.GetContent(unityWebRequest[i]);
                    backgroundMusics.Add(audios[i]);
                }
            }
        }

        PlayMusic();
    }

    private void PlayMusic()
    {
        int rndSong = Random.Range(0, backgroundMusics.Count);
        audioSource.clip = backgroundMusics[rndSong];
        audioSource.enabled = true;
        audioSource.Play();
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
