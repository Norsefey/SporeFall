using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovementAttackDriver : MonoBehaviour
{
    public AttackInstance Instance { get; private set; }
    public Damageable Target { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public bool IsActive { get; private set; }

    public event System.Action OnCompleted;

    private MovementAttack _attack;

    private void Awake() => Agent = GetComponent<NavMeshAgent>();

    public void Begin(MovementAttack attack, AttackInstance instance, Damageable target)
    {
        // Cancel any in-progress movement attack before starting a new one
        if (IsActive) ForceComplete();

        _attack = attack;
        Instance = instance;
        Target = target;
        IsActive = true;

        // Hand off locomotion: disable agent so we can move freely
        if (Agent != null) Agent.enabled = false;

        attack.BeginMovement(this);
    }

    private void Update()
    {
        if (!IsActive) return;

        // Target died mid-leap — abort cleanly
        if (Target == null || !Target.IsAlive)
        {
            ForceComplete();
            return;
        }

        if (_attack.TickMovement(this, Time.deltaTime))
        {
            _attack.OnImpact(this);
            Finish();
        }
    }

    private void Finish()
    {
        IsActive = false;
        RestoreAgent();
        OnCompleted?.Invoke();
    }

    public void ForceComplete()
    {
        IsActive = false;
        RestoreAgent();
    }

    private void RestoreAgent()
    {
        if (Agent != null)
        {
            Agent.enabled = true;
            // Warp to current position so the agent doesn't try to correct a large gap
            if (Agent.isOnNavMesh)
                Agent.Warp(transform.position);
        }
    }

    private void OnDisable() => ForceComplete();
}
