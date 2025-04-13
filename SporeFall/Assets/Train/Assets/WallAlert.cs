using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAlert : MonoBehaviour
{
    [SerializeField] private MeshRenderer wallVisual;
    [SerializeField] private float alertDuration = 1f;
    [SerializeField] private float transitionDuration = 1f;

    [SerializeField] Color alertColor = Color.red;
    private Color defaultColor;

    private bool colorChanged = false;

    private void Awake()
    {
        defaultColor = wallVisual.materials[0].GetColor("_FresnelColor");
        GetComponent<TrainRelayHP>().OnRelayHit += AlertColor;
    }
    private void AlertColor()
    {
        if(!colorChanged)
            StartCoroutine(ChangeColor());
    }
    IEnumerator ChangeColor()
    {
        colorChanged = true;
        // Instantly change to red for alert
        wallVisual.materials[0].SetColor("_FresnelColor", alertColor);

        // Wait for alert duration
        yield return new WaitForSeconds(alertDuration);

        // Smooth transition back to default color
        float elapsedTime = 0;
        Color startColor = alertColor;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            Color newColor = Color.Lerp(startColor, defaultColor, t);
            wallVisual.materials[0].SetColor("_FresnelColor", newColor);
            yield return null;
        }

        // Ensure we end exactly at the default color
        wallVisual.materials[0].SetColor("_FresnelColor", defaultColor);
        colorChanged = false;
    }
}
