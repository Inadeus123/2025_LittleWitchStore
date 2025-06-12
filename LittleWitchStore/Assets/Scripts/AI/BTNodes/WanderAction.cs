using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class WanderAction : Action
{
    public Cat cat;
    public SharedFloat wanderRadius = 25.0f;
    
    private NavMeshAgent agent;
    private Vector3 destination;
    private bool hasDestination;
    
    //动画控制
    private Animator animator;
    

    public override void OnStart()
    {
        cat = gameObject.GetComponent<Cat>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        hasDestination = false;

        //设置动画参数
        if (animator != null)
        {
            animator.SetBool(cat.animationData.WalkingParameterHash, true);
        }
    }
    
    public override TaskStatus OnUpdate()
    {
        if (!hasDestination)
        {
            //随便找一个NavMesh上的点
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius.Value;
            randomDirection += transform.position;
            randomDirection.y = 0;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, wanderRadius.Value, NavMesh.AllAreas))
            {
                destination = hit.position;
                agent.SetDestination(destination);
                hasDestination = true;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
        
        //如果到了目标点位
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            return TaskStatus.Success;
        }
            
        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        animator.SetBool(cat.animationData.WalkingParameterHash, false);
    }
}
