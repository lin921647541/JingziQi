using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

//博弈树状态枚举
public enum TreeState
{
    NoTree,         //没有树任务
    CreateStart,         //生成中
    CreateEnd,
    SearchStart,         //搜索中
    End             //搜索完毕
}

//每一着的当前评分
public struct ValueOfPos
{
    public int value;
    public int posi;
    public int posj;
}

//博弈树结点
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

//ScorePos类比较
class ValueOfPosComparer : IComparer<ValueOfPos>
{          //接口，方便直接用sort。只有实现这个才可以直接用sort
    public int Compare(ValueOfPos x, ValueOfPos y)
    {
        if (x.value < y.value) return 1;    //从大到小排序
        else if (x.value > y.value) return -1;
        else return 0;
    }
}

/// <summary>
/// 博弈树人机。此人机也要猜先手
/// </summary>
public class hardRenji : MonoBehaviour
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
    public TextMeshProUGUI resetMap_Times;
    public TextMeshProUGUI putChessSkill_Times;

    public GameObject skillReadingMe;
    public GameObject skill_Putchess;

    [SerializeField]
    bool isQuan = true;
    [SerializeField]
    TreeState treeState;                //博弈树状态

    private int width = 5;  //每层向下拓展的宽度
    private int depth = 7;  //总层数（必须是奇数）

    private int aiAnsPosi, aiAnsPosj;                //搜索的答案落子位置
    ValueOfPosComparer compare;   //比较规则
    private GamblingTree root;              //博弈树的根节点
    bool keyStep = false;           //关键步则不需要再思考太多
    [SerializeField]
    int AIchess = -1;        //1都是圈儿，是先手,-1是后手

    int chessNum = 0;
    public int resetMapMaxChessNum = 3;
    int resetMapUseageNum = 3;
    int putChessUseageNum = 1;
    bool isSkillPutChess = false;

    BGMPlayer soundPlayer;
    // Start is called before the first frame update
    void Start()
    {
        soundPlayer = GameObject.Find("音量控制").GetComponent<BGMPlayer>();

        wincheck = GameObject.Find("winCheck").GetComponent<winCheck>();
        isTimeLast = false;
        timeQuanText.text = "圈剩余：\n∞秒";
        timeChaText.text = "叉剩余：\n∞秒";
        resetMap_Times.text = "打乱-" + resetMapUseageNum.ToString();
        putChessSkill_Times.text = "落子-" + putChessUseageNum.ToString();
        int rd = UnityEngine.Random.Range(0, 2);        //生成 [0, 2) 的随机整数
        isQuan = rd == 0;
        AIchess = isQuan ? -1 : 1;              //人类是圈先手，ai就是后手，棋子是-1

        resetMapMaxChessNum = isQuan ? 2 : 5;

        compare = new ValueOfPosComparer();
        aiAnsPosi = aiAnsPosj = 0;              //赋一个初始值
    }

    // Update is called once per frame
    void Update()
    {
        //UI
        resetMap_Times.text = "打乱-" + resetMapUseageNum.ToString();
        putChessSkill_Times.text = "落子-" + putChessUseageNum.ToString();
        if(isQuan)
        {
            //落子技能按钮置否
            skill_Putchess.SetActive(false);
        }
        else {
            skill_Putchess.SetActive(true);
        }
        //维护当前map里棋子数量
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

            //是时间限制玩法
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
            timeQuanText.text = "圈剩余：\n" + timeLast_Quan.ToString("#0.00") + "秒";
            timeChaText.text = "叉剩余：\n" + timeLast_Cha.ToString("#0.00") + "秒";
        }
        if (wincheck.state == States.Quan)
        {
            //开始了，物体始终跟随鼠标
            //圈的回合
            if (isQuan)
            {
                //并且我们确实是拿的圈
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
            //开始了，物体始终跟随鼠标
            //叉的回合
            if (isQuan)
            {
                //并且我们拿的是圈，这时应该机器落子
                AIChess(chaMat);
            }
            else
            {
                //我们的回合
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
        //机器落子。使用先手博弈树（其实就是chess换成1）
        if (treeState == TreeState.NoTree)
        {
            //生成博弈树
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
                //是关键步，不需要再搜索了，直接落子
                treeState = TreeState.End;
                Debug.Log("33332");
                keyStep = false;          //解除关键步
            }
            else
            {
                Debug.Log("4444");
                treeState = TreeState.CreateEnd;
            }
        }
        //树生成完毕时，状态会变成createEnd
        if (treeState == TreeState.CreateEnd)
        {
            //搜索
            Debug.Log(root.child.Length);
            treeState = TreeState.SearchStart;
            SearchABTree(root);
        }
        if (treeState == TreeState.End)
        {
            //已经搜索完毕，应该更新好了anser值，直接在此点落子
            //在此处落子
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
            //修改此索引物体的材质
            GameObject.Find(index.ToString()).GetComponent<Renderer>().material = mat;
            //wincheck.state = States.Cha;
            //树的状态是无树
            treeState = TreeState.NoTree;
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
        int rd = UnityEngine.Random.Range(0, 2);        //生成 [0, 2) 的随机整数
        isQuan = rd == 0;
        AIchess = isQuan ? -1 : 1;              //人类是圈先手，ai就是后手，棋子是-1
        resetMapMaxChessNum = isQuan ? 2 : 5;
        resetMapUseageNum = 3;
        putChessUseageNum = 1;
    }
    /// <summary>
    /// 重置，并且设置时间限制为5s
    /// </summary>
    public void Button_Reset_5s()
    {
        ///重置棋盘、棋子和状态，重新开始
        Button_Reset();
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
            //按下左键，射一射线看打到哪个格子
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
                    //造成了胜利，回退
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
            //按下左键，射一射线看打到哪个格子
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (wincheck.PutChess(chess, int.Parse(hit.collider.name)))
                {
                    //下了一个字
                    hit.collider.GetComponent<Renderer>().material = mat;
                }
            }
        }
    }

    public void Button_Skill_ResetMap()
    {
        //在至多N个子的情况下重置整个棋盘的棋子
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
            //循环修改Data.map直至一个无胜利的map
            //直接修改data,map就行，不用备份
            while (true)
            {
                //先随机放置圈
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
                //后随机放置叉
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
                //检测是否胜利
                if(Check_ThreeChessNum(Data.Map, 1) == 0 && Check_ThreeChessNum(Data.Map, -1) == 0)
                {
                    //只要两边三线数量都是0
                    break;
                }
                //否则需要置零
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Data.Map[i, j] = 0;
                    }
                }
            }

            //根据修改后的Data.map重新修改棋盘外观
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
        //开始强制放置棋子，但这一着不可直接获胜
        if(putChessUseageNum > 0)
        {
            isSkillPutChess = true;

            putChessUseageNum -= 1;
        }
    }
    /// <summary>
    /// 生成博弈树
    /// </summary>
    /// <param name="map"></param>
    /// <param name="father"></param>
    //以下是树生成、ab剪枝搜索要用的一系列函数
    private void CreateGamblingTree(int[,] map, GamblingTree father)
    {
        ValueOfPos[] val = new ValueOfPos[9];                     //每个位置落子的价值
        int nullPos = 0;
        //筛选前几个高价值的落子点
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (map[i, j] == 0)
                {
                    //val[i * 3 + j].value = GetValue(i, j, map);
                    val[i * 3 + j].value = 1;       //把空子点通过排序放前面去
                    val[i * 3 + j].posi = i;
                    val[i * 3 + j].posj = j;
                    nullPos++;
                }
            }
        }
        Array.Sort(val, compare);
        //通过关键步判断可以提前结束生成树和搜索的过程
        //但这里没用
        if (father.depth == 1)
        {
            //如果出现了大于6666的val，不需要再搜索，必须下在这里，要么赢要么堵
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

            father.child[i] = point;        //父子关系
            point.father = father;
            point.depth = father.depth + 1; //博弈树深度
            point.posi = val[i].posi;        //落子位置
            point.posj = val[i].posj;
            point.chess = -father.chess;      //交替落子

            //拓展新地图，这个地图map1就是point节点所观测到的棋盘，需要上一步的落子
            int[,] map1 = new int[3, 3];
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    map1[j, k] = map[j, k];
                }
            }
            map1[point.posi, point.posj] = point.chess;                 //棋盘落子

            //如果此时已经有人获胜了
            if (CheckHasN(point.posi, point.posj, point.chess, map1, 3))
            {
                //获胜的人是point.chess
                if (point.chess != AIchess)
                {
                    //人赢了
                    point.score = -999999;            //局势评分无穷小

                }
                else
                {
                    //AI赢了
                    point.score = 999999;             //局势评分无穷大
                }
                //只要有人赢了，就不再继续生孩子
                continue;
            }
            else if(CheckHe(map1))
            {
                //和棋了，不继续生孩子
                point.score = 0;
                continue;
            }
            //对局仍然进行中，向下生孩子
            //计算分数。这里的分数是指对AI棋子的局势总评分
            //在阿尔法贝塔剪枝算法里，只有叶子节点不需要赋予ab值，取而代之的是需要计算叶子的局势分数
            if (point.depth == depth)
            {
                //达到了规定的最深节点，计算局势分数
                point.score = GetLeafScore_V2(map1, AIchess);
            }
            //更新解决方案                                      //即记录子树都是走哪个子得到的，因为只能走1步
            if (point.depth == 2)
            { //节点隶属的解决方案
                point.solve = i;
                point.fatherI = i;
            }
            else
            {
                point.solve = father.solve;     //是根节点的第几个子树里的节点
                point.fatherI = i;                //是父亲的第几个孩子（似乎没用到，但不敢乱删）
            }
            //没有达到最大深度，就继续生成
            if (point.depth < depth)
            {
                CreateGamblingTree(map1, point);                                  //递归此过程
            }
        }

    }
    /// <summary>
    /// 获取博弈树叶子节点的局势分数
    /// 采取的分数算法是：
    /// fn = 空位全放自己的子连成的3线数量 - 空位全放敌方子连成的敌方3线数量
    /// 算法来源于一个B站教程的教授
    /// </summary>
    /// <param name="map1">棋盘</param>
    /// <param name="chess">相对于哪个棋子而言的局势分数</param>
    /// <returns></returns>
    int GetLeafScore_V2(int[,] map1, int chess)
    {
        int value = 0;
        int[,] map_aichess = (int[,])map1.Clone();
        //空位全放chess，有几个三线
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
    /// 搜索当前棋盘内，连成3线的数量有多少
    /// </summary>
    /// <param name="map">棋盘，应该是无空位的</param>
    /// <param name="chess">要搜索哪个棋子三线</param>
    /// <returns></returns>
    int Check_ThreeChessNum(int[,] map, int chess)
    {
        int threeChessNum = 0;
        int num = 0;
        //纵线向
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
        //横线向
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
        //斜向
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
    ///落子价值获取
    ///井字棋范围有限，不需要筛选高价值子位。此函数未调用
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="map"></param>
    /// <returns></returns>
    private int GetValue(int i, int j, int[,] map)
    {
        int val = 66;       //只要为空子，就有初始价值
        //判断该点的落白子价值
        //首先看落白子之后可不可以获胜
        int chess = AIchess;
        map[i, j] = chess;
        if (CheckHasN(i, j, chess, map, 3))
        {
            //白子获胜了，直接返回极大的落子价值
            val += 999999;
        }
        //再看落黑子之后黑子会不会获胜，如果会，那么需要堵
        map[i, j] = -chess;
        if (CheckHasN(i, j, chess, map, 3))
        {
            //黑子获胜了，返回次大落子价值
            val += 888888;
        }
        map[i, j] = 0;
        return val;
    }
    
    bool CheckHasN(int i, int j, int chess, int[,] map, int N)
    {
        int num = 0;
        //横向
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
        //纵线向
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
        //斜向
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
                    //只要有一个空位就还没和棋
                    return false;
                }
            }
        }
        return true;
    }
    
    /// <summary>
    /// alpha-beta剪枝搜索博弈树
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    private int SearchABTree(GamblingTree p)
    {
        int solution = 0;
        int ansScore = MaxValue(root, -999999, 999999);             //ab初始值分别为负无穷和正无穷
        //我们可以获得一个最优的alpha值
        //去第一层孩子里搜索，谁的分数==这个答案，就走它的分支
        Debug.Log("ansScore = = = " + ansScore);
        for (int i = 0; i < root.child.Length; i++)
        {
            Debug.Log("child--- " + i + "的score === " + root.child[i].score);
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
        //极大值搜寻，当前节点是Max层。
        //Max搜索孩子中b值最高的一个作为自己的分数
        //意为“此节点，最少能获得x点收益”
        //（因为孩子是Min层，只改变了b）

        //p已经没有孩子了，是叶子。
        //叶子要用自身的局势分数去和父亲的ab值做比较
        //所以返回自身局势分数
        if (p.child == null)
        {
            return p.score;
        }
        //如果不是叶子，是向下探寻的过程。Max层只改alpha，初始赋负无穷
        p.score = -999999;
        //依次探寻所有孩子
        for (int i = 0; i < p.child.Length; i++)
        {
            //探寻这个孩子b值是多少
            //更新B值，则需要探寻孩子的所有a值的最小值
            int tempMax = MinValue(p.child[i], alpha, beta);
            //Max层的score值就是alpha
            p.score = Mathf.Max(tempMax, p.score);
            //a>b
            //剪枝，直接返回alpha值
            if (p.score >= beta)
            {
                return p.score;
            }
            //这一句只是为了使得代码可读性增强。实际上
            //int tempMax = MinValue(p.child[i], alpha, beta);
            //完全可以改成
            //int tempMax = MinValue(p.child[i], p.score, beta);
            alpha = Mathf.Max(alpha, p.score);
        }
        //返回的就是alpha值
        return p.score;                                             //求最大
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
        return p.score;                                         //求最小
    }
}
