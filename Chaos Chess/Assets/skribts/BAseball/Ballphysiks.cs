using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody), typeof(NetworkObject))]
public class NetworkProjectile : NetworkBehaviour
{
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    public override void OnNetworkSpawn()
    {
        
        if (IsServer)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
        else
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
    }
}