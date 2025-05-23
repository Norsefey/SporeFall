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

    [Header("Revival")]
    [SerializeField] private GameObject reviveZone;
    
    public float deathTime = 10;
    public float deathTimeCounter = 0;
    public bool isDieing = false;

    // Start is called before the first frame update
    private void Start()
    {
        currentLives = defaultMaxLives;
        currentHP = maxHP;
    }
    public override void TakeDamage(float damage)
    {
        float previousHP = currentHP;
        base.TakeDamage(damage);

        if (pMan != null && pMan.audioSource != null)
        {
            float threshhold = 0.25f * maxHP;
            if (previousHP > threshhold && currentHP <= threshhold)
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
        if (isDead)
        {
            pMan.StartRespawn(0, true);
        }
        else
        {
            currentLives++;
            pMan.pUI.UpdateLifeDisplay(currentLives);
        }
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
        if (pMan.audioSource != null && CurrentLives > 1)
        {
            pMan.audioSource.Stop(); // Stop previous audio before playing new one
            pMan.audioSource.PlayOneShot(pMan.deathSound, 1.5f);
        }
        // prevent more damage
        pMan.pHealth.canHoldCorruption = false;
        pMan.pHealth.canTakeDamage = false;
        // Death effect
        deathVFX.SetActive(true);
        pMan.pInput.DisableAllInputs();
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ToggleUnscaledUpdateMode(true);
        pMan.pAnime.ToggleIsDead(true);

        StartCoroutine(DeathEffectRoutine());
    }
    
    
    private IEnumerator DeathEffectRoutine()
    {
     
        // allow death animation to play a bit
        yield return new WaitForSecondsRealtime(.5f);
        // Freeze the game
        Time.timeScale = 0.1f;
        // pan camera around player
        StartCoroutine(pMan.pCamera.PanAroundPlayer(transform, 3, 90));
        yield return new WaitForSecondsRealtime(3);

        // Unfreeze game and Start Death Save if in Coop
        Time.timeScale = 1f;
        pMan.pAnime.ToggleIsDead(false);

       
        if (GameManager.Instance.players.Count > 1)
        {
            if (pMan.GetPlayerIndex() == 0)
            {
                if (GameManager.Instance.players[1].pHealth.isDead || GameManager.Instance.players[1].pHealth.isDieing)
                {
                    isDieing = false;
                    pMan.pAnime.ToggleUnscaledUpdateMode(false);
                    deathVFX.SetActive(false);
                    DepleteLife();
                    pMan.StartRespawn(1, true);
                    yield break;
                }
            }
            else 
            {
                if (GameManager.Instance.players[0].pHealth.isDead || GameManager.Instance.players[0].pHealth.isDieing)
                {
                    isDieing = false;
                    pMan.pAnime.ToggleUnscaledUpdateMode(false);
                    deathVFX.SetActive(false);
                    DepleteLife();
                    pMan.StartRespawn(1, true);
                    yield break;
                }
            }

            // Set the dying state and activate revive zone
            isDieing = true;
            reviveZone.SetActive(true);

            // Wait for either revival or timeout
            deathTimeCounter = 0;
            while (isDieing && deathTimeCounter < deathTime)
            {
                deathTimeCounter += Time.deltaTime;
                yield return null;
            }

            // Disable revive zone
            reviveZone.SetActive(false);

            // If player was not revived (still dying), deplete life and respawn
            if (isDieing)
            {
                isDieing = false;
                pMan.pAnime.ToggleUnscaledUpdateMode(false);
                deathVFX.SetActive(false);
                DepleteLife();
                pMan.StartRespawn(1, true);
            }
            // If player was revived, the revival process would have been handled by PlayerRevive
            // The isDieing flag would have been set to false by PlayerRevive
        }
        else
        {
            Debug.Log("Single PLayer Respawn");
            // Single player - proceed directly to life depletion and respawn
            deathVFX.SetActive(false);
            DepleteLife();
            pMan.pAnime.ToggleUnscaledUpdateMode(false);
            pMan.StartRespawn(3, true);
        }

    }
    public override void ResetHealth()
    {
        base.ResetHealth();
        pMan.pHealth.canHoldCorruption = true;
        pMan.pHealth.canTakeDamage = true;
    }
    public void Revive()
    {
        isDieing = false;
        isDead = false;
        canHoldCorruption = true;
        canTakeDamage = true;
    }
    public override void IncreaseCorruption(float amount)
    {
        pMan.pCorruption.IncreaseCorruption(amount);
    }
}
