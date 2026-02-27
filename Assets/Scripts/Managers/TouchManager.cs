using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    public Vector2 mapMinPosition;
    public Vector2 mapMaxPosition;

    public float zoomSpeed = 0.01f;
    public float minZoom = 2.0f;
    public float maxZoom = 5.0f;
    public float moveSpeed = 10f;

    public LayerMask focusLayerMask;
    public GameObject uiBlocker; // UI 잠금용 오버레이

    public float focusDuration = 0.5f;

    [Header("Tap/Drag")]
    public float dragThresholdPixels = 12f;   // 이 이상 움직이면 드래그로 판정
    public float tapMaxTime = 0.35f;          // 선택: 너무 길게 누르면 탭으로 안 보게

    private Camera cam;

    private bool isDragging;
    private bool inputLocked;

    // Tap 판정용
    private Vector2 pointerDownScreenPos;
    private float pointerDownTime;
    private Vector3 dragOriginWorld;
    private bool focusCandidate;              // Down 시 오브젝트를 눌렀는지
    private Vector3 focusCandidateWorldPos;   // Up에서 Raycast할 때 쓸 위치(Down 당시)
    private int activeFingerId = -999;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (inputLocked) return;

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
            pointerDownScreenPos = Input.mousePosition;
            pointerDownTime = Time.unscaledTime;

            Vector3 worldPos = cam.ScreenToWorldPoint(pointerDownScreenPos);
            worldPos.z = cam.transform.position.z;

            focusCandidateWorldPos = worldPos;
            focusCandidate = HitFocusable(worldPos); // Down에서는 "후보"만 체크

            dragOriginWorld = worldPos;
            isDragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 currScreen = Input.mousePosition;
            float moved = (currScreen - pointerDownScreenPos).magnitude;

            // 임계값 넘으면 드래그 시작(포커스 후보 취소)
            if (!isDragging && moved >= dragThresholdPixels)
            {
                isDragging = true;
                focusCandidate = false;
            }

            if (isDragging)
            {
                Vector3 currentWorld = cam.ScreenToWorldPoint(currScreen);
                currentWorld.z = cam.transform.position.z;

                Vector3 delta = dragOriginWorld - currentWorld;
                cam.transform.Translate(delta, Space.World);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            float held = Time.unscaledTime - pointerDownTime;

            // 드래그가 아니고, 짧은 탭이면 포커스 실행
            if (!isDragging && focusCandidate && held <= tapMaxTime)
            {
                TryFocusAt(focusCandidateWorldPos);
            }

            isDragging = false;
            focusCandidate = false;
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
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

            if (touch.phase == TouchPhase.Began)
            {
                activeFingerId = touch.fingerId;
                pointerDownScreenPos = touch.position;
                pointerDownTime = Time.unscaledTime;

                Vector3 worldPos = cam.ScreenToWorldPoint(pointerDownScreenPos);
                worldPos.z = cam.transform.position.z;

                focusCandidateWorldPos = worldPos;
                focusCandidate = HitFocusable(worldPos); // Down에서는 "후보"만 체크

                dragOriginWorld = worldPos;
                isDragging = false;
            }
            else if (touch.phase == TouchPhase.Moved && touch.fingerId == activeFingerId)
            {
                float moved = (touch.position - pointerDownScreenPos).magnitude;

                if (!isDragging && moved >= dragThresholdPixels)
                {
                    isDragging = true;
                    focusCandidate = false;
                }

                if (isDragging)
                {
                    Vector3 currentWorld = cam.ScreenToWorldPoint(touch.position);
                    currentWorld.z = cam.transform.position.z;

                    Vector3 delta = dragOriginWorld - currentWorld;
                    cam.transform.Translate(delta, Space.World);
                }
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && touch.fingerId == activeFingerId)
            {
                float held = Time.unscaledTime - pointerDownTime;

                if (!isDragging && focusCandidate && held <= tapMaxTime)
                {
                    // 탭으로 확정된 경우에만 포커스
                    TryFocusAt(focusCandidateWorldPos);
                }

                isDragging = false;
                focusCandidate = false;
                activeFingerId = -999;
            }
        }
        else if (Input.touchCount == 2)
        {
            // 핀치 중에는 포커스 후보/드래그 상태 리셋(실수 방지)
            focusCandidate = false;
            isDragging = false;

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

    bool HitFocusable(Vector3 worldPosition)
    {
        Vector2 origin = new Vector2(worldPosition.x, worldPosition.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 0f, focusLayerMask);
        return hit.collider != null;
    }

    void TryFocusAt(Vector3 worldPosition)
    {
        Vector2 origin = new Vector2(worldPosition.x, worldPosition.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero, 0f, focusLayerMask);

        if (hit.collider == null) return;

        Vector3 rawTarget = new Vector3(hit.transform.position.x, hit.transform.position.y, cam.transform.position.z);

        float targetZoom = Mathf.Clamp(cam.orthographicSize * 0.8f, minZoom, maxZoom);

        // Clamp를 "타겟 줌 기준"으로 계산하려고 임시 적용
        float prevZoom = cam.orthographicSize;
        cam.orthographicSize = targetZoom;
        Vector3 clampedTarget = ClampPosition(rawTarget);
        cam.orthographicSize = prevZoom;

        LockUI(true);
        inputLocked = true;

        cam.transform.DOKill();
        DOTween.Kill(cam); // orthographicSize 트윈도 같이 끊고 싶으면(옵션)

        Sequence seq = DOTween.Sequence();
        seq.Join(DOTween.To(() => cam.orthographicSize, x => cam.orthographicSize = x, targetZoom, focusDuration).SetEase(Ease.InOutSine));
        seq.Join(cam.transform.DOMove(clampedTarget, focusDuration).SetEase(Ease.InOutSine));
        seq.OnComplete(() =>
        {
            LockUI(false);
            inputLocked = false;
            OnFocusComplete(hit.transform);
        });
    }

    void OnFocusComplete(Transform target)
    {
        Debug.Log("포커스 완료: " + target.name);
        ButtonManager.Instance.OnTowerDetail(target.name);
    }

    void LockUI(bool state)
    {
        if (uiBlocker != null)
            uiBlocker.SetActive(state);
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

    Vector3 ClampPosition(Vector3 targetPos)
    {
        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * cam.aspect;

        targetPos.x = Mathf.Clamp(targetPos.x, mapMinPosition.x + horzExtent, mapMaxPosition.x - horzExtent);
        targetPos.y = Mathf.Clamp(targetPos.y, mapMinPosition.y + vertExtent, mapMaxPosition.y - vertExtent);
        return targetPos;
    }
}