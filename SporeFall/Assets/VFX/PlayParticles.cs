using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticles : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleEffects;

    public void PlayEffects()
    {
        foreach (ParticleSystem effect in particleEffects)
        {
            var main = effect.main;
            main.loop = true; // Ensure looping is enabled while playing
            effect.Play();
        }
    }

    public void StopEffects()
    {
        foreach (ParticleSystem effect in particleEffects)
        {
            var main = effect.main;
            main.loop = false; // Stop looping to allow current particles to finish
        }
    }
}
