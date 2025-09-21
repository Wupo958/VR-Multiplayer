using UnityEngine;

public class ModuleSocket : MonoBehaviour
{
    public enum SocketType { PathIn, PathOut }
    public SocketType type = SocketType.PathOut;
    public Vector3 normal = Vector3.forward;
}
