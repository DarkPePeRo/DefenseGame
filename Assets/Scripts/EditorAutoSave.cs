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
            Debug.Log("[Editor Save] ������ ���� �õ� ������");
            if (Application.isPlaying)
            {
                GameManager.Instance?.SaveAll(); // ���� ���� ���� �Լ� ȣ��
            }
        };
    }
}
#endif
