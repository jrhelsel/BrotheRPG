using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyPathing : MonoBehaviour
{
    [SerializeField]
    Collider visionCone;

    public enum AiState{
        wandering,
        waiting,
        patrolling,
        chasingPlayer
    }
    private AiState aiState;
    
    //State AI goes into when not chasing the player or waiting
    [SerializeField]
    AiState defaultAiState;

    //Tweakable values
    [SerializeField][Range(1, 50)]
    int wanderRadius;
    [SerializeField]
    float wanderWaitTime;
    float wanderWaitTimer;
    [SerializeField]
    float chaseSpeed;
    [SerializeField]
    float wanderSpeed;

    [SerializeField]
    PatrolRoute patrolRoute;
    [SerializeField]
    int currentPatrolPoint;

    Transform player;
    NavMeshAgent agent;
    
    RaycastHit losCheck;
    Vector3 playerDirection;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = gameObject.GetComponent<NavMeshAgent>();
        agent.destination = transform.position;

        aiState = AiState.wandering;
        wanderWaitTimer = 0;
    }

    void Update()
    {
        HandleState();
    }

    
    void HandleState(){
        switch(aiState){
            case AiState.wandering:
                agent.speed = wanderSpeed;
                if(ReachedDestination()){
                    aiState = AiState.waiting;
                }
                break;
            case AiState.patrolling:
                if(ReachedDestination()){
                    aiState = AiState.waiting;
                }
                break;
            case AiState.waiting:
                if(wanderWaitTimer >= wanderWaitTime){
                    //Reset timer, fetch new destination, change to default state
                    wanderWaitTimer = 0;
                    if(defaultAiState == AiState.wandering){
                        agent.destination = RandomNavmeshLocation(wanderRadius);
                    }
                    if(defaultAiState == AiState.patrolling){
                        //Increment current patrol point
                        if(currentPatrolPoint < patrolRoute.PatrolPoints().Length - 1){
                            currentPatrolPoint++;
                        }
                        else{
                            currentPatrolPoint = 0;
                        }

                        //Get next destination from patrol points
                        agent.destination =  patrolRoute.PatrolPoints()[currentPatrolPoint].transform.position;
                    }

                    aiState = defaultAiState;
                }
                else{
                    //Increment timer
                    wanderWaitTimer += Time.deltaTime;
                }

                break;
            case AiState.chasingPlayer:
                agent.speed = chaseSpeed;
                if (CheckLOS()) {
                    agent.destination = player.position;
                }
                else if(ReachedDestination()){
                    //If AI reaches players last known position and does not see him, go back to wandering
                    aiState = defaultAiState;
                    Debug.Log("Lost player, going back to wandering.");
                }
                break;
        }
    }

    //Returns true if agent has reached the end of it's path
    bool ReachedDestination(){
        if (!agent.pathPending){
            if (agent.remainingDistance <= agent.stoppingDistance){
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f){
                return true;
                }
            }
        }
        return false;
    }

    //Fetch a random location on the NavMesh in radius
    public Vector3 RandomNavmeshLocation(float radius) {
         Vector3 randomDirection = Random.insideUnitSphere * radius;
         randomDirection += transform.position;
         NavMeshHit hit;
         Vector3 finalPosition = Vector3.zero;
         if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1)) {
             finalPosition = hit.position;            
         }
         return finalPosition;
     }

    //Check if player is in trigger for aggro
    void OnTriggerStay(Collider other){
        if(other.tag == "Player" && CheckLOS()){
            aiState = AiState.chasingPlayer;
            Debug.Log("Chasing the player!");
        }
    }

    //Checks if there is an uninterrupted raycast from the enemy to the player
    bool CheckLOS(){
        playerDirection = player.position - transform.position;

        if (Physics.Raycast (transform.position, playerDirection, out losCheck)) {
            if (losCheck.transform == player) {
                return true;
            }
        }
        return false;
    }


}
