using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Eyesight : MonoBehaviour
{
    /*
    private float speed = 0.1f;

    // 処理回数を保持する変数です。
    int iterationNum = 0;

    // 交換回数を保持する変数です。
    int swapNum = 0;

    Vector2 forward;

    private Vector3 pforward;

    private ProbabilityMap pm;

    private SearchTarget st;

    // Start is called before the first frame update
    void Start()
    {
        pm = this.gameObject.GetComponent<ProbabilityMap>();
        pm.Initialize();

        st = this.gameObject.GetComponent<SearchTarget>();
        st.Initialize();

    }
    
    int to_binary(float num)
    {
        return num > 0 ? 1 : -1;
    }

    // Update is called once per frame
    void Update()
    {
        pforward = this.gameObject.transform.forward;

        //pm.UpdateProbabilityMap(pforward);

        if (st.IsReachTarget())
        {
            st.SelectTarget(pm);
        }

        forward.x = to_binary(pforward.x);
        forward.y = to_binary(pforward.y);

        st.SelectTarget_in_Obst(forward);

        //st.Coloring_Target();

        st.Move_Target(speed);


    }

    */
}
