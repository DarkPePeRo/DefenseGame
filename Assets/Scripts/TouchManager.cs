// Unity 2D 모바일 게임용 카메라 터치 컨트롤 (매끄러운 버전)
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchManager : MonoBehaviour
{
    public Vector2 mapMinPosition;
    public Vector2 mapMaxPosition;

    public float zoomSpeed = 0.01f;
    public float minZoom = 2.0f;
    public float maxZoom = 5.0f;
    public float moveSpeed = 10f;

    private Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        // 에디터/모바일 구분하여 UI 위 터치만 무시
#if UNITY_EDITOR
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(-1)) return;
#else
        if (Input.touchCount > 0 && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) return;
#endif

#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
        ClampCameraPosition();
    }

    void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            ShowTouchEffect(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = dragOrigin - currentPos;
            cam.transform.Translate(delta, Space.World);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > Mathf.Epsilon)
        {
            ZoomCamera(-scroll * 1000f, Input.mousePosition);
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

            if (touch.phase == TouchPhase.Began)
            {
                dragOrigin = cam.ScreenToWorldPoint(touch.position);
                isDragging = true;
                ShowTouchEffect(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(touch.position);
                Vector3 delta = dragOrigin - currentPos;
                cam.transform.Translate(delta, Space.World);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevT0 = t0.position - t0.deltaPosition;
            Vector2 prevT1 = t1.position - t1.deltaPosition;

            float prevDist = (prevT0 - prevT1).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = currDist - prevDist;
            Vector2 mid = (t0.position + t1.position) * 0.5f;
            ZoomCamera(delta, mid);
        }
    }

    void ZoomCamera(float delta, Vector2 focusScreenPoint)
    {
        float oldSize = cam.orthographicSize;
        float newSize = Mathf.Clamp(oldSize - delta * zoomSpeed, minZoom, maxZoom);

        if (Mathf.Approximately(oldSize, newSize)) return;

        Vector3 focusWorld = cam.ScreenToWorldPoint(focusScreenPoint);
        cam.orthographicSize = newSize;
        Vector3 afterFocusWorld = cam.ScreenToWorldPoint(focusScreenPoint);
        Vector3 diff = focusWorld - afterFocusWorld;

        cam.transform.position += diff;
    }

    void ClampCameraPosition()
    {
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        Vector3 pos = cam.transform.position;
        pos.x = Mathf.Clamp(pos.x, mapMinPosition.x + horzExtent, mapMaxPosition.x - horzExtent);
        pos.y = Mathf.Clamp(pos.y, mapMinPosition.y + vertExtent, mapMaxPosition.y - vertExtent);
        cam.transform.position = pos;
    }
    void ShowTouchEffect(Vector2 screenPosition)
    {
        if (FocusPulsePool.Instance == null) return;

        FocusPulsePool.Instance.Request(screenPosition);
    }

}

