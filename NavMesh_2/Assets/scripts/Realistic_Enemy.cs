using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Realistic_Enemy : MonoBehaviour
{
    public GameObject player;
    public EnemyState state;
    public EnemyState lastState;
    public LayerMask playerMask;

    private NavMeshAgent agent;

    private bool playerInSightView;
    private Vector3 lastPosition;
    private bool lookingAroundEnabled =true;
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if(lastState != state){
            OnStateChanged();
            OnLastStateChanged();
        }
        lastState = state;
        playerInSightView = CheckForVision();
        
        // Debug.Log(lastPosition  +" != "+  transform.position);
        // Debug.Log(Vector3.Distance(lastPosition,transform.position));
        Debug.Log((Vector3.Distance(lastPosition,transform.position) < 4f) +" && " +(state==EnemyState.EndChasing));
        if (playerInSightView){
            state =EnemyState.Chasing;
            lookingAroundEnabled=false;
        } else if (Vector3.Distance(lastPosition,transform.position) > 4f && state== EnemyState.Chasing){
            //Debug.Log("Distancia: "+Vector3.Distance(lastPosition,transform.position));
            //Debug.Log("direction: " + agent.destination);
            state =EnemyState.EndChasing;
        } else if (Vector3.Distance(lastPosition,transform.position) < 4f && state==EnemyState.EndChasing){
            Debug.Log("Entra");
            lookingAroundEnabled = true;
            state = EnemyState.LookingAround;
        }
        
        StartCoroutine("CheckForState");
    }

    private IEnumerator CheckForState(){
        switch(state){
            case EnemyState.EndChasing:
            //al NavMeshAgent no le gusta mandarle varias veces al mismo punto
                // agent.destination = lastPosition;
                yield return new WaitForSeconds(0.05f);
                break;
            case EnemyState.LookingAround:
                //lastPosition = transform.position;
                yield return new WaitForSeconds(0.05f);
                break;
            case EnemyState.Chasing:
                agent.destination = player.transform.position;
                
                //esto evita que el enemigo se quede quieto
                yield return new WaitForSeconds(0.05f);
                break;
            case EnemyState.Catching:
                break;
        }
        yield return null;
    }
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position,25);
        Gizmos.DrawLine(transform.position,agent.destination);
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x+15,transform.position.y,transform.position.z+20));
        Gizmos.DrawLine(transform.position,new Vector3(transform.position.x-15,transform.position.y,transform.position.z+20));

    }
    bool CheckForVision(){
        if (Physics.CheckSphere(transform.position,25,playerMask)){//phisics.SphereOverlapse puede ser una mejor solucion para no tener que tener la referencia a la layermask
            RaycastHit hit;
            if (Physics.Raycast(transform.position+Vector3.up,player.transform.position-transform.position, out hit,(player.transform.position-transform.position).magnitude)){
                if(hit.collider.gameObject.CompareTag("Player")){
                        return CheckForAngle(hit.transform);
                }
                return false;
            }
            return false;
        } else {
            return false;
        }
    }

    public bool CheckForAngle(Transform hit){
        if(hit.position.z <= transform.position.z+10 && hit.transform.position.z >= transform.position.z-10 && hit.position.x <= transform.position.x+3 && hit.position.x >= transform.position.x-20){
            Debug.Log("Encontrado en: " + hit.transform + " estando en: " + transform.position);
            return true;
        }
        return false;
    }
    public void OnStateChanged(){}
    public void OnLastStateChanged(){}
}
