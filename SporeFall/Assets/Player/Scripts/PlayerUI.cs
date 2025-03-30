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
    [SerializeField] private GameObject weaponUI;
    public Image weaponIcon;
    public Slider chargeGunSlider;
    [Space(5)]
    [SerializeField] private TMP_Text ammoIndicator;
    [SerializeField] private GameObject promptHolder;
    [SerializeField] public GameObject controlsHolder;
    [SerializeField] public TMP_Text textPrompt;
    [SerializeField] private TMP_Text textControls;
    [SerializeField] private Slider HPBar;
    [SerializeField] private Slider HPDelayBar;
    private float delayedHP;
    [SerializeField] private GameObject[] lifeIcons;
    [Header("Corruption UI")]
    [SerializeField] private Slider corruptionBar;
    [SerializeField] private GameObject corruptedVisionHolder;
    [SerializeField] private Image corruptedVisionImage;
    [SerializeField] private Sprite[] corruptionSprites; // Array of corruption vision sprites
    [Header("Build/Structures UI")]
    public GameObject buildUI;
    [SerializeField] private Image selectedStructureIcon;
    [SerializeField] private Image rightStructureIcon;
    [SerializeField] private Image leftStructureIcon;
    [SerializeField] private Sprite turretSprite;
    [SerializeField] private Sprite flamethrowerSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite shermanSprite;
    [SerializeField] private Sprite repairTowerSprite;
    //[SerializeField] private Sprite lilySprite;
    

    private void Start()
    {
        corruptionBar.maxValue = pMan.pCorruption.maxCorruption;
        HPBar.maxValue = pMan.pHealth.MaxHP;
        HPDelayBar.maxValue = pMan.pHealth.MaxHP;
        delayedHP = pMan.pHealth.MaxHP;
        corruptedVisionHolder.SetActive(false);

        pMan.pHealth.OnHPChange += UpdateHPDisplay;
    }
    public void UpdateCorruptionDisplay(float value)
    {
        if (corruptionBar != null)
        {
            corruptionBar.value = value;
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

        // Optional: Adjust opacity based on corruption level
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
    /* public void DisplayCorruptedVision(float value)
     {
         if (value < 60)
         {
             corruptedVisionHolder.SetActive(false);
         }
         else if (value >= 60 && value < 75)
         {
             corruptedVisionHolder.SetActive(true);
             corruptedVisionImage.sprite = corruptionSpread1;
         }
         else if (value >= 75 && value < 90)
         {
             corruptedVisionImage.sprite = corruptionSpread2;
         }
         else if (value >= 90)
         {
             corruptedVisionImage.sprite = corruptionSpread3;
         }

     }*/
    public void AmmoDisplay(Weapon currentWeapon)
    {
        if (currentWeapon.IsReloading)
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
        weaponIcon.sprite = pMan.currentWeapon.weaponSprite;
    }
    public void UpdateHPDisplay(Damageable hpScript, float value)
    {
        if (HPBar != null)
            HPBar.value = pMan.pHealth.CurrentHP;
        if (delayedHP < pMan.pHealth.CurrentHP)
        {
            Debug.Log("DelayedHP is less than current HP");
            delayedHP = pMan.pHealth.CurrentHP;
            HPDelayBar.value = delayedHP;
            Debug.Log("Raising delayedHP to equal current HP");
        }
        else if (delayedHP > pMan.pHealth.CurrentHP)
        {
            Debug.Log("DelayedHP is greater than current HP");
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
        promptHolder.SetActive(false);
        controlsHolder.SetActive(false);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
    public void SwitchStructureIcon()
    {
        // A more modular way to set the sprites for each structure
        int leftIndex = bGun.currentBuildIndex - 1;
        if (leftIndex < 0)
        {
            leftIndex = bGun.buildableStructures.Length - 1;
        }
         rightStructureIcon.sprite = bGun.buildableStructures[leftIndex].GetComponent<Structure>().structureStats.icon;       

        int rightIndex = bGun.currentBuildIndex + 1;
        if(rightIndex >= bGun.buildableStructures.Length)
        {
            rightIndex = 0;
        }
        leftStructureIcon.sprite = bGun.buildableStructures[rightIndex].GetComponent<Structure>().structureStats.icon;

        selectedStructureIcon.sprite = bGun.buildableStructures[bGun.currentBuildIndex].GetComponent<Structure>().structureStats.icon;
        

       /* if (bGun.currentBuildIndex == 0)
        {
            selectedStructureIcon.sprite = turretSprite;
            leftStructureIcon.sprite = shermanSprite;
            rightStructureIcon.sprite = flamethrowerSprite;
        }

       else if (bGun.currentBuildIndex == 1)
        {
            selectedStructureIcon.sprite = flamethrowerSprite;
            leftStructureIcon.sprite = turretSprite;
            rightStructureIcon.sprite = wallSprite;
        }

        else if (bGun.currentBuildIndex == 2)
        {
            selectedStructureIcon.sprite = wallSprite;
            leftStructureIcon.sprite = flamethrowerSprite;
            rightStructureIcon.sprite = repairTowerSprite;
        }

        else if(bGun.currentBuildIndex == 3)
        {
            selectedStructureIcon.sprite = repairTowerSprite;
            leftStructureIcon.sprite = wallSprite;
            rightStructureIcon.sprite = shermanSprite;
        }

        else if (bGun.currentBuildIndex == 4)
        {
            selectedStructureIcon.sprite = shermanSprite;
            leftStructureIcon.sprite = repairTowerSprite;
            rightStructureIcon.sprite = turretSprite;
        }*/
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

    IEnumerator HPDelayCooldown()
    {
        yield return new WaitForSeconds(1f);
        while (delayedHP > pMan.pHealth.CurrentHP)
        {
            Debug.Log("Reducing delayedHP");
            StartCoroutine(HPDelayDecrease());
        }
        if (delayedHP < pMan.pHealth.CurrentHP)
        {
            Debug.Log("DelayedHP has been reduced lower than current HP, raising");
            delayedHP = pMan.pHealth.CurrentHP;
            HPDelayBar.value = delayedHP;
        }
    }

    IEnumerator HPDelayDecrease()
    {
        yield return new WaitForSeconds(.1f);
        delayedHP = delayedHP - .5f;
        HPDelayBar.value = delayedHP;
    }
}
