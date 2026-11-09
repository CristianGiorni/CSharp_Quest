using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controla la ventana de la terminal flotante. Va en el objeto TerminalCanvas (siempre activo).
public class TerminalUI : MonoBehaviour
{
    public static TerminalUI Instance { get; private set; }

    [Header("Panel")]
    public GameObject panel;

    [Header("Codigo")]
    public TMP_Text topText;
    public TMP_Text leftText;
    public TMP_Text rightText;
    public TMP_Text bottomText;
    public TerminalSlot slot;

    [Header("Piezas")]
    public Transform optionsContainer;
    public TerminalOption optionTemplate;

    [Header("Controles")]
    public Button executeButton;
    public Button closeButton;
    public TMP_Text feedbackText;
    public KeyCode closeKey = KeyCode.Escape;

    [Header("Corazones (intentos)")]
    public Image[] hearts;
    public Color heartFull = new Color(0.9f, 0.2f, 0.25f);
    public Color heartEmpty = new Color(0.25f, 0.25f, 0.3f);

    public bool IsOpen { get; private set; }

    private ConsoleTerminal current;
    private readonly List<TerminalOption> spawned = new List<TerminalOption>();
    private PlayerMovementGrid movement;
    private PlayerInteraction interaction;

    void Awake()
    {
        Instance = this;

        panel.SetActive(false);
        optionTemplate.gameObject.SetActive(false);

        executeButton.onClick.AddListener(Execute);
        closeButton.onClick.AddListener(Close);
    }

    void Update()
    {
        if (!IsOpen) return;

        if (Input.GetKeyDown(closeKey))
        {
            Close();
            return;
        }

        if (current != null && current.LockedUntil > 0f) UpdateLock();
    }

    public void Open(ConsoleTerminal console)
    {
        if (IsOpen) return;

        current = console;
        IsOpen = true;

        SetCode(topText, console.codeTop);
        SetCode(leftText, console.codeBeforeSlot);
        SetCode(rightText, console.codeAfterSlot);
        SetCode(bottomText, console.codeBottom);
        Say("");

        slot.Clear();
        slot.checkType = console.checkTypeOnDrop;
        slot.expectedType = console.expectedType;

        ClearOptions();
        for (int i = 0; i < console.options.Length; i++)
        {
            TerminalOptionData data = console.options[i];
            TerminalOption option = Instantiate(optionTemplate, optionsContainer);
            option.gameObject.SetActive(true);
            option.Setup(data.type, data.value, data.feedback, optionsContainer, i + 1);
            spawned.Add(option);
        }

        executeButton.interactable = !console.IsLocked;
        RefreshHearts();

        panel.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)panel.transform);

        FreezePlayer(true);
    }

    public void Close()
    {
        if (!IsOpen) return;

        IsOpen = false;
        current = null;
        panel.SetActive(false);
        ClearOptions();
        FreezePlayer(false);
    }

    public void Say(string text)
    {
        if (feedbackText == null || feedbackText.text == text) return;
        feedbackText.text = text;
    }

    // Muestra el texto, o oculta la linea si esta vacia
    private void SetCode(TMP_Text target, string text)
    {
        target.text = text;
        target.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    // Boton "Ejecutar"
    private void Execute()
    {
        if (!IsOpen || current == null || current.IsLocked) return;

        TerminalOption chosen = slot.Current;

        if (chosen == null)
        {
            Say("Arrastra una pieza al hueco [ ___ ] antes de ejecutar.");
            return;
        }

        bool correct = chosen.value == current.expectedValue
                       && (!current.checkTypeOnDrop || chosen.type == current.expectedType);

        if (correct)
        {
            ConsoleTerminal solvedConsole = current;
            Close();
            solvedConsole.Solve();
            return;
        }

        // Respuesta incorrecta: se pierde un corazon
        string reason = string.IsNullOrEmpty(chosen.feedback) ? current.wrongLogicMessage : chosen.feedback;

        current.HeartsLeft = Mathf.Max(0, current.HeartsLeft - 1);
        RefreshHearts();
        slot.Flash();
        current.onWrongLogic.Invoke();

        if (current.HeartsLeft <= 0)
        {
            LockTerminal();
        }
        else
        {
            string attempts = current.HeartsLeft == 1 ? " intento." : " intentos.";
            Say(reason + "\nTe quedan " + current.HeartsLeft + attempts);
        }
    }

    // Sin corazones: la terminal se reinicia (espera + pista), sin perder progreso del nivel
    private void LockTerminal()
    {
        current.LockedUntil = Time.time + current.lockSeconds;
        executeButton.interactable = false;

        TerminalOption placed = slot.Current;
        if (placed != null)
        {
            slot.Release(placed);
            placed.ReturnHome();
        }

        UpdateLock();
    }

    private void UpdateLock()
    {
        float remaining = current.LockedUntil - Time.time;

        if (remaining > 0f)
        {
            string wait = "Sin intentos. Reiniciando sistema... " + Mathf.CeilToInt(remaining) + " s";
            Say(current.showHintOnLock && !string.IsNullOrEmpty(current.hint) ? current.hint + "\n" + wait : wait);
            return;
        }

        // Termino la espera: vuelven los corazones
        current.LockedUntil = 0f;
        current.HeartsLeft = current.maxHearts;
        executeButton.interactable = true;
        RefreshHearts();
        Say("Sistema reiniciado. Tenes de nuevo todos los intentos.");
    }

    private void RefreshHearts()
    {
        if (hearts == null || current == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            hearts[i].gameObject.SetActive(i < current.maxHearts);
            hearts[i].color = i < current.HeartsLeft ? heartFull : heartEmpty;
        }
    }

    private void ClearOptions()
    {
        foreach (TerminalOption option in spawned)
        {
            if (option != null) Destroy(option.gameObject);
        }
        spawned.Clear();
    }

    // Mientras la terminal esta abierta, el Player no se mueve ni interactua
    private void FreezePlayer(bool freeze)
    {
        if (movement == null) movement = FindFirstObjectByType<PlayerMovementGrid>();
        if (movement == null) return;

        if (interaction == null) interaction = movement.GetComponent<PlayerInteraction>();

        Rigidbody rb = movement.GetComponent<Rigidbody>();
        if (freeze && rb != null)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }

        movement.enabled = !freeze;
        if (interaction != null) interaction.enabled = !freeze;
    }
}