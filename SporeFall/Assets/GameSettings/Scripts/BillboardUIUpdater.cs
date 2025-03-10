using System.Collections;
using TMPro;
using UnityEngine;

public class BillboardUIUpdater : MonoBehaviour
{
    private Transform targetCamera = null;
    [SerializeField] private TMP_Text hpDisplay;
    [SerializeField] private Damageable hpManager;
    private void InitializeUI()
    {
        hpDisplay.text = hpManager.CurrentHP + "/" + hpManager.MaxHP;
    }
    private void OnEnable()
    {
        InitializeUI();
        hpManager.OnHPChange += HandleHPChange;
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
       Debug.Log("Updating Enemy HP UI");
       hpDisplay.text = damagedEnemy.CurrentHP + "/" + damagedEnemy.MaxHP;        
    }
    public void SetupTarget(Transform target)
    {
        Debug.Log("Setting Player Target");
        targetCamera = target;
    }
}
