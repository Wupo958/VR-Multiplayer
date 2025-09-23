using UnityEngine;
using Unity.Netcode; // Für OwnerClientId
using XRMultiplayer.MiniGames;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

namespace XRMultiplayer.MiniGames
{
    /// <summary>
    /// Manages the local state and lifecycle of the Baseball minigame.
    /// </summary>
    public class MiniGame_PingPong : MiniGameBase
    {
        /// <summary>
        /// Reference to the networked component that handles synchronized actions (spawns one ball machine per player).
        /// </summary>
        [SerializeField] private networkedPingPong m_NetworkedGameplay;
        [SerializeField] float m_BatResetTime = .25f;
        [SerializeField] public bool player1Turn;

        readonly Dictionary<XRBaseInteractable, Pose> m_InteractablePoses = new();

        /// <inheritdoc/>
        public override void SetupGame()
        {
            base.SetupGame();
            player1Turn = true;
            Debug.Log("--- MiniGame_PingPong.SetupGame() ---");
        }

        /// <inheritdoc/>
        public override void StartGame()
        {
            base.StartGame();
            Debug.Log("--- MiniGame_PingPong.StartGame() ---");
            
        }

        /// <inheritdoc/>
        public override void FinishGame(bool submitScore = true)
        {
            player1Turn=true;
            base.FinishGame(submitScore);
            Debug.Log($"PingPong finished.");
        }

        public override void Start()
        {
            base.Start();

            foreach (var interactable in m_GameInteractables)
            {
                if (!m_InteractablePoses.ContainsKey(interactable))
                {
                    m_InteractablePoses.Add(interactable, new Pose(interactable.transform.position, interactable.transform.rotation));
                    interactable.selectExited.AddListener(BatDropped);
                }
            }
        }

        void OnDestroy()
        {
            foreach (var kvp in m_InteractablePoses)
            {
                kvp.Key.selectExited.RemoveListener(BatDropped);
            }
        }

        void BatDropped(BaseInteractionEventArgs args)
        {
            XRBaseInteractable interactable = (XRBaseInteractable)args.interactableObject;
            if (m_InteractablePoses.ContainsKey(interactable))
            {
                StartCoroutine(DropBatAfterTimeRoutine(interactable));
            }
        }

        IEnumerator DropBatAfterTimeRoutine(XRBaseInteractable interactable)
        {
            yield return new WaitForSeconds(m_BatResetTime);
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
    }
}