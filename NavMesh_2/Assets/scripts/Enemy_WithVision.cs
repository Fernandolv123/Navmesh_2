using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_WithVision : MonoBehaviour
{
    public GameObject player;
    public EnemyState state;
    public EnemyState lastState;

    public float detectionRadius=25f;

    private bool playerInSightView;
    public LayerMask playerMask;
    private NavMeshAgent agent;
    public Vector3 lastPosition;
    public List<Transform> wanderingPoints;

    private bool lookingAroundEnabled =true;
    private Transform activeWayPoint;
    public GameObject playerLost;
    public GameObject playerFound;

    // Start is called before the first frame update
    void Start()
    {
        activeWayPoint = wanderingPoints[0];
        agent = GetComponent<NavMeshAgent>();
        lastPosition= transform.position;
        StartCoroutine("LookingAround");
    }

    // Update is called once per frame
    void Update()
    {
        //Gestor de estados
        if(lastState != state){
            OnStateChanged();
            OnLastStateChanged();
        }
        lastState = state;

        playerInSightView = CheckForVision();
        
        if (playerInSightView && state != EnemyState.Sleep){
            state =EnemyState.Chasing;
            lookingAroundEnabled=false;
        } else if (Vector3.Distance(lastPosition,transform.position) > 4f && state== EnemyState.Chasing){
            state =EnemyState.EndChasing;
        } else if (Vector3.Distance(lastPosition,transform.position) < 4f && state==EnemyState.EndChasing){
            lookingAroundEnabled = true;
            state = EnemyState.LookingAround;
        }
        StartCoroutine("CheckForState");
    }
    
    private IEnumerator CheckForState(){
        switch(state){
            case EnemyState.EndChasing:
                break;
            case EnemyState.LookingAround:
                break;
            case EnemyState.Chasing:
                //Debemos de actualizar el seguimiento del jugador en cada frame
                agent.destination = player.transform.position;
                break;
            case EnemyState.Catching:
                break;
        }
        yield return null;
    }
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position,detectionRadius);
        Gizmos.DrawLine(transform.position,agent.destination);
    }
    bool CheckForVision(){
        //Collider[] = Physics.OverlapSphere este método devuelve los colliders detectados
        if (Physics.CheckSphere(transform.position,detectionRadius,playerMask)){
            RaycastHit hit;
            if (Physics.Raycast(transform.position+Vector3.up,player.transform.position-transform.position, out hit,(player.transform.position-transform.position).magnitude)){
                if(hit.collider.gameObject.CompareTag("Player"))return true;
                return false;
            }
            return false;
        } else {
            return false;
        }
    }

    private IEnumerator LookingAround(){
        while (lookingAroundEnabled){
            yield return null;
            for (int i = wanderingPoints.IndexOf(activeWayPoint) == wanderingPoints.Count-1 ? 0 : wanderingPoints.IndexOf(activeWayPoint); i<=wanderingPoints.Count-1;i++){
                if(!lookingAroundEnabled) break;
                activeWayPoint = wanderingPoints[i];
                agent.destination = wanderingPoints[i].position;
                yield return new WaitForSeconds(0.05f);
                yield return new WaitUntil(() => agent.remainingDistance <=1);
                if(agent.destination != (lastPosition - Vector3.up*4)) state = EnemyState.Sleep;
            }
        }
        if(!lookingAroundEnabled){
            yield return new WaitUntil(() => lookingAroundEnabled);
            StartCoroutine("LookingAround");
        }
    }
    public void OnLastStateChanged(){
        //OnLastStateChanged se llama con el estado previo al cambio
        //Ej: si el cambio es LookingAround => Chasing el valor utilizado es LookingAround
        GameObject go;
        switch(lastState){
            case EnemyState.Chasing:
            go =Instantiate(playerLost,transform.position+Vector3.up*4,Quaternion.identity);
            go.transform.parent = transform;
            agent.isStopped=true;
            Invoke("ReturnPath",2f);
            break;
            case EnemyState.LookingAround:
            //si el estado anterior, estaba patrullando, se cambia la ultima posicion
            lastPosition = transform.position;
            break;
            case EnemyState.Sleep:
            agent.isStopped=false;
            break;

        }
    }
    public void OnStateChanged(){
        //OnStateChanged se llama con el valor al que se ha cambiado
        //Ej: si el cambio es LookingAround => Chasing el valor utilizado es Chasing
        GameObject go;
        switch(state){
            case EnemyState.Chasing:
                //si el nuevo estado es Chasing, se mostrara el gameobject de player encontrado
                go = Instantiate(playerFound,transform.position+Vector3.up*4,Quaternion.identity);
                go.transform.parent = transform;
                agent.isStopped=true;
                //velocidad en Seguimiento
                agent.speed = 20;
                Invoke("ReturnPath",0.5f);
                break;
            case EnemyState.LookingAround:
                //velocidad en patrulla
                agent.speed = 15;
                break;
            case EnemyState.EndChasing:
                //velocidad en Regreso
                agent.speed = 15;
                //si el nuevo estado es EndChasing, se devuelve a la posicion en la que estaba antes de continuar
                agent.destination = lastPosition;
                break;
            case EnemyState.Sleep:
                agent.isStopped = true;
                Invoke("ReturnPath",5f);
            break;
        }
    }
    public void ReturnPath(){
        agent.isStopped=false;

        if (state == EnemyState.Sleep) state=EnemyState.LookingAround;
    }
}