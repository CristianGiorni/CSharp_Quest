using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Inventario")]
    public int capacity = 3;

    [Header("Interaccion")]
    public float interactRadius = 1.6f;
    public KeyCode interactKey = KeyCode.E;
    public KeyCode nextItemKey = KeyCode.Q;
    public KeyCode dropKey = KeyCode.G;

    [Header("Soltar caja")]
    [Tooltip("A que distancia delante del jugador cae la caja")]
    public float dropDistance = 1.3f;
    [Tooltip("Capas que bloquean el lugar donde se suelta (paredes, objetos)")]
    public LayerMask dropBlockers = ~0;

    private readonly List<DataBlock> items = new List<DataBlock>();
    private int selected;

    private DataBlock nearBlock;
    private SlotReceiver nearSlot;
    private float blockDistance;
    private float slotDistance;

    private Rigidbody body;
    private Vector3 lastMoveDir = Vector3.forward;

    private string message = "";
    private float messageTimer;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // En pausa no se interactua
        if (Time.timeScale == 0f) return;

        TrackDirection();
        FindTargets();
        HandleSelection();

        if (Input.GetKeyDown(interactKey)) Interact();
        if (Input.GetKeyDown(dropKey)) DropSelected();

        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
    }

    // Recuerda hacia donde se esta moviendo el jugador, para soltar la caja adelante
    void TrackDirection()
    {
        if (body == null) return;

        Vector3 v = body.linearVelocity;
        v.y = 0f;
        if (v.sqrMagnitude > 0.05f) lastMoveDir = v.normalized;
    }

    // Suelta la caja seleccionada en el suelo, delante del jugador
    void DropSelected()
    {
        if (items.Count == 0)
        {
            Say("No tenes cajas para soltar");
            return;
        }

        // No soltar dentro de una pared: si hay algo delante, la caja cae mas cerca
        Vector3 origin = transform.position;
        float distance = dropDistance;

        RaycastHit hit;
        if (Physics.Raycast(origin, lastMoveDir, out hit, dropDistance + 0.5f, dropBlockers, QueryTriggerInteraction.Ignore)
            && hit.collider.GetComponentInParent<DataBlock>() == null)
        {
            distance = Mathf.Max(0.3f, hit.distance - 0.6f);
        }

        DataBlock block = items[selected];
        items.RemoveAt(selected);
        selected = Mathf.Clamp(selected, 0, Mathf.Max(0, items.Count - 1));

        block.DropAt(origin + lastMoveDir * distance);
        AudioManager.Click();
    }

    // Busca la caja y la ranura mas cercanas dentro del radio
    void FindTargets()
    {
        nearBlock = null;
        nearSlot = null;
        blockDistance = float.MaxValue;
        slotDistance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(transform.position, interactRadius, ~0, QueryTriggerInteraction.Collide);

        foreach (Collider c in hits)
        {
            DataBlock block = c.GetComponentInParent<DataBlock>();
            if (block != null)
            {
                float d = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
                if (block.State == BlockState.InWorld && d < blockDistance)
                {
                    nearBlock = block;
                    blockDistance = d;
                }
                continue;
            }

            SlotReceiver slot = c.GetComponentInParent<SlotReceiver>();
            if (slot != null)
            {
                float d = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
                if (d < slotDistance)
                {
                    nearSlot = slot;
                    slotDistance = d;
                }
            }
        }
    }

    // Q cambia a la siguiente caja; las teclas 1, 2, 3... eligen directamente
    void HandleSelection()
    {
        if (items.Count == 0) return;

        if (Input.GetKeyDown(nextItemKey)) selected = (selected + 1) % items.Count;

        for (int i = 0; i < items.Count && i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) selected = i;
        }
    }

    bool SlotIsTarget()
    {
        return nearSlot != null && (nearBlock == null || slotDistance <= blockDistance);
    }

    void Interact()
    {
        if (SlotIsTarget()) InteractWithSlot();
        else if (nearBlock != null) PickUpBlock();
    }

    void PickUpBlock()
    {
        if (items.Count >= capacity)
        {
            Say("Inventario lleno. " + dropKey + ": soltar una caja");
            return;
        }

        AddItem(nearBlock);
    }

    void InteractWithSlot()
    {
        if (nearSlot.Solved)
        {
            Say("Esta ranura ya esta resuelta");
            return;
        }

        // Ranura con una caja equivocada: la retira
        if (nearSlot.Occupant != null)
        {
            if (items.Count >= capacity)
            {
                Say("Inventario lleno. " + dropKey + ": soltar una caja");
                return;
            }

            AddItem(nearSlot.TakeBack());
            return;
        }

        // Sin intentos: hay que esperar a que se reinicie
        if (nearSlot.IsLocked)
        {
            Say("La ranura se esta reiniciando. Espera " + Mathf.CeilToInt(nearSlot.Attempts.RemainingLock) + " s.");
            return;
        }

        // Ranura vacia: coloca la caja seleccionada
        if (items.Count == 0)
        {
            Say("No tenes cajas en el inventario");
            return;
        }

        DataBlock block = items[selected];
        items.RemoveAt(selected);
        selected = Mathf.Clamp(selected, 0, Mathf.Max(0, items.Count - 1));
        nearSlot.TryPlace(block);
    }

    void AddItem(DataBlock block)
    {
        AudioManager.Pickup();
        block.PickUp();
        items.Add(block);
        selected = items.Count - 1;
    }

    void Say(string text, float seconds = 2f)
    {
        message = text;
        messageTimer = seconds;
    }

    string Describe(DataBlock b)
    {
        string typeName = b.type.ToString().ToLower();

        // Si el valor ya es el nombre del tipo (cajas del nivel 1), no se repite: "int" en vez de "int int"
        if (string.Equals(b.value, typeName, System.StringComparison.OrdinalIgnoreCase)) return typeName;

        return typeName + " " + b.value;
    }

    string GetPrompt()
    {
        if (SlotIsTarget())
        {
            if (nearSlot.Solved) return "";
            if (nearSlot.Occupant != null) return interactKey + ": retirar la caja de la ranura";
            if (nearSlot.IsLocked) return "";
            if (items.Count > 0) return interactKey + ": colocar " + Describe(items[selected]);
            return "";
        }

        if (nearBlock != null) return interactKey + ": recoger " + Describe(nearBlock);
        return "";
    }

    // Intentos restantes, o la pista con cuenta regresiva si la ranura esta bloqueada
    string GetSlotInfo()
    {
        if (!SlotIsTarget() || nearSlot.Solved || nearSlot.Attempts == null) return "";

        PuzzleAttempts a = nearSlot.Attempts;

        if (a.IsLocked)
        {
            return a.hint + "\nReiniciando ranura... " + Mathf.CeilToInt(a.RemainingLock) + " s";
        }

        return "Intentos: " + a.HeartsLeft + "/" + a.maxHearts;
    }

    // HUD simple para el prototipo (despues se reemplaza por UI de verdad)
    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 24;
        style.normal.textColor = Color.white;

        string inv = "Inventario (" + items.Count + "/" + capacity + ")";
        if (items.Count == 0) inv += ": vacio";

        for (int i = 0; i < items.Count; i++)
        {
            string text = (i + 1) + ": " + Describe(items[i]);
            inv += (i == selected) ? "   [" + text + "]" : "   " + text;
        }

        GUI.Label(new Rect(20, 20, 1400, 40), inv, style);

        if (items.Count > 0)
        {
            string help = dropKey + ": soltar la caja seleccionada";
            if (items.Count > 1) help = nextItemKey + " o numero: cambiar de caja   |   " + help;
            GUI.Label(new Rect(20, 55, 1400, 40), help, style);
        }

        string slotInfo = GetSlotInfo();
        if (slotInfo != "")
        {
            GUIStyle info = new GUIStyle(style);
            info.wordWrap = true;
            info.alignment = TextAnchor.LowerLeft;
            info.normal.textColor = new Color(1f, 0.85f, 0.3f);
            GUI.Label(new Rect(20, Screen.height - 220, 1100, 110), slotInfo, info);
        }

        string prompt = GetPrompt();
        if (prompt != "") GUI.Label(new Rect(20, Screen.height - 100, 1400, 40), prompt, style);

        if (messageTimer > 0f) GUI.Label(new Rect(20, Screen.height - 60, 1400, 40), message, style);
    }
}