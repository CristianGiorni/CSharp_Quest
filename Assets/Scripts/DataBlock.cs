using UnityEngine;
using TMPro;

public enum DataType { Int, Float, Bool, String }
public enum BlockState { InWorld, Carried, InSlot }

public class DataBlock : MonoBehaviour
{
    [Header("Dato que representa esta caja")]
    public DataType type = DataType.Bool;
    public string value = "true";

    [Header("Texto que muestra el valor (opcional)")]
    public TMP_Text label;

    public BlockState State { get; private set; } = BlockState.InWorld;

    private float restY;

    void Awake()
    {
        restY = transform.position.y;
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

    // La caja pasa al inventario del jugador y desaparece del mundo
    public void PickUp()
    {
        State = BlockState.Carried;
        gameObject.SetActive(false);
    }

    // El jugador suelta la caja en el suelo (conserva la altura a la que estaba al inicio)
    public void DropAt(Vector3 position)
    {
        State = BlockState.InWorld;
        position.y = restY;
        transform.position = position;
        gameObject.SetActive(true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // La caja aparece en la posicion de la ranura
    public void PlaceAt(Vector3 position)
    {
        State = BlockState.InSlot;
        transform.position = position;
        gameObject.SetActive(true);
    }
}