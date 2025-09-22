using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// NetworkedBaseball spawnt für jeden Spieler eine eigene Ballmaschine
    /// und verwaltet deren Lebenszyklus (Join/Leave).
    /// </summary>
    public class networkedBaseball : NetworkBehaviour
    {
        [Header("Ballmaschinen")]
        [SerializeField] private GameObject[] ballMachines;

        private MiniGame_Baseball m_MiniGame;

        public override void OnNetworkSpawn()
        {
            Debug.Log("--- NetworkedGolf OnNetworkSpawn() CALLED ---");
            TryGetComponent(out m_MiniGame);
        }

        public override void OnNetworkDespawn()
        {
            //m_RandomSeed.OnValueChanged -= OnSeedChanged;
        }

        public void startShooting()
        {
            foreach (var machine in ballMachines)
            {
                machine.GetComponent<ShootBall>().shooting = true;
            }
        }

        public void stopShooting()
        {
            foreach (var machine in ballMachines)
            {
                machine.GetComponent<ShootBall>().shooting = false;
            }
        }
    }
}
