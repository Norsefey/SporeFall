using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BillboardUIUpdater : MonoBehaviour
{
    private Transform targetCamera = null;
    [SerializeField] private Slider hpDisplay;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Damageable hpManager;
    [SerializeField] private bool hideUI = true;
    private CanvasGroup groupAlpha;
    private Coroutine hideUICoroutine;
    private void InitializeUI()
    {
        if(groupAlpha == null)
            groupAlpha = GetComponent<CanvasGroup>();
        groupAlpha.alpha = 0.5f;
        hpText.text = hpManager.CurrentHP + "/" + hpManager.MaxHP;
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
        if (targetCamera != null)
        {
            transform.LookAt(transform.position + targetCamera.forward);
        }
    }
    public void HandleHPChange(Damageable damagedEnemy, float damage)
    {
        //Debug.Log("Updating Enemy HP UI");
        hpText.text = damagedEnemy.CurrentHP.ToString("F0") + "/" + damagedEnemy.MaxHP;
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
        targetCamera = target;
    }
    IEnumerator HideUI()
    {
        if (groupAlpha == null)
            yield break;

        yield return new WaitForSeconds(1.5f);

        groupAlpha.alpha = 0;

    }
}
