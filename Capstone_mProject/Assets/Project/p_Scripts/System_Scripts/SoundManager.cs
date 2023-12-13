using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance = null;
    public static SoundManager Instance
    {
        get
        {
            if (null == instance)
            {
                return null;
            }
            return instance;
        }
    }

    [Header("BGM")]
    public AudioClip[] bgmClip;
    private AudioSource bgmPlayer;
    public enum BGM
    {
        Title,
        Ingame
    }

    [Header("PlayerSound")]
    public AudioClip[] playerSoundClips;
    private AudioSource playerSoundPlayer;
    public enum PlayerSound
    {
        Hit,
        GetDamage,
        Death
    }

    [Header("MonsterSound")]
    //몬스터 사운드 클립은 몬스터 스크립트로 따로 관리
    private AudioSource[] mosterSoundPlayer;
    public int monsterSound_channels;
    private int monsterSound_ChannelIndex;

    [Header("Other sfx Sound")] //기타. 효과음.
    public AudioClip[] sfxClips;
    private AudioSource[] sfxPlayer;
    public int sfx_channels;
    private int sfx_channelIndex;
    public enum SfxSound
    {
        UI
    }

    void Awake()
    {
        Init();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);

        //배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = 1;

        //플레이어 사운드 플레이어 초기화
        GameObject playerSoundObject = new GameObject("playerSoundPlayer");
        playerSoundObject.transform.parent = transform;
        playerSoundPlayer = playerSoundObject.AddComponent<AudioSource>();
        playerSoundPlayer.playOnAwake = false;
        playerSoundPlayer.loop = true;
        playerSoundPlayer.volume = 1;

        //몬스터 사운드 플레이어 초기화
        GameObject monsterSoundObject = new GameObject("monsterSoundPlayer");
        monsterSoundObject.transform.parent = transform;
        mosterSoundPlayer = new AudioSource[monsterSound_channels];

        for (int i = 0; i < monsterSound_channels; i++)
        {
            mosterSoundPlayer[i] = monsterSoundObject.AddComponent<AudioSource>();
            mosterSoundPlayer[i].playOnAwake = false;
            mosterSoundPlayer[i].loop = true;
            mosterSoundPlayer[i].volume = 1;
        }

        //기타 효과음
        GameObject sfxObject = new GameObject("sfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayer = new AudioSource[sfx_channels];

        for (int i = 0; i < sfx_channels; i++)
        {
            sfxPlayer[i] = sfxObject.AddComponent<AudioSource>();
            sfxPlayer[i].playOnAwake = false;
            sfxPlayer[i].loop = true;
            sfxPlayer[i].volume = 1;
        }
    }

    public void Play_BGM(BGM bgm, bool useLoop = false)
    {
        playerSoundPlayer.loop = useLoop;
        bgmPlayer.clip = bgmClip[(int)bgm];
        bgmPlayer.Play();
    }

    public void Play_PlayerSound(PlayerSound p_Sound, bool useLoop = false)
    {
        //플레이어 캐릭터 사운드
        playerSoundPlayer.loop = useLoop;
        playerSoundPlayer.clip = playerSoundClips[(int)p_Sound];
        playerSoundPlayer.Play();

    }

    public void Play_MonsterSound(AudioClip monsterSoundClip, bool useLoof = false)
    {
        for (int index = 0; index < mosterSoundPlayer.Length; index++)
        {
            int loopIndex = (index + monsterSound_ChannelIndex) % mosterSoundPlayer.Length;

            if (mosterSoundPlayer[loopIndex].isPlaying)
                continue;

            monsterSound_ChannelIndex = loopIndex;
            mosterSoundPlayer[monsterSound_ChannelIndex].loop = useLoof;
            mosterSoundPlayer[monsterSound_ChannelIndex].clip = monsterSoundClip;
            mosterSoundPlayer[monsterSound_ChannelIndex].Play();
            break;
        }
    }

    public void Play_SfxSound(SfxSound sfx_Sound)
    {
        for (int index = 0; index < sfxPlayer.Length; index++)
        {
            int loopIndex = (index + sfx_channelIndex) % sfxPlayer.Length;

            if (sfxPlayer[loopIndex].isPlaying)
                continue;

            sfx_channelIndex = loopIndex;
            sfxPlayer[sfx_channelIndex].clip = sfxClips[(int)sfx_Sound];
            sfxPlayer[sfx_channelIndex].Play();
            break;
        }
    }

}
