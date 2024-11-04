using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{

    public static Transform[] points; //publid static으로 선언함으로 어디서든 접근할수있도록 설정.

    private void Awake()
    {
        //WayPoints의 자식을 배열에 담아줌.
        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            //5개의 WayPoint가 담겨질것임.
            points[i] = transform.GetChild(i);
        }
    }

}
