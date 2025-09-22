using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ShootBall : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject ballPrefab; // Muss einen Rigidbody + Collider haben
    [SerializeField] private Transform firePoint;   // Da spawnt der Ball (Position + Richtung)
    [SerializeField] private GameObject bat;   // Da spawnt der Ball (Position + Richtung)

    [Header("Schuss-Settings")]
    [SerializeField] private float muzzleSpeed = 45f; // m/s entlang firePoint.forward
    [SerializeField] private bool useGravity = true;
    [SerializeField] private bool inheritLauncherVelocity = true; // Startgeschw. des Werfers addieren
    [SerializeField] private float lifeTime = 8f;    // Auto-Despawn

    [Header("Optional: misc")]
    [SerializeField] private bool shooting = false;
    private float nextShootTime;

    private void Start()
    {
        if (!firePoint) firePoint = transform;
    }

    private void Update()
    {
        if (bat.GetComponent<XRGrabInteractable>().isSelected && !shooting)
        {
            InvokeRepeating(nameof(Fire), 2.5f, 2.5f);
            shooting = true;
        }
    }

    // --- Manuell aufrufen (UI Button, XR-Event, Input Action) ---
    public Rigidbody Fire()
    {
        if (Time.time < nextShootTime) return null;
        if (!ballPrefab)
        {
            Debug.LogWarning("ShootBall: Kein ballPrefab zugewiesen.");
            return null;
        }

        // Ball instanziieren
        GameObject ball = Instantiate(ballPrefab, firePoint.position, firePoint.rotation);

        // Rigidbody holen/absichern
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        if (!rb) rb = ball.AddComponent<Rigidbody>();
        rb.useGravity = useGravity;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; // weniger Tunneling

        // Startgeschwindigkeit berechnen
        Vector3 v = firePoint.forward * -muzzleSpeed; // falls nötig Richtung umdrehen: * -muzzleSpeed

        // Geschwindigkeit des Launchers addieren (falls der Shooter sich bewegt)
        if (inheritLauncherVelocity)
        {
            var shooterRb = GetComponentInParent<Rigidbody>();
            if (shooterRb) v += shooterRb.linearVelocity; // <-- korrekt: velocity
        }

        rb.linearVelocity = v;

        // kleiner Up-Impuls (optional)
        rb.AddForce(Vector3.up * 3f, ForceMode.Impulse);

        // Sicherheit: nach X Sekunden zerstören
        if (lifeTime > 0f) Destroy(ball, lifeTime);

        return rb;
    }
}
