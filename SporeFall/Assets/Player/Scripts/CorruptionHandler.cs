using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionHandler : MonoBehaviour
{
    PlayerManager pMan;

    [Header("Corruption Variables")]
    public float corruptionLevel = 0;
    public float purifyRate = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (pMan.holdingCorruption)
        {
            corruptionLevel += pMan.currentWeapon.corruptionRate * Time.deltaTime;
            pMan.pUI.DisplayCorruption(corruptionLevel);
        }
        else if (corruptionLevel > 0)
        {
            corruptionLevel -= Time.deltaTime * purifyRate;
            pMan.pUI.DisplayCorruption(corruptionLevel);
        }
    }

    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
