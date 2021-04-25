using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFX { JUMP, EXPLOSION, GRAPPLE, HURT, OPEN_TREASURE, BAT_HURT, DEATH, START }

public class SFXController : MonoBehaviour
{
    public static SFXController instance;

    public AudioClip jump = null;
    public AudioClip explosion = null;
    public AudioClip grapple = null;
    public AudioClip hurt = null;
    public AudioClip openTreasure = null;
    public AudioClip batHurt = null;
    public AudioClip death = null;
    public AudioClip start = null;

    private Dictionary<SFX, AudioSource> audioSources;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this);

        audioSources = new Dictionary<SFX, AudioSource>();

        PreLoadSFX();
    }

    private void PreLoadSFX()
    {
        audioSources.Add(SFX.JUMP, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.JUMP].clip = jump;

        audioSources.Add(SFX.EXPLOSION, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.EXPLOSION].clip = explosion;

        audioSources.Add(SFX.GRAPPLE, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.GRAPPLE].clip = grapple;

        audioSources.Add(SFX.HURT, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.HURT].clip = hurt;

        audioSources.Add(SFX.OPEN_TREASURE, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.OPEN_TREASURE].clip = openTreasure;

        audioSources.Add(SFX.BAT_HURT, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.BAT_HURT].clip = batHurt;

        audioSources.Add(SFX.DEATH, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.DEATH].clip = death;

        audioSources.Add(SFX.START, gameObject.AddComponent<AudioSource>());
        audioSources[SFX.START].clip = start;
    }

    public void Play(SFX sfx, float volume)
    {
        //Debug.Log("asdf");
        AudioSource source = audioSources[sfx];
        //AudioSource source = new AudioSource();
        source.pitch = 1;
        source.volume = volume;
        //throw new System.Exception();

        source.Play();
    }

    //public IEnumerator Play(SFX sfx, float volume)
    //{
    //    AudioSource source = this.gameObject.AddComponent<AudioSource>();
    //    source.pitch = 1;
    //    source.volume = volume;
    //    //throw new System.Exception();
    //    switch (sfx)
    //    {
    //        case SFX.JUMP:
    //            source.clip = jump;
    //            break;
    //        case SFX.EXPLOSION:
    //            source.clip = explosion;
    //            break;
    //        case SFX.GRAPPLE:
    //            source.clip = grapple;
    //            break;
    //        case SFX.HURT:
    //            source.clip = hurt;
    //            break;
    //        case SFX.BAT_HURT:
    //            source.clip = batHurt;
    //            break;
    //        case SFX.OPEN_TREASURE:
    //            source.clip = openTreasure;
    //                break;
    //        //case SFX.WALK:
    //        //    source.clip = walk[Random.Range(0, walk.Length)];
    //        //    break;
    //    }
    //    source.Play();
    //    yield return new WaitUntil(() => source.timeSamples >= source.clip.samples);
    //    Destroy(source);
    //}
}
