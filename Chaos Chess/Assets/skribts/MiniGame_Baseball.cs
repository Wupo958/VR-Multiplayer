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
    public class MiniGame_Baseball : MiniGameBase
    {
        /// <summary>
        /// Reference to the networked component that handles synchronized actions (spawns one ball machine per player).
        /// </summary>
        [SerializeField] private networkedBaseball m_NetworkedGameplay;
        [SerializeField] float m_BatResetTime = .25f;
        readonly Dictionary<XRBaseInteractable, Pose> m_InteractablePoses = new();

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
            m_NetworkedGameplay.startShooting();
        }

        /// <inheritdoc/>
        public override void FinishGame(bool submitScore = true)
        {
            m_NetworkedGameplay.stopShooting(); m_NetworkedGameplay.stopShooting();
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