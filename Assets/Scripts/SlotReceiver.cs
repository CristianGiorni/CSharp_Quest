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

    [Header("Eventos")]
    public UnityEvent onSuccess;
    public UnityEvent onReject;

    private bool solved;

    // Al agregar el componente, el collider pasa a ser Trigger automaticamente
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Algo entro al trigger: " + other.name);

        if (solved) return;

        DataBlock block = other.GetComponentInParent<DataBlock>();
        if (block == null || block.IsPlaced)
        {
            Debug.Log("Sin DataBlock (o ya colocada) en: " + other.name);
            return;
        }

        bool correct = block.type == expectedType && block.value == expectedValue;

        if (correct)
        {
            solved = true;
            Vector3 pos = snapPoint != null ? snapPoint.position : transform.position;
            block.LockAt(pos);
            Debug.Log("Encajada, se dispara onSuccess");
            onSuccess.Invoke();
        }
        else
        {
            Debug.Log("Rechazada: " + block.type + " / " + block.value);
            block.Bounce(transform.position);
            onReject.Invoke();
        }
    }
}