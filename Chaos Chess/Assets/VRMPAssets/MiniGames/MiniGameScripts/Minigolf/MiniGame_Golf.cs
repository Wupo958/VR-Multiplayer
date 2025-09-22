using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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

        [SerializeField] float m_ClubResetTime = .25f;

        /// <summary>
        /// The interactable objects to use for the mini-game.
        /// </summary>
        readonly Dictionary<XRBaseInteractable, Pose> m_InteractablePoses = new();


        /// <summary>

        /// The player's current stroke count for the hole.

        /// </summary>

        private int m_CurrentStrokes = 0;


        /// <inheritdoc/>

        /// 

        public override void Start()
        {

            base.Start();

            foreach (var interactable in m_GameInteractables)
            {
                if (!m_InteractablePoses.ContainsKey(interactable))
                {
                    m_InteractablePoses.Add(interactable, new Pose(interactable.transform.position, interactable.transform.rotation));
                    interactable.selectExited.AddListener(ClubDropped);
                }
            }

        }

        void OnDestroy()
        {
            foreach (var kvp in m_InteractablePoses)
            {
                kvp.Key.selectExited.RemoveListener(ClubDropped);
            }
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

        void ClubDropped(BaseInteractionEventArgs args)
        {
            XRBaseInteractable interactable = (XRBaseInteractable)args.interactableObject;
            if (m_InteractablePoses.ContainsKey(interactable))
            {
                StartCoroutine(DropClubAfterTimeRoutine(interactable));
            }
        }

        /// <summary>
        /// Coroutine that drops the hammer after a specified time and resets the interactable's position.
        /// </summary>
        /// <param name="interactable">The interactable object.</param>
        IEnumerator DropClubAfterTimeRoutine(XRBaseInteractable interactable)
        {
            yield return new WaitForSeconds(m_ClubResetTime);
            if (!interactable.isSelected)
            {
                Rigidbody body = interactable.GetComponent<Rigidbody>();
                bool wasKinematic = body.isKinematic;
                body.isKinematic = true;
                interactable.transform.SetPositionAndRotation(m_InteractablePoses[interactable].position, m_InteractablePoses[interactable].rotation);
                yield return new WaitForFixedUpdate();
                body.isKinematic = wasKinematic;
                foreach (var collider in interactable.colliders)
                {
                    collider.enabled = true;
                }
            }
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