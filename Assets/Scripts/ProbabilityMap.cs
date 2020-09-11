using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbabilityMap : MonoBehaviour
{

    // 敵とステージ間のベクトル
    private Vector3 stageDirection;


    // 色の変化量
    private float delta_prob = 0.5f;

    //public List<Stage> stageList;

    /*
    public void Initialize()
    {
        // ステージを取得
        GameObject[] stageObjs = GameObject.FindGameObjectsWithTag("Stage");

        // ステージ構造体の初期化
        stageList = new List<Stage>();

        // ステージ構造体のパラメータの初期化
        for (int i = 0; i < stageObjs.Length; i++)
        {
            Stage s = new Stage();

            // ゲームオブジェクトの初期化
            s.StageObj = stageObjs[i];
            // 確率の初期化
            s.StageProb = 0.0f;
            // 
            s.StageProbIndex = i;

            stageList.Add(s);
        }

        deltaColor = 0.2f;

        //Debug.Log(stageList[21].StageObj.transform.position);

    }
    */

    public Stage Sort_Prob(List<Stage> stageList)
    {
        stageList.Sort((a, b) => a.prob.CompareTo(b.prob));

        return stageList[0];

    }

    public void UpdateProbabilityMap(ref List<Stage> stageList, Vector3 pforward, Vector3 enemyPos)
    {
        // 扇形の視界に入ったら確率マップを更新する
        for (int i = 0; i < stageList.Count; i++)
        {
            // ステージと敵の間のベクトルを計算
            stageDirection = stageList[i].obj.transform.position - enemyPos;

            // ベクトルから角度を計算
            var angle = Vector3.Angle(pforward, stageDirection);

            // 扇形の視界に入っているステージの
            if (angle <= 60f && stageDirection.sqrMagnitude < 30f)
            {
                if (stageList[i].prob <= 1.0f)
                {
                    Stage tmpData = stageList[i];
                    tmpData.prob += delta_prob * Time.deltaTime;
                    stageList[i] = tmpData;
                }

            }
            else
            {
                if (stageList[i].prob > 0.0f)
                {
                    Stage tmpData = stageList[i];
                    tmpData.prob -= (delta_prob / (stageDirection.sqrMagnitude * 3)) * Time.deltaTime;
                    stageList[i] = tmpData;


                    //Debug.Log(stageColors[cnt]);
                }
            }

            //stageList[i].material.SetColor("_Color", new Color(stageList[i].prob, stageList[i].material.color.g, stageList[i].material.color.b, 0.0f));

            //stageList[i].obj.GetComponent<Renderer>().material.color = new Color(stageList[i].prob, 0.0f, 0.0f, 0.0f);
        }
    }
}
