using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TentacleEnemy : BaseEnemy
{
    private OctoBoss mainBody;
    private bool isDying = false;

    public override void Initialize()
    {
        Debug.Log("Tentacle Initialize");
        mainBody = (OctoBoss)GameManager.Instance.waveManager.BossEnemy;
        mainBody.AddTentacle(this);

        base.Initialize();
    }
    public override void Die()
    {
        if (isDying)
            return;

        isDying = true;
        if (mainBody != null)
            mainBody.RemoveTentacle(this);
        base.Die();
    }
}
