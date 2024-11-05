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
        if(lives <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pMan.GameOver();

        }
    }
    public void IncreaseLife()
    {
        lives++;
        if (lives == 2)
        {
            pMan.pUI.life1.SetActive(true);
        }
        if (lives == 3)
        {
            pMan.pUI.life2.SetActive(true);
        }
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
    protected override void Die()
    {
        DepleteLife();
        pMan.StartRespawn();
    }
    protected override void UpdateUI()
    {
        pMan.pUI.UpdateHPDisplay(currentHP);
    }
}
