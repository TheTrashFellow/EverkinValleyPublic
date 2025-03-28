using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{    

    [Header("Audio Source Global")]
    public AudioSource musicSource;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Musiques d’ambiance par scène")]
    public AudioClip loginMusic;
    public AudioClip openWorldMusic;
    public AudioClip villageMusic;
    public AudioClip constructionMusic;

    [Header("SFX Clips par cat?gorie")]
    public List<AudioClip> uiClips;
    public List<AudioClip> playerClips;
    public List<AudioClip> npcMaleClips;
    public List<AudioClip> npcFemaleClips;
    public List<AudioClip> villageClips;
    public List<AudioClip> constructionClips;
    public List<AudioClip> biomeClips;

    private Dictionary<SoundCategory, List<AudioClip>> sfxDict = new();
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private int maxSFXSources = 10;

    public enum SoundCategory
    {
        UI,
        Player,
        NPC_Male,
        NPC_Female,
        Village,
        Construction,
        Biome
    }

    

    private void Start()
    {
        SelectSceneMusic(true, "null");
    }

    public void SelectSceneMusic(bool isOnStart, string songName)
    {
        Debug.LogError("Changing song ...");
        if (isOnStart)
            PlaySceneMusic(SceneManager.GetActiveScene().name);
        else
            PlaySceneMusic(songName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            //ToggleMusicKey();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        StartCoroutine(FadeMusic(clip));
    }

    public IEnumerator FadeMusic(AudioClip newClip, float fadeDuration = 0.5f)
    {
        if (musicSource.clip == newClip) yield break;

        float startVolume = musicSource.volume;

        // Fade Out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade In
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0, musicVolume, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public void PlaySceneMusic(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                PlayMusic(loginMusic);
                break;
            case "OpenWorld":
                PlayMusic(openWorldMusic);
                break;
            case "Village_network":
                PlayMusic(villageMusic);
                break;
            case "Construction":
                PlayMusic(constructionMusic);
                break;
            default:
                Debug.LogWarning($"Aucune musique définie pour la scène : {sceneName}");
                break;
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;

        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }
    /*
    public void ToggleMusicKey()
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
            musicSource.volume = (musicSource.volume > 0f) ? 0f : 1f;
            Debug.Log("Musique " + (musicSource.volume > 0f ? "activ?e" : "coup?e"));
        }
    }*/

    private void InitSFXDict()
    {
        sfxDict.Add(SoundCategory.UI, uiClips);
        sfxDict.Add(SoundCategory.Player, playerClips);
        sfxDict.Add(SoundCategory.NPC_Male, npcMaleClips);
        sfxDict.Add(SoundCategory.NPC_Female, npcFemaleClips);
        sfxDict.Add(SoundCategory.Village, villageClips);
        sfxDict.Add(SoundCategory.Construction, constructionClips);
        sfxDict.Add(SoundCategory.Biome, biomeClips);
    }

    private void InitSFXSources(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject sfxObj = new GameObject("SFXSource_" + i);
            AudioSource newSource = sfxObj.AddComponent<AudioSource>();
            newSource.transform.parent = transform;
            sfxSources.Add(newSource);
        }
    }

    public void PlaySFX(SoundCategory category, int index, float volumeMultiplier = 1f)
    {
        if (sfxDict.ContainsKey(category) && index >= 0 && index < sfxDict[category].Count)
        {
            AudioSource source = GetAvailableSFXSource();
            source.clip = sfxDict[category][index];
            source.volume = Mathf.Clamp(sfxVolume * volumeMultiplier, 0f, 1f);
            source.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
            source.Play();
        }
        else
        {
            Debug.LogWarning($"Tentative de jouer un son hors limite: {category} index {index}");
        }
    }

    public void PlayRandomSFX(SoundCategory category)
    {
        if (sfxDict.ContainsKey(category) && sfxDict[category].Count > 0)
        {
            int randomIndex = Random.Range(0, sfxDict[category].Count);
            PlaySFX(category, randomIndex);
        }
        else
        {
            Debug.LogWarning($"Aucun son disponible pour la cat?gorie : {category}");
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxSources)
        {
            if (!source.isPlaying)
                return source;
        }

        if (sfxSources.Count >= maxSFXSources)
        {
            Debug.LogWarning("Limite de sources SFX atteinte !");
            return sfxSources[0];
        }

        GameObject newSFXObject = new GameObject("SFXSource");
        AudioSource newSource = newSFXObject.AddComponent<AudioSource>();
        newSource.transform.parent = transform;
        sfxSources.Add(newSource);
        return newSource;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();

        foreach (var source in sfxSources)
        {
            source.volume = sfxVolume;
        }
    }

    public void PlaySFX_Index(int index)
    {
        PlaySFX(SoundCategory.UI, index);
    }

}
