using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticles : MonoBehaviour
{
    [SerializeField] private List<GameObject> particleEffects;

    public void PlayEffects()
    {

        foreach (GameObject effect in particleEffects)
        {
            effect.SetActive(true);
        }
    }
}
