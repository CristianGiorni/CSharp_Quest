using UnityEngine;

// Intentos (corazones) de un puzle. Se agrega al objeto de la ranura, junto a SlotReceiver.
// Al agotarse, el puzle se bloquea unos segundos mostrando una pista, y los intentos vuelven.
public class PuzzleAttempts : MonoBehaviour
{
    [Header("Intentos")]
    public int maxHearts = 3;
    [Tooltip("Segundos de espera cuando se acaban los intentos")]
    public float lockSeconds = 8f;

    [Header("Pista que se muestra al quedarse sin intentos")]
    [TextArea(1, 3)] public string hint = "Pista: fijate en el tipo de dato que espera la ranura y en el valor de cada caja.";

    public int HeartsLeft { get; private set; }
    public float LockedUntil { get; private set; }

    public bool IsLocked
    {
        get { return LockedUntil > 0f && Time.time < LockedUntil; }
    }

    public float RemainingLock
    {
        get { return Mathf.Max(0f, LockedUntil - Time.time); }
    }

    void Awake()
    {
        HeartsLeft = maxHearts;
    }

    void Update()
    {
        // Termino la espera: vuelven todos los intentos
        if (LockedUntil > 0f && Time.time >= LockedUntil)
        {
            LockedUntil = 0f;
            HeartsLeft = maxHearts;
        }
    }

    // Se llama cada vez que el jugador se equivoca
    public void RegisterMistake()
    {
        if (IsLocked) return;

        HeartsLeft = Mathf.Max(0, HeartsLeft - 1);

        if (HeartsLeft <= 0)
        {
            LockedUntil = Time.time + lockSeconds;
        }
    }
}