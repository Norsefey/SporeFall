using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDefenseController : MonoBehaviour
{
    [Header("Block")]
    [Tooltip("Chance (0–1) to attempt a block when hit, provided not on cooldown. " +
                 "Block is checked first; dodge is only checked if block fails/is on cooldown.")]
    [Range(0f, 1f)]
    public float blockChance = 0.3f;
    [Tooltip("Fraction of damage blocked (0.8 = 80% reduction).")]
    [Range(0f, 1f)]
    public float blockDamageReduction = 0.8f;
    [Tooltip("How long the block window stays active (seconds).")]
    public float blockDuration = 0.4f;
    [Tooltip("Seconds before the enemy can block again.")]
    public float blockCooldown = 4f;

    [Header("FX")]
    public GameObject blockFXPrefab;
    public AudioClip blockSound;

    private EnemyController _controller;
    private NavMeshAgent _agent;

    private float _blockCooldownRemaining;
    private bool _isBlocking;

    public bool IsBlocking => _isBlocking;


    private void Awake()
    {
        _controller = GetComponent<EnemyController>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (_blockCooldownRemaining > 0f) _blockCooldownRemaining -= Time.deltaTime;
    }

    public float ProcessIncomingHit(float rawDamage)
    {
        // Block check first — reduces damage in place
        if (TryBlock(rawDamage, out float mitigatedDamage))
            return mitigatedDamage;

        return rawDamage;
    }

    private bool TryBlock(float rawDamage, out float mitigatedDamage)
    {
        mitigatedDamage = rawDamage;
        if (_isBlocking || _blockCooldownRemaining > 0f) return false;
        if (Random.value > blockChance) return false;

        mitigatedDamage = rawDamage * (1f - blockDamageReduction);
        StartCoroutine(BlockRoutine());
        return true;
    }

    private IEnumerator BlockRoutine()
    {
        _isBlocking = true;
        _blockCooldownRemaining = blockCooldown;

        if (blockFXPrefab != null)
            Instantiate(blockFXPrefab, transform.position, Quaternion.identity);
        if (blockSound != null)
            AudioSource.PlayClipAtPoint(blockSound, transform.position);

        Debug.Log("Attack Blocked");
        yield return new WaitForSeconds(blockDuration);
        _isBlocking = false;
    }
}
