using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class MoveJumptyDumpty : NetworkBehaviour
{

    public KleeAbilities parent;
    [SerializeField] private float YForce;
    [SerializeField] private float shootForce;
    private Rigidbody rb;
    [SerializeField] private float gravity;
    private float TempYForce;
    [SerializeField] private int bounces;
    [SerializeField] private GameObject bombParticles; 
    /**
     Overview of physics:
     - add initial upward force 
     - ball moves with a constant xz froce
     - if ball falls below certain threshhold then reapply the initial y-force

     */

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // create the initial Y force
        rb.AddForce(new Vector3(rb.transform.forward.x * shootForce, YForce, rb.transform.forward.z * shootForce));
        TempYForce = YForce;
    }

    // Update is called once per frame
    void Update()
    {
        // do physics here
        // According to the Unity docs, F = m*v (force = mass * velocity) since mass = 1, velocity = force.
        rb.velocity = new Vector3(rb.transform.forward.x * shootForce, TempYForce, rb.transform.forward.z * shootForce);
        // decrease the force based off of how much time has passed.
        TempYForce = TempYForce - (gravity * Time.deltaTime);

        // bounce if it y position is less than radius + .1 and if the ball can still bounce
        if (rb.position.y < 0.71 && bounces > 0)
        {
            // increase the position so it doesnt enter multiple times
            rb.position = new Vector3(rb.position.x, .72f, rb.position.z);
            // reapply upward force
            rb.AddForce(new Vector3(rb.transform.forward.x * shootForce, YForce, rb.transform.forward.z * shootForce));
            // reset force
            TempYForce = YForce;
            bounces -= 1;
        }
        else if (rb.position.y < 0.71 && bounces == 0)
        {
            SpawnBombParticlesServerRpc(rb.position);
        }

    }


    // collider here
    /*
     This function is for when the third bounce happens and it spawns in some mini bombs that then cause the damage
     */
    [ServerRpc(RequireOwnership = false)]
    private void SpawnBombParticlesServerRpc(Vector3 position)
    {
        Debug.Log("Bomb Position: " + position);
        GameObject particles = Instantiate(bombParticles, new Vector3(position.x, 0.3f, position.z), Quaternion.Euler(90, 0, 0));
        particles.GetComponent<HandleBombParticleCollision>().parent = parent;
        particles.GetComponent<NetworkObject>().Spawn();
        DestroyBombServerRpc();
    }


    // destroy game obj here
    [ServerRpc (RequireOwnership = false)]
    private void DestroyBombServerRpc()
    {
        GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }


}
