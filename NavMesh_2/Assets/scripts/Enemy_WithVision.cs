using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_WithVision : MonoBehaviour
{
    public GameObject player;
    public EnemyState state;
    public EnemyState lastState;

    private bool playerInSightView;
    public LayerMask playerMask;
    private NavMeshAgent agent;
    public Vector3 lastPosition;

    public List<Transform> wanderingPoints;
    public float remainingdistance;
    public Vector3 destino;
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
        if(lastState != state){
            OnStateChanged();
            OnLastStateChanged();
        }
        lastState = state;

        remainingdistance = agent.remainingDistance;
        destino = agent.destination;

        playerInSightView = CheckForVision();
        
        if (playerInSightView && state != EnemyState.Sleep){
            state =EnemyState.Chasing;
            lookingAroundEnabled=false;
        } else if (Vector3.Distance(lastPosition,transform.position) > 4f && state== EnemyState.Chasing){
            state =EnemyState.EndChasing;
        } else if (Vector3.Distance(lastPosition,transform.position) < 4f && state==EnemyState.EndChasing){
            //Debug.Log("Entra");
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
                agent.destination = player.transform.position;
                //yield return new WaitForSeconds(0.05f);
                break;
            case EnemyState.Catching:
                break;
        }
        yield return null;
    }
    void OnDrawGizmos(){
        Gizmos.DrawWireSphere(transform.position,25);
        Gizmos.DrawLine(transform.position,agent.destination);
    }
    bool CheckForVision(){
        //Collider[] = Physics.OverlapSphere este método devuelve los colliders detectados
        if (Physics.CheckSphere(transform.position,25,playerMask)){//phisics.SphereOverlapse puede ser una mejor solucion para no tener que tener la referencia a la layermask
            RaycastHit hit;
            if (Physics.Raycast(transform.position+Vector3.up,player.transform.position-transform.position, out hit,(player.transform.position-transform.position).magnitude)){
                //Debug.Log(hit.collider.name);
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
            //Debug.Log(wanderingPoints.IndexOf(activeWayPoint) + " || " + wanderingPoints.Count-1);
            //Debug.Log(wanderingPoints.IndexOf(activeWayPoint) == wanderingPoints.Count-1 ? 0 : wanderingPoints.IndexOf(activeWayPoint));
            //if (agent.destination == lastPosition) state = EnemyState.LookingAround;
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
            //ResumePatrol();
            yield return new WaitUntil(() => lookingAroundEnabled);
            StartCoroutine("LookingAround");
        }
    }

    private void ResumePatrol(){
        Vector3 a;
        Debug.Log("Antes: " + activeWayPoint.position);
        Debug.Log(wanderingPoints.Find((x) => x == activeWayPoint).position);
        activeWayPoint = wanderingPoints.Find((x) => x == activeWayPoint);
        Debug.Log("Despues: " + activeWayPoint.position);
        //Transform a =wanderingPoints.Find((x) => activeWayPoint);
        //Debug.Log("Estas en: " + wanderingPoints.IndexOf(a));
    }
    public void OnLastStateChanged(){
        //OnLastStateChanged se llama con el estado previo al cambio
        //Ej: si el cambio es LookingAround => Chasing el valor utilizado es LookingAround
        //Debug.Log("Cambiado desde: " +state);
        GameObject go;
        switch(lastState){
            case EnemyState.Chasing:
            go =Instantiate(playerLost,transform.position+Vector3.up*4,Quaternion.identity);
            go.transform.parent = transform;
            agent.isStopped=true;
            Invoke("ReturnPath",2f);
            break;
            case EnemyState.EndChasing:
            //agent.destination = lastPosition;
            break;
            case EnemyState.LookingAround:
            //si el estado anterior, estaba patrullando, se cambia la ultima posicion
            lastPosition = transform.position;
            //Invoke("ReturnPath",2f);
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
                agent.speed = 20;
                Invoke("ReturnPath",0.5f);
                break;
            case EnemyState.LookingAround:
                //Debug.Log("Entra para la position");
                //agent.isStopped=true;
                //Invoke("ReturnPath",1f);
                //lastPosition = transform.position;
                agent.speed = 15;
                break;
            case EnemyState.EndChasing:
                agent.speed = 15;
                //si el nuevo estado es EndChasing, se devuelve a la posicion en la que estaba antes de continuar
                agent.destination = lastPosition;
                break;
            case EnemyState.Sleep:
            //if (agent.destination != lastPosition) state = EnemyState.LookingAround;
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