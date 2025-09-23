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

    // Client ruft das hier, Server spawnt wirklich
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnBallServerRpc(ulong requesterClientId)
    {
        Transform spawn = m_MiniGame.player1Turn ? ballSpawnPlayer1 : ballSpawnPlayer2;

        GameObject go = Instantiate(ball, spawn.position, spawn.rotation);
        var no = go.GetComponent<NetworkObject>();
        // Optional Ownership: dem anfragenden Spieler geben
        //no.SpawnWithOwnership(requesterClientId);

        // Falls du keine Ownership brauchst:
        no.Spawn();

        // Toggle Zug
        m_MiniGame.player1Turn = !m_MiniGame.player1Turn;
    }
}
