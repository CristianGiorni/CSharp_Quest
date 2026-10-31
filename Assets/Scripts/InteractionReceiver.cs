using System.Collections;
using UnityEngine;

public class InteractionReceiver : MonoBehaviour
{
    [Header("Estado final (rotacion local en grados)")]
    public Vector3 openEulerAngles = Vector3.zero;
    public float duration = 1f;

    // Se llama desde el evento onSuccess de la ranura
    public void Activate()
    {
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    private IEnumerator Animate()
    {
        Quaternion from = transform.localRotation;
        Quaternion to = Quaternion.Euler(openEulerAngles);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localRotation = to;
    }
}