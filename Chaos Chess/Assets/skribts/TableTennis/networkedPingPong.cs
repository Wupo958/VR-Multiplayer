using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace XRMultiplayer.MiniGames
{
    public class networkedPingPong : NetworkBehaviour
    {
        [Header("Ballmaschinen")]
        [SerializeField] private GameObject ball;
        [SerializeField] private GameObject spawnedBall;
        [SerializeField] GameObject ballSpawnPlayer1;
        [SerializeField] GameObject ballSpawnPlayer2;

        private MiniGame_PingPong m_MiniGame;

        public override void OnNetworkSpawn()
        {
            Debug.Log("--- NetworkedGolf OnNetworkSpawn() CALLED ---");
            TryGetComponent(out m_MiniGame);
        }

        public override void OnNetworkDespawn()
        {
            //m_RandomSeed.OnValueChanged -= OnSeedChanged;
        }

        public void SpawnBall()
        {
            Debug.Log("sapwntBalls");
            if (m_MiniGame.player1Turn == true)
            {
                Debug.Log("sapwntBalls2");
                spawnedBall = Instantiate(ball, ballSpawnPlayer1.transform);
                spawnedBall.transform.position = ballSpawnPlayer1.transform.position;
                spawnedBall.GetComponent<PingPongBallScript>().networkedPingPong = gameObject.GetComponent<networkedPingPong>();
                m_MiniGame.player1Turn = false;
            }
            else
            {
                Debug.Log("sapwntBalls2");
                spawnedBall = Instantiate(ball, ballSpawnPlayer2.transform);
                spawnedBall.transform.position = ballSpawnPlayer2.transform.position;
                spawnedBall.GetComponent<PingPongBallScript>().networkedPingPong = gameObject.GetComponent<networkedPingPong>();
                m_MiniGame.player1Turn = true;
            }
        }
    }
}
