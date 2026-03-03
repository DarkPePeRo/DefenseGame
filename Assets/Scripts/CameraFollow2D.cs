using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Tooltip("타겟 기준 오프셋(월드 좌표). 보통 z는 카메라 z 유지용으로 0으로 둠.")]
    public Vector3 offset = new Vector3(0f, 0f, 0f);

    [Header("Map Clamp")]
    public Vector2 mapMinPosition;
    public Vector2 mapMaxPosition;

    [Header("Follow")]
    [Tooltip("0이면 즉시 따라감. 값이 클수록 부드럽게(지연) 따라감.")]
    public float smoothTime = 0.12f;

    [Header("Zoom (Optional)")]
    public bool enableZoom = true;
    public float zoomSpeed = 0.01f;
    public float minZoom = 2.0f;
    public float maxZoom = 5.0f;

    Camera cam;
    Vector3 velocity;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // 1) 타겟 따라가기
        Vector3 desired = target.position + offset;
        desired.z = transform.position.z; // 카메라 z는 유지(2D 기준)

        if (smoothTime <= 0f)
            transform.position = desired;
        else
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        // 2) 줌(선택)
        if (enableZoom)
            HandleZoom();

        // 3) 맵 경계 클램프
        ClampCameraPosition();
    }

    void HandleZoom()
    {
#if UNITY_EDITOR
        // UI 위 스크롤이면 줌 안되게
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1)) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            float old = cam.orthographicSize;
            float next = Mathf.Clamp(old - scroll * 1000f * zoomSpeed, minZoom, maxZoom);
            cam.orthographicSize = next;
        }
#else
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            // UI 위에서 핀치면 줌 안되게(두 손가락 중 하나라도 UI면 막기)
            if (EventSystem.current != null &&
                (EventSystem.current.IsPointerOverGameObject(t0.fingerId) ||
                 EventSystem.current.IsPointerOverGameObject(t1.fingerId)))
                return;

            Vector2 prevT0 = t0.position - t0.deltaPosition;
            Vector2 prevT1 = t1.position - t1.deltaPosition;

            float prevDist = (prevT0 - prevT1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = currDist - prevDist;

            float old = cam.orthographicSize;
            float next = Mathf.Clamp(old - delta * zoomSpeed, minZoom, maxZoom);
            cam.orthographicSize = next;
        }
#endif
    }

    void ClampCameraPosition()
    {
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, mapMinPosition.x + horzExtent, mapMaxPosition.x - horzExtent);
        pos.y = Mathf.Clamp(pos.y, mapMinPosition.y + vertExtent, mapMaxPosition.y - vertExtent);
        transform.position = pos;
    }
}