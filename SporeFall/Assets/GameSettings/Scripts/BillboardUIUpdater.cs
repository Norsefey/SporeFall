using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BillboardUIUpdater : MonoBehaviour
{
    private Transform lookAtTarget = null;
    [SerializeField] private Slider hpDisplay;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Damageable hpManager;
    [SerializeField] private bool hideUI = true;
    private CanvasGroup groupAlpha;
    private Coroutine hideUICoroutine;

    public bool lockYAxisOnly = false;

    private void InitializeUI()
    {
        if(groupAlpha == null)
            groupAlpha = GetComponent<CanvasGroup>();
        groupAlpha.alpha = 0.5f;
        hpText.text = hpManager.CurrentHealth.ToString("F0") + "/" + hpManager.maxHealth.ToString("F0");
        hpDisplay.maxValue = hpManager.maxHealth;
        hpDisplay.value = hpManager.maxHealth;
    }
    private void OnEnable()
    {
        InitializeUI();
        hpManager.OnHPChange += HandleHPChange;
        if (hideUICoroutine != null)
            hideUICoroutine = StartCoroutine(HideUI(1.5f));
    }
    private void OnDisable()
    {
        hpManager.OnHPChange -= HandleHPChange;
    }
    private void LateUpdate()
    {
        if(lookAtTarget != null)
        {
            // Billboard the UI to face the camera
            if (lockYAxisOnly)
            {
                // Only rotate around Y axis (good for character-attached UI)
                Vector3 directionToCamera = lookAtTarget.position - transform.position;
                directionToCamera.y = 0; // Zero out the Y component

                if (directionToCamera != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-directionToCamera);
                }
            }
            else
            {
                // Full billboarding - always face camera directly
                transform.rotation = lookAtTarget.rotation;
            }
        }
    }
    public void HandleHPChange(Damageable damageable, float damage)
    {
        // make sure slider is accurate
        if (hpDisplay.maxValue != damageable.maxHealth)
        {
            hpDisplay.maxValue = damageable.maxHealth;
            hpDisplay.value = hpManager.CurrentHealth;
        }

        hpText.text = damageable.CurrentHealth.ToString("F0") + "/" + damageable.maxHealth.ToString("F0");
        hpDisplay.value = damageable.CurrentHealth;
        
        if(hideUI && groupAlpha != null)
        {
            if(hideUICoroutine != null)
                StopCoroutine(hideUICoroutine);
            
            groupAlpha.alpha = .5f;
            if(hideUI)
                hideUICoroutine = StartCoroutine(HideUI(1.5f));
        }
    }
    public void SetupTarget(Transform target)
    {
        lookAtTarget = target;
    }
    public void DisplayMessage(string newText, float displayTime)
    {
        hpText.text = newText;
        if (hideUI && groupAlpha != null)
        {
            if (hideUICoroutine != null)
                StopCoroutine(hideUICoroutine);

            groupAlpha.alpha = .5f;
            if (hideUI)
                hideUICoroutine = StartCoroutine(HideUI(displayTime));
        }
    }
    IEnumerator HideUI(float delay)
    {
        if (groupAlpha == null)
            yield break;

        yield return new WaitForSeconds(delay);

        groupAlpha.alpha = 0;

    }
}
