using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoints : MonoBehaviour
{

    public static Transform[] points; //publid static���� ���������� ��𼭵� �����Ҽ��ֵ��� ����.

    private void Awake()
    {
        //WayPoints�� �ڽ��� �迭�� �����.
        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            //5���� WayPoint�� ���������.
            points[i] = transform.GetChild(i);
        }
    }

}
