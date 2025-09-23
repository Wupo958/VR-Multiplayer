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
        [SerializeField] private GameObject ballSpawnPlayer1;
        [SerializeField] private GameObject ballSpawnPlayer2;

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
            if (IsServer)
            {
                SpawnBallServer();
            }
            else
            {
                SpawnBallServerRpc();
            }
        }

        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnBallServerRpc()
        {
            SpawnBallServer();
        }

        
        private void SpawnBallServer()
        {
            Debug.Log("SpawnBall (Server)");

            
            if (spawnedBall != null)
            {
                var oldNO = spawnedBall.GetComponent<NetworkObject>();
                if (oldNO != null && oldNO.IsSpawned)
                    oldNO.Despawn();
                Destroy(spawnedBall);
                spawnedBall = null;
            }

           
            var spawnGO = m_MiniGame.player1Turn ? ballSpawnPlayer1 : ballSpawnPlayer2;
            var spawnPos = spawnGO.transform.position;
            var spawnRot = spawnGO.transform.rotation;

            
            var instance = Instantiate(ball, spawnPos, spawnRot);

            
            var netObj = instance.GetComponent<NetworkObject>();
            if (netObj == null)
            {
                Debug.LogError("Ball-Prefab hat keine NetworkObject-Komponente!");
                Destroy(instance);
                return;
            }
            netObj.Spawn(true);

            // Referenzen setzen
            var ballScript = instance.GetComponent<PingPongBallScript>();
            if (ballScript != null)
            {
                ballScript.networkedPingPong = this;
            }

            spawnedBall = instance;

            
            m_MiniGame.player1Turn = !m_MiniGame.player1Turn;
        }
    }
}
