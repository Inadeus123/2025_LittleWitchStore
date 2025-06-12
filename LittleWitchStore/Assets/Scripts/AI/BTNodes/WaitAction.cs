using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

public class WaitAction : Action
{
    public Cat cat;
    public SharedFloat waitTime = 4f;
    private float waitCounter;
    
    public Animator animator;

    public override void OnStart()
    {
        waitCounter = waitTime.Value;
        cat = gameObject.GetComponent<Cat>();
        animator = GetComponent<Animator>();
        animator.SetBool(cat.animationData.IdlingParameterHash, true);

    }

    public override TaskStatus OnUpdate()
    {
        waitCounter -= Time.deltaTime;
        if (waitCounter <= 0) return TaskStatus.Success;
        return TaskStatus.Running;
    }
    
    public override void OnEnd()
    {
        animator.SetBool(cat.animationData.IdlingParameterHash, false);
    }
}
