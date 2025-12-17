// MonsterAnimator.cs
using UnityEngine;

[RequireComponent(typeof(PathMover))]
public class MonsterAnimator : MonoBehaviour
{
    Animator anim; PathMover mover;
    static readonly int Hor = Animator.StringToHash("Horizontal");
    static readonly int Ver = Animator.StringToHash("Vertical");
    static readonly int Walk = Animator.StringToHash("Walk");

    void Awake() { anim = GetComponent<Animator>(); mover = GetComponent<PathMover>(); }
    void Update()
    {
        var f = mover.Facing;
        anim.SetFloat(Hor, f.x);
        anim.SetFloat(Ver, f.y);
        anim.SetBool(Walk, true);
    }
}
