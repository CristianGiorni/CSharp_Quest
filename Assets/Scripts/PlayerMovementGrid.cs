using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementGrid : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float moveSpeed = 6f;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private Vector3 knockbackVelocity;
    private float knockbackTimeLeft;

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

        // Rota el input 45 grados para que coincida con la camara isometrica
        moveDirection = Quaternion.Euler(0f, 45f, 0f) * new Vector3(inputX, 0f, inputZ).normalized;
    }

    void FixedUpdate()
    {
        Vector3 velocity;

        if (knockbackTimeLeft > 0f)
        {
            // Durante el retroceso se ignora el input
            knockbackTimeLeft -= Time.fixedDeltaTime;
            velocity = knockbackVelocity;
        }
        else
        {
            velocity = moveDirection * moveSpeed;
        }

        // Se conserva la velocidad vertical (gravedad)
        velocity.y = rb.linearVelocity.y;
        rb.linearVelocity = velocity;
    }

    // Lo llama la ranura cuando rechaza una caja
    public void Knockback(Vector3 direction, float speed, float duration)
    {
        direction.y = 0f;
        knockbackVelocity = direction.normalized * speed;
        knockbackTimeLeft = duration;
    }
}