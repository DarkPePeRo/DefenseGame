using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameInit : MonoBehaviour
{
    public int frameLinmit;
    void Start()
    {
        QualitySettings.vSyncCount = 0;  // VSync ��Ȱ��ȭ
        Application.targetFrameRate = frameLinmit; // ������ ���� ����
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
