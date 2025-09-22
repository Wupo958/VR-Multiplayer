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
        // Make sure the object that entered is a golf ball
        if (other.CompareTag("GolfBall"))
        {
            GolfBall ball = other.GetComponentInParent<GolfBall>();

            // IMPORTANT: Check if it's the LOCAL player's ball before submitting.
            // Every player's game will detect the ball entering the trigger,
            // but only the ball's owner should report it.
            if (ball.IsOwner)
            {
                m_MiniGameGolf.PlayerHoledOut();

                // Optional: Disable the ball's physics so it can't be knocked out.
                // We'll also disable the trigger to prevent it from being called multiple times.
                ball.GetComponent<Rigidbody>().isKinematic = true;
                GetComponent<Collider>().enabled = false;
            }
        }
    }
}