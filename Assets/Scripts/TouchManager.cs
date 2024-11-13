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
    public Vector2 mapMinPosition; // ���� �ּ� ��ġ (���� ��ǥ ����)
    public Vector2 mapMaxPosition; // ���� �ִ� ��ġ (���� ��ǥ ����)
    //�� �ӵ� ����
    private float perspectiveZoomSpeed = 0.008f;

    private float orthoZoomSpeed = 0.0001f;

    //�̵� �ӵ�
    public float moveSpeed = 0.001f;
    //ȸ�� �ӵ�
    private float rotateSpeed = 0.1f;
    private const float ZoomSpeed = 1.0f; // �ѹ��� �� �Է��� �� �Ǵ� ����
    private const float MinZoomSize = 2.0f; // �ּ� ī�޶� ������
    private const float MaxZoomSize = 3.5f; //  �ִ� ī�޶� ������

    private const float DirectionForceReduceRate = 0.935f; // ���Ӻ���
    private const float DirectionForceMin = 0.1f; // ����ġ ������ ��� �������� ����

    // ���� : �̵� ����
    private bool _userMoveInput; // ���� ������ �ϰ��ִ��� Ȯ���� ���� ����
    private Vector3 _startPosition;  // �Է� ���� ��ġ�� ���
    public Vector3 _directionForce; // ������ �������� ������ �����ϸ鼭 �̵� ��Ű�� ���� ����

    public bool isActive = false;
    public bool isActiveZoom = false;
    public Vector3 target;
    void LateUpdate()
    {
        // UI�� �巡�� ���̸� ī�޶� ��� ���� ����
        if (IsPointerOverUIObject())
        {
            return;
        }
        // ���콺 �� ī�޶� ��
        ControllerZoom();

        // ī�޶� ������ �̵�
        ControlCameraPosition();

        // ������ �������� ����
        ReduceDirectionForce();

        // ī�޶� ��ġ ������Ʈ
        UpdateCameraPosition();

        //// ��ġ �� ī�޶� �̵� ����
        //TouchCameraMove();
    }
    private bool IsPointerOverUIObject()
    {
        // ���콺 �����ͳ� ��ġ ��ġ�� UI ��� ���� �ִ��� Ȯ��
        return EventSystem.current.IsPointerOverGameObject();
    }
    private void ControllerZoom()
    {
        // ���콺 ��ũ�� �Է� �ޱ�
        var scrollInput = Input.GetAxis("Mouse ScrollWheel");
        var hasScrollInput = Mathf.Abs(scrollInput) > Mathf.Epsilon;
        if (!hasScrollInput)
        {
            return;
        }

        // ī�޶� ũ�⸦ ���콺 ��ũ�� �Է¿� ���� �����Ͽ� Ȯ��/���
        var newSize = GetComponent<Camera>().orthographicSize - scrollInput * ZoomSpeed;

        // ī�޶� ũ�� ���� �ּҰ��� �ִ밪 ���̷� ����
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
        // ���� ���϶��� �ƹ��͵� ����
        if (_userMoveInput)
        {
            return;
        }

        // ���� ��ġ ����
        _directionForce *= DirectionForceReduceRate;

        // ���� ��ġ�� �Ǹ� ������ ����
        if (_directionForce.magnitude < DirectionForceMin)
        {
            _directionForce = Vector3.zero;
        }
    }
    private void UpdateCameraPosition()
    {
        // �̵� ��ġ�� ������ �ƹ��͵� ����
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
        // ī�޶��� orthographicSize�� ��Ⱦ�� ���� ���� ���� ���
        Camera cam = GetComponent<Camera>();
        float cameraHeight = cam.orthographicSize * 2;
        float cameraWidth = cameraHeight * cam.aspect;

        // ī�޶� �̵� ���� ���
        float minX = mapMinPosition.x + cameraWidth / 2;
        float maxX = mapMaxPosition.x - cameraWidth / 2;
        float minY = mapMinPosition.y + cameraHeight / 2;
        float maxY = mapMaxPosition.y - cameraHeight / 2;

        // ī�޶��� ���� ��ġ�� ���� �̵��� ���
        var currentPosition = transform.position;
        var targetPosition = currentPosition + _directionForce;

        // ī�޶� ��ġ�� ���� ���� ���� ����
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // ���ѵ� ��ġ�� ī�޶� �̵�
        transform.position = Vector3.Lerp(currentPosition, targetPosition, 0.5f);
    }

    //private void TouchCameraMove()
    //{
    //    ;
    //    // ������Ʈ Ŭ�� ���ο� ���� ó��
    //    if (Input.touchCount == 1)
    //    {
    //        Touch touch = Input.GetTouch(0);

    //        if (touch.phase == TouchPhase.Began)
    //        {
    //            // ��ġ�� ���۵� �� Ŭ���� ������Ʈ�� Ȯ��
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

    //        //�浹�� ����
    //        if (isObjectSelected)
    //        {
    //            //��ġ ������ ����
    //            switch (touch.phase)
    //            {
    //                //��ġ�� �����̸�
    //                case TouchPhase.Moved:
    //                    //��ġ�� �̵����� ����
    //                    Vector2 touchDelta = touch.deltaPosition;

    //                    //x�Ǵ� y�� �ϳ��� 0�� �ƴϸ� ȸ��(�����̸� ȸ��)
    //                    if (touchDelta.x != 0 || touchDelta.y != 0)
    //                    {
    //                    }
    //                    break;
    //            }
    //        }

    //        //�浹X
    //        else
    //        {
    //            switch (touch.phase)
    //            {
    //                //��ġ�� �̵��ϸ�
    //                case TouchPhase.Moved:
    //                    Vector2 touchDelta = touch.deltaPosition;

    //                    //��ġ�� �̵��ߴ��� Ȯ��
    //                    if (touchDelta.x != 0 || touchDelta.y != 0)
    //                    {
    //                        //ī�޶� ��ġ�� �̵����� ����� x�� y�������� �̵��ϰ�, �̵����� moveSpeed�� ����Ѵ�.
    //                        //-�� ���� ������ ����Ƽ ��ǥ�迡�� ī�޶� �����̴� ������ �ݴ�� �ϱ� ���ؼ���
    //                        transform.Translate(-touchDelta.x * moveSpeed, -touchDelta.y * moveSpeed, 0);
    //                    }
    //                    break;
    //            }
    //        }
    //    }

    //    //���˵Ǿ� �ִ� �հ��� ������ 2�϶�(�� �հ����� ��ġ������)
    //    else if (Input.touchCount == 2)
    //    {
    //        //ù��° ��ġ ���� ����
    //        Touch touchZero = Input.GetTouch(0);
    //        //�ι�° ��ġ ���� ����
    //        Touch touchOne = Input.GetTouch(1);

    //        //���� ��ġ�� ���� ��ġ�� ����Ѵ�.
    //        Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
    //        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

    //        //(������-������ġ).magnitude : ���� �Ÿ�
    //        //���� ��ġ�� ���� ��ġ���� �Ÿ��� ����Ѵ�
    //        float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
    //        float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

    //        //�� �հ��� ���� �Ÿ��� ����Ѵ�
    //        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

    //        //orthographic����϶�
    //        if (GetComponent<Camera>().orthographic)
    //        {
    //            GetComponent<Camera>().orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
    //            GetComponent<Camera>().orthographicSize = Mathf.Max(GetComponent<Camera>().orthographicSize, 0.1f);
    //        }
    //        //fieldOfView����϶�
    //        else
    //        {
    //            GetComponent<Camera>().fieldOfView += deltaMagnitudeDiff * perspectiveZoomSpeed;
    //            GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView, 0.1f, 179.9f);
    //        }
    //    }
    //}
}