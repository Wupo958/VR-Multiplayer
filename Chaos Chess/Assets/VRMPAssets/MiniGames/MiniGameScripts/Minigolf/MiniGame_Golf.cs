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

        /// 

        public override void Start()
        {

            base.Start();

        }


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

        }

        public override void FinishGame(bool submitScore = true)
        {

            base.FinishGame(submitScore);

            Debug.Log($"Finished hole with {m_CurrentStrokes} strokes.");

        }


        /// <summary>

        /// Called by the local player's club when it hits the ball.

        /// </summary>

        public void IncrementStrokeCount()
        {
            Debug.Log("Hit! Incrementing stroke count.");
            m_CurrentStrokes++;

            m_MiniGameManager.SubmitScoreRpc(m_CurrentStrokes, XRINetworkPlayer.LocalPlayer.OwnerClientId);

        }


        public void PlayerHoledOut()
        {
            Debug.Log("Player holed out!");
            m_CurrentStrokes--;

            bool finishGameOnSubmit = true;

            m_MiniGameManager.SubmitScoreRpc(m_CurrentStrokes, XRINetworkPlayer.LocalPlayer.OwnerClientId, finishGameOnSubmit);

        }

    }

}