using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject Monster;
    public float Timer;

    void Start()
    {
        
    }

    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > 1)
        {
            Instantiate(Monster, new Vector3(-5, -6, 0), Quaternion.identity);
            Timer = 0;
        }
    }
}
