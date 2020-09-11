using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tmp_Eyesight : MonoBehaviour
{
    // シーン上にあるステージ
    private GameObject[] stages;

    // シーン上にある壁
    private GameObject[] walls;


    // 敵とステージ間のベクトル
    private Vector3 stageDirection;

    // 敵と壁間のベクトル
    private Vector3 wallDirection;


    private float[] stageColors;
    private float[] tmpStageColors;
    private int[] stageColorsIndex;
    private int[] tmpStageColorsIndex;

    // 色の変化量
    private float deltaColor;

    private List<GameObject> points;


    private float speed = 0.1f;
    private int targetIndex;
    private Vector3 targetPos;
    private GameObject targetStage;
    private Stack<Vector3> targetPosStk;
    private bool ObstFlag;

    // 処理回数を保持する変数です。
    int iterationNum = 0;

    // 交換回数を保持する変数です。
    int swapNum = 0;

    private ProbabilityMap pm;

    // Start is called before the first frame update
    void Start()
    {
        points = new List<GameObject>(GameObject.FindGameObjectsWithTag("Point"));

        targetIndex = 0;

        targetPos = this.transform.position;

        targetPosStk = new Stack<Vector3>();
        targetPosStk.Push(targetPos);

        // ステージを取得
        stages = GameObject.FindGameObjectsWithTag("Stage");
        walls = GameObject.FindGameObjectsWithTag("Wall");

        stageColors = new float[stages.Length];

        tmpStageColors = new float[stages.Length];

        stageColorsIndex = new int[stages.Length];

        tmpStageColorsIndex = new int[stages.Length];
        for (int i = 0; i < stages.Length; i++)
        {
            stageColors[i] = 0.0f;
            stageColorsIndex[i] = i;
        }

        deltaColor = 0.2f;

    }
    public void UpdateProbabilityMap(Vector3 pforward)
    {
        int cnt = 0;

        // 扇形の視界に入ったら確率マップを更新する
        foreach (GameObject stage in stages)
        {
            // ステージと敵の間のベクトルを計算
            stageDirection = stage.gameObject.transform.position - this.gameObject.transform.position;

            // ベクトルから角度を計算
            var angle = Vector3.Angle(pforward, stageDirection);

            // 扇形の視界に入っているステージの
            if (angle <= 60f && stageDirection.sqrMagnitude < 15f)
            {
                if (stageColors[cnt] <= 1.0f)
                {
                    stageColors[cnt] += deltaColor * Time.deltaTime;
                }

            }
            else
            {
                if (stageColors[cnt] > 0.0f)
                {
                    stageColors[cnt] -= (deltaColor / (stageDirection.sqrMagnitude * 3)) * Time.deltaTime;
                    //Debug.Log(stageColors[cnt]);
                }
            }


            stage.GetComponent<Renderer>().material.color = new Color(stageColors[cnt], 0.0f, 0.0f, 0.0f);

            cnt++;
        }
    }


    // Update is called once per frame
    void Update()
    {
        float rotate = Mathf.PI / 4;


        Vector3 pforward = this.gameObject.transform.forward;

        UpdateProbabilityMap(pforward);

        // 目的地に到着したら、次の目的地を設定する
        if (Vector3.SqrMagnitude(targetPosStk.Peek() - this.transform.position) < 0.1f)
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
                Array.Copy(stageColors, tmpStageColors, stages.Length);
                Array.Copy(stageColorsIndex, tmpStageColorsIndex, stages.Length);

                ExecuteQuickSort(tmpStageColors, tmpStageColorsIndex, 0, stageColors.Length - 1);

                targetStage = stages[tmpStageColorsIndex[targetIndex]];

                targetPos = new Vector3(targetStage.transform.position.x, targetStage.transform.position.y + 1, targetStage.transform.position.z);

                targetPosStk.Push(targetPos);

                points = new List<GameObject>(GameObject.FindGameObjectsWithTag("Point"));

                Debug.Log("Target Lock On");
            }

            ObstFlag = false;


        }

        targetStage.GetComponent<Renderer>().material.color = new Color(0.0f, 1.0f, 0.0f, 0.0f);

        // 障害物を感知していない場合
        if (ObstFlag == false)
        {
            // 障害物を検索
            foreach (GameObject wall in walls)
            {
                wallDirection = wall.gameObject.transform.position - this.gameObject.transform.position;
                var angle = Vector3.Angle(targetPosStk.Peek() - this.transform.position, wallDirection);

                // 目的地までの直線に障害物があれば
                if (angle <= 30f && wallDirection.sqrMagnitude < (targetPosStk.Peek() - this.transform.position).sqrMagnitude)
                {
                    //Debug.Log("wall : " + wallDirection.sqrMagnitude);
                    //Debug.Log("target : " + (targetPos - this.transform.position).sqrMagnitude);

                    float minPointDirection = (points[0].transform.position - this.transform.position).sqrMagnitude;
                    int minCnt = 0;
                    for (int i = 1; i < points.Count; i++)
                    {
                        float pointDirection = (points[i].transform.position - this.transform.position).sqrMagnitude;
                        var pointAngle = Vector3.Angle(points[i].transform.position - this.transform.position, wallDirection);
                        if (pointAngle <= 10f && wallDirection.sqrMagnitude < pointDirection)
                        {
                            Debug.Log("weipoint");
                            continue;
                        }

                        if (minPointDirection > pointDirection)
                        {
                            minPointDirection = pointDirection;
                            minCnt = i;
                        }
                    }
                    targetPos = new Vector3(points[minCnt].transform.position.x, points[minCnt].transform.position.y + 1, points[minCnt].transform.position.z);
                    points.Remove(points[minCnt]);

                    targetPosStk.Push(targetPos);

                    foreach (Vector3 targetpos in targetPosStk.ToArray())
                    {
                        Debug.Log(targetpos);

                    }
                    ObstFlag = true;
                    //Debug.Log(minCnt);
                    break;
                }


            }
        }





        //Debug.Log(stages[tmpStageColorsIndex[0]]);
        Quaternion rot = Quaternion.LookRotation(targetPosStk.Peek() - this.gameObject.transform.position);

        this.gameObject.transform.rotation = Quaternion.Slerp(this.transform.rotation, rot, speed);

        this.gameObject.transform.position = Vector3.MoveTowards(this.gameObject.transform.position, targetPosStk.Peek(), speed / 20);



    }

    /// <Summary>
    /// 引数で渡された値の中から中央値を返します。
    /// </Summary>
    /// <param id="top">確認範囲の最初の要素</param>
    /// <param id="mid">中確認範囲の真ん中の要素</param>
    /// <param id="bottom">確認範囲の最後の要素</param>
    float GetMediumValue(float top, float mid, float bottom)
    {
        if (top < mid)
        {
            if (mid < bottom)
            {
                return mid;
            }
            else if (bottom < top)
            {
                return top;
            }
            else
            {
                return bottom;
            }
        }
        else
        {
            if (bottom < mid)
            {
                return mid;
            }
            else if (top < bottom)
            {
                return top;
            }
            else
            {
                return bottom;
            }
        }
    }

    /// <Summary>
    /// クイックソートを行います。
    /// </Summary>
    /// <param id="array">ソート対象の配列</param>
    /// <param id="left">ソート範囲の最初のインデックス</param>
    /// <param id="right">ソート範囲の最後のインデックス</param>
    void ExecuteQuickSort(float[] array, int[] index, int left, int right)
    {
        iterationNum++;
        //Debug.Log("ExecuteQuickSortが呼ばれました。");

        // 確認範囲が1要素しかない場合は処理を抜けます。
        if (left >= right)
        {
            return;
        }

        // 左から確認していくインデックスを格納します。
        int i = left;

        // 右から確認していくインデックスを格納します。
        int j = right;

        // ピボット選択に使う配列の真ん中のインデックスを計算します。
        int mid = (left + right) / 2;

        // ピボットを決定します。
        float pivot = GetMediumValue(array[i], array[mid], array[j]);

        while (true)
        {
            // ピボットの値以上の値を持つ要素を左から確認します。
            while (array[i] < pivot)
            {
                i++;
            }

            // ピボットの値以下の値を持つ要素を右から確認します。
            while (array[j] > pivot)
            {
                j--;
            }

            // 左から確認したインデックスが、右から確認したインデックス以上であれば外側のwhileループを抜けます。
            if (i >= j)
            {
                break;
            }

            // そうでなければ、見つけた要素を交換します。
            float temp = array[j];
            array[j] = array[i];
            array[i] = temp;

            int tempIndex = index[j];
            index[j] = index[i];
            index[i] = tempIndex;

            // 交換を行なった要素の次の要素にインデックスを進めます。
            i++;
            j--;

            // 交換回数を増やします。
            swapNum++;
        }

        // 左側の範囲について再帰的にソートを行います。
        ExecuteQuickSort(array, index, left, i - 1);

        // 右側の範囲について再帰的にソートを行います。
        ExecuteQuickSort(array, index, j + 1, right);
    }
}
