using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonsFunc_main : MonoBehaviour
{
    BGMPlayer bgmp;
    public Slider slider_bgm;
    public Slider slider_sound;
    public GameObject collerPanel;
    private void Start()
    {
        bgmp = GameObject.Find("音量控制").GetComponent<BGMPlayer>();
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            if(SceneManager.GetActiveScene().buildIndex == 0)
            {
                //在主场景，退出
                Button_Esc();
            }
            else
            {
                //不在主场景，上退到主场景
                Button_mainScene();
            }
        }

    }
    public void Button_mainScene()
    {
        SceneManager.LoadScene(0);
    }
    public void Button_selfPlay()
    {
        SceneManager.LoadScene(1);
    }
    public void Button_simpleRenji()
    {
        SceneManager.LoadScene(2);
    }
    public void Button_hardRenji()
    {
        SceneManager.LoadScene(3);
    }
    public void Button_Esc()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    public void On_Slider_Changed_BGM()
    {
        bgmp.bgmAS.volume = (float)slider_bgm.value / 100f;
    }

    public void On_Slider_Changed_Sound()
    {
        bgmp.putChessAs.volume = (float)slider_sound.value / 100f;
    }
    public void Button_volumeControl()
    {
        if (collerPanel.activeSelf)
        {
            collerPanel.SetActive(false);
        }
        else
        {
            collerPanel.SetActive(true);
        }
    }
}
