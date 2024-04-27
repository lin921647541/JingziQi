using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class simpleRenji : MonoBehaviour
{
    winCheck wincheck;

    //���������Ӿ�����
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

    [SerializeField]
    bool isQuan = true;
    // Start is called before the first frame update
    void Start()
    {
        wincheck = GameObject.Find("winCheck").GetComponent<winCheck>();
        isTimeLast = false;
        timeQuanText.text = "Ȧʣ�ࣺ\n����";
        timeChaText.text = "��ʣ�ࣺ\n����"; 
        int rd = Random.Range(0, 2);        //���� [0, 2) ���������
        isQuan = rd == 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTimeLast)
        {

            //��ʱ�������淨
            if (wincheck.state == States.Quan)
            {
                timeLast_Quan -= Time.deltaTime;
            }
            if (wincheck.state == States.Cha)
            {
                timeLast_Cha -= Time.deltaTime;
            }
            if (timeLast_Quan <= 0)
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
            timeQuanText.text = "Ȧʣ�ࣺ\n" + timeLast_Quan.ToString("#0.00") + "��";
            timeChaText.text = "��ʣ�ࣺ\n" + timeLast_Cha.ToString("#0.00") + "��";
        }
        if (wincheck.state == States.Quan)
        {
            //��ʼ�ˣ�����ʼ�ո������
            //Ȧ�Ļغ�
            if(isQuan)
            {
                //��������ȷʵ���õ�Ȧ
                chaMouse.gameObject.SetActive(false);
                quanMouse.gameObject.SetActive(true);
                Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                quanMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
                putChess_Click(1, quanMat);
            }
            else
            {
                //�������ӡ����˻����������
                putChess_Random(1, quanMat);
            }
        }
        if (wincheck.state == States.Cha)
        {
            //��ʼ�ˣ�����ʼ�ո������
            //��Ļغ�
            if (isQuan)
            {
                //���������õ���Ȧ����ʱӦ�û�������
                putChess_Random(-1, chaMat);
            }
            else
            {
                //���ǵĻغ�
                chaMouse.gameObject.SetActive(true);
                quanMouse.gameObject.SetActive(false);
                Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                chaMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
                putChess_Click(-1, chaMat);
            }
        }
        if (wincheck.state != States.Quan && wincheck.state != States.Cha)
        {
            quanMouse.gameObject.SetActive(false);
            chaMouse.gameObject.SetActive(false);
        }
    }

    public void Button_Reset()
    {
        ///�������̡����Ӻ�״̬�����¿�ʼ
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
        timeQuanText.text = "Ȧʣ�ࣺ\n����";
        timeChaText.text = "��ʣ�ࣺ\n����";
        isTimeLast = false;
        int rd = Random.Range(0, 2);        //���� [0, 4) ���������
        isQuan = rd == 0;
    }
    /// <summary>
    /// ���ã���������ʱ������Ϊ5s
    /// </summary>
    public void Button_Reset_5s()
    {
        ///�������̡����Ӻ�״̬�����¿�ʼ
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
        int rd = Random.Range(0, 2);        //���� [0, 2) ���������
        isQuan = rd == 0;
    }
    /// <summary>
    /// ���ã���������ʱ������Ϊ3s
    /// </summary>
    public void Button_Reset_3s()
    {
        ///�������̡����Ӻ�״̬�����¿�ʼ
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
        int rd = Random.Range(0, 2);        //���� [0, 2) ���������
        isQuan = rd == 0;
    }
    void putChess_Click(int chess, Material mat)
    {
        if (Input.GetMouseButton(0))
        {
            //�����������һ���߿����ĸ�����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (wincheck.PutChess(chess, int.Parse(hit.collider.name)))
                {
                    //����һ����
                    hit.collider.GetComponent<Renderer>().material = mat;
                }
            }
        }
    }
    void putChess_Random(int chess, Material mat)
    {
        List<int> noChessPos = new List<int>();
        //�������п�����λ��
        int index = 0;
        for(int i = 0; i < 3; i++) 
        {
            for(int j = 0; j < 3; j++)
            {
                if (Data.Map[j, i] == 0)
                {
                    noChessPos.Add(index);
                }
                index++;
            }
        }
        //�����ȡһ������
        int rd = Random.Range(0, noChessPos.Count);
        //�ڴ˴�����
        wincheck.PutChess(chess, noChessPos[rd]);
        //�޸Ĵ���������Ĳ���
        GameObject.Find(noChessPos[rd].ToString()).GetComponent<Renderer>().material = mat;
    }
}
