using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLine : MonoBehaviour
{
    public bool isEnd = false;
    public float detectionRadius = 0.5f; // ���� ���� �ݰ�
    public LayerMask enemyLayer; // �� ���̾� ����ũ ����
    public WaveSystem waveSystem;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        Collider2D collider = Physics2D.OverlapCircle(transform.position, detectionRadius, enemyLayer);

        if (collider != null)
        {
            isEnd = true;
            Debug.Log("Enemy detected!");
            // �浹 �� ó���� ������ ���⿡ �ۼ�
            waveSystem.AgainWave();

        }

    }

    private void OnDrawGizmosSelected()
    {
        // ���� ���� �ð�ȭ
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
