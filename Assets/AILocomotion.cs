using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AILocomotion : MonoBehaviour
{
  // Start is called before the first frame update
    NavMeshAgent agent;
    public Transform player;
    Animator animator;
    public float speed = 1f;
    public RootMotion.Demos.FBIKBoxing boxingController;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position + (player.forward);
        animator.SetFloat("Speed", agent.velocity.magnitude);
        agent.isStopped = false;
        Vector3 distance = player.position - transform.position;
        distance.y = 0;
        animator.SetFloat("Distance", distance.magnitude);
        //Stopping
        if (agent.velocity.magnitude == 0)
        {
            //Face the target
            var lookPos = player.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);

            //Currently is not punching
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Punch"))
            {
                if (distance.magnitude < 1.5f)
                {
                    agent.isStopped = true;
                    animator.SetTrigger("TrPunch");
                    boxingController.SetHeadPosition();
                }
                else
                {
                    agent.isStopped = false;
                }
            }
            //Is punching
            else
            {
                agent.isStopped = true;
            }
        }
    }

}
