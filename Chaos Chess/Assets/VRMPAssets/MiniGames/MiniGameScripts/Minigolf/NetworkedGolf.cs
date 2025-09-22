using UnityEngine;

using Unity.Netcode;

using System.Collections.Generic;


namespace XRMultiplayer.MiniGames

{

    /// <summary>

    /// Handles synchronized logic for the Golf minigame, including course generation and turn management.

    /// </summary>

    public class NetworkedGolf : NetworkBehaviour

    {

        [Header("Course Generation")]

        [SerializeField] private GameObject[] m_CoursePiecePrefabs;

        [SerializeField] private GameObject m_StartPiecePrefab;

        [SerializeField] private GameObject m_EndPiecePrefab; // The piece with the hole

        [SerializeField] private int m_NumberOfMiddlePieces = 5;

        [SerializeField] private Transform m_CourseParent; // An empty GameObject to hold the generated course


        [Header("Game Objects")]

        [SerializeField] private GameObject m_GolfBallPrefab; // Ensure this has a NetworkObject and NetworkTransform


        private MiniGame_Golf m_MiniGame;

        private List<GameObject> m_SpawnedCoursePieces = new List<GameObject>();


        // This seed is synced to all clients so they generate the same course.

        private NetworkVariable<int> m_RandomSeed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        public override void OnNetworkSpawn()

        {

            Debug.Log("--- NetworkedGolf OnNetworkSpawn() CALLED ---");

            TryGetComponent(out m_MiniGame);

            //m_RandomSeed.OnValueChanged += OnSeedChanged;

        }


        public override void OnNetworkDespawn()

        {

            //m_RandomSeed.OnValueChanged -= OnSeedChanged;

        }


        [Rpc(SendTo.Everyone)]

        private void BuildCourseClientRpc(int seed)

        {

            Debug.Log($"--- 3. BuildCourseClientRpc() CALLED ON CLIENT. Building course with seed {seed} ---");

            BuildCourse(seed);

        }


        /// <summary>

        /// Called on the server to generate a seed and trigger course generation on clients.

        /// </summary>


        public void GenerateCourse()

        {

            Debug.Log("--- 2. GenerateCourseServerRpc() CALLED ON SERVER ---");

            ClearCourse();

            m_RandomSeed.Value = Random.Range(0, 99999);

            BuildCourseClientRpc(m_RandomSeed.Value);

        }


        private void OnSeedChanged(int previousValue, int newValue)

        {

            Debug.Log($"--- 3. OnSeedChanged() CALLED ON CLIENT. New seed is {newValue} ---");

            BuildCourse(newValue);

        }


        /// <summary>

        /// Builds the golf course procedurally using the synchronized seed.

        /// </summary>

        private void BuildCourse(int seed)

        {

            ClearCourse();

            Random.InitState(seed);

            Transform lastSocket = m_CourseParent;


            // 1. Spawn Start Piece

            GameObject startPiece = Instantiate(m_StartPiecePrefab, m_CourseParent.position, m_CourseParent.rotation, m_CourseParent);

            GameObject ball = Instantiate(m_GolfBallPrefab, startPiece.transform.Find("BallSpawnPoint").position, Quaternion.identity);

            ball.GetComponent<NetworkObject>().Spawn(true);

            ball.SetActive(true);

            m_SpawnedCoursePieces.Add(startPiece);

            lastSocket = startPiece.transform.Find("ConnectionPoint_End");

            if (lastSocket == null) { Debug.LogError($"Start Piece is missing 'ConnectionPoint_End'."); return; }


            // 2. Spawn Middle Pieces

            for (int i = 0; i < m_NumberOfMiddlePieces; i++)

            {

                GameObject newPiece = Instantiate(m_CoursePiecePrefabs[Random.Range(0, m_CoursePiecePrefabs.Length)], m_CourseParent);

                Transform startSocket = newPiece.transform.Find("ConnectionPoint_Start");

                if (startSocket == null) { Debug.LogError($"Prefab {newPiece.name} is missing 'ConnectionPoint_Start'."); Destroy(newPiece); continue; }


                // --- Alignment Logic ---

                newPiece.transform.rotation = lastSocket.rotation * Quaternion.Inverse(startSocket.localRotation);

                newPiece.transform.position = lastSocket.position - (startSocket.position - newPiece.transform.position);


                m_SpawnedCoursePieces.Add(newPiece);


                // --- VISUAL DEBUGGING ---

                // Draw a thick green line showing the direction the LAST socket was facing. Lasts 15 seconds.

                Debug.DrawRay(lastSocket.position, lastSocket.forward * 2.0f, Color.green, 15.0f);


                // After aligning the new piece, find its start socket again to see where it ended up.

                Transform newStartSocket = newPiece.transform.Find("ConnectionPoint_Start");

                // Draw a red line showing the direction the NEW socket is now facing.

                Debug.DrawRay(newStartSocket.position, newStartSocket.forward * 2.0f, Color.red, 15.0f);


                // Log the positions for comparison.

                Debug.Log($"Connected '{newPiece.name}'. Last socket at {lastSocket.position.ToString("F2")}, New socket at {newStartSocket.position.ToString("F2")}");

                // --- END VISUAL DEBUGGING ---


                lastSocket = newPiece.transform.Find("ConnectionPoint_End");

                if (lastSocket == null) { Debug.LogError($"Prefab {newPiece.name} is missing 'ConnectionPoint_End'."); return; }

            }


            // (Code for spawning End Piece remains the same)

            GameObject endPiece = Instantiate(m_EndPiecePrefab, m_CourseParent);

            Transform endStartSocket = endPiece.transform.Find("ConnectionPoint_Start");

            if (endStartSocket == null) { Debug.LogError($"End Piece is missing 'ConnectionPoint_Start'."); return; }

            endPiece.transform.rotation = lastSocket.rotation * Quaternion.Inverse(endStartSocket.localRotation);

            endPiece.transform.position = lastSocket.position - (endStartSocket.position - endPiece.transform.position);

            m_SpawnedCoursePieces.Add(endPiece);

        }


        private void ClearCourse()

        {

            foreach (var piece in m_SpawnedCoursePieces)

            {

                Destroy(piece);

            }

            m_SpawnedCoursePieces.Clear();

            GameObject ball = GameObject.FindWithTag("GolfBall");

            Destroy(ball);

        }

    }

}