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

    private readonly List<DataBlock> items = new List<DataBlock>();
    private int selected;

    private DataBlock nearBlock;
    private SlotReceiver nearSlot;
    private float blockDistance;
    private float slotDistance;

    private string message = "";
    private float messageTimer;

    void Update()
    {
        // En pausa no se interactua
        if (Time.timeScale == 0f) return;

        FindTargets();
        HandleSelection();

        if (Input.GetKeyDown(interactKey)) Interact();

        if (messageTimer > 0f) messageTimer -= Time.deltaTime;
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
            Say("Inventario lleno");
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
                Say("Inventario lleno");
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
        return b.type.ToString().ToLower() + " " + b.value;
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

        if (items.Count > 1)
        {
            GUI.Label(new Rect(20, 55, 1400, 40), nextItemKey + " o numero: cambiar de caja", style);
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