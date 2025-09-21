using UnityEngine;
using XRMultiplayer.MiniGames;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// Manages the local state and lifecycle of the Golf minigame.
    /// </summary>
    public class MiniGame_Golf : MiniGameBase
    {
        /// <summary>
        /// Reference to the networked component that handles synchronized actions.
        /// </summary>
        [SerializeField] private NetworkedGolf m_NetworkedGameplay;

        /// <summary>
        /// The player's current stroke count for the hole.
        /// </summary>
        private int m_CurrentStrokes = 0;

        /// <inheritdoc/>
        public override void SetupGame()
        {
            base.SetupGame();
            m_CurrentStrokes = 0;
            Debug.Log("--- 1. MiniGame_Golf.SetupGame() CALLED ---");

            // Only the server should initiate course generation.
            if (m_NetworkedGameplay.IsOwner)
            {
                Debug.Log("Is owner");
                m_NetworkedGameplay.GenerateCourse();
            }
        }

        /// <inheritdoc/>
        public override void StartGame()
        {
            base.StartGame();
            // Logic to enable player controls, place their ball at the start, etc.
            m_NetworkedGameplay.StartPlayerTurn();
        }

        /// <inheritdoc/>
        public override void FinishGame(bool submitScore = true)
        {
            // In golf, lower score is better. The MiniGameManager assumes higher is better.
            // We can submit a negative score or modify the manager. For simplicity, we'll submit the raw strokes.
            base.FinishGame(submitScore);
            Debug.Log($"Finished hole with {m_CurrentStrokes} strokes.");
        }

        /// <summary>
        /// Called by the local player's club when it hits the ball.
        /// </summary>
        public void IncrementStrokeCount()
        {
            m_CurrentStrokes++;
            m_MiniGameManager.SubmitScoreRpc(m_CurrentStrokes, XRINetworkPlayer.LocalPlayer.OwnerClientId);

            // Tell the server this player has finished their turn.
            m_NetworkedGameplay.EndPlayerTurnServerRpc(XRINetworkPlayer.LocalPlayer.OwnerClientId);
        }
    }
}