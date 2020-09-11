﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shot : Token
{
    // 管理オブジェクト
    //public static TokenMgr<Shot> parent = null;

    protected string tagName;
    public void SetTagName(string tag_name) { tagName = tag_name; }

    // 速度
    protected Vector3 velocity;

    protected GameMgr gm;

    [SerializeField]
    private GameObject Effect;

    /*
    public static Shot Add(string tag_name, float x, float y, float z)
    {
        // Enemyインスタンスの取得
        Shot s = parent.Add(x, y, z);

        s.SetTagName(tag_name);

        return s;
    }
    */

    // 指定された角度（ 0 ～ 360 ）をベクトルに変換して返す
    public static Vector3 GetDirection(float angle)
    {
        return new Vector3
        (
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );
    }

    // 弾を発射する時に初期化するための関数
    public void Init(float angle, float speed, GameMgr gm)
    {
        this.gm = gm;

        // 弾の発射角度をベクトルに変換する
        var direction = GetDirection(angle);

        // 発射角度と速さから速度を求める
        velocity = direction * speed;

        // 弾が進行方向を向くようにする
        var angles = transform.localEulerAngles;
        angles.y = angle - 90;
        transform.localEulerAngles = angles;

        // 2 秒後に削除する
        //Destroy(gameObject, 2);
    }

    void Update()
    {
        // 移動する
        transform.localPosition += velocity * Time.deltaTime;
    }

    



}

