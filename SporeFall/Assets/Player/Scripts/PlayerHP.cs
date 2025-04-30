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
        float previousHP = currentHP;
        if (currentHP / maxHP <= .25f)
        {
            damageModifier = .25f;
        }
        else
        {
            damageModifier = 0;
        }
        base.TakeDamage(damage);

        if (pMan != null && pMan.audioSource != null)
        {
            // These have no audio clip so disabled them to prevent errors
            /*if (previousHP > 75 && currentHP <= 75)
            {
                pMan.audioSource.Stop();
                pMan.audioSource.PlayOneShot(pMan.health75Sound, 1.5f);
            }
            else if (previousHP > 50 && currentHP <= 50)
            {
                pMan.audioSource.Stop();
                pMan.audioSource.PlayOneShot(pMan.health50Sound, 1.5f);

            }*/
           /* else*/ if (previousHP > 25 && currentHP <= 25)
            {
                pMan.audioSource.Stop(); // Stop previous audio before playing new one
                pMan.audioSource.PlayOneShot(pMan.health25Sound, 1.5f);
            }
        }
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
        if (pMan.audioSource != null)
        {
            pMan.audioSource.Stop(); // Stop previous audio before playing new one
            pMan.audioSource.PlayOneShot(pMan.deathSound, 1.5f);
        }

        pMan.pAnime.ActivateATrigger("Dead");
        StartCoroutine(DeathEffectRoutine());
    }
    private IEnumerator DeathEffectRoutine()
    {
        // prevent more damage
        pMan.pHealth.canHoldCorruption = false;
        pMan.pHealth.canTakeDamage = false;
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
        DepleteLife();
        pMan.StartRespawn(3, true);
    }
    public override void ResetHealth()
    {
        base.ResetHealth();
        pMan.pHealth.canHoldCorruption = true;
        pMan.pHealth.canTakeDamage = true;
    }
    public override void IncreaseCorruption(float amount)
    {
        pMan.pCorruption.IncreaseCorruption(amount);
    }
}
