using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : Damageable
{
    [SerializeField] private PlayerManager pMan;
    [SerializeField] private int defaultMaxLives = 3;
    private int currentLives = 3;
    public int CurrentLives => currentLives;
    [SerializeField] GameObject deathVFX;

    [Header("Revival")]
    [SerializeField] private GameObject reviveZone;
    
    public float deathTime = 10;
    public float deathTimeCounter = 0;
    public bool isDead = false;
    public float startingHP = 100f;

    private void Awake()
    {
        currentLives = defaultMaxLives;
        targetType = TargetType.Player;
        maxHealth = startingHP;
        MakeAlive();


        // Register with the target registry so enemies can find this player
        EnemyTargetRegistry.Instance?.Register(this);
    }
    private void OnDisable()
    {
        EnemyTargetRegistry.Instance?.Unregister(this);
    }
    protected override float OnReceiveDamage(float amount)
    {
        float previousHP = _health;
        float finalDamage = amount - (amount * dmgReduction);
        _health -= finalDamage;
        Debug.Log($"[Player] Took {finalDamage} damage. HP: {_health}/{maxHealth}");

        if (pMan != null && pMan.audioSource != null)
        {
            float threshhold = 0.25f * maxHealth;
            if (previousHP > threshhold && _health <= threshhold)
            {
                pMan.audioSource.Stop(); // Stop previous audio before playing new one
                pMan.audioSource.PlayOneShot(pMan.health25Sound, 1.5f);
            }
        }

        if (_health <= 0f)
            Die();

        return finalDamage;
    }
    protected override void Die()
    {
        if (pMan.audioSource != null && CurrentLives > 1)
        {
            pMan.audioSource.Stop(); // Stop previous audio before playing new one
            pMan.audioSource.PlayOneShot(pMan.deathSound, 1.5f);
        }
        // prevent more damage
        canHoldCorruption = false;
        canTakeDamage = false;
        // Death effect
        deathVFX.SetActive(true);
        pMan.pInput.DisableAllInputs();
        pMan.pAnime.ToggleIKAim(false);
        pMan.pAnime.ToggleUnscaledUpdateMode(true);
        pMan.pAnime.ToggleIsDead(true);

        StartCoroutine(DeathEffectRoutine());
    }
    public void DepleteLife()
    {
        currentLives--;
        if (pMan != null)
            pMan.pUI.UpdateLifeDisplay(currentLives);
        if (currentLives <= 0)
        {
            currentLives = 0;
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
    public void SetManager(PlayerManager player)
    {
        pMan = player;
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
                if (GameManager.Instance.players[1].pHealth.isDead)
                {
                    isDead = false;
                    pMan.pAnime.ToggleUnscaledUpdateMode(false);
                    deathVFX.SetActive(false);
                    DepleteLife();
                    pMan.StartRespawn(1, true);
                    yield break;
                }
            }
            else 
            {
                if (GameManager.Instance.players[0].pHealth.isDead)
                {
                    isDead = false;
                    pMan.pAnime.ToggleUnscaledUpdateMode(false);
                    deathVFX.SetActive(false);
                    DepleteLife();
                    pMan.StartRespawn(1, true);
                    yield break;
                }
            }

            // Set the dying state and activate revive zone
            isDead = true;
            reviveZone.SetActive(true);

            // Wait for either revival or timeout
            deathTimeCounter = 0;
            while (isDead && deathTimeCounter < deathTime)
            {
                deathTimeCounter += Time.deltaTime;
                yield return null;
            }

            // Disable revive zone
            reviveZone.SetActive(false);

            // If player was not revived (still dying), deplete life and respawn
            if (isDead)
            {
                isDead = false;
                pMan.pAnime.ToggleUnscaledUpdateMode(false);
                deathVFX.SetActive(false);
                DepleteLife();
                pMan.StartRespawn(1, true);
            }
            // If player was revived, the revival process would have been handled by PlayerRevive
            // The isDead flag would have been set to false by PlayerRevive
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
    public void Revive()
    {
        isDead = false;
        canHoldCorruption = true;
        canTakeDamage = true;
    }
    public void IncreaseCorruption(float amount)
    {
        if (!canHoldCorruption)
            return;

        pMan.pCorruption.IncreaseCorruption(amount);
    }
}
