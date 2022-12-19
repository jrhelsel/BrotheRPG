using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathing : MonoBehaviour
{
    

    Transform player;
    NavMeshAgent agent;
    
    RaycastHit losCheck;
    Vector3 playerDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = gameObject.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        playerDirection = player.position - transform.position;

        if (Physics.Raycast (transform.position, playerDirection, out losCheck)) {
            if (losCheck.transform == player) {
                agent.destination = player.position;
            }
        }
    }
}
