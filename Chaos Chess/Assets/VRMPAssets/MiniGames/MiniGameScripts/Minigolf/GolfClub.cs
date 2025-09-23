using UnityEngine;
using Unity.Netcode;
using XRMultiplayer.MiniGames;

public class GolfClub : MonoBehaviour
{
    private MiniGame_Golf m_MiniGameGolf;

    private void Start()
    {
        m_MiniGameGolf = FindFirstObjectByType<MiniGame_Golf>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<GolfBall>(out GolfBall ball))
        {
            if (ball.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {

                m_MiniGameGolf.IncrementStrokeCount();
            }
        }
    }
}
