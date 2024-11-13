using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

public class TouchManager : MonoBehaviour
{
    public Vector2 mapMinPosition; // 맵의 최소 위치 (월드 좌표 기준)
    public Vector2 mapMaxPosition; // 맵의 최대 위치 (월드 좌표 기준)
    //줌 속도 변수
    private float perspectiveZoomSpeed = 0.008f;

    private float orthoZoomSpeed = 0.0001f;

    //이동 속도
    public float moveSpeed = 0.001f;
    //회전 속도
    private float rotateSpeed = 0.1f;
    private const float ZoomSpeed = 1.0f; // 한번의 줌 입력의 줌 되는 정도
    private const float MinZoomSize = 2.0f; // 최소 카메라 사이즈
    private const float MaxZoomSize = 3.5f; //  최대 카메라 사이즈

    private const float DirectionForceReduceRate = 0.935f; // 감속비율
    private const float DirectionForceMin = 0.1f; // 설정치 이하일 경우 움직임을 멈춤

    // 변수 : 이동 관련
    private bool _userMoveInput; // 현재 조작을 하고있는지 확인을 위한 변수
    private Vector3 _startPosition;  // 입력 시작 위치를 기억
    public Vector3 _directionForce; // 조작을 멈췄을때 서서히 감속하면서 이동 시키기 위한 변수

    public bool isActive = false;
    public bool isActiveZoom = false;
    public Vector3 target;
    void LateUpdate()
    {
        // UI를 드래그 중이면 카메라 제어를 하지 않음
        if (IsPointerOverUIObject())
        {
            return;
        }
        // 마우스 휠 카메라 줌
        ControllerZoom();

        // 카메라 포지션 이동
        ControlCameraPosition();

        // 조작을 멈췄을때 감속
        ReduceDirectionForce();

        // 카메라 위치 업데이트
        UpdateCameraPosition();

        //// 터치 시 카메라 이동 관련
        //TouchCameraMove();
    }
    private bool IsPointerOverUIObject()
    {
        // 마우스 포인터나 터치 위치가 UI 요소 위에 있는지 확인
        return EventSystem.current.IsPointerOverGameObject();
    }
    private void ControllerZoom()
    {
        // 마우스 스크롤 입력 받기
        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        var hasScrollInput = Mathf.Abs(scrollInput) > Mathf.Epsilon;
        if (!hasScrollInput)
        {
            return;
        }

        // 카메라 크기를 마우스 스크롤 입력에 따라 변경하여 확대/축소
        var newSize = GetComponent<Camera>().orthographicSize - scrollInput * ZoomSpeed;

        // 카메라 크기 값을 최소값과 최대값 사이로 유지
        GetComponent<Camera>().orthographicSize = Mathf.Clamp(newSize, MinZoomSize, MaxZoomSize);
    }
    private void ControlCameraPosition()
    {
        var mouseWorldPosition = GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            CameraPositionMoveStart(mouseWorldPosition);
        }
        else if (Input.GetMouseButton(0))
        {
            CameraPositionMoveProgress(mouseWorldPosition);
        }
        else
        {
            CameraPositionMoveEnd();
        }
    }
    private void CameraPositionMoveStart(Vector3 startPosition)
    {
        _userMoveInput = true;
        _startPosition = startPosition;
        _directionForce = Vector2.zero;
    }
    private void CameraPositionMoveProgress(Vector3 targetPosition)
    {
        if (!_userMoveInput)
        {
            CameraPositionMoveStart(targetPosition);
            return;
        }

        _directionForce = _startPosition - targetPosition;
    }
    private void CameraPositionMoveEnd()
    {
        _userMoveInput = false;
    }
    private void ReduceDirectionForce()
    {
        // 조작 중일때는 아무것도 안함
        if (_userMoveInput)
        {
            return;
        }

        // 감속 수치 적용
        _directionForce *= DirectionForceReduceRate;

        // 작은 수치가 되면 강제로 멈춤
        if (_directionForce.magnitude < DirectionForceMin)
        {
            _directionForce = Vector3.zero;
        }
    }
    private void UpdateCameraPosition()
    {
        // 이동 수치가 없으면 아무것도 안함
        Vector2 worldpoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(worldpoint, Vector2.zero);

        if (Input.GetMouseButtonUp(0) && hit.collider != null && _directionForce == Vector3.zero)
        {
            Debug.Log(hit.collider.name);
            Debug.Log(hit.collider.transform.position);
            target = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y + 0.5f, -10);
            isActive = true;
            isActiveZoom = true;
        }
        if (isActive)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, target, Time.deltaTime * 4);
            if (Vector3.Distance(this.transform.position, target) < 0.2f)
            {
                isActive = false;
            }
        }
        if (isActiveZoom)
        {
            GetComponent<Camera>().orthographicSize = Mathf.Lerp(GetComponent<Camera>().orthographicSize, 2.5f, Time.deltaTime * 5);
            if (GetComponent<Camera>().orthographicSize < 2.6f)
            {
                isActiveZoom = false;
            }
        }
        // 카메라의 orthographicSize와 종횡비에 따른 가시 범위 계산
        Camera cam = GetComponent<Camera>();
        float cameraHeight = cam.orthographicSize * 2;
        float cameraWidth = cameraHeight * cam.aspect;

        // 카메라 이동 범위 계산
        float minX = mapMinPosition.x + cameraWidth / 2;
        float maxX = mapMaxPosition.x - cameraWidth / 2;
        float minY = mapMinPosition.y + cameraHeight / 2;
        float maxY = mapMaxPosition.y - cameraHeight / 2;

        // 카메라의 현재 위치와 방향 이동값 계산
        var currentPosition = transform.position;
        var targetPosition = currentPosition + _directionForce;

        // 카메라 위치를 가시 범위 내로 제한
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // 제한된 위치로 카메라 이동
        transform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    //private void TouchCameraMove()
    //{
    //    ;
    //    // 오브젝트 클릭 여부에 따라 처리
    //    if (Input.touchCount == 1)
    //    {
    //        Touch touch = Input.GetTouch(0);

    //        if (touch.phase == TouchPhase.Began)
    //        {
    //            // 터치가 시작될 때 클릭된 오브젝트를 확인
    //            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

    //            if (hit.collider != null)
    //            {
    //                Debug.Log(hit.collider.name);
    //                Debug.Log(hit.collider.transform.position);
    //                Vector3.Lerp(this.transform.position, hit.collider.transform.position, moveSpeed);
    //                this.transform.position = hit.collider.transform.position;
    //                isObjectSelected = true;
    //            }
    //            else
    //            {
    //            }
    //        }

    //        //충돌이 나면
    //        if (isObjectSelected)
    //        {
    //            //터치 정보에 따라
    //            switch (touch.phase)
    //            {
    //                //터치가 움직이면
    //                case TouchPhase.Moved:
    //                    //터치의 이동량을 저장
    //                    Vector2 touchDelta = touch.deltaPosition;

    //                    //x또는 y가 하나라도 0이 아니면 회전(움직이면 회전)
    //                    if (touchDelta.x != 0 || touchDelta.y != 0)
    //                    {
    //                    }
    //                    break;
    //            }
    //        }

    //        //충돌X
    //        else
    //        {
    //            switch (touch.phase)
    //            {
    //                //터치가 이동하면
    //                case TouchPhase.Moved:
    //                    Vector2 touchDelta = touch.deltaPosition;

    //                    //터치가 이동했는지 확인
    //                    if (touchDelta.x != 0 || touchDelta.y != 0)
    //                    {
    //                        //카메라 터치의 이동량에 비례해 x와 y방향으로 이동하고, 이동량은 moveSpeed에 비례한다.
    //                        //-를 붙인 이유는 유니티 좌표계에서 카메라가 움직이는 방향을 반대로 하기 위해서임
    //                        transform.Translate(-touchDelta.x * moveSpeed, -touchDelta.y * moveSpeed, 0);
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    //접촉되어 있는 손가락 개수가 2일때(두 손가락을 터치했을때)
    //    else if (Input.touchCount == 2)
    //    {
    //        //첫번째 터치 정보 저장
    //        Touch touchZero = Input.GetTouch(0);
    //        //두번째 터치 정보 저장
    //        Touch touchOne = Input.GetTouch(1);

    //        //각각 터치의 이전 위치를 계산한다.
    //        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

    //        //(목적지-현재위치).magnitude : 남은 거리
    //        //이전 위치와 현재 위치간의 거리를 계산한다
    //        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

    //        //두 손가락 간의 거리를 계산한다
    //        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

    //        //orthographic모드일때
    //        if (GetComponent<Camera>().orthographic)
    //        {
    //            GetComponent<Camera>().orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
    //            GetComponent<Camera>().orthographicSize = Mathf.Max(GetComponent<Camera>().orthographicSize, 0.1f);
    //        }
    //        //fieldOfView모드일때
    //        else
    //        {
    //            GetComponent<Camera>().fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
    //            GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, 0.1f, 179.9f);
    //        }
    //    }
    //}
}