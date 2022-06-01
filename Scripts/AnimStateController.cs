using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Carousel{
    
namespace BaselineAgent{
    
public class AnimStateController : MonoBehaviour
{
    Animator animator;
    public float velocity =0f;
    public float direction =0.5f;
    public bool isDancing;
    public bool applyRootMotion;
    int velocityHash;
    int directionHash;
    int isDancingHash;
    Vector3 prevPos;
    public List<AnimatorOverrideController> animationOverriders;


    void Awake()
    {
        animator = GetComponent<Animator>();
        velocityHash = Animator.StringToHash("velocity");
        directionHash = Animator.StringToHash("direction");
        isDancingHash = Animator.StringToHash("isDancing");
        prevPos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        var delta =transform.position -prevPos;
        velocity = delta.magnitude;
        animator.SetFloat(velocityHash, velocity*10);
        animator.SetFloat(directionHash, direction);
        animator.SetBool(isDancingHash, isDancing);
        animator.applyRootMotion = applyRootMotion;
        prevPos = transform.position;
    }
    
    public void ToggleDancing(){
        print("dance "+name);
        isDancing = !isDancing;
        applyRootMotion = isDancing;
        if (animationOverriders.Count == 0)return;
        int idx = Random.Range(0, animationOverriders.Count);
        animator.runtimeAnimatorController = animationOverriders[idx];
    }
}

}
}