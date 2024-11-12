using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StructuresUI : MonoBehaviour
{

    [SerializeField] private BuildGun bGun;
    public Image selectedStructureIcon;
    public Image rightStructureIcon;
    public Image leftStructureIcon;

    [SerializeField] private Sprite turretSprite;
    [SerializeField] private Sprite flamethrowerSprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite shermanSprite;
    [SerializeField] private Sprite repairTowerSprite;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchStructureIcon()
    {
        //selectedStructureIcon.sprite = bGun.selectedStructure.structureSprite;
        if (bGun.currentBuildIndex == 0)
        {
            selectedStructureIcon.sprite = turretSprite;
            leftStructureIcon.sprite = shermanSprite;
            rightStructureIcon.sprite = flamethrowerSprite;
        }

        if (bGun.currentBuildIndex == 1)
        {
            selectedStructureIcon.sprite = flamethrowerSprite;
            leftStructureIcon.sprite = turretSprite;
            rightStructureIcon.sprite = wallSprite;
        }

        if (bGun.currentBuildIndex == 2)
        {
            selectedStructureIcon.sprite = wallSprite;
            leftStructureIcon.sprite = flamethrowerSprite;
            rightStructureIcon.sprite = shermanSprite;
        }

        if (bGun.currentBuildIndex == 3)
        {
            selectedStructureIcon.sprite = repairTowerSprite;
            leftStructureIcon.sprite = wallSprite;
            rightStructureIcon.sprite = shermanSprite;
        }

        if (bGun.currentBuildIndex == 4)
        {
            selectedStructureIcon.sprite = shermanSprite;
            leftStructureIcon.sprite = repairTowerSprite;
            rightStructureIcon.sprite = turretSprite;
        }
    }


}
