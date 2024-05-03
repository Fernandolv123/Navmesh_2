using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_WithVision : MonoBehaviour
{
    public GameObject player;
    public EnemyState state;
    private bool playerInSightView;
    public LayerMask playerMask;
    private NavMeshAgent agent;
    private Vector3 lastPosition;

    public Transform wanderingPoint1;
    public Transform wanderingPoint2;
    public int round = 3;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition= transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        playerInSightView = CheckForVision();
        
        Debug.Log(lastPosition  +" != "+  transform.position);
        Debug.Log(Vector3.Distance(lastPosition,transform.position));

        if (playerInSightView){
            state =EnemyState.Chasing;
        } else if (Vector3.Distance(lastPosition,transform.position) > 1){
            state =EnemyState.EndChasing;
        } else {
            state = EnemyState.LookingAround;
        }
        CheckForState();
    }
    
    private void CheckForState(){
        Debug.Log(state);
        switch(state){
            case EnemyState.EndChasing:
                agent.destination = lastPosition;
                break;
            case EnemyState.LookingAround:
                if(round ==3 ) {
                    agent.destination = wanderingPoint1.position;
                    round =1;
                }//(first time)
                if(agent.remainingDistance >= 1 && round==2){
                    agent.destination = wanderingPoint1.position;
                    round=1;
                }
                if(agent.remainingDistance >= 1 && round==1){
                    agent.destination = wanderingPoint2.position;
                    round=2;
                }

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
    bool CheckForVision(){
        if (Physics.CheckSphere(transform.position,25,playerMask)){//phisics.SphereOverlapse puede ser una mejor solucion para no tener que tener la referencia a la layermask
            RaycastHit hit;
            if (Physics.Raycast(transform.position+Vector3.up,player.transform.position-transform.position, out hit,(player.transform.position-transform.position).magnitude)){
                Debug.Log(hit.collider.name);
                if(hit.collider.gameObject.CompareTag("Player"))return true;
                return false;
            }
            return false;
        } else {
            return false;
        }
    }
}