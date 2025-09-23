using UnityEngine;
using XRMultiplayer.MiniGames;

public class HoleTrigger : MonoBehaviour
{
    private MiniGame_Golf m_MiniGameGolf;

    void Start()
    {
        m_MiniGameGolf = FindFirstObjectByType<MiniGame_Golf>();
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("GolfBall"))
        {
            GolfBall ball = other.GetComponentInParent<GolfBall>();

            if (ball.IsOwner)
            {
                m_MiniGameGolf.PlayerHoledOut();

                ball.GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}