using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Token
{
    // 管理オブジェクト
    public static TokenMgr<Enemy> parent = null;

    private GameMgr gm;
    private void SetGmaeMgr(GameMgr gm) { this.gm = gm; }

    [SerializeField]
    private int ID;
    public void SetParam(int id) { ID = id; }

    public float speed;

    // 敵と壁間のベクトル
    private Vector3 wallDirection;

    private Vector3 targetDirection;
    private Vector3 playerDirection;

    //private Vector3 targetPos;
    private Stage targetStage;
    private Stack<Vector3> targetPosStk;
    public void PushTargetPosStk(Vector3 target)
    {
        targetPosStk.Push(target);
        MoveFlag = false;
    }

    // シーン上にあるウェイポイント
    public static WayPoint wp;

    private bool ObstFlag;
    
    public bool MoveFlag;

    public bool GetObstFlag() { return ObstFlag; }
    public bool GetMoveFlag() { return MoveFlag; }

    //public void SetTargetPos(Enemy e) { targetPos = e.transform.position; }

    private bool Attack;
    public bool GetAttack() { return Attack; }

    // 速度
    public Vector3 velocity;

    private Vector2 forward;

    


    [SerializeField]
    private int SHOT_NUM;

    public float m_shotSpeed; // 弾の移動の速さ
    public float m_shotAngleRange; // 複数の弾を発射する時の角度
    public float m_shotTimer; // 弾の発射タイミングを管理するタイマー
    public int m_shotCount; // 弾の発射数
    public float m_shotInterval; // 弾の発射間隔（秒）

    public string States;


    public static Enemy Add(int id, float x, float y, float z, GameMgr gm)
    {
        // Enemyインスタンスの取得
        Enemy e = parent.Add(x, y, z);

        e.SetGmaeMgr(gm);

        // IDを設定したり固有の処理をする
        e.SetParam(id);

        return e;
    }

    public void InitMgrTarget(float speed, Vector3 InitTargetPos)
    {
        //targetPosStk = new Stack<Vector3>();
        targetPosStk = new Stack<Vector3>();

        targetPosStk.Push(InitTargetPos);

        this.MoveFlag = false;

        this.Attack = false;

        this.speed = speed;

        //
        wp = this.gameObject.GetComponent<WayPoint>();
        wp.Initialize();

        States = "None";

    }

    public void Initilize_Shot()
    {
        // 管理オブジェクトを生成
        EnemyShot.parent = new TokenMgr<EnemyShot>("EnemyShot", SHOT_NUM);
    }

    public void Look()
    {

        //Debug.Log(stages[tmpStageColorsIndex[0]]);
        Quaternion rot = Quaternion.LookRotation(targetPosStk.Peek() - this.transform.position);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rot, speed / 30);


    }

    public bool Look_Target()
    {

        //Debug.Log(stages[tmpStageColorsIndex[0]]);
        Quaternion rot = Quaternion.LookRotation(targetPosStk.Peek() - this.transform.position);
        if (Vector3.Angle(rot.eulerAngles, this.transform.rotation.eulerAngles) > 1f && MoveFlag == false)
        {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rot, speed);
            return true;
        }
        else
        {
            MoveFlag = true;
            return false;
        }


    }

    // 指定された角度（ 0 ～ 360 ）をベクトルに変換して返す
    public Vector3 GetDirection(float angle)
    {
        return new Vector3
        (
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );
    }

    // 指定された 2 つの位置から角度を求めて返す
    public float GetAngle(Vector3 from, Vector3 to)
    {
        var dx = to.x - from.x;
        var dz = to.z - from.z;
        var rad = Mathf.Atan2(dz, dx);
        return rad * Mathf.Rad2Deg;
    }

    public void Move_Target()
    {
        if (!Look_Target())
        {
            velocity = (targetPosStk.Peek() - this.transform.position).normalized * speed;
            transform.position += velocity * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, targetPosStk.Peek(), speed * Time.deltaTime);
        }

    }
    
    public void Coloring_Target(ref Stage stage, Color targetColor)
    {
        stage.material.SetColor("_Color", targetColor);
    }

    public bool IsReachTarget(ref List<Stage> stageList)
    {
        if (targetPosStk.Count==0)
        {
            //Debug.Log("not set to target");
            return false;
        }

        //Coloring_Target(ref stageList, new Color(stageList[0].material.color.r, 0.0f, stageList[0].material.color.b, 0.0f));

        

        return Vector3.SqrMagnitude(targetPosStk.Peek() - this.transform.position) < 0.4f ? true : false;
    }

    public void SelectTarget(ProbabilityMap pm, ref List<Stage> stageList)
    {
        /*
        for (int i = 0; i < stages.Length; i++)
        {
            tmpStageColors[i] += (stages[i].gameObject.transform.position - this.gameObject.transform.position).magnitude;
        }
        */

        targetPosStk.Pop();

        if (targetPosStk.Count == 0)
        {
            //Debug.Log("Select Target");

            targetStage = pm.Sort_Prob(stageList);

            Vector3 targetPos = new Vector3(targetStage.obj.transform.position.x, targetStage.obj.transform.position.y + 1, targetStage.obj.transform.position.z);
            targetPos = targetPos + (targetPos - this.transform.position).normalized / 2;

            targetPosStk.Push(targetPos);

            wp.Initialize();

        }

        
        MoveFlag = false;
    }

    public bool obst(GameObject wall, Vector2 normal_start_to_end, Vector2 start_to_end)
    {
        Vector2 start_to_wall = new Vector2(wall.transform.position.x - this.transform.position.x, wall.transform.position.z - this.transform.position.z);
        Vector2 end_to_wall = new Vector2(wall.transform.position.x - targetPosStk.Peek().x, wall.transform.position.z - targetPosStk.Peek().z);

        float dist_projection = start_to_wall.x * start_to_end.y - start_to_end.x * start_to_wall.y;

        if (Mathf.Abs(dist_projection) < 0.5f)
        {
            float dot01 = start_to_wall.x * start_to_end.x + start_to_wall.y * start_to_end.y;
            float dot02 = end_to_wall.x * start_to_end.x + end_to_wall.y * start_to_end.y;

            if (dot01 * dot02 <= 0.0f)
            {
                return true;
            }
            else if (start_to_wall.magnitude < 0.5f || end_to_wall.magnitude < 0.5f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    public void SelectTarget_in_Obst(Vector2 forward, ref List<Wall> walls)
    {
        // 障害物を感知していない場合
        //if (ObstFlag == false)
        // {
        Vector2 start_to_end = new Vector2(targetPosStk.Peek().x - this.transform.position.x, targetPosStk.Peek().z - this.transform.position.z);
        Vector2 normal_start_to_end = start_to_end.normalized;

        Vector3 targetPos;

        // 障害物を検索
        //foreach (Wall wall in walls)
        for (int i = 0; i < walls.Count; i++)
        {
            // 
            if (this.transform.position.x * forward.x > walls[i].obj.transform.position.x * forward.x || this.transform.position.z * forward.y > walls[i].obj.transform.position.z * forward.y)
            {
                States = "Search";
                continue;
            }
            //Debug.Log("near obst");

            //wallDirection = wall.gameObject.transform.position - this.gameObject.transform.position;
            //var angle = Vector3.Angle(targetPosStk.Peek() - this.transform.position, wallDirection);

            // 目的地までの直線に障害物があれば
            //if (angle <= 10f && wallDirection.sqrMagnitude < (targetPosStk.Peek() - this.transform.position).sqrMagnitude)
            if (obst(walls[i].obj, start_to_end, normal_start_to_end) == true)
            {
                //Debug.Log("found obst");
                //walls[i].color = new Color(0.0f, 1.0f, 0.0f, 0.0f);

                wallDirection = walls[i].obj.transform.position - this.transform.position;

                targetPos = wp.SearchWayPoint(wallDirection, targetPosStk.Peek(), this.transform.position);
                targetPos = targetPos + (targetPos - this.transform.position).normalized / 2;

                targetPosStk.Push(targetPos);

                States = "Obst";



                //ObstFlag = true;
                //Debug.Log(minCnt);
                break;
            }

        }

    }

    public void SearchPlayer(Vector3 playerPos)
    {
        // ステージと敵の間のベクトルを計算
        playerDirection = playerPos - this.transform.position;

        if( playerDirection.sqrMagnitude < 40f)
        {
            // ベクトルから角度を計算
            //var angle = Vector3.Angle(this.transform.forward, playerDirection);

            // 扇形の視界に入っているステージの
            //if (angle <= 60f)
           // {
            targetPosStk.Clear();
            targetPosStk.Push(playerPos);
            Attack = true;
            //}
        }

        
    }

    // 弾を発射する関数
    private void ShootNWay(float angleBase, float angleRange, float speed, int count)
    {
        var pos = transform.position + transform.forward; // プレイヤーの位置
        var rot = transform.rotation; // プレイヤーの向き

        // 弾を複数発射する場合
        if (1 < count)
        {
            // 発射する回数分ループする
            for (int i = 0; i < count; ++i)
            {
                // 弾の発射角度を計算する
                var angle = angleBase +
                    angleRange * ((float)i / (count - 1) - 0.5f);

                // 発射する弾を生成する
                //var shot = Instantiate(shotPrefab, pos, rot);
                var shot = EnemyShot.Add(this.gameObject.tag, pos.x, pos.y, pos.z);

                // 弾を発射する方向と速さを設定する
                shot.Init(angle, speed, gm);

                ///shot.UpdateShot();
            }
        }
        // 弾を 1 つだけ発射する場合
        else if (count == 1)
        {
            // 発射する弾を生成する
            //var shot = Instantiate(shotPrefab, pos, rot);
            var shot = EnemyShot.Add(this.gameObject.tag, pos.x, pos.y, pos.z);

            // 弾を発射する方向と速さを設定する
            shot.Init(angleBase, speed, gm);

            //shot.UpdateShot();
        }
    }

    

    public void Shoot(Vector3 playerPos)
    {
        // 弾の発射タイミングを管理するタイマーを更新する
        m_shotTimer += Time.deltaTime;

        // まだ弾の発射タイミングではない場合は、ここで処理を終える
        //if (m_shotTimer < m_shotInterval) return;

        if (m_shotTimer < m_shotInterval) return;
        // 弾を発射する
        ShootNWay(GetAngle(this.transform.position, playerPos), m_shotAngleRange, m_shotSpeed, m_shotCount);

        // 弾の発射タイミングを管理するタイマーをリセットする
        m_shotTimer = 0;
    }

    int to_binary(float num)
    {
        return num > 0 ? 1 : -1;
    }

    public void UpdateEnemy(ProbabilityMap pm, List<Stage> stageList, List<Wall> wallList, Vector3 playerPos)
    {
        pm.UpdateProbabilityMap(ref stageList, this.transform.forward, this.transform.position);

        if (GetAttack())
        {
            if (IsReachTarget(ref stageList))
            {
                PushTargetPosStk(playerPos);
            }

            Look();
            Shoot(playerPos);
        }
        else
        {

            if (IsReachTarget(ref stageList))
            {
                //Debug.Log("Reach Target");
                SelectTarget(pm, ref stageList);

            }

            SearchPlayer(playerPos);

        }


        forward.x = to_binary(this.transform.forward.x);
        forward.y = to_binary(this.transform.forward.y);

        SelectTarget_in_Obst(forward, ref wallList);

        //Coloring_Target(ref targetStage, new Color(targetStage.material.color.r, 1.0f, targetStage.material.color.b, 0.0f));

        Move_Target();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Shot")
        {
            //Debug.Log("ID :" + ID);
            //gm.RemoveEnemy_at_elist(ID);
            //Debug.Log("e_list :" + gm.e_list.Count);
            Vanish();
        }

    }




}
