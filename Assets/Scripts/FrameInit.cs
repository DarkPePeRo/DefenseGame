using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameInit : MonoBehaviour
{
    public int frameLinmit;
    void Start()
    {
        QualitySettings.vSyncCount = 0;  // VSync 비활성화
        Application.targetFrameRate = frameLinmit; // 프레임 제한 해제
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
