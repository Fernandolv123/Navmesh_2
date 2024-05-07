using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public GameObject player;
    public EnemyState state;
    private bool playerInSightView;
    public LayerMask playerMask;
    private NavMeshAgent agent;
    private Vector3 lastPosition;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition= transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightView = Physics.CheckSphere(transform.position,25,playerMask);
        
        // Debug.Log(Vector3.Distance(lastPosition,transform.position));
        //Debug.Log(lastPosition  !=  transform.position);
        if (playerInSightView){
            state =EnemyState.Chasing;
        } else if (Vector3.Distance(lastPosition,transform.position) > 5){
            state =EnemyState.EndChasing;
        } else {
            state = EnemyState.LookingAround;
        }
        CheckForState();
    }
    
    private void CheckForState(){
        // Debug.Log(state);
        switch(state){
            case EnemyState.EndChasing:
                agent.destination = lastPosition;
                break;
            case EnemyState.LookingAround:
                lastPosition = transform.position;
                break;
            case EnemyState.Chasing:
                agent.destination = player.transform.position;
                break;
            case EnemyState.Catching:
                break;
        }
    }
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position,25);
        Gizmos.DrawLine(transform.position,player.transform.position);
    }
}
public enum EnemyState{
    LookingAround,
    Chasing,
    EndChasing,
    Sleep,
    Catching
}
