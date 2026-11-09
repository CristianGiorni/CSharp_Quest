using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Puerta que se desliza al activarse (como la del ascensor). Va en el objeto padre de las hojas.
// Se conecta con On Solved > SlidingDoor > Activate ().
public class SlidingDoor : MonoBehaviour
{
    [Header("Hojas de la puerta (hijos de este objeto)")]
    [Tooltip("La primera se desliza hacia -eje y la segunda hacia +eje. Si hay una sola, se desliza hacia +eje.")]
    public Transform[] leaves;

    [Header("Movimiento")]
    [Tooltip("Direccion del deslizamiento, en el espacio local de este objeto. (1,0,0) = hacia los costados en X.")]
    public Vector3 slideAxis = Vector3.right;
    public float openDistance = 1.5f;
    public float openDuration = 1.2f;

    [Header("Eventos")]
    public UnityEvent onOpened;

    private Vector3[] closedPositions;
    private bool opened;

    void Awake()
    {
        closedPositions = new Vector3[leaves.Length];
        for (int i = 0; i < leaves.Length; i++)
        {
            if (leaves[i] != null) closedPositions[i] = leaves[i].position;
        }
    }

    public void Activate()
    {
        if (opened) return;
        opened = true;
        AudioManager.Door();
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        Vector3 worldAxis = transform.TransformDirection(slideAxis.normalized);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / openDuration;
            float k = Mathf.SmoothStep(0f, 1f, t);

            for (int i = 0; i < leaves.Length; i++)
            {
                if (leaves[i] == null) continue;

                float dir = (leaves.Length == 1) ? 1f : (i % 2 == 0 ? -1f : 1f);
                leaves[i].position = closedPositions[i] + worldAxis * (dir * openDistance * k);
            }

            yield return null;
        }

        onOpened.Invoke();
    }
}