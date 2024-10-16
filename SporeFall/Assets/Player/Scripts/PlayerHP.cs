using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    private PlayerManager pMan;

    public int lives = 3;
    public float maxHP = 100;
    private float currentHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log("Player Taking Damage: " + damageAmount);
        currentHP -= damageAmount;
        pMan.pUI.UpdateHPDisplay(currentHP);
        if (currentHP <= 0)
        {
            lives -= 1;
            if (lives <= 0)
            {
                pMan.GameOver();
            }
            else
            {
               StartCoroutine(pMan.Respawn());
            }
        }
    }

    public void RestoreHP(float value)
    {
        currentHP += value;
        if (currentHP > maxHP) 
            currentHP = maxHP;

        pMan.pUI.UpdateHPDisplay(currentHP);
    }
    public void DepleteLife()
    {
        lives--;
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
}
