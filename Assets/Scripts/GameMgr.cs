using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMgr : MonoBehaviour
{
    // シーン上にあるステージ
    public static List<Stage> stageList;

    // シーン上にある壁
    public static List<Wall> wallList;

    //
    private ProbabilityMap pm;

    //
    public List<Enemy> e_list;

    public int GetActiveEnemyNum()
    {
        return e_list.Count;
    }

    public void RemoveEnemy_at_elist(int id)
    {
        e_list.RemoveAt(id);
    }

    private Player p;

    private Vector2 forward;

    private Stage TargetStage;

    private Color TargetColor;

    private float player_move_speed = 3.0f;

    private float player_apply_speed = 0.2f;

    public static float enemy_move_speed = 3.0f;

    public static float enemy_apply_speed = 0.2f;

    private bool[] step = { false, false, false };

    private float enemyTime;
    private int enemyCount;

    private Follow f;

    private Vector3 CameraPos;


    [SerializeField]
    public static int ENEMY_NUM;

    [SerializeField]
    private int PLAYER_NUM;


    public static int ROOM_NUM;



    public void StartExploson(Collision col, float time)
    {
        StartCoroutine(Exploson(col.gameObject, time));
    }
    
    public IEnumerator Exploson(GameObject col, float time)
    {
        ShotEffect es = ShotEffect.Add(col.transform.position.x, col.transform.position.y, col.transform.position.z);
        yield return new WaitForSeconds(time);
        es.Vanish();
    }

    void Initilize_Stage()
    {
        // ステージを取得
        GameObject[] stageObjs = GameObject.FindGameObjectsWithTag("Stage");

        // ステージ構造体のパラメータの初期化
        for (int i = 0; i < stageObjs.Length; i++)
        {
            Stage s = new Stage();

            // ゲームオブジェクトの初期化
            s.obj = stageObjs[i];
            // 確率の初期化
            s.prob = 0.0f;
            // 
            s.probIndex = i;

            s.material = s.obj.GetComponent<Renderer>().material;

            s.target = new Vector3(s.obj.transform.position.x, s.obj.transform.position.y + 2, s.obj.transform.position.z);

            stageList.Add(s);
        }
    }

    void Initilize_Wall()
    {
        // 壁を取得
        GameObject[] wallObjs = GameObject.FindGameObjectsWithTag("Wall");

        Vector3 targetpos;

        // 壁構造体のパラメータの初期化
        for (int i = 0; i < wallObjs.Length; i++)
        {
            targetpos = new Vector3(wallObjs[i].transform.position.x, wallObjs[i].transform.position.y + 3, wallObjs[i].transform.position.z);
            Wall w = new Wall(wallObjs[i], wallObjs[i].GetComponent<Renderer>().material.color, targetpos);

            // ゲームオブジェクトの初期化
            //w.obj = wallObjs[i];

            // 色の初期化
            //w.color = w.obj.GetComponent<Renderer>().material.color;

            wallList.Add(w);
        }

    }

    void Initilize_Enemy()
    {
        // 管理オブジェクトを生成
        Enemy.parent = new TokenMgr<Enemy>("Enemy", ENEMY_NUM);
        Random.InitState(10);


        e_list = new List<Enemy>(ENEMY_NUM / 4);
        for (int i = 0; i < ENEMY_NUM / 4; i++)
        {
            int ind = Random.Range(0, stageList.Count);
            Stage tmp = stageList[ind];
            if(stageList.Remove(stageList[ind]))
            {
                stageList.Insert(0, tmp);
            }
            ind = Random.Range(1, stageList.Count);
            Enemy e = Enemy.Add(e_list.Count, stageList[ind].obj.transform.position.x, 1.0f, stageList[ind].obj.transform.position.z, this);
            e.InitMgrTarget(enemy_move_speed / 2, new Vector3(stageList[0].obj.transform.position.x, stageList[0].obj.transform.position.y+1, stageList[0].obj.transform.position.z) );

            //Debug.Log(stageList[0].obj.transform.position);

            e.Initilize_Shot();

            e_list.Add(e);
        }
    }

    public void Add_Enemy(Vector3 pos)
    {
        Enemy e = Enemy.Add(e_list.Count, pos.x, 0.75f, pos.z, this);
        Stage targetStage = pm.Sort_Prob(stageList);
        
        e.InitMgrTarget(enemy_move_speed / 2, new Vector3(targetStage.obj.transform.position.x, targetStage.obj.transform.position.y + 1, targetStage.obj.transform.position.z));

        e_list.Add(e);

    }

    void Initilize_Player()
    {
        // 管理オブジェクトを生成
        Player.parent = new TokenMgr<Player>("Player", PLAYER_NUM);

        int ind = Random.Range(0, stageList.Count);

        p = Player.Add(0, player_move_speed, player_apply_speed, this, stageList[ind].obj.transform.position.x, 1.0f, stageList[ind].obj.transform.position.z);

        p.Initilize_Hp();

        p.Initilize_Shot();

        Follow.objTarget = p.gameObject;
    }

    

    int to_binary(float num)
    {
        return num > 0 ? 1 : -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        CameraPos = this.transform.position;

        enemyTime = 0;
        enemyCount = 1;

        stageList = new List<Stage>();
        wallList = new List<Wall>();

        CreateStage cs = this.GetComponent<CreateStage>();
        cs.Create(new Vector3(-1,0,1), ROOM_NUM);

        f = this.GetComponent<Follow>();

        TargetColor = new Color(0.0f, 1.0f, 0.0f, 0.0f);

        // 作成していたStageを読み取り初期化
        Initilize_Stage();

        // 
        Initilize_Wall();

        // 
        pm = this.gameObject.GetComponent<ProbabilityMap>();

        ShotEffect.parent = new TokenMgr<ShotEffect>("Exploson", ENEMY_NUM);

    }

    // Update is called once per frame
    void Update()
    {

        if (step[0] == false)
        {
            if ((stageList[0].target - stageList[0].obj.transform.position).sqrMagnitude > 0f)
            {

                for (int i = 0; i < stageList.Count; i++)
                {
                    stageList[i].obj.transform.position = Vector3.MoveTowards(stageList[i].obj.transform.position, stageList[i].target, Time.deltaTime);
                }
                
            }
            else
            {
                step[0] = true;
            }
        }
        else if (step[1] == false)
        {
            if ((wallList[0].target - wallList[0].obj.transform.position).sqrMagnitude > 0f)
            {
                for (int i = 0; i < wallList.Count; i++)
                {
                    wallList[i].obj.transform.position = Vector3.MoveTowards(wallList[i].obj.transform.position, wallList[i].target, Time.deltaTime);
                }
            }
            else
            {
                step[1] = true;

                Initilize_Enemy();

                Initilize_Player();

                f.enabled = true;
            }
        }
        else
        {

            /*
            enemyTime += Time.deltaTime;

            if (enemyTime >= 10)
            {
                int ind = Random.Range(1, stageList.Count);
                for (int i = 0; i < enemyCount; i++)
                {
                    Add_Enemy(new Vector3(stageList[ind+i].obj.transform.position.x, stageList[ind + i].obj.transform.position.y + 1, stageList[ind + i].obj.transform.position.z));
                }

                enemyTime = 0;
            }
            */

            Enemy.parent.ForEachExist(t => t.UpdateEnemy(pm, stageList, wallList, p.transform.position));

            if (Enemy.parent.Count() == 0)
            {
                SceneManager.LoadScene("GameClear");
            }

            Player.parent.ForEachExist(t => t.UpdatePlayer());

            if (p.GetHp() <= 0)
            {
                SceneManager.LoadScene("GameOver");
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                f.enabled = !f.enabled;
                this.transform.position = CameraPos;
            }

            /*
            foreach (Enemy e in e_list)
            {
                if (!e.Exists)
                {
                    continue;
                }

                pm.UpdateProbabilityMap(ref stageList, e.transform.forward, e.transform.position);

                if (e.GetAttack())
                {
                    if (e.IsReachTarget())
                    {
                        e.PushTargetPosStk(p.transform.position);
                    }

                    e.Look();
                    //e.Shoot(p.transform.position);
                }
                else
                {
                    if (e.IsReachTarget())
                    {
                        //Debug.Log("Reach Target");
                        e.SelectTarget(pm, ref stageList);

                    }

                    e.SearchPlayer(p.transform.position);

                }


                forward.x = to_binary(e.transform.forward.x);
                forward.y = to_binary(e.transform.forward.y);

                e.SelectTarget_in_Obst(forward, ref wallList);

                //e.Coloring_Target(ref stageList, new Color(stageList[0].material.color.r, 1.0f, stageList[0].material.color.b, 0.0f));

                e.Move_Target();

            }
            */

            //p.UpdatePlayer();

        }

        




    }
}
