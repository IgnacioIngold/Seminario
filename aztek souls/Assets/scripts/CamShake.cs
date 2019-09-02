using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    public Transform Target;
    [Header("Shake Effect")]
    public float ShakeDuration = 2f;
    public float ShakeMagnitude = 4f;

    private void Awake()
    {
        Target.GetComponent<Player>().OnGetHit += ShakeOnce;
    }


    public void ShakeOnce()
    {
        StartCoroutine(Shake(ShakeDuration, ShakeMagnitude));
    }

    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            float z = UnityEngine.Random.Range(-1f, 1f) * magnitude;

            var bla = (transform.right * x * magnitude + transform.up * z * magnitude).normalized;

            transform.localPosition = bla;

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
