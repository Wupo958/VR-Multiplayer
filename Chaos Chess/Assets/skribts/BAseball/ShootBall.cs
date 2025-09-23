using UnityEngine;
using Unity.Netcode;
using System.Collections;

[RequireComponent(typeof(NetworkObject))]
public class ShootBall : NetworkBehaviour
{
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float muzzleSpeed = 45f;
    [SerializeField] private bool useGravity = true;
    [SerializeField] private bool inheritLauncherVelocity = true;
    [SerializeField] private float lifeTime = 15f;
    [SerializeField] public bool shooting = false;

    private float nextShootTime = 2.5f;

    private void Start()
    {
        if (!firePoint) firePoint = transform;
    }

    private void Update()
    {
        nextShootTime -= Time.deltaTime;
        if (shooting && nextShootTime < 0f)
        {
            Fire();
            nextShootTime = 2.5f;
        }
    }

    public void Fire()
    {
        if (!ballPrefab) return;

        var pos = firePoint ? firePoint.position : transform.position;
        var rot = firePoint ? firePoint.rotation : transform.rotation;

        if (IsServer)
            FireServer(pos, rot);
        else
            FireServerRpc(pos, rot);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FireServerRpc(Vector3 pos, Quaternion rot)
    {
        FireServer(pos, rot);
    }

    private void FireServer(Vector3 pos, Quaternion rot)
    {
        var ball = Instantiate(ballPrefab, pos, rot);
        var no = ball.GetComponent<NetworkObject>();
        if (no) no.Spawn(true);

        var rb = ball.GetComponent<Rigidbody>() ?? ball.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        var dir = rot * Vector3.forward;
        var v = dir * -muzzleSpeed;
        if (inheritLauncherVelocity)
        {
            var shooterRb = GetComponentInParent<Rigidbody>();
            if (shooterRb) v += shooterRb.linearVelocity;
        }

        rb.linearVelocity = v;
        rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);

        if (lifeTime > 0f) StartCoroutine(DespawnAfter(no, ball, lifeTime));
    }

    private IEnumerator DespawnAfter(NetworkObject no, GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (no && no.IsSpawned) no.Despawn();
        Destroy(go);
    }
}
