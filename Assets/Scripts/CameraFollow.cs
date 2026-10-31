using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("A quien sigue")]
    public Transform target;

    [Header("Suavizado (0 = pegada al Player)")]
    public float smoothTime = 0.2f;

    private Vector3 offset;
    private Vector3 velocity;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: falta asignar el Target (Player).");
            enabled = false;
            return;
        }

        // Toma como distancia la posicion actual de la camara respecto del Player
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }
}