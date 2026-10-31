using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementGrid : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 6f; // Lo subí un puntito para que sea más ágil

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueamos las rotaciones para que no se caiga ni ruede
        rb.freezeRotation = true;
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // Rota el input 45° para que coincida con la cámara isométrica
        moveDirection = Quaternion.Euler(0f, 45f, 0f) * new Vector3(inputX, 0f, inputZ).normalized;
    }

    void FixedUpdate()
    {
        // Velocidad horizontal según el input; se conserva la vertical (gravedad/saltos)
        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }
}