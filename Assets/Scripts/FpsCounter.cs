using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    public TMP_Text text;
    float _t;
    int _frames;

    void Update()
    {
        _t += Time.unscaledDeltaTime;
        _frames++;

        if (_t >= 0.5f)
        {
            float fps = _frames / _t;
            text.text = $"{fps:F1} FPS  (target={Application.targetFrameRate})";
            _t = 0f;
            _frames = 0;
        }
    }
}
