using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class networkedPingPong : NetworkBehaviour
{
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject ballSpawnPlayer1;
    [SerializeField] private GameObject ballSpawnPlayer2;

    private MiniGame_PingPong m_MiniGame;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out m_MiniGame);
    }

    // in networkedPingPong
    public void spawnBall()
    {
        Transform spawn = m_MiniGame.player1Turn ? ballSpawnPlayer1.transform : ballSpawnPlayer2.transform;

        GameObject go = NetworkObject.Instantiate(ball, spawn.position, spawn.rotation);
        var no = go.GetComponent<NetworkObject>();
        

        m_MiniGame.player1Turn = !m_MiniGame.player1Turn;
    }
}
