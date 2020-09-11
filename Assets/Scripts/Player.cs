using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : Token
{
    // 管理オブジェクト
    public static TokenMgr<Player> parent = null;

    private GameMgr gm;
    private void SetGmaeMgr(GameMgr gm) { this.gm = gm; }


    private int ID;
    public void SetParam(int id) { ID = id; }

    private Vector3 velocity;

    private float moveSpeed;
    public void SetMoveSpeed(float move_speed) { moveSpeed = move_speed; }

    private float applySpeed;
    public void SetApplySpeed(float apply_speed) { applySpeed = apply_speed; }

    [SerializeField]
    private int SHOT_NUM;

    public float m_shotSpeed; // 弾の移動の速さ
    public float m_shotAngleRange; // 複数の弾を発射する時の角度
    public float m_shotTimer; // 弾の発射タイミングを管理するタイマー
    public int m_shotCount; // 弾の発射数
    public float m_shotInterval; // 弾の発射間隔（秒

    //　敵のMaxHP
    private int maxHp = 5;
    //　敵のHP
    [SerializeField]
    private int hp;

    //　HP表示用UI
    private GameObject HPUI;
    //　HP表示用スライダー
    private Slider hpSlider;

    public static Player Add(int id, float move_speed, float apply_speed, GameMgr gm,  float x, float y, float z)
    {
        // Enemyインスタンスの取得
        Player p = parent.Add(x, y, z);

        // IDを設定したり固有の処理をする
        p.SetParam(id);

        p.SetMoveSpeed(move_speed);

        p.SetApplySpeed(apply_speed);

        p.SetGmaeMgr(gm);

        return p;
    }

    public void Initilize_Hp()
    {
        GameObject _prefab = Resources.Load("Prefabs/" + "HPUI") as GameObject;
        GameObject g = GameObject.Instantiate(_prefab, this.transform.position, Quaternion.identity) as GameObject;
        hp = maxHp;
        hpSlider = g.transform.Find("HPBar").GetComponent<Slider>();
        hpSlider.value = 1f;
    }

    public void SetHp(int hp)
    {
        this.hp = hp;

        //　HP表示用UIのアップデート
        UpdateHPValue();

        /*
        if (hp <= 0)
        {
            //　HP表示用UIを非表示にする
            HideStatusUI();
        }
        */
    }

    public int GetHp()
    {
        return hp;
    }

    public int GetMaxHp()
    {
        return maxHp;
    }

    //　死んだらHPUIを非表示にする
    public void HideStatusUI()
    {
        HPUI.SetActive(false);
    }

    public void UpdateHPValue()
    {
        hpSlider.value = (float)GetHp() / (float)GetMaxHp();
    }


    // 指定された 2 つの位置から角度を求めて返す
    public float GetAngle(Vector2 from, Vector2 to)
    {
        var dx = to.x - from.x;
        var dy = to.y - from.y;
        var rad = Mathf.Atan2(dy, dx);
        return rad * Mathf.Rad2Deg;
    }

    public void Initilize_Shot()
    {
        // 管理オブジェクトを生成
        PlayerShot.parent = new TokenMgr<PlayerShot>("Shot", SHOT_NUM);
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
                var shot = PlayerShot.Add(this.gameObject.tag, pos.x, pos.y, pos.z);

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
            var shot = PlayerShot.Add(this.gameObject.tag, pos.x, pos.y, pos.z);

            // 弾を発射する方向と速さを設定する
            shot.Init(angleBase, speed, gm);

            //shot.UpdateShot();
        }
    }

    public void UpdatePlayer()
    {
        // WASD入力から、XZ平面(水平な地面)を移動する方向(velocity)を得ます
        velocity = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            velocity.z += 1;
        if (Input.GetKey(KeyCode.A))
            velocity.x -= 1;
        if (Input.GetKey(KeyCode.S))
            velocity.z -= 1;
        if (Input.GetKey(KeyCode.D))
            velocity.x += 1;

        // 速度ベクトルの長さを1秒でmoveSpeedだけ進むように調整します
        velocity = velocity.normalized * moveSpeed * Time.deltaTime;

        // いずれかの方向に移動している場合
        if (velocity.magnitude > 0)
        {
            // プレイヤーの位置(transform.position)の更新
            // 移動方向ベクトル(velocity)を足し込みます
            this.transform.position += velocity;
        }

        

        // プレイヤーのスクリーン座標を計算する
        var screenPos = Camera.main.WorldToScreenPoint(this.transform.position);

        // プレイヤーから見たマウスカーソルの方向を計算する
        var direction = Input.mousePosition - screenPos;

        // マウスカーソルが存在する方向の角度を取得する
        var angle = GetAngle(Vector3.zero, direction);

        // プレイヤーがマウスカーソルの方向を見るようにする
        var angles = transform.localEulerAngles;
        angles.y = angle - 90;
        transform.localEulerAngles = -angles;

        // 弾の発射タイミングを管理するタイマーを更新する
        m_shotTimer += Time.deltaTime;

        // まだ弾の発射タイミングではない場合は、ここで処理を終える
        //if (m_shotTimer < m_shotInterval) return;

        

        if (Input.GetMouseButtonDown(0))
        {
            if (m_shotTimer < m_shotInterval) return;
            // 弾を発射する
            ShootNWay(angle, m_shotAngleRange, m_shotSpeed, m_shotCount);

            // 弾の発射タイミングを管理するタイマーをリセットする
            m_shotTimer = 0;
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.tag == "EnemyShot")
        {
            //hp--;
            SetHp(hp);
        }
        
    }
}
