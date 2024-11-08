using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public MultiPrefabPool objectPool;
    public float Timer;
    public float spawnDelay;
    void Start()
    {
        objectPool = GameObject.Find("PoolManager").GetComponent<MultiPrefabPool>();
    }

    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > spawnDelay)
        {
            
            GameObject moster = objectPool.GetObject("Skeleton");
            moster.transform.position = transform.position;
            Timer = 0;
        }
    }
}
