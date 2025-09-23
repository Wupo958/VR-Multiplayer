using System.Collections.Generic;
using UnityEngine;
using XRMultiplayer.MiniGames;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using static XRMultiplayer.MiniGames.MiniGameBase;
using System.Linq;
namespace XRMultiplayer.MiniGames

{

    public class MiniGame_Golf : MiniGameBase

    {


        [SerializeField] private NetworkedGolf m_NetworkedGameplay;

        [SerializeField] float m_ClubResetTime = .25f;

        readonly Dictionary<XRBaseInteractable, Pose> m_InteractablePoses = new();


        private int m_CurrentStrokes = 0;

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

            MasterReset();

            if (m_NetworkedGameplay.IsOwner)
            {

                Debug.Log("Is owner");

                m_NetworkedGameplay.GenerateCourse();

            }

        }

        public void MasterReset()
        {
            foreach (var interactable in m_GameInteractables)
            {
                if (interactable.TryGetComponent<NetworkPhysicsInteractable>(out var networkInteractable))
                {
                    networkInteractable.spawnLocked = false;
                    networkInteractable.ResetObject();
                }
            }
        }

        public override void StartGame()
        {
            base.StartGame();
            if (m_NetworkedGameplay.IsOwner)
            {
                List<ulong> playerIds = m_MiniGameManager.currentPlayerDictionary.Keys.Select(p => p.OwnerClientId).ToList();

                StartCoroutine(m_NetworkedGameplay.SpawnPlayerBalls(playerIds, m_NetworkedGameplay.IsOwner));
            }
        }

        public override void FinishGame(bool submitScore = true)
        {
            StopAllCoroutines();

            base.FinishGame(submitScore);

            Debug.Log($"Finished hole with {m_CurrentStrokes} strokes.");

            MasterReset();

        }

        public void IncrementStrokeCount()
        {
            Debug.Log("Hit! Incrementing stroke count.");
            m_CurrentStrokes++;

            m_MiniGameManager.SubmitScoreRpc(m_CurrentStrokes, XRINetworkPlayer.LocalPlayer.OwnerClientId);

        }

        void ClubDropped(BaseInteractionEventArgs args)
        {
            if (m_MiniGameManager.currentNetworkedGameState != MiniGameManager.GameState.InGame) return;

            XRBaseInteractable interactable = (XRBaseInteractable)args.interactableObject;
            if (m_InteractablePoses.ContainsKey(interactable))
            {
                StartCoroutine(DropClubAfterTimeRoutine(interactable));
            }
        }

        IEnumerator DropClubAfterTimeRoutine(XRBaseInteractable interactable)
        {
            yield return new WaitForSeconds(m_ClubResetTime);

            if (!interactable.isSelected)
            {

                if (interactable.TryGetComponent<NetworkPhysicsInteractable>(out var networkInteractable))
                {

                    networkInteractable.ResetObject();

                }
            }
        }

        public void PlayerHoledOut()
        {
            Debug.Log("Player holed out!");
            if (m_CurrentStrokes > 1)
                m_CurrentStrokes--;

            bool finishGameOnSubmit = true;

            m_MiniGameManager.SubmitScoreRpc(m_CurrentStrokes, XRINetworkPlayer.LocalPlayer.OwnerClientId, finishGameOnSubmit);
        }

    }

}