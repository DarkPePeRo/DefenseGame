#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorAutoSave
{
    static EditorAutoSave()
    {
        EditorApplication.quitting += () =>
        {
            Debug.Log("[Editor Save] 에디터 종료 시도 감지됨");
            if (Application.isPlaying)
            {
                GameManager.Instance?.SaveAll(); // 직접 만든 저장 함수 호출
            }
        };
    }
}
#endif
