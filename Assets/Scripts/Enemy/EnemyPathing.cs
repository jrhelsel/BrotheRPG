using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPathing : MonoBehaviour
{
    

    [SerializeField]
    Transform player;
    [SerializeField]
    NavMeshAgent agent;
    
    RaycastHit losCheck;
    Vector3 playerDirection;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
