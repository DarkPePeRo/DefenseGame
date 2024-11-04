using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DemoPlayer : MonoBehaviour
{
    public float speed;
    public float timer;
    public float waitingTimeF;
    public float waitingTimeL;
    public float waitingTimeLL;

    public float HP = 100;

    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");

    private Vector2 _playerRotation = Vector2.zero;
    public Animator _animator;
    private bool _initialized;

    public Vector3 dir;

    private Transform target;
    private int wavepointIndex = 0; //maximum 5

    // Start is called before the first frame update
    void Start()
    {

        _animator = GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("[DemoPlayer.cs] Can't find 'Animator' component in scene." +
                           " Add character prefab with 'Animator' component attached to it in the scene");
            return;
        }


        _initialized = true;


        target = Waypoints.points[0]; //Enemy의 target으로 WayPoint로 지정 
    }


    // Update is called once per frame
    void Update()
    {
        if(!_initialized)
            return;
        timer += Time.deltaTime;

        dir = target.position - transform.position;
        if (Vector3.Distance(transform.position, target.position) <= 0.2f)
        {
            GetNextWayPoint();
        }
        if (timer < waitingTimeF)
        {
            transform.Translate(dir.normalized * speed * 0.7f * Time.deltaTime, Space.World);
            _animator.SetBool("Walk", true);
        }
        if (timer > waitingTimeF && timer < waitingTimeL)
        {
            transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);
            _animator.SetBool("Walk", true);
        }
        if (timer > waitingTimeL)
        {
            timer = 0;
            HP -= 10;
            _animator.SetBool("Hit", true);
            _animator.SetBool("Walk", false);
        }
        else if (timer > waitingTimeL && timer < waitingTimeLL)
        {
        }
        else
        {
            _animator.SetBool("Hit", false);
            _animator.SetBool("Walk", true);
        }
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
        PlayerDir();
        UpdateParams();

    }

    private void UpdateParams()
    {
        _animator.SetFloat(Horizontal, _playerRotation.x);
        _animator.SetFloat(Vertical, _playerRotation.y);
    }

    private void GetNextWayPoint()
    {
        if (wavepointIndex >= Waypoints.points.Length - 1)
        {
            speed = 0;
            return;
        }
        wavepointIndex++;
        target = Waypoints.points[wavepointIndex];
    }

    private void PlayerDir()
    {
        if(dir.normalized.x > 0)
        {
            _playerRotation.x = 1;
        }
        else _playerRotation.x = -1;
        if (dir.normalized.y > 0)
        {
            _playerRotation.y = 1;
        }
        else _playerRotation.y = -1;
    }
}