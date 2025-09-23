using UnityEngine;
using Unity.Netcode;

public class PingPongBallScript : NetworkBehaviour
{
    public networkedPingPong networkedPingPong;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // nur der Server triggert den nächsten Ball

        if (collision.gameObject.CompareTag("Ground"))
        {
            networkedPingPong.RequestSpawnBallServerRpc();
            GetComponent<NetworkObject>().Despawn(true); // statt Destroy()
        }
    }
}
