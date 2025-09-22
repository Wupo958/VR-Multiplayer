using UnityEngine;
using Unity.Netcode; // Für OwnerClientId
using XRMultiplayer.MiniGames;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// Manages the local state and lifecycle of the Baseball minigame.
    /// </summary>
    public class MiniGame_Baseball : MiniGameBase
    {
        /// <summary>
        /// Reference to the networked component that handles synchronized actions (spawns one ball machine per player).
        /// </summary>
        [SerializeField] private networkedBaseball m_NetworkedGameplay;

        /// <summary>
        /// Local player's current hit count (or points).
        /// </summary>
        private int m_CurrentHits = 0;

        /// <inheritdoc/>
        public override void SetupGame()
        {
            base.SetupGame();
            m_CurrentHits = 0;
            Debug.Log("--- MiniGame_Baseball.SetupGame() ---");
        }

        /// <inheritdoc/>
        public override void StartGame()
        {
            base.StartGame();
            Debug.Log("--- MiniGame_Baseball.StartGame() ---");
            m_NetworkedGameplay.spawnBats();
        }

        /// <inheritdoc/>
        public override void FinishGame(bool submitScore = true)
        {
            base.FinishGame(submitScore);
            Debug.Log($"Baseball finished with {m_CurrentHits} hits/points.");
        }

        /// <summary>
        /// Call this when the local player successfully hits a ball or scores points.
        /// </summary>
        public void SetDistance(int distance)
        {
            m_MiniGameManager.SubmitScoreRpc(distance, XRINetworkPlayer.LocalPlayer.OwnerClientId);
        }
    }
}