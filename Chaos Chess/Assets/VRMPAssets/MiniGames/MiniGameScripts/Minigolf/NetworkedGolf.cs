using UnityEngine;

using Unity.Netcode;

using System.Collections.Generic;


namespace XRMultiplayer.MiniGames

{

    public class NetworkedGolf : NetworkBehaviour

    {

        [Header("Course Generation")]

        [SerializeField] private GameObject[] m_CoursePiecePrefabs;

        [SerializeField] private GameObject m_StartPiecePrefab;

        [SerializeField] private GameObject m_EndPiecePrefab;

        [SerializeField] private int m_NumberOfMiddlePieces = 5;

        [SerializeField] private Transform m_CourseParent;


        [Header("Game Objects")]

        [SerializeField] private GameObject m_GolfBallPrefab;

        private MiniGame_Golf m_MiniGame;

        private List<GameObject> m_SpawnedCoursePieces = new List<GameObject>();

        private NetworkVariable<int> m_RandomSeed = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);


        public override void OnNetworkSpawn()
        {

            Debug.Log("--- NetworkedGolf OnNetworkSpawn() CALLED ---");

            TryGetComponent(out m_MiniGame);

        }


        [Rpc(SendTo.Everyone)]

        private void BuildCourseClientRpc(int seed)
        {

            Debug.Log($"--- 3. BuildCourseClientRpc() CALLED ON CLIENT. Building course with seed {seed} ---");

            BuildCourse(seed);

        }


        public void GenerateCourse()
        {

            Debug.Log("--- 2. GenerateCourseServerRpc() CALLED ON SERVER ---");

            ClearCourse();

            m_RandomSeed.Value = Random.Range(0, 99999);

            BuildCourseClientRpc(m_RandomSeed.Value);

        }

        private void BuildCourse(int seed)
        {

            ClearCourse();

            Random.InitState(seed);

            Transform lastSocket = m_CourseParent;


            GameObject startPiece = Instantiate(m_StartPiecePrefab, m_CourseParent.position, m_CourseParent.rotation, m_CourseParent);

            m_SpawnedCoursePieces.Add(startPiece);

            lastSocket = startPiece.transform.Find("ConnectionPoint_End");

            if (lastSocket == null) { Debug.LogError($"Start Piece is missing 'ConnectionPoint_End'."); return; }


            for (int i = 0; i < m_NumberOfMiddlePieces; i++)

            {

                GameObject newPiece = Instantiate(m_CoursePiecePrefabs[Random.Range(0, m_CoursePiecePrefabs.Length)], m_CourseParent);

                Transform startSocket = newPiece.transform.Find("ConnectionPoint_Start");

                if (startSocket == null) { Debug.LogError($"Prefab {newPiece.name} is missing 'ConnectionPoint_Start'."); Destroy(newPiece); continue; }


                newPiece.transform.rotation = lastSocket.rotation * Quaternion.Inverse(startSocket.localRotation);

                newPiece.transform.position = lastSocket.position - (startSocket.position - newPiece.transform.position);


                m_SpawnedCoursePieces.Add(newPiece);


                Transform newStartSocket = newPiece.transform.Find("ConnectionPoint_Start");


                Debug.Log($"Connected '{newPiece.name}'. Last socket at {lastSocket.position.ToString("F2")}, New socket at {newStartSocket.position.ToString("F2")}");

                lastSocket = newPiece.transform.Find("ConnectionPoint_End");

                if (lastSocket == null) { Debug.LogError($"Prefab {newPiece.name} is missing 'ConnectionPoint_End'."); return; }

            }

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

            GameObject[] balls = GameObject.FindGameObjectsWithTag("GolfBall");

            foreach (GameObject ball in balls)
            {
                if (ball.TryGetComponent<NetworkObject>(out var netObj))
                {
                    netObj.Despawn();
                }
                else
                {
                    Destroy(ball);
                }
            }

        }

        public void BringBackBalls()
        {
            // This function will only be called on the server, which is correct.
            GameObject[] balls = GameObject.FindGameObjectsWithTag("GolfBall");
            if (balls.Length == 0) return;

            var spawnPoints = new System.Collections.Generic.List<Transform>();
            Transform startPiece = m_SpawnedCoursePieces[0].transform;
            foreach (Transform child in startPiece)
            {
                if (child.CompareTag("BallSpawnPoint"))
                {
                    spawnPoints.Add(child);
                }
            }

            if (spawnPoints.Count == 0) return;

            Debug.Log($"Duct Tape Fix: Found {balls.Length} balls. Teleporting them back to {spawnPoints.Count} spawn points.");

            // Loop through all found balls and assign them to a spawn point.
            // This is safer than assuming balls[0] and balls[1] exist.
            for (int i = 0; i < balls.Length; i++)
            {
                // Make sure we have a spawn point for this ball
                if (i < spawnPoints.Count)
                {
                    GameObject ball = balls[i];
                    Transform spawnPoint = spawnPoints[i];

                    // Get the Rigidbody component
                    Rigidbody rb = ball.GetComponent<Rigidbody>();

                    // IMPORTANT: Reset physics before teleporting
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    // Now, set the position. The NetworkTransform will sync this.
                    ball.transform.position = spawnPoint.position;
                    ball.transform.rotation = spawnPoint.rotation;
                }
            }
        }

        public void SpawnPlayerBalls(IReadOnlyList<ulong> playersInGame, bool isServer)
        {
            Debug.Log("Called SpawnPlayerBalls() on server to spawn balls for players.");
            if (!isServer) return;

            Debug.Log("Not server");
            Transform startPiece = m_SpawnedCoursePieces[0].transform;
            var spawnPoints = new System.Collections.Generic.List<Transform>();

            foreach (Transform child in startPiece)
            {
                if (child.CompareTag("BallSpawnPoint"))
                {
                    spawnPoints.Add(child);
                }
            }

            if (spawnPoints.Count < playersInGame.Count)
            {
                Debug.LogError($"CRITICAL: Not enough spawn points! Have {spawnPoints.Count}, but need {playersInGame.Count}.");
                return;
            }

            for (int i = 0; i < playersInGame.Count; i++)
            {
                ulong clientId = playersInGame[i];
                Transform spawnPoint = spawnPoints[i];

                Debug.Log("Spawning ball for client " + clientId);

                GameObject ballGo = Instantiate(m_GolfBallPrefab, spawnPoint.position, Quaternion.identity);
                ballGo.name = $"GolfBall_Player_{clientId}";
                ballGo.transform.position = spawnPoint.position;
                ballGo.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                NetworkObject ballNetObj = ballGo.GetComponent<NetworkObject>();
                ballNetObj.SpawnWithOwnership(clientId);
            }
        }

    }

}