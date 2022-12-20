using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    [SerializeField]
    GameObject[] patrolPoints;

    public GameObject[] PatrolPoints(){
        return patrolPoints;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
