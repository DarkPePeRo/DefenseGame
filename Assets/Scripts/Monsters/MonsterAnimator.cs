using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PathMover))]
public class MonsterAnimator : MonoBehaviour
{
    Animator anim; PathMover mover;

    static readonly int Hor = Animator.StringToHash("Horizontal");
    static readonly int Ver = Animator.StringToHash("Vertical");
    static readonly int State = Animator.StringToHash("State");

    Coroutine dieCo;
    bool isDead;

    void Awake() { anim = GetComponent<Animator>(); mover = GetComponent<PathMover>(); }

    private void Start() => anim.SetInteger(State, 2);

    private void OnEnable()
    {
        isDead = false;
        dieCo = null;
        anim.SetInteger(State, 2);
    }

    void Update()
    {
        if (isDead) return;

        var f = mover.Facing;
        anim.SetFloat(Hor, f.x);
        anim.SetFloat(Ver, f.y);
    }

    public void PlayHit(float duration = 0.2f)
    {
        if (isDead) return;

        StopAllCoroutines();
        StartCoroutine(HitRoutine(duration));
    }

    IEnumerator HitRoutine(float duration)
    {
        anim.SetInteger(State, 4);
        yield return new WaitForSeconds(duration);
        if (!isDead) anim.SetInteger(State, 2);
    }
    public void PlayDie(Action onFinished, float duration = 1f)
    {
        if (isDead) return;

        isDead = true;
        StopAllCoroutines();
        dieCo = StartCoroutine(DieRoutine(onFinished, duration));
    }

    IEnumerator DieRoutine(Action onFinished, float duration)
    {
        anim.SetInteger(State, 9);
        yield return new WaitForSeconds(duration);
        onFinished?.Invoke();
    }
}