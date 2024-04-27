using System.Collections;
using System.Collections.Generic;
using System.Media;
using TMPro;
using UnityEngine;

public class selfPlay : MonoBehaviour
{
    winCheck wincheck;

    //跟随鼠标的视觉物体
    public Transform chaMouse;
    public Transform quanMouse;

    public Material quanMat;
    public Material chaMat;
    public Material empty;

    float timeLast_Quan;
    float timeLast_Cha;
    bool isTimeLast = false;

    public TextMeshProUGUI timeQuanText;
    public TextMeshProUGUI timeChaText;

    // Start is called before the first frame update
    void Start()
    {
        wincheck = GameObject.Find("winCheck").GetComponent<winCheck>();
        isTimeLast = false;
        timeQuanText.text = "圈剩余：\n∞秒";
        timeChaText.text = "叉剩余：\n∞秒";
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimeLast)
        {

            //是时间限制玩法
            if(wincheck.state == States.Quan)
            {
                timeLast_Quan -= Time.deltaTime;
            }
            if(wincheck.state == States.Cha)
            {
                timeLast_Cha -= Time.deltaTime;
            }
            if(timeLast_Quan <= 0) 
            {
                timeLast_Quan = 0;
                wincheck.state = States.QuanTimeOut;
                wincheck.soundPlayer.Play_GameOverChessSound();
                isTimeLast = false;
            }
            if (timeLast_Cha <= 0)
            {
                timeLast_Cha = 0;
                wincheck.state = States.ChaTimeOut;
                wincheck.soundPlayer.Play_GameOverChessSound();
                isTimeLast = false;
            }
            timeQuanText.text = "圈剩余：\n" + timeLast_Quan.ToString("#0.00") + "秒";
            timeChaText.text = "叉剩余：\n" + timeLast_Cha.ToString("#0.00") + "秒";
        }
        if(wincheck.state == States.Quan)
        {
            //开始了，物体始终跟随鼠标
            //圈的回合
            chaMouse.gameObject.SetActive(false);
            quanMouse.gameObject.SetActive(true);
            Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            quanMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
            putChess_Click(1, quanMat);
        }
        if (wincheck.state == States.Cha)
        {
            //开始了，物体始终跟随鼠标
            //圈的回合
            chaMouse.gameObject.SetActive(true);
            quanMouse.gameObject.SetActive(false);
            Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            chaMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
            putChess_Click(-1, chaMat);
        }
        if(wincheck.state != States.Quan && wincheck.state != States.Cha)
        {
            quanMouse.gameObject.SetActive(false);
            chaMouse.gameObject.SetActive(false);
        }
    }

    public void Button_Reset()
    {
        ///重置棋盘、棋子和状态，重新开始
        wincheck.state = States.Quan;
        Data.ResetMap();

        for (int i = 0; i < 9; i++)
        {
            GameObject.Find(i.ToString()).GetComponent<Renderer>().material = empty;
            if (i <= 7)
            {
                wincheck.connectBars[i].SetActive(false);
            }
        }
        timeQuanText.text = "圈剩余：\n∞秒";
        timeChaText.text = "叉剩余：\n∞秒";
        isTimeLast = false;
    }
    /// <summary>
    /// 重置，并且设置时间限制为5s
    /// </summary>
    public void Button_Reset_5s()
    {
        ///重置棋盘、棋子和状态，重新开始
        wincheck.state = States.Quan;
        Data.ResetMap();

        for (int i = 0; i < 9; i++)
        {
            GameObject.Find(i.ToString()).GetComponent<Renderer>().material = empty;
            if (i <= 7)
            {
                wincheck.connectBars[i].SetActive(false);
            }
        }
        isTimeLast = true;
        timeLast_Quan = 5f;
        timeLast_Cha = 5f;
    }
    /// <summary>
    /// 重置，并且设置时间限制为3s
    /// </summary>
    public void Button_Reset_3s()
    {
        ///重置棋盘、棋子和状态，重新开始
        wincheck.state = States.Quan;
        Data.ResetMap();

        for (int i = 0; i < 9; i++)
        {
            GameObject.Find(i.ToString()).GetComponent<Renderer>().material = empty;
            if (i <= 7)
            {
                wincheck.connectBars[i].SetActive(false);
            }
        }
        isTimeLast = true;
        timeLast_Quan = 3f;
        timeLast_Cha = 3f;
    }
    void putChess_Click(int chess, Material mat)
    {
        if (Input.GetMouseButtonUp(0))
        {
            //按下左键，射一射线看打到哪个格子
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (wincheck.PutChess(chess, int.Parse(hit.collider.name)))
                {
                    //下了一个字
                    hit.collider.GetComponent<Renderer>().material = mat;
                    //wincheck.soundPlayer.Play_PutChessSound();
                }
            }
        }
    }
}
