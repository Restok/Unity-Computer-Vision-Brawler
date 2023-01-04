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
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position + player.forward;
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetFloat("Distance", (agent.destination-transform.position).magnitude);
        agent.isStopped = false;
        //Stopping

        if (agent.velocity.magnitude == 0)
        {
            //Face the target
            var lookPos = player.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * speed);
            //Isn't punching
            Debug.Log(agent.speed);
            //Currently is not punching
            if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Punch"))
            {
                animator.SetTrigger("TrPunch");
            }
            else
            {
                agent.isStopped = true;
            }
        }
    }

}
