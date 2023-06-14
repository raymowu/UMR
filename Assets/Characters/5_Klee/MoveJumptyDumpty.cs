using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoveJumptyDumpty : MonoBehaviour
{

    public KleeAbilities parent;
    [SerializeField] private float YForce;
    [SerializeField] private float shootForce;
    private Rigidbody rb;
    [SerializeField] private float gravity = 20f;
    private float TempYForce = 0f;
    private float time = 0f;
    /**
     Overview of physics:
     - add initial upward force and let the rigidbody gravity bring it down
     - ball moves with a constant xz froce
     - if ball falls below certain threshhold then reapply a force at y-force / 2

     */

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(new Vector3(rb.transform.forward.x * shootForce, YForce, rb.transform.forward.z * shootForce));
    }

    // Update is called once per frame
    void Update()
    {
        // do physics here        
        Vector3 velocity = new Vector3(rb.transform.forward.x * shootForce, VelocityFromForce(TempYForce, time), rb.transform.forward.z * shootForce);
        //velocity.y = VelocityFromForce(TempYForce, Time.deltaTime);
        rb.velocity = velocity;
        TempYForce = YForce - (gravity * time);
        Debug.Log(TempYForce);
        time += 1;


        if (rb.position.y < 5)
        {
            YForce = YForce / 2f;
            TempYForce = YForce;
        }
        Debug.Log(Time.deltaTime);

    }

    float VelocityFromForce(float force, float time) 
    {
        return (force / 1) * time;
    }
    

    // collider here

    // destroy game obj here


}
