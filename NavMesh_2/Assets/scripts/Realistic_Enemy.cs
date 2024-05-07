using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Realistic_Enemy : MonoBehaviour
{
    public int visionAngle;
    public GameObject player;
    public EnemyState state;
    public EnemyState lastState;
    public LayerMask playerMask;
    private float viewDistance = 30f;

    private float viewAngle1;
    private float viewAngle2;

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
        viewAngle1 = DirectionAngle(transform.eulerAngles.y, -visionAngle / 2);
        viewAngle2 = DirectionAngle(transform.eulerAngles.y, visionAngle / 2);
        if(lastState != state){
            OnStateChanged();
            OnLastStateChanged();
        }
        lastState = state;
        playerInSightView = CheckForVision();
        
        // Debug.Log(lastPosition  +" != "+  transform.position);
        // Debug.Log(Vector3.Distance(lastPosition,transform.position));
        // Debug.Log((Vector3.Distance(lastPosition,transform.position) < 4f) +" && " +(state==EnemyState.EndChasing));
        Vector3 directionToTarget = (player.transform.position - transform.position).normalized;
        // Debug.Log(Vector3.Angle(transform.forward,directionToTarget));
        // Debug.Log("View1: " + viewAngle1);
        // Debug.Log("View2: " + viewAngle2);

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
        //Handles.DrawWireArc(transform.position,Vector3.up,Vector3.forward,360,radius);
        Vector3 viewDirection1 = Quaternion.Euler(0, viewAngle1, 0) * transform.forward;
        Vector3 viewDirection2 = Quaternion.Euler(0, viewAngle2, 0) * transform.forward;
        //Handles.DrawLine(transform.position,transform.position + viewAngle1 * visionAngle);
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, viewDirection1 * viewDistance);
        Gizmos.DrawRay(transform.position, viewDirection2 * viewDistance);
        //Handles.DrawLine(transform.position,new Vector3(transform.forward.x+viewAngle2.x,0,transform.forward.z+viewAngle2.z));
        //Handles.DrawLine(transform.position,new Vector3(transform.forward.x+viewAngle1.x,0,transform.forward.z+viewAngle1.z));
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
        Vector3 directionToTarget = (hit.position - transform.position).normalized;
        Debug.Log(Vector3.Angle(transform.forward,directionToTarget) + " < " + visionAngle);
        return Vector3.Angle(transform.forward,directionToTarget) < visionAngle/2;
    }
    public float DirectionAngle(float eulerY, float angleInDegrees){
        angleInDegrees += eulerY;
        return angleInDegrees;
    }
    public void OnStateChanged(){}
    public void OnLastStateChanged(){}
}
