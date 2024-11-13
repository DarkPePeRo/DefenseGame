using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bow : MonoBehaviour
{
    public Animator anim;
    public Archer archer;
    public float previousShotDelay;
    void Start()
    {
        anim = GetComponent<Animator>();
        previousShotDelay = archer.shotDelay;
    }

    // Update is called once per frame
    void Update()
    {
        if(archer.shotDelay != previousShotDelay)
        {
            anim.SetFloat("Speed", 1 / archer.shotDelay);
        }
    }
}
