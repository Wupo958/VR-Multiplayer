using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class networkedPingPong : NetworkBehaviour
{
    [SerializeField] private GameObject ball;
    [SerializeField] private Transform ballSpawnPlayer1;
    [SerializeField] private Transform ballSpawnPlayer2;

    private MiniGame_PingPong m_MiniGame;

    public override void OnNetworkSpawn()
    {
        TryGetComponent(out m_MiniGame);
    }

    // in networkedPingPong
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnBallServerRpc()
    {
        Transform spawn = m_MiniGame.player1Turn ? ballSpawnPlayer1.transform : ballSpawnPlayer2.transform;

        GameObject go = Instantiate(ball, spawn.position, spawn.rotation);
        var no = go.GetComponent<NetworkObject>();
        no.Spawn(); // Ownership optional vergeben, falls nötig

        m_MiniGame.player1Turn = !m_MiniGame.player1Turn;
    }
}
