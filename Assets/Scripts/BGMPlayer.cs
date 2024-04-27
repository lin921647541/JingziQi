using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGMPlayer : MonoBehaviour
{
    public AudioSource bgmAS;
    public AudioSource putChessAs;
    public GameObject collerPanel;
    public AudioClip putChess_AudioClip;
    public AudioClip gameOver_AudioClip;
    public AudioClip wrongChess_AudioClip;

    static BGMPlayer _instance;
    public static BGMPlayer instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<BGMPlayer>();
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
    private void Awake()
    {
        //�˽ű��������٣�����ÿ�ν����ʼ����ʱ�����жϣ��������ظ���������
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else if (this != _instance)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        bgmAS = GetComponent<AudioSource>();
        putChess_AudioClip = Resources.Load<AudioClip>("Sounds/putChess");
        gameOver_AudioClip = Resources.Load<AudioClip>("Sounds/gameOver");
        wrongChess_AudioClip = Resources.Load<AudioClip>("Sounds/wrongChess");

        //putChessAs = GameObject.Find("putChessAS").GetComponent<AudioSource>();
        //bgmVolume = GameObject.Find("����").GetComponent<Slider>();
        //putChessVolume = GameObject.Find("��Ч").GetComponent<Slider>();
    }

    public void Play_PutChessSound()
    {
        if (putChessAs.isPlaying)
        {
            putChessAs.Stop();
        }
        putChessAs.clip = putChess_AudioClip;
        putChessAs.Play();
    }
    public void Play_WrongChessSound()
    {
        if (putChessAs.isPlaying)
        {
            putChessAs.Stop();
        }
        putChessAs.clip = wrongChess_AudioClip;
        putChessAs.Play();
        Debug.Log("1111111111111111");
    }
    public void Play_GameOverChessSound()
    {
        if (putChessAs.isPlaying)
        {
            putChessAs.Stop();
        }
        putChessAs.clip = gameOver_AudioClip;
        putChessAs.Play();
    }
}
