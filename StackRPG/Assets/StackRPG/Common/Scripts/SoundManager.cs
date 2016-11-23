using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    static SoundManager m_instance;
    public static SoundManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                // 현재 씬 내에서 GameManager 컴포넌트를 검색
                m_instance = FindObjectOfType(typeof(SoundManager)) as SoundManager;
                if (m_instance == null)
                {
                    // 현재 씬에 GameManager 컴포넌트가 없으면 새로 생성
                    m_instance = new GameObject("SoundManager", typeof(SoundManager)).GetComponent<SoundManager>();
                    m_instance.gameObject.AddComponent<AudioListener>();
                }
            }
            return m_instance;
        }
    }

    void Init()
    {
        //SetSoundVolume(ApplicationManager.Instance.m_volumeSound);
        //SetBGMVolume(ApplicationManager.Instance.m_volumeBGM);
    }

    public class Audio
    {
        public string m_clipName;
        public AudioClip m_audioClip;
        public List<AudioSource> m_audioSource;
        
        public int m_playOffset;

        public Audio()
        {
            m_audioSource = new List<AudioSource>();
            m_playOffset = 0;
        }

        public void Play(float volume)
        {
            if (m_audioSource.Count <= 0)
                return;

            m_audioSource[m_playOffset].volume = volume;
            //m_audioSource[m_playOffset].pitch = MSettings.Random(0.9f, 1.1f);

            m_audioSource[m_playOffset].Play();

            m_playOffset++;
            if (m_playOffset >= m_audioSource.Count)
                m_playOffset = 0;

        }
    }

    public AudioClip[] Sound;
    public Dictionary<int, Audio> m_audioMap;
    public Dictionary<int, AudioSource> m_bgmMap;

    AudioSource m_currentBGM;
    AudioSource m_nextBGM;

    const float m_bgmFadeSpeed = 0.5f;

    protected SoundManager()
    {
        m_audioMap = new Dictionary<int, Audio>();
        m_bgmMap = new Dictionary<int, AudioSource>();

        m_currentBGM = null;
        m_nextBGM = null;
    }

    public void Awake()
    {
        // 씬이 변경되어도 제거되지 않도록 설정
        DontDestroyOnLoad(gameObject);

        Init();

        //LoadBGM();
        //LoadSound();
    }

    void LoadBGM()
    {
        Object[] objs = Resources.LoadAll("BGM", typeof(AudioClip));

        foreach (Object obj in objs)
        {
            AudioClip audioClip = (AudioClip)obj;
            AudioSource audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
            audioSource.clip = audioClip;
            audioSource.loop = true;

            m_bgmMap.Add(audioClip.name.GetHashCode(), audioSource);
        }
    }

    void LoadBGM(string clipName)
    {
        Object obj = Resources.Load("BGM/" + clipName, typeof(AudioClip));

        AudioClip audioClip = (AudioClip)obj;
        AudioSource audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
        audioSource.clip = audioClip;
        audioSource.loop = true;

        m_bgmMap.Add(audioClip.name.GetHashCode(), audioSource);
    }

    void LoadSound()
    {
        Object[] objs = Resources.LoadAll("Sound", typeof(AudioClip));

        foreach (Object obj in objs)
        {
            AudioClip audioClip = (AudioClip)obj;
            Audio audio = new Audio();
            audio.m_clipName = audioClip.name;
            audio.m_audioClip = audioClip;

            int i;
            for (i = 0; i < 2; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>() as AudioSource;
                audioSource.clip = audioClip;
                audioSource.loop = false;

                audio.m_audioSource.Add(audioSource);
            }

            m_audioMap.Add(audio.m_clipName.GetHashCode(), audio);
        }
    }

    float m_deltaTime = 0.01f;

    void Update()
    {
        if (m_nextBGM != null)
        {
            if (m_currentBGM != null)
            {
                m_currentBGM.volume -= m_deltaTime * m_bgmFadeSpeed;
            }

            m_nextBGM.volume += m_deltaTime * m_bgmFadeSpeed;

            if (m_nextBGM.volume >= 1f * m_volumeBGM)
            {
                m_nextBGM.volume = 1f * m_volumeBGM;

                if (m_currentBGM != null)
                    m_currentBGM.Stop();

                m_currentBGM = m_nextBGM;
                m_nextBGM = null;
            }
        }
        else if (m_currentBGM != null)
        {
            if (m_currentBGM.volume < 1f * m_volumeBGM)
            {
                m_currentBGM.volume += m_deltaTime * m_bgmFadeSpeed;
                if (m_currentBGM.volume >= 1f * m_volumeBGM)
                    m_currentBGM.volume = 1f * m_volumeBGM;
            }
            else if (m_currentBGM.volume > 1f * m_volumeBGM)
            {
                m_currentBGM.volume -= m_deltaTime * m_bgmFadeSpeed;
                if (m_currentBGM.volume <= 1f * m_volumeBGM)
                    m_currentBGM.volume = 1f * m_volumeBGM;
            }
        }
    }

    public void PlaySound(string clipName)
    {
        if (string.IsNullOrEmpty(clipName) == true)
        {
            return;
        }

        Audio audio = null;
        if (m_audioMap.TryGetValue(clipName.GetHashCode(), out audio) == true)
        {
            audio.Play(m_volumeSound);
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log(clipName + " 파일이 없습니다.");
#endif
        }
    }

    public void PlayBGM(string clipName)
    {
        int bgmHash = clipName.GetHashCode();
        if (m_bgmMap.ContainsKey(bgmHash) == false) LoadBGM(clipName);

        PlayBGM(bgmHash);
    }

    public void PlayBGM(int clipNameHash)
    {
        AudioSource audio = null;
        
        if (m_bgmMap.TryGetValue(clipNameHash, out audio) == true)
        {
            if (audio == m_currentBGM)
            {
                if (m_nextBGM == null)
				{
					return;
				}
                /*else
                {
					m_nextBGM = null;
                }*///필요없는 로직인것 같아서 삭제합니다.
            }
            else if (audio == m_nextBGM)
            {
                return;
            }

            if (m_nextBGM != null)
            {
                if (m_currentBGM != null)
                    m_currentBGM.Stop();

                m_currentBGM = m_nextBGM;
            }

            m_nextBGM = audio;
            m_nextBGM.volume = 0.0f;
            m_nextBGM.Play();
        }
    }

    public void StopBGM()
    {
        if (m_currentBGM != null)
        {   
            m_currentBGM.Stop();
            m_currentBGM = null;
        }

        if (m_nextBGM != null)
        {
            m_nextBGM.Stop();
            m_nextBGM = null;
        }
    }

    public void PauseBGM()
    {
        if (m_currentBGM != null)
        {
            m_currentBGM.Pause();
        }

        if (m_nextBGM != null)
        {
            m_nextBGM.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (m_currentBGM != null)
        {
            if (m_currentBGM.isPlaying == false) m_currentBGM.Play();
        }

        if (m_nextBGM != null)
        {
            if (m_nextBGM.isPlaying == false) m_nextBGM.Play();
        }
    }

    public int GetCurrentBGMNameHash()
    {
        if (m_currentBGM == null)
            return 0;

        foreach (KeyValuePair<int, AudioSource> value in m_bgmMap)
        {
            if (value.Value == m_currentBGM)
            {
                return value.Key;
            }
        }

        return 0;
    }

    float m_volumeBGM = 1;
    float m_volumeSound = 1;

    public void SetBGMVolume(float value)
    {
        m_volumeBGM = value;
        if(m_volumeBGM > 1) m_volumeBGM = 1;
        if (m_volumeBGM < 0) m_volumeBGM = 0;
    }

    public void SetSoundVolume(float value)
    {
        m_volumeSound = value;
        if (m_volumeSound > 1) m_volumeSound = 1;
        if (m_volumeSound < 0) m_volumeSound = 0;
    }

    public void BGMVolumeIncrease()
    {
        SetBGMVolume(m_volumeBGM + 0.1f);
    }

    public void BGMVolumeDecrease()
    {
        SetBGMVolume(m_volumeBGM - 0.1f);
    }

    public void SoundVolumeIncrease()
    {
        SetSoundVolume(m_volumeSound + 0.1f);
    }

    public void SoundVolumeDecrease()
    {
        SetSoundVolume(m_volumeSound - 0.1f);
    }
}