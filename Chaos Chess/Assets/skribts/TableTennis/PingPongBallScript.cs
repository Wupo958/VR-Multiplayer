using UnityEngine;
using XRMultiplayer;
using XRMultiplayer.MiniGames;

public class PingPongBallScript : MonoBehaviour
{
    public networkedPingPong networkedPingPong;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            networkedPingPong.SpawnBall();
            Destroy(gameObject);
        }
    }
}
