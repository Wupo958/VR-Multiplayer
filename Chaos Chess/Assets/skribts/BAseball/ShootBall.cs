using Unity.Netcode;
using UnityEngine;
using XRMultiplayer.MiniGames;

public class ShootBall : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Schuss-Settings")]
    [SerializeField] private float muzzleSpeed = 45f; 
    [SerializeField] private bool useGravity = true;
    [SerializeField] private bool inheritLauncherVelocity = true; 
    [SerializeField] private float lifeTime = 10f;

    [Header("Optional: misc")]
    [SerializeField] public bool shooting = false;
    [SerializeField] private networkedBaseball networkedBaseball;
    private float nextShootTime = 2.5f;

    private void Start()
    {
        if (!firePoint) firePoint = transform;
    }

    private void Update()
    {
        nextShootTime -= Time.deltaTime;
        if (shooting && nextShootTime < 0)
        {
            Fire();
            nextShootTime = 2.5f;
        }
    }

    public void Fire()
    {

        if (Time.time < nextShootTime) return;
        if (!ballPrefab)
        {
            Debug.LogWarning("ShootBall: Kein ballPrefab zugewiesen.");
            return;
        }
        GameObject ball = NetworkManager.Instantiate(ballPrefab, firePoint.position, firePoint.rotation);
        //GameObject ball = Instantiate(ballPrefab, firePoint.position, firePoint.rotation);

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (!rb) rb = ball.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        Vector3 v = firePoint.forward * -muzzleSpeed;

        if (inheritLauncherVelocity)
        {
            var shooterRb = GetComponentInParent<Rigidbody>();
            if (shooterRb) v += shooterRb.linearVelocity;
        }

        rb.linearVelocity = v;

        rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);

        if (lifeTime > 0f) Destroy(ball, lifeTime);
    }
}
