using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum States
{
    Ready,
    Quan,
    Cha,
    QuanWin,
    ChaWin,
    He,             //����
    QuanTimeOut,
    ChaTimeOut,
    PlayerTurn,
    AITurn
}

public static class Data
{
    private static int[,] map = new int[3, 3];
    public static int[,] Map { get => map; set => map = value; }

    public static Dictionary<int, Vector2> pos = new Dictionary<int, Vector2>();

    static bool isSettedDic = false;

    public static void ResetMap()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                map[i, j] = 0;
            }
        }
    }
    /// <summary>
    /// ��0~8ӳ�䵽xy����
    /// </summary>
    /// 0   1   2
    /// 3   4   5
    /// 6   7   8
    public static void ResetDictionary()
    {
        if(!isSettedDic)
        {
            int key = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    pos.Add(key, new Vector2(j, i));
                    key++;
                }
            }
            isSettedDic = true;
        }
    }
}

public class winCheck : MonoBehaviour
{
    public States state;

    public TextMeshProUGUI stateText;

    public GameObject[] connectBars;

    Dictionary<States, string> stateString_Dic;

    public BGMPlayer soundPlayer;
    // Start is called before the first frame update
    void Start()
    {
        state = States.Quan;
        Data.ResetMap();
        Data.ResetDictionary();

        stateString_Dic = new Dictionary<States, string>();
        stateString_Dic.Add(States.Quan, "Ȧ�غ�");
        stateString_Dic.Add(States.Cha, "��غ�");
        stateString_Dic.Add(States.QuanWin, "Ȧʤ����");
        stateString_Dic.Add(States.ChaWin, "��ʤ����");
        stateString_Dic.Add(States.He, "����");
        stateString_Dic.Add(States.QuanTimeOut, "Ȧ��ʱ��");
        stateString_Dic.Add(States.ChaTimeOut, "�泬ʱ��");

        soundPlayer = GameObject.Find("��������").GetComponent<BGMPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        stateText.text = stateString_Dic[state]; 
    }

    bool Check_Win(int i, int j, int chess)
    {
        int num = 0;
        //����
        for(int k = 0; k < 3; k++)
        {
            if (Data.Map[k, j] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) { connectBars[j].SetActive(true); return true; }
        num = 0;
        //������
        for (int k = 0; k < 3; k++)
        {
            if (Data.Map[i, k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) { connectBars[i + 3].SetActive(true); return true; }
        num = 0;
        //б��
        for (int k = 0; k < 3; k++)
        {
            if (Data.Map[k, k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) { connectBars[6].SetActive(true); return true; }
        num = 0;
        for (int k = 0; k < 3; k++)
        {
            if (Data.Map[k, 2 - k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) { connectBars[7].SetActive(true); return true; }
        return false;
    }
    public bool PutChess(int chess, int posIndex)
    {
        int i = (int)Data.pos[posIndex].x;
        int j = (int)Data.pos[posIndex].y;
        return PutChess(chess, i, j);
    }
    public bool PutChess(int chess, int i, int j)
    {
        if(Data.Map[i, j] != 0)
        {
            //soundPlayer.Play_WrongChessSound();
            return false;
        }
        Data.Map[i, j] = chess;
        if(Check_Win(i, j, chess))
        {
            state = chess == 1 ? States.QuanWin : States.ChaWin;
        }
        else
        {
            state = chess == 1 ? States.Cha : States.Quan;
            int k = 0;
            for(int u = 0; u < 3; u++)
            {
                for(int o = 0; o < 3; o++)
                {
                    if (Data.Map[u, o] != 0)
                    {
                        k++;
                    }
                }
            }
            if(k == 9)
            {
                //����������Ҳû�г���ʤ��
                state = States.He;
            }
        }
        if(state == States.He || state == States.QuanWin || state == States.ChaWin)
        { //��Ϸ��������Ч
            soundPlayer.Play_GameOverChessSound();
        }
        else
        {//������Ч
            soundPlayer.Play_PutChessSound();
        }
        return true;
    }
}
