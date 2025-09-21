using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects when a specific object enters its trigger and invokes a UnityEvent.
/// </summary>
public class HoleTrigger : MonoBehaviour
{
    /// <summary>
    /// The tag of the object we are looking for (e.g., the golf ball).
    /// </summary>
    [SerializeField]
    private string m_TargetTag = "GolfBall";

    /// <summary>
    /// This event is invoked when the target object enters the trigger.
    /// </summary>
    public UnityEvent OnBallEntered;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that entered has the correct tag.
        if (other.CompareTag(m_TargetTag))
        {
            Debug.Log("Ball entered the hole!");

            // Invoke the event so that any listening scripts are notified.
            OnBallEntered.Invoke();
        }
    }
}