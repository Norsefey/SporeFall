using System.Collections;
using TMPro;
using UnityEngine;

public class BillboardUIUpdater : MonoBehaviour
{
    private Transform targetCamera = null;
    [SerializeField] private TMP_Text hpDisplay;
    [SerializeField] private Damageable hpManager;
    [SerializeField] private CanvasGroup groupAlpha;

    private Coroutine hideUICoroutine;
    private void InitializeUI()
    {
        hpDisplay.text = hpManager.CurrentHP + "/" + hpManager.MaxHP;
    }
    private void OnEnable()
    {
        InitializeUI();
        hpManager.OnHPChange += HandleHPChange;

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
        hpDisplay.text = damagedEnemy.CurrentHP.ToString("F0") + "/" + damagedEnemy.MaxHP;
        
        if(groupAlpha != null)
        {
            if(hideUICoroutine != null)
                StopCoroutine(hideUICoroutine);
            
            groupAlpha.alpha = 1;
            hideUICoroutine = StartCoroutine(HideUI());
        }
    }
    public void SetupTarget(Transform target)
    {
        //Debug.Log("Setting Player Target");
        targetCamera = target;
    }

    IEnumerator HideUI()
    {
        if (groupAlpha == null)
            yield break;

        yield return new WaitForSeconds(1);

        groupAlpha.alpha = 0;

    }
}
