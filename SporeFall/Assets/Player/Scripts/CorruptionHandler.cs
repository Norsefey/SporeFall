using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptionHandler : MonoBehaviour
{
    private PlayerManager pMan;
    [SerializeField] private GameObject corruptedRobot;
    [Header("Corruption Variables")]
    public float maxCorruption = 10;
    public float corruptionLevel = 0;
    public float purifyRate = 1;
    // Start is called before the first frame update
    void Start()
    {
        pMan.pUI.corruptionBar.maxValue = maxCorruption;
    }

    // Update is called once per frame
    void Update()
    {
        if(corruptionLevel >= maxCorruption)
        {
            CorruptPlayer();
        }

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
    private void CorruptPlayer()
    {
        corruptionLevel = 0;
        pMan.pUI.DisplayCorruption(corruptionLevel);
        // Spawn a corrupted Player
        Instantiate(corruptedRobot, pMan.pController.transform.position, Quaternion.identity);
        // player loses life and respawns
        pMan.pHP.DepleteLife();
        StartCoroutine(pMan.Respawn());
    }
}
