using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : Damageable
{
    private PlayerManager pMan;
    public int lives = 3;
    // Start is called before the first frame update
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    public void DepleteLife()
    {
        lives--;
    }
    public void IncreaseLife()
    {
        lives++;
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
    protected override void Die()
    {
        pMan.StartRespawn();
    }
    protected override void UpdateUI()
    {
        pMan.pUI.UpdateHPDisplay(currentHP);
    }
}
