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
        hpText.text = hpManager.CurrentHP.ToString("F0") + "/" + hpManager.MaxHP.ToString("F0");
        hpDisplay.maxValue = hpManager.MaxHP;
        hpDisplay.value = hpManager.MaxHP;
    }
    private void OnEnable()
    {
        InitializeUI();
        hpManager.OnHPChange += HandleHPChange;
        if (hideUICoroutine != null)
            hideUICoroutine = StartCoroutine(HideUI());
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
    public void HandleHPChange(Damageable damagedEnemy, float damage)
    {
        // make sure slider is accurate
        if (hpDisplay.maxValue != damagedEnemy.MaxHP)
        {
            hpDisplay.maxValue = damagedEnemy.MaxHP;
            hpDisplay.value = hpManager.CurrentHP;
        }

        hpText.text = damagedEnemy.CurrentHP.ToString("F0") + "/" + damagedEnemy.MaxHP.ToString("F0");
        hpDisplay.value = damagedEnemy.CurrentHP;
        
        if(hideUI && groupAlpha != null)
        {
            if(hideUICoroutine != null)
                StopCoroutine(hideUICoroutine);
            
            groupAlpha.alpha = .5f;
            if(hideUI)
                hideUICoroutine = StartCoroutine(HideUI());
        }
    }
    public void SetupTarget(Transform target)
    {
        lookAtTarget = target;
    }
    IEnumerator HideUI()
    {
        if (groupAlpha == null)
            yield break;

        yield return new WaitForSeconds(1.5f);

        groupAlpha.alpha = 0;

    }
}
