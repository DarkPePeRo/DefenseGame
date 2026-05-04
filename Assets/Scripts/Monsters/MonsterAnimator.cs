using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(PathMover))]
public class MonsterAnimator : MonoBehaviour
{
    Animator anim; PathMover mover;

    static readonly int Hor = Animator.StringToHash("Horizontal");
    static readonly int Ver = Animator.StringToHash("Vertical");
    static readonly int State = Animator.StringToHash("State"); 
    static readonly int Attack = Animator.StringToHash("Attack");
    private SpriteRenderer sprite;

    Coroutine dieCo;
    bool isDead;

    void Awake() { 
        anim = GetComponent<Animator>(); 
        mover = GetComponent<PathMover>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start() => anim.SetInteger(State, 2);

    private void OnEnable()
    {
        isDead = false;
        dieCo = null;
        anim.SetInteger(State, 2);
        if (mover.def.monsterId != "WolfA")
        {
            sprite.color = new Color(1, 1, 1, 1);
        }
    }

    void Update()
    {
        if (isDead) return;

        var f = mover.Facing;
        anim.SetFloat(Hor, f.x);
        anim.SetFloat(Ver, f.y);
        if (mover.IsStopped)
        {
            PlayAttack();
        }
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
        if(mover.def.monsterId != "WolfA")
        {
            sprite.color = new Color(1, 205 / 255f, 205 / 255f, 1);
            yield return new WaitForSeconds(0.03f);
            sprite.color = new Color(1, 1, 1, 1);
        }
        yield return new WaitForSeconds(duration - 0.03f);
        if (!isDead) anim.SetInteger(State, 2);
    }

    public void PlayAttack(float duration = 0.2f)
    {
        if (isDead) return;

        StopAllCoroutines();
        StartCoroutine(AttackRoutine(duration));
    }

    IEnumerator AttackRoutine(float duration)
    {
        anim.SetBool(Attack, true);
        yield return new WaitForSeconds(duration);
        anim.SetBool(Attack, false);
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