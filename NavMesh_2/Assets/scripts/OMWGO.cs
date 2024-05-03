using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OMWGO : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject,1f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up*Time.deltaTime*90,Space.Self);
    }
}
