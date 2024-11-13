using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndLine : MonoBehaviour
{
    public bool isEnd = false;
    public float detectionRadius = 0.5f; // 감지 범위 반경
    public LayerMask enemyLayer; // 적 레이어 마스크 설정
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
            // 충돌 시 처리할 로직을 여기에 작성
            waveSystem.AgainWave();

        }

    }

    private void OnDrawGizmosSelected()
    {
        // 감지 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
