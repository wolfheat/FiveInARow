using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Blinker : MonoBehaviour
{
    [SerializeField] Image blinkerLight;

    private float BlinkTime { get; set; }
    private const float DefaultBlinkTime = 0.4f;

    public void StartBlink(float time = DefaultBlinkTime)
    {
        if (!gameObject.activeSelf) return;
        BlinkTime = time;
        StartCoroutine(BlinkCoroutine());
    }

    private IEnumerator BlinkCoroutine()
    {
        float timer = 0;
        Color start = blinkerLight.color;
        start = new Color(start.r, start.b, start.b, 0);
        Color end = new Color(start.r,start.b,start.b, 1);
        while (timer < BlinkTime) 
        {
            timer += Time.deltaTime;
            float changingValue = 2*timer / BlinkTime;
            float distance = 1-Math.Abs(changingValue-1f);
            blinkerLight.color = Vector4.Lerp(start, end, distance);
            yield return null;
        }
        blinkerLight.color = start;
        yield return null;
    }

}
