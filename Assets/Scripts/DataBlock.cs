using UnityEngine;
using TMPro;

public enum DataType { Int, Float, Bool, String }

[RequireComponent(typeof(Rigidbody))]
public class DataBlock : MonoBehaviour
{
    [Header("Dato que representa esta caja")]
    public DataType type = DataType.Bool;
    public string value = "true";

    [Header("Texto que muestra el valor (opcional)")]
    public TMP_Text label;

    public bool IsPlaced { get; private set; }

    private Rigidbody rb;
    private Vector3 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        UpdateLabel();
    }

    // Actualiza el texto tambien en el editor al cambiar el valor
    void OnValidate()
    {
        UpdateLabel();
    }

    void UpdateLabel()
    {
        if (label != null) label.text = value;
    }

    void Update()
    {
        // Evita el soft-lock: R devuelve la caja a su posicion inicial
        if (Input.GetKeyDown(KeyCode.R) && !IsPlaced)
        {
            ResetBlock();
        }
    }

    // Fija la caja en la ranura y la deja inmovil
    public void LockAt(Vector3 position)
    {
        IsPlaced = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.position = position;
    }

    // Rebote al ser rechazada: se aleja del punto indicado
    public void Bounce(Vector3 fromPoint, float force = 4f)
    {
        Vector3 dir = transform.position - fromPoint;
        dir.y = 0f;
        rb.AddForce(dir.normalized * force, ForceMode.Impulse);
    }

    // Vuelve a su posicion inicial
    public void ResetBlock()
    {
        IsPlaced = false;
        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPosition;
    }
}