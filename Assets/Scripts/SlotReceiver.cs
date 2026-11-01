using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class SlotReceiver : MonoBehaviour
{
    [Header("Que acepta esta ranura")]
    public DataType expectedType = DataType.Bool;
    public string expectedValue = "true";

    [Header("Donde queda la caja al encajar (centro de la caja)")]
    public Transform snapPoint;

    [Header("Rechazo: retroceso del Player")]
    public float knockbackSpeed = 8f;
    public float knockbackDuration = 0.3f;

    [Header("Eventos")]
    public UnityEvent onSuccess;
    public UnityEvent onReject;

    private bool solved;
    private PlayerMovementGrid player;

    // Al agregar el componente, el collider pasa a ser Trigger automaticamente
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Start()
    {
        player = FindFirstObjectByType<PlayerMovementGrid>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (solved) return;

        DataBlock block = other.GetComponentInParent<DataBlock>();
        if (block == null || block.IsPlaced) return;

        bool correct = block.type == expectedType && block.value == expectedValue;

        if (correct)
        {
            solved = true;
            Vector3 pos = snapPoint != null ? snapPoint.position : transform.position;
            block.LockAt(pos);
            onSuccess.Invoke();
        }
        else
        {
            // La caja sale rebotada y el Player retrocede en la misma direccion,
            // asi no queda pegado a la caja y el rebote se nota.
            Vector3 away = block.transform.position - transform.position;
            away.y = 0f;

            block.Bounce(transform.position);

            if (player != null)
            {
                player.Knockback(away, knockbackSpeed, knockbackDuration);
            }

            onReject.Invoke();
        }
    }
}