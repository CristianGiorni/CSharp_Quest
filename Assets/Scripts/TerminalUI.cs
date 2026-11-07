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
        if (IsOpen && Input.GetKeyDown(closeKey)) Close();
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
        if (feedbackText != null) feedbackText.text = text;
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
        if (!IsOpen || current == null) return;


        TerminalOption chosen = slot.Current;
        Debug.Log("Boton Ejecutar presionado");
        Debug.Log("Pieza en el hueco: " + (chosen != null ? chosen.value : "NINGUNA") + " | esperado: " + current.expectedValue);
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
        }
        else
        {
            slot.Flash();
            Say(string.IsNullOrEmpty(chosen.feedback) ? current.wrongLogicMessage : chosen.feedback);
            current.onWrongLogic.Invoke();
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
