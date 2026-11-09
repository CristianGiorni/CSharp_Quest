using System.Collections.Generic;
using UnityEngine;

// Registro de errores y aciertos del nivel actual. Es estatico: no hace falta agregarlo a ningun objeto.
// Lo alimentan SlotReceiver, ConsoleTerminal y TerminalUI; lo muestra Elevator al terminar el nivel.
public static class LevelStats
{
    public class Entry
    {
        public string label;
        public int mistakes;
        public bool solved;
    }

    private static readonly List<Entry> entries = new List<Entry>();

    public static IReadOnlyList<Entry> Entries { get { return entries; } }

    public static int TotalMistakes
    {
        get
        {
            int n = 0;
            foreach (Entry e in entries) n += e.mistakes;
            return n;
        }
    }

    // Lo llama el ascensor al iniciar el nivel
    public static void Reset()
    {
        entries.Clear();
    }

    public static void Mistake(string label)
    {
        GetOrCreate(label).mistakes++;
    }

    public static void Solved(string label)
    {
        GetOrCreate(label).solved = true;
    }

    private static Entry GetOrCreate(string label)
    {
        foreach (Entry e in entries)
        {
            if (e.label == label) return e;
        }

        Entry created = new Entry { label = label };
        entries.Add(created);
        return created;
    }
}