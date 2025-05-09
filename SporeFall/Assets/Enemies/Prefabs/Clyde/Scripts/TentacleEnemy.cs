using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TentacleType
{
    Melee,
    Ranged
}
public class TentacleEnemy : BaseEnemy
{
    private OctoBoss mainBody;

    public override void Initialize()
    {
        base.Initialize();

        Debug.Log("Tentacle Initialize");
        mainBody = (OctoBoss)GameManager.Instance.waveManager.BossEnemy;
        mainBody.AddTentacle(this);
    }

    public override void Die()
    {
        mainBody.RemoveTentacle(this);
        base.Die();
    }
}
