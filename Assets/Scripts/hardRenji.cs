using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

//������״̬ö��
public enum TreeState
{
    NoTree,         //û��������
    CreateStart,         //������
    CreateEnd,
    SearchStart,         //������
    End             //�������
}

//ÿһ�ŵĵ�ǰ����
public struct ValueOfPos
{
    public int value;
    public int posi;
    public int posj;
}

//���������
public class GamblingTree
{
    public GamblingTree[] child;
    public GamblingTree father;
    public int depth;
    public int posi;
    public int posj;
    public int chess;
    public int solve;
    public int score;
    public int fatherI;
    public int winChess;
}

//ScorePos��Ƚ�
class ValueOfPosComparer : IComparer<ValueOfPos>
{          //�ӿڣ�����ֱ����sort��ֻ��ʵ������ſ���ֱ����sort
    public int Compare(ValueOfPos x, ValueOfPos y)
    {
        if (x.value < y.value) return 1;    //�Ӵ�С����
        else if (x.value > y.value) return -1;
        else return 0;
    }
}

/// <summary>
/// �������˻������˻�ҲҪ������
/// </summary>
public class hardRenji : MonoBehaviour
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
    public TextMeshProUGUI resetMap_Times;
    public TextMeshProUGUI putChessSkill_Times;

    public GameObject skillReadingMe;
    public GameObject skill_Putchess;

    [SerializeField]
    bool isQuan = true;
    [SerializeField]
    TreeState treeState;                //������״̬

    private int width = 5;  //ÿ��������չ�Ŀ��
    private int depth = 7;  //�ܲ�����������������

    private int aiAnsPosi, aiAnsPosj;                //�����Ĵ�����λ��
    ValueOfPosComparer compare;   //�ȽϹ���
    private GamblingTree root;              //�������ĸ��ڵ�
    bool keyStep = false;           //�ؼ�������Ҫ��˼��̫��
    [SerializeField]
    int AIchess = -1;        //1����Ȧ����������,-1�Ǻ���

    int chessNum = 0;
    public int resetMapMaxChessNum = 3;
    int resetMapUseageNum = 3;
    int putChessUseageNum = 1;
    bool isSkillPutChess = false;

    BGMPlayer soundPlayer;
    // Start is called before the first frame update
    void Start()
    {
        soundPlayer = GameObject.Find("��������").GetComponent<BGMPlayer>();

        wincheck = GameObject.Find("winCheck").GetComponent<winCheck>();
        isTimeLast = false;
        timeQuanText.text = "Ȧʣ�ࣺ\n����";
        timeChaText.text = "��ʣ�ࣺ\n����";
        resetMap_Times.text = "����-" + resetMapUseageNum.ToString();
        putChessSkill_Times.text = "����-" + putChessUseageNum.ToString();
        int rd = UnityEngine.Random.Range(0, 2);        //���� [0, 2) ���������
        isQuan = rd == 0;
        AIchess = isQuan ? -1 : 1;              //������Ȧ���֣�ai���Ǻ��֣�������-1

        resetMapMaxChessNum = isQuan ? 2 : 5;

        compare = new ValueOfPosComparer();
        aiAnsPosi = aiAnsPosj = 0;              //��һ����ʼֵ
    }

    // Update is called once per frame
    void Update()
    {
        //UI
        resetMap_Times.text = "����-" + resetMapUseageNum.ToString();
        putChessSkill_Times.text = "����-" + putChessUseageNum.ToString();
        if(isQuan)
        {
            //���Ӽ��ܰ�ť�÷�
            skill_Putchess.SetActive(false);
        }
        else {
            skill_Putchess.SetActive(true);
        }
        //ά����ǰmap����������
        int tmpChessNum = 0;
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                if (Data.Map[i, j] != 0)
                {
                    tmpChessNum++;
                }
            }
        }
        chessNum = tmpChessNum;
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
            if (isQuan)
            {
                //��������ȷʵ���õ�Ȧ
                chaMouse.gameObject.SetActive(false);
                quanMouse.gameObject.SetActive(true);
                Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                quanMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
                if(isSkillPutChess)
                {
                    putChess_Skill(1, quanMat);
                }
                else
                {
                    putChess_Click(1, quanMat);
                }
            }
            else
            {
                AIChess(quanMat);
            }
        }
        if (wincheck.state == States.Cha)
        {
            //��ʼ�ˣ�����ʼ�ո������
            //��Ļغ�
            if (isQuan)
            {
                //���������õ���Ȧ����ʱӦ�û�������
                AIChess(chaMat);
            }
            else
            {
                //���ǵĻغ�
                chaMouse.gameObject.SetActive(true);
                quanMouse.gameObject.SetActive(false);
                Vector3 mouse2world = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                chaMouse.transform.position = new Vector3(mouse2world.x, 1, mouse2world.z);
                if (isSkillPutChess)
                {
                    putChess_Skill(-1, chaMat);
                }
                else
                {
                    putChess_Click(-1, chaMat);
                }
            }
        }
        if (wincheck.state != States.Quan && wincheck.state != States.Cha)
        {
            quanMouse.gameObject.SetActive(false);
            chaMouse.gameObject.SetActive(false);
        }
    }

    void AIChess(Material mat)
    {
        //�������ӡ�ʹ�����ֲ���������ʵ����chess����1��
        if (treeState == TreeState.NoTree)
        {
            //���ɲ�����
            root = new GamblingTree();
            root.chess = -AIchess;
            root.depth = 1;
            root.winChess = 0;
            treeState = TreeState.CreateStart;
            Debug.Log("2222");
            CreateGamblingTree(Data.Map, root);
            //Debug.Log(root.child.Length);
            if (keyStep)
            {
                //�ǹؼ���������Ҫ�������ˣ�ֱ������
                treeState = TreeState.End;
                Debug.Log("33332");
                keyStep = false;          //����ؼ���
            }
            else
            {
                Debug.Log("4444");
                treeState = TreeState.CreateEnd;
            }
        }
        //���������ʱ��״̬����createEnd
        if (treeState == TreeState.CreateEnd)
        {
            //����
            Debug.Log(root.child.Length);
            treeState = TreeState.SearchStart;
            SearchABTree(root);
        }
        if (treeState == TreeState.End)
        {
            //�Ѿ�������ϣ�Ӧ�ø��º���anserֵ��ֱ���ڴ˵�����
            //�ڴ˴�����
            wincheck.PutChess(AIchess, aiAnsPosi, aiAnsPosj);
            int index = 0;
            Vector2 anser = new Vector2(aiAnsPosi, aiAnsPosj);
            foreach (var pair in Data.pos)
            {
                if (pair.Value == anser)
                {
                    index = pair.Key;
                    break;
                }
            }
            //�޸Ĵ���������Ĳ���
            GameObject.Find(index.ToString()).GetComponent<Renderer>().material = mat;
            //wincheck.state = States.Cha;
            //����״̬������
            treeState = TreeState.NoTree;
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
        int rd = UnityEngine.Random.Range(0, 2);        //���� [0, 2) ���������
        isQuan = rd == 0;
        AIchess = isQuan ? -1 : 1;              //������Ȧ���֣�ai���Ǻ��֣�������-1
        resetMapMaxChessNum = isQuan ? 2 : 5;
        resetMapUseageNum = 3;
        putChessUseageNum = 1;
    }
    /// <summary>
    /// ���ã���������ʱ������Ϊ5s
    /// </summary>
    public void Button_Reset_5s()
    {
        ///�������̡����Ӻ�״̬�����¿�ʼ
        Button_Reset();
        isTimeLast = true;
        timeLast_Quan = 5f;
        timeLast_Cha = 5f;
    }
    /// <summary>
    /// ���ã���������ʱ������Ϊ3s
    /// </summary>
    public void Button_Reset_3s()
    {
        ///�������̡����Ӻ�״̬�����¿�ʼ
        Button_Reset();
        isTimeLast = true;
        timeLast_Quan = 3f;
        timeLast_Cha = 3f;
    }

    public void Button_SkillReadingMe()
    {
        if(skillReadingMe.activeSelf)
        {
            skillReadingMe.SetActive(false);
        }
        else
        {
            skillReadingMe.SetActive(true);
        }
    }
    void putChess_Skill(int chess, Material mat)
    {
        if (Input.GetMouseButton(0))
        {
            //�����������һ���߿����ĸ�����
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                int putPos = int.Parse(hit.collider.name);
                int putPos_i = (int)Data.pos[putPos].x;
                int putPos_j = (int)Data.pos[putPos].y;

                int previousChess = Data.Map[putPos_i, putPos_j];
                Data.Map[putPos_i, putPos_j] = chess;

                if(CheckHasN(putPos_i, putPos_j, chess, Data.Map, 3))
                {
                    //�����ʤ��������
                    Data.Map[putPos_i, putPos_j] = previousChess;
                    soundPlayer.Play_WrongChessSound();
                }
                else
                {
                    hit.collider.GetComponent<Renderer>().material = mat;
                    wincheck.state = chess == 1 ? States.Cha : States.Quan;
                    isSkillPutChess = false;
                    soundPlayer.Play_PutChessSound();
                }
            }
        }
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

    public void Button_Skill_ResetMap()
    {
        //������N���ӵ�����������������̵�����
        if(chessNum > resetMapMaxChessNum)
        {
            soundPlayer.Play_WrongChessSound();
            return;
        }
        if(resetMapUseageNum > 0)
        {
            int quanChess = 0;
            int chaChess = 0;
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < 3; j++)
                {
                    if (Data.Map[i, j] == 1) quanChess++;
                    if (Data.Map[i, j] == -1) chaChess++;
                    Data.Map[i, j] = 0;
                }
            }
            //ѭ���޸�Data.mapֱ��һ����ʤ����map
            //ֱ���޸�data,map���У����ñ���
            while (true)
            {
                //���������Ȧ
                for(int  i = 0; i < quanChess; i++)
                {
                    while (true)
                    {
                        int rdm = UnityEngine.Random.Range(0, 9);
                        if (Data.Map[(int)Data.pos[rdm].x, (int)Data.pos[rdm].y] == 0)
                        {
                            Data.Map[(int)Data.pos[rdm].x, (int)Data.pos[rdm].y] = 1;
                            break;
                        }
                    }
                }
                //��������ò�
                for (int i = 0; i < chaChess; i++)
                {
                    while (true)
                    {
                        int rdm = UnityEngine.Random.Range(0, 9);
                        if (Data.Map[(int)Data.pos[rdm].x, (int)Data.pos[rdm].y] == 0)
                        {
                            Data.Map[(int)Data.pos[rdm].x, (int)Data.pos[rdm].y] = -1;
                            break;
                        }
                    }
                }
                //����Ƿ�ʤ��
                if(Check_ThreeChessNum(Data.Map, 1) == 0 && Check_ThreeChessNum(Data.Map, -1) == 0)
                {
                    //ֻҪ����������������0
                    break;
                }
                //������Ҫ����
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Data.Map[i, j] = 0;
                    }
                }
            }

            //�����޸ĺ��Data.map�����޸��������
            for(int i = 0; i < 9; i++)
            {
                GameObject go = GameObject.Find(i.ToString());
                if (Data.Map[(int)Data.pos[i].x, (int)Data.pos[i].y] == 0){
                    go.GetComponent<Renderer>().material = empty;
                }
                else if (Data.Map[(int)Data.pos[i].x, (int)Data.pos[i].y] == 1)
                {
                    go.GetComponent<Renderer>().material = quanMat;
                }
                else if (Data.Map[(int)Data.pos[i].x, (int)Data.pos[i].y] == -1)
                {
                    go.GetComponent<Renderer>().material = chaMat;
                }
            }
            resetMapUseageNum--;
            soundPlayer.Play_GameOverChessSound();
        }
    }

    public void Button_Skill_StartPutChess()
    {
        //��ʼǿ�Ʒ������ӣ�����һ�Ų���ֱ�ӻ�ʤ
        if(putChessUseageNum > 0)
        {
            isSkillPutChess = true;

            putChessUseageNum -= 1;
        }
    }
    /// <summary>
    /// ���ɲ�����
    /// </summary>
    /// <param name="map"></param>
    /// <param name="father"></param>
    //�����������ɡ�ab��֦����Ҫ�õ�һϵ�к���
    private void CreateGamblingTree(int[,] map, GamblingTree father)
    {
        ValueOfPos[] val = new ValueOfPos[9];                     //ÿ��λ�����ӵļ�ֵ
        int nullPos = 0;
        //ɸѡǰ�����߼�ֵ�����ӵ�
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j] == 0)
                {
                    //val[i * 3 + j].value = GetValue(i, j, map);
                    val[i * 3 + j].value = 1;       //�ѿ��ӵ�ͨ�������ǰ��ȥ
                    val[i * 3 + j].posi = i;
                    val[i * 3 + j].posj = j;
                    nullPos++;
                }
            }
        }
        Array.Sort(val, compare);
        //ͨ���ؼ����жϿ�����ǰ�����������������Ĺ���
        //������û��
        if (father.depth == 1)
        {
            //��������˴���6666��val������Ҫ�������������������ҪôӮҪô��
            /*
            if (val[0].value >= 6666)
            {
                keyStep = true;
                aiAnsPosi = val[0].posi;
                aiAnsPosj = val[0].posj;
                return;
            }*/
        }
        father.child = new GamblingTree[nullPos]; 
        for (int i = 0; i < nullPos; i++)
        {
            GamblingTree point = new GamblingTree();

            father.child[i] = point;        //���ӹ�ϵ
            point.father = father;
            point.depth = father.depth + 1; //���������
            point.posi = val[i].posi;        //����λ��
            point.posj = val[i].posj;
            point.chess = -father.chess;      //��������

            //��չ�µ�ͼ�������ͼmap1����point�ڵ����۲⵽�����̣���Ҫ��һ��������
            int[,] map1 = new int[3, 3];
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    map1[j, k] = map[j, k];
                }
            }
            map1[point.posi, point.posj] = point.chess;                 //��������

            //�����ʱ�Ѿ����˻�ʤ��
            if (CheckHasN(point.posi, point.posj, point.chess, map1, 3))
            {
                //��ʤ������point.chess
                if (point.chess != AIchess)
                {
                    //��Ӯ��
                    point.score = -999999;            //������������С

                }
                else
                {
                    //AIӮ��
                    point.score = 999999;             //�������������
                }
                //ֻҪ����Ӯ�ˣ��Ͳ��ټ���������
                continue;
            }
            else if(CheckHe(map1))
            {
                //�����ˣ�������������
                point.score = 0;
                continue;
            }
            //�Ծ���Ȼ�����У�����������
            //�������������ķ�����ָ��AI���ӵľ���������
            //�ڰ�����������֦�㷨�ֻ��Ҷ�ӽڵ㲻��Ҫ����abֵ��ȡ����֮������Ҫ����Ҷ�ӵľ��Ʒ���
            if (point.depth == depth)
            {
                //�ﵽ�˹涨������ڵ㣬������Ʒ���
                point.score = GetLeafScore_V2(map1, AIchess);
            }
            //���½������                                      //����¼�����������ĸ��ӵõ��ģ���Ϊֻ����1��
            if (point.depth == 2)
            { //�ڵ������Ľ������
                point.solve = i;
                point.fatherI = i;
            }
            else
            {
                point.solve = father.solve;     //�Ǹ��ڵ�ĵڼ���������Ľڵ�
                point.fatherI = i;                //�Ǹ��׵ĵڼ������ӣ��ƺ�û�õ�����������ɾ��
            }
            //û�дﵽ�����ȣ��ͼ�������
            if (point.depth < depth)
            {
                CreateGamblingTree(map1, point);                                  //�ݹ�˹���
            }
        }

    }
    /// <summary>
    /// ��ȡ������Ҷ�ӽڵ�ľ��Ʒ���
    /// ��ȡ�ķ����㷨�ǣ�
    /// fn = ��λȫ���Լ��������ɵ�3������ - ��λȫ�ŵз������ɵĵз�3������
    /// �㷨��Դ��һ��Bվ�̵̳Ľ���
    /// </summary>
    /// <param name="map1">����</param>
    /// <param name="chess">������ĸ����Ӷ��Եľ��Ʒ���</param>
    /// <returns></returns>
    int GetLeafScore_V2(int[,] map1, int chess)
    {
        int value = 0;
        int[,] map_aichess = (int[,])map1.Clone();
        //��λȫ��chess���м�������
        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < 3; j++)
            {
                if (map_aichess[i, j] == 0)
                {
                    map_aichess[i, j] = chess;
                }
            }
        }
        int chessScore = Check_ThreeChessNum(map_aichess, chess);
        int[,] map_humanChess = (int[,])map1.Clone();
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map_humanChess[i, j] == 0)
                {
                    map_humanChess[i, j] = -chess;
                }
            }
        }
        int chessHumanScore = Check_ThreeChessNum(map_aichess, -chess);
        value = chessScore - chessHumanScore;
        return value;
    }
    /// <summary>
    /// ������ǰ�����ڣ�����3�ߵ������ж���
    /// </summary>
    /// <param name="map">���̣�Ӧ�����޿�λ��</param>
    /// <param name="chess">Ҫ�����ĸ���������</param>
    /// <returns></returns>
    int Check_ThreeChessNum(int[,] map, int chess)
    {
        int threeChessNum = 0;
        int num = 0;
        //������
        for (int i = 0; i < 3; i ++)
        {
            for(int j = 0; j < 3; j++)
            {
                if (map[i, j] != chess) break;
                num++;
            }
            if (num == 3) threeChessNum++;
            num = 0;
        }
        num = 0;
        //������
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[j, i] != chess) break;
                num++;
            }
            if (num == 3) threeChessNum++;
            num = 0;
        }
        num = 0;
        //б��
        for (int k = 0; k < 3; k++)
        {
            if (map[k, k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) threeChessNum++;
        num = 0;
        for (int k = 0; k < 3; k++)
        {
            if (map[k, 2 - k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == 3) threeChessNum++;

        return threeChessNum;
    }
    /// <summary>
    /// 
    ///���Ӽ�ֵ��ȡ
    ///�����巶Χ���ޣ�����Ҫɸѡ�߼�ֵ��λ���˺���δ����
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private int GetValue(int i, int j, int[,] map)
    {
        int val = 66;       //ֻҪΪ���ӣ����г�ʼ��ֵ
        //�жϸõ������Ӽ�ֵ
        //���ȿ������֮��ɲ����Ի�ʤ
        int chess = AIchess;
        map[i, j] = chess;
        if (CheckHasN(i, j, chess, map, 3))
        {
            //���ӻ�ʤ�ˣ�ֱ�ӷ��ؼ�������Ӽ�ֵ
            val += 999999;
        }
        //�ٿ������֮����ӻ᲻���ʤ������ᣬ��ô��Ҫ��
        map[i, j] = -chess;
        if (CheckHasN(i, j, chess, map, 3))
        {
            //���ӻ�ʤ�ˣ����شδ����Ӽ�ֵ
            val += 888888;
        }
        map[i, j] = 0;
        return val;
    }
    
    bool CheckHasN(int i, int j, int chess, int[,] map, int N)
    {
        int num = 0;
        //����
        for (int k = 0; k < 3; k++)
        {
            if (map[k, j] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == N) { return true; }
        num = 0;
        //������
        for (int k = 0; k < 3; k++)
        {
            if (map[i, k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == N) { return true; }
        num = 0;
        //б��
        for (int k = 0; k < 3; k++)
        {
            if (map[k, k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == N) { return true; }
        num = 0;
        for (int k = 0; k < 3; k++)
        {
            if (map[k, 2 - k] == chess)
            {
                num++;
            }
            else break;
        }
        if (num == N) { return true; }
        return false;
    }
    bool CheckHe(int[,] map)
    {
        for(int i = 0; i < 3; i ++)
        {
            for(int j = 0; j < 3; j++)
            {
                if (map[i, j] == 0)
                {
                    //ֻҪ��һ����λ�ͻ�û����
                    return false;
                }
            }
        }
        return true;
    }
    
    /// <summary>
    /// alpha-beta��֦����������
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private int SearchABTree(GamblingTree p)
    {
        int solution = 0;
        int ansScore = MaxValue(root, -999999, 999999);             //ab��ʼֵ�ֱ�Ϊ�������������
        //���ǿ��Ի��һ�����ŵ�alphaֵ
        //ȥ��һ�㺢����������˭�ķ���==����𰸣��������ķ�֧
        Debug.Log("ansScore = = = " + ansScore);
        for (int i = 0; i < root.child.Length; i++)
        {
            Debug.Log("child--- " + i + "��score === " + root.child[i].score);
            if (root.child[i].score == ansScore)
            {
                solution = i;
                break;
            }
        }
        aiAnsPosi = root.child[solution].posi;
        aiAnsPosj = root.child[solution].posj;
        treeState = TreeState.End;
        return solution;

    }
    private int MaxValue(GamblingTree p, int alpha, int beta)
    {
        // if(p.depth==depth){
        //     return p.score;
        // }
        //����ֵ��Ѱ����ǰ�ڵ���Max�㡣
        //Max����������bֵ��ߵ�һ����Ϊ�Լ��ķ���
        //��Ϊ���˽ڵ㣬�����ܻ��x�����桱
        //����Ϊ������Min�㣬ֻ�ı���b��

        //p�Ѿ�û�к����ˣ���Ҷ�ӡ�
        //Ҷ��Ҫ������ľ��Ʒ���ȥ�͸��׵�abֵ���Ƚ�
        //���Է���������Ʒ���
        if (p.child == null)
        {
            return p.score;
        }
        //�������Ҷ�ӣ�������̽Ѱ�Ĺ��̡�Max��ֻ��alpha����ʼ��������
        p.score = -999999;
        //����̽Ѱ���к���
        for (int i = 0; i < p.child.Length; i++)
        {
            //̽Ѱ�������bֵ�Ƕ���
            //����Bֵ������Ҫ̽Ѱ���ӵ�����aֵ����Сֵ
            int tempMax = MinValue(p.child[i], alpha, beta);
            //Max���scoreֵ����alpha
            p.score = Mathf.Max(tempMax, p.score);
            //a>b
            //��֦��ֱ�ӷ���alphaֵ
            if (p.score >= beta)
            {
                return p.score;
            }
            //��һ��ֻ��Ϊ��ʹ�ô���ɶ�����ǿ��ʵ����
            //int tempMax = MinValue(p.child[i], alpha, beta);
            //��ȫ���Ըĳ�
            //int tempMax = MinValue(p.child[i], p.score, beta);
            alpha = Mathf.Max(alpha, p.score);
        }
        //���صľ���alphaֵ
        return p.score;                                             //�����
    }

    private int MinValue(GamblingTree p, int alpha, int beta)
    {
        // if(p.depth==depth){
        //     return p.score;
        // }
        if (p.child == null)
        {
            return p.score;
        }
        p.score = 999999;
        for (int i = 0; i < p.child.Length; i++)
        {
            int tempMin = MaxValue(p.child[i], alpha, beta);
            p.score = Mathf.Min(p.score, tempMin);
            if (p.score <= alpha)
            {
                return p.score;
            }
            beta = Mathf.Min(beta, p.score);
        }
        return p.score;                                         //����С
    }
}
