using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    // ウェイポイント
    private List<GameObject> points;

    public void Initialize()
    {
        points = new List<GameObject>(GameObject.FindGameObjectsWithTag("Point"));
    }

    public Vector3 SearchWayPoint(Vector3 wallDirection, Vector3 targetPos, Vector3 EnemyPos)
    {
        if(points.Count < 1)
        {
            Initialize();
        }
        float minPointDirection = (points[0].transform.position - EnemyPos).sqrMagnitude + (points[0].transform.position - targetPos).sqrMagnitude;
        //float minPointDirection = (points[0].transform.position - this.transform.position).sqrMagnitude;
        
        int minCnt = 0;
        for (int i = 1; i < points.Count; i++)
        {
            float pointDirection = (points[i].transform.position - EnemyPos).sqrMagnitude;
            float direction = pointDirection + (targetPos - points[i].transform.position).sqrMagnitude;
            var pointAngle = Vector3.Angle(points[i].transform.position - EnemyPos, wallDirection);
            if (pointAngle <= 10f && wallDirection.sqrMagnitude < pointDirection)
            {
                //Debug.Log("weipoint");
                continue;
            }

            if (minPointDirection > direction)
            {
                minPointDirection = direction;
                minCnt = i;
            }
        }
        targetPos = new Vector3(points[minCnt].transform.position.x, points[minCnt].transform.position.y + 1, points[minCnt].transform.position.z);
        points.Remove(points[minCnt]);

        return targetPos;
    }

}
