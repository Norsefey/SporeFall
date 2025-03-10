using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : Damageable
{
    [SerializeField] private PlayerManager pMan;
    [SerializeField] private int defaultMaxLives = 3;
    [SerializeField] private int coopMaxLives = 2;
    private int currentLives = 3;
    public int CurrentLives => currentLives;
    [SerializeField] GameObject deathVFX;
    // Start is called before the first frame update
    private void Start()
    {
        currentHP = maxHP;
    }
    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }

    public void DepleteLife()
    {
        currentLives--;
        if (pMan != null)
            pMan.pUI.UpdateLifeDisplay(currentLives);
        if (currentLives <= 0)
        {
            currentLives = 0;
            isDead = true;
            GameManager.Instance.GameOver();
        }
    }
    public void IncreaseLife()
    {
        currentLives++;
        pMan.pUI.UpdateLifeDisplay(currentLives);
    }
    public void SetReducedLife()
    {
        // When another player joins, i want each to have 2 lives instead of the default 3, but only if player hasn't already died once
        if(currentLives > coopMaxLives)
            currentLives = coopMaxLives;
        else if(currentLives == coopMaxLives)// if player has lost a life, they do not regain a life
            currentLives = coopMaxLives - 1;
        // finally if player lives is below the coop max lives, we do not need to do anything
       
        // Update UI
        pMan.pUI.UpdateLifeDisplay(coopMaxLives);
    }
    // when coop player leaves return lives to normal
    public void SetDefaultLife()
    {
        // restore life lost with coop
        if (currentLives < defaultMaxLives)
            IncreaseLife();
    }
    public void SetManager(PlayerManager player)
    {
        pMan = player;
    }
    protected override void Die()
    {
        pMan.pAnime.ActivateATrigger("Dead");
        StartCoroutine(DeathEffectRoutine());
    }
    private IEnumerator DeathEffectRoutine()
    {
        // Death effect
        deathVFX.SetActive(true);
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ToggleUnscaledUpdateMode(true);
        pMan.pInput.DisableAllInputs();

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
    public override void ResetHealth()
    {
        base.ResetHealth();
    }
    public override void IncreaseCorruption(float amount)
    {
        pMan.pCorruption.IncreaseCorruption(amount);
    }
}
