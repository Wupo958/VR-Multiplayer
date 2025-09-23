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
}
