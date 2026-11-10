using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementGrid : MonoBehaviour
{
    [Header("Configuracion de Movimiento")]
    public float moveSpeed = 6f;

    [Header("Modelo visual (opcional)")]
    [Tooltip("El modelo hijo del Player (el robot). Si queda vacio, se busca automaticamente.")]
    public Transform visual;
    [Tooltip("Que tan rapido gira el modelo hacia donde camina")]
    public float turnSpeed = 14f;
    [Tooltip("Nombre del parametro Float del Animator que recibe la velocidad (si no existe, se ignora)")]
    public string speedParameter = "Speed";

    private Animator animator;
    private bool hasSpeedParameter;

    private Rigidbody rb;
    private Vector3 moveDirection;

    private Vector3 knockbackVelocity;
    private float knockbackTimeLeft;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Bloqueamos las rotaciones para que no se caiga ni ruede
        rb.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // El movimiento lo hace la fisica, no la animacion
            animator.applyRootMotion = false;

            foreach (AnimatorControllerParameter p in animator.parameters)
            {
                if (p.name == speedParameter && p.type == AnimatorControllerParameterType.Float) hasSpeedParameter = true;
            }

            if (visual == null) visual = animator.transform;
        }
    }

    void Update()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        // Rota el input 45 grados para que coincida con la camara isometrica
        moveDirection = Quaternion.Euler(0f, 45f, 0f) * new Vector3(inputX, 0f, inputZ).normalized;

        UpdateVisual();
    }

    // El modelo mira hacia donde camina y la animacion cambia entre reposo y correr
    void UpdateVisual()
    {
        bool moving = moveDirection.sqrMagnitude > 0.01f && Time.timeScale > 0f;

        if (visual != null && moving)
        {
            Quaternion target = Quaternion.LookRotation(moveDirection, Vector3.up);
            visual.rotation = Quaternion.Slerp(visual.rotation, target, turnSpeed * Time.deltaTime);
        }

        if (animator != null && hasSpeedParameter)
        {
            animator.SetFloat(speedParameter, moving ? 1f : 0f);
        }
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