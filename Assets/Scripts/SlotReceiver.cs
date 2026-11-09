using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class SlotReceiver : MonoBehaviour
{
    [Header("Que acepta esta ranura")]
    public DataType expectedType = DataType.Bool;
    public string expectedValue = "true";

    [Header("Donde queda la caja al colocarla (centro de la caja)")]
    public Transform snapPoint;

    [Header("Eventos")]
    public UnityEvent onSuccess;
    public UnityEvent onReject;

    public DataBlock Occupant { get; private set; }
    public bool Solved { get; private set; }

    // Opcional: si el objeto tiene PuzzleAttempts, los errores cuestan intentos
    public PuzzleAttempts Attempts { get; private set; }

    public bool IsLocked
    {
        get { return Attempts != null && Attempts.IsLocked; }
    }

    private SlotFeedback feedback;
    private SlotCodeLabel codeLabel;

    // Al agregar el componente, el collider pasa a ser Trigger automaticamente
    void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    void Awake()
    {
        feedback = GetComponent<SlotFeedback>();
        Attempts = GetComponent<PuzzleAttempts>();
        codeLabel = GetComponentInChildren<SlotCodeLabel>(true);
    }

    // Lo llama el jugador al colocar una caja. Devuelve false si no se pudo colocar.
    public bool TryPlace(DataBlock block)
    {
        if (Occupant != null || Solved || IsLocked) return false;

        Occupant = block;
        Vector3 pos = snapPoint != null ? snapPoint.position : transform.position;
        block.PlaceAt(pos);

        bool correct = block.type == expectedType && block.value == expectedValue;

        // El hueco del texto de codigo se completa con la caja colocada
        if (codeLabel != null) codeLabel.ShowFilled(block, correct);

        if (correct)
        {
            Solved = true;
            if (feedback != null) feedback.ShowSuccess();
            onSuccess.Invoke();
        }
        else
        {
            if (Attempts != null) Attempts.RegisterMistake();
            if (feedback != null) feedback.ShowReject();
            onReject.Invoke();
        }

        return true;
    }

    // Lo llama el jugador para retirar una caja equivocada
    public DataBlock TakeBack()
    {
        if (Occupant == null || Solved) return null;

        DataBlock block = Occupant;
        Occupant = null;
        if (feedback != null) feedback.ShowEmpty();
        if (codeLabel != null) codeLabel.ShowEmpty();
        return block;
    }
}