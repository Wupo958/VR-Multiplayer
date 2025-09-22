using UnityEngine;
using XRMultiplayer.MiniGames;

public class BallTrackDistance : MonoBehaviour
{
    private bool hit = false;
    private int playerNum;
    private float distance;
    private GameObject canvas;
    private GameObject bat;
    private MiniGame_Baseball miniGame_Baseball;

    public void Start()
    {
        miniGame_Baseball = FindAnyObjectByType<MiniGame_Baseball>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 a = transform.position;
        Vector3 b = bat.transform.position;
        a.y = 0f;
        b.y = 0f;
        float distance = Vector3.Distance(a, b);

        if (hit == true && transform.position.y > 1 && canvas.GetComponent<updateLeaderboard>().playerDist[playerNum] < distance)
        {
            canvas.GetComponent<updateLeaderboard>().playerDist[playerNum] = distance;
            miniGame_Baseball.SetDistance((int)distance);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("bat") == true && hit == false)
        {
            Debug.Log("Ball hit");
            bat = collision.gameObject;
            hit = true;
            canvas = collision.gameObject.GetComponent<BatPlayerNum>().canvas;
            playerNum = collision.gameObject.GetComponent<BatPlayerNum>().playerNum;
        }
    }
}
