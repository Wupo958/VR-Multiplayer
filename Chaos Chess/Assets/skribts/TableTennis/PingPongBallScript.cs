using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class PingPongBallScript : NetworkBehaviour
{
    [Header("Maximale Geschwindigkeit (m/s)")]
    [SerializeField] private float maxSpeed = 10f;

    private Rigidbody rb;
    private float maxSpeedSqr;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        maxSpeedSqr = maxSpeed * maxSpeed;
    }

    public override void OnNetworkSpawn()
    {
        // Physik am besten server-seitig steuern
        if (!IsServer && rb != null)
        {
            // Sicherheitshalber: Client-Physik nicht simulieren
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        // Nur der Server sollte die Physik (inkl. Capping) machen
        if (!IsServer || rb == null) return;

        Vector3 v = rb.linearVelocity;
        if (v.sqrMagnitude > maxSpeedSqr)
        {
            rb.linearVelocity = v.normalized * maxSpeed;
        }
    }

    // Falls du den Wert zur Laufzeit �nderst:
    private void OnValidate()
    {
        if (maxSpeed < 0f) maxSpeed = 0f;
        maxSpeedSqr = maxSpeed * maxSpeed;
    }
}
