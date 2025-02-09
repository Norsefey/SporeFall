using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : Damageable
{
    private PlayerManager pMan;
    public int lives = 3;

    [SerializeField] GameObject flashImage;
    // Start is called before the first frame update
    private void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
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
        pMan.pAnime.ActivateATrigger("Dead");

        DepleteLife();
        StartCoroutine(DeathEffectRoutine());
    }
    private IEnumerator DeathEffectRoutine()
    {
        // Flash effect
        flashImage.SetActive(true);

        yield return new WaitForSecondsRealtime(.2f);
        // Freeze the game
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(.9f);

        // Restore normal time scale
        flashImage.SetActive(false);
        Time.timeScale = 1f;
        pMan.StartRespawn();

    }
    protected override void UpdateUI()
    {
        pMan.pUI.UpdateHPDisplay(currentHP);
    }
    public override void ResetHealth()
    {
        base.ResetHealth();
        UpdateUI();
    }
    public override void IncreaseCorruption(float amount)
    {
        pMan.pCorruption.IncreaseCorruption(amount);
    }
}
