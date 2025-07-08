using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    private PlayerManager pMan;
    [SerializeField] private BuildGun bGun;
    
    [Header("Gameplay UI")]
    public GameObject controlsHolder;
    public TMP_Text textPrompt;
    [SerializeField] private TMP_Text textControls;
    [SerializeField] private GameObject connectionPrompt;
    
    [Header("Weapon UI")]
    [SerializeField] private GameObject weaponUI;
    [SerializeField] private GameObject defaultUI;
    [SerializeField] private GameObject promptHolder;
    [SerializeField] private TMP_Text ammoIndicator;
    public Image weaponIcon;
    public Slider chargeGunSlider;

    [Header("HP UI")]
    [SerializeField] private Slider HPBar;
    [SerializeField] private Slider HPDelayBar;
    [SerializeField] private TMP_Text HPText;
    private float delayedHP;
    [SerializeField] private GameObject[] lifeIcons;
    
    [Header("Corruption UI")]
    [SerializeField] private Slider corruptionBar;
    [SerializeField] private TMP_Text corruptionText;
    [SerializeField] private GameObject corruptedVisionHolder;
    [SerializeField] private Image corruptedVisionImage;
    [SerializeField] private Sprite[] corruptionSprites; // Array of corruption vision sprites
    
    [Header("Build/Structures UI")]
    public GameObject buildUI;
    [SerializeField] private Image selectedStructureIcon;
    [SerializeField] private Image rightStructureIcon;
    [SerializeField] private Image leftStructureIcon;
    [SerializeField] private Sprite defaultPromptSprite;
    [SerializeField] private Sprite buildPromptSprite;
    [SerializeField] private Sprite editPromptSprite;
    [SerializeField] private Image infoPanel;
    [SerializeField] private Image controlsPanel;
    //[SerializeField] private Sprite lilySprite;
    private void Start()
    {
        corruptionBar.maxValue = pMan.pCorruption.MaxCorruption;
        HPBar.maxValue = pMan.pHealth.MaxHP;
        HPDelayBar.maxValue = pMan.pHealth.MaxHP;
        delayedHP = pMan.pHealth.MaxHP;
        corruptedVisionHolder.SetActive(false);

        pMan.pHealth.OnHPChange += UpdateHPDisplay;
        infoPanel.sprite = defaultPromptSprite;
    }

    public void UpdateCorruptionDisplay(float value)
    {
        if (corruptionBar != null)
        {
            if(corruptionBar.maxValue != pMan.pCorruption.MaxCorruption)
            {
                corruptionBar.maxValue = pMan.pCorruption.MaxCorruption;
            }
            corruptionBar.value = value;
        }
        if(corruptionText != null)
        {
            corruptionText.text = $"{value.ToString("F0")} / {pMan.pCorruption.MaxCorruption}";
        }
    }
    public void UpdateCorruptedVision(int corruptionStage)
    {
        if (corruptionStage == 0)
        {
            corruptedVisionHolder.SetActive(false);
            return;
        }

        corruptedVisionHolder.SetActive(true);

        // Ensure the stage index is within bounds
        int spriteIndex = Mathf.Clamp(corruptionStage - 1, 0, corruptionSprites.Length - 1);
        corruptedVisionImage.sprite = corruptionSprites[spriteIndex];

        float alpha = Mathf.Lerp(0.3f, 1f, (float)corruptionStage / 3f);
        Color imageColor = corruptedVisionImage.color;
        imageColor.a = alpha;
        corruptedVisionImage.color = imageColor;
    }
    public IEnumerator ShowInsufficientFundsWarning(string text)
    {
        // Show warning text
        if (textPrompt != null)
        {
            bool isActive; // store wether or not prompt is was active before calling this
            if (textPrompt.gameObject.activeSelf)
            {
                isActive = true;
            }
            else
            {
                textPrompt.gameObject.SetActive(true);
                isActive = false;
            }
            string originalText = textPrompt.text;
            textPrompt.color = Color.red;
            textPrompt.text = text;
            yield return new WaitForSeconds(1.5f);
            textPrompt.color = Color.white;
            textPrompt.text = originalText;

            textPrompt.gameObject.SetActive(isActive);
        }
    }
    public void AmmoDisplay(Weapon currentWeapon)
    {
        if(currentWeapon == null)
        {
            ammoIndicator.text = "";
            return;
        }
        if (currentWeapon.limitedAmmo && currentWeapon.totalAmmo <= 0 && currentWeapon.bulletCount <= 0)
            ammoIndicator.text = "<color=red> No Ammo </color>";
        else if (currentWeapon.IsReloading)
            ammoIndicator.text = "Reloading";
        else if (currentWeapon.limitedAmmo)
            ammoIndicator.text = currentWeapon.bulletCount + "/" + currentWeapon.totalAmmo;
        else if (currentWeapon is BuildGun)
            ammoIndicator.text = "Build Mode";
        else
            ammoIndicator.text = $"{currentWeapon.bulletCount}";
    }
    public void SwitchWeaponIcon()
    {
        if(pMan.currentWeapon == null)
        {
            pMan.pUI.weaponUI.SetActive(false);
            return;
        }
        weaponIcon.enabled = true;

        if (pMan.currentWeapon.weaponSprite != null)
            weaponIcon.sprite = pMan.currentWeapon.weaponSprite;
        else
        {
            weaponIcon.enabled = false;
        }
    }
    public void UpdateHPDisplay(Damageable hpScript, float value)
    {
        if (HPBar != null)
        {
            HPBar.value = hpScript.CurrentHP;

            if (HPBar.maxValue != hpScript.MaxHP)
            {
                HPBar.maxValue = hpScript.MaxHP;
                HPBar.value = hpScript.MaxHP;
                HPDelayBar.maxValue = hpScript.MaxHP;
                HPDelayBar.value = hpScript.MaxHP;
                delayedHP = hpScript.MaxHP;

            }
        }
        if (HPText != null)
        {
            float currentHPInt = Mathf.CeilToInt(hpScript.CurrentHP);

            HPText.text = $"{currentHPInt.ToString("F0")} / {hpScript.MaxHP.ToString("F0")}";
        }

        if (delayedHP < hpScript.CurrentHP)
        {
            //Debug.Log("DelayedHP is less than current HP");
            delayedHP = hpScript.CurrentHP;
            HPDelayBar.value = delayedHP;
            //Debug.Log("Raising delayedHP to equal current HP");
        }
        else if (delayedHP > hpScript.CurrentHP)
        {
            //Debug.Log("DelayedHP is greater than current HP");
            if(isActiveAndEnabled)
                StartCoroutine(HPDelayCooldown());
        }
    }
    public void EnablePrompt(string text)
    {
        promptHolder.SetActive(true);
        textPrompt.text = text;
    }
    public void EnableControls(string text)
    {
        controlsHolder.SetActive(true);
        textControls.text = text;
    }
    public void DisablePrompt()
    {
        if(promptHolder != null)
            promptHolder.SetActive(false);
        if(controlsHolder != null)
            controlsHolder.SetActive(false);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
    public void SwitchStructureIcon()
    {
        // A more modular way to set the sprites for each structure
        int leftIndex = bGun.currentBuildIndex + 1;
        if (leftIndex >= bGun.buildableStructures.Count)
        {
            leftIndex = 0;
        }
         rightStructureIcon.sprite = bGun.buildableStructures[leftIndex].GetComponent<Structure>().structureStats.icon;       

        int rightIndex = bGun.currentBuildIndex - 1;
        if(rightIndex < 0)
        {
            rightIndex = bGun.buildableStructures.Count - 1; ;
        }
        leftStructureIcon.sprite = bGun.buildableStructures[rightIndex].GetComponent<Structure>().structureStats.icon;

        selectedStructureIcon.sprite = bGun.buildableStructures[bGun.currentBuildIndex].GetComponent<Structure>().structureStats.icon;
    }
    public void ToggleChargeGunSlider(bool enable)
    {
        chargeGunSlider.gameObject.SetActive(enable);
    }
    public void UpdateChargeGunSlider(float value)
    {
        chargeGunSlider.value = value;
    }
    public void ToggleWeaponUI(bool toggle)
    {
        if (toggle && weaponUI.activeSelf)
            return;// already on return
        weaponUI.SetActive(toggle);
    }
    public void UpdateLifeDisplay(int currentLives)
    {
        switch (currentLives)
        {
            case 1:
                lifeIcons[0].SetActive(false);
                lifeIcons[1].SetActive(false);
                break;
            case 2:
                lifeIcons[0].SetActive(true);
                lifeIcons[1].SetActive(false);
                break;
            case 3:
                lifeIcons[0].SetActive(true);
                lifeIcons[1].SetActive(true);
                break;
        }
    }
    private void OnDestroy()
    {
        if(pMan != null)
        pMan.pHealth.OnHPChange -= UpdateHPDisplay;

    }

    public void DisplayDefaultPanel()
    {
        infoPanel.sprite = defaultPromptSprite;
    }

    public void DisplayBuildPanel()
    {
        infoPanel.sprite = buildPromptSprite;
        controlsPanel.sprite = buildPromptSprite;
    }

    public void DisplayEditPanel()
    {
        infoPanel.sprite = editPromptSprite;
        controlsPanel.sprite = editPromptSprite;
    }
    public void ToggleConnectionPrompt(bool toggle)
    {
        connectionPrompt.SetActive(toggle);
    }
    IEnumerator HPDelayCooldown()
    {
        yield return new WaitForSeconds(1f);
        while (delayedHP > pMan.pHealth.CurrentHP)
        {
            //Debug.Log("Reducing delayedHP");
            delayedHP -= .5f;
            HPDelayBar.value = delayedHP;
        }
        if (delayedHP < pMan.pHealth.CurrentHP)
        {
            //Debug.Log("DelayedHP has been reduced lower than current HP, raising");
            delayedHP = pMan.pHealth.CurrentHP;
            HPDelayBar.value = delayedHP;
        }
    }
}
