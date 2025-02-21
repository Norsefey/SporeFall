using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : Damageable
{
    [SerializeField]private PlayerManager pMan;
    public int lives = 3;

    [SerializeField] GameObject deathVFX;
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
        if(pMan != null)
            pMan.pUI.UpdateLifeDisplay(lives);
        if (lives <= 0)
        {
            lives = 0;
            isDead = true;
            GameManager.Instance.GameOver();
        }
    }
    public void IncreaseLife()
    {
        lives++;
        pMan.pUI.UpdateLifeDisplay(lives);
    }
    public void SetReducedLife(int setAmount)
    {
        // When another player joins, i want each to have 2 lives instead of the default 3, but only if player hasn't already died once
        if(lives > setAmount)
            lives = setAmount;
        pMan.pUI.UpdateLifeDisplay(lives);
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;

        if (pMan.pUI != null)
            pMan.pUI.UpdateLifeDisplay(lives);
    }
    protected override void Die()
    {
        pMan.pAnime.ActivateATrigger("Dead");
        StartCoroutine(DeathEffectRoutine());
    }
    private IEnumerator DeathEffectRoutine()
    {
        // Flash effect
        deathVFX.SetActive(true);
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ToggleUnscaledUpdateMode(true);
        // allow death animation to play abit
        yield return new WaitForSecondsRealtime(.5f);
        // Freeze the game
        Time.timeScale = 0.1f;
        // pan camera around player
        StartCoroutine( pMan.pCamera.PanAroundPlayer(transform, 3, 90));
        yield return new WaitForSecondsRealtime(3f);

        // Unfreeze game and respawn
        Time.timeScale = 1f;

        pMan.pAnime.ToggleUnscaledUpdateMode(false);
        deathVFX.SetActive(false);
        pMan.pAnime.ToggleIKAim(true);
        DepleteLife();

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
