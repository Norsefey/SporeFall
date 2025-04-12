using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LilyRepairShop : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    public List<LilyRepairBot> lilyBots = new List<LilyRepairBot>();

    public int maxActiveLilies = 1;
    private void Awake()
    {
        // Initialize all bots with shop structure reference
        foreach (var bot in lilyBots)
        {
            bot.SetShopStructure(transform);
        }
    }
    private void OnEnable()
    {
        //StartCoroutine(ActivateLilyBots());
    }
    private void OnDisable()
    {
        ReturnAllBots();
    }
    public void ReturnAllBots()
    {
        foreach (var bot in lilyBots)
        {
            if (bot.isActive)
            {
                bot.DeactivateBot();
            }
        }
    }
    public IEnumerator ActivateLilyBots()
    {
        // Ensure we don't try to activate more bots than available
        int botsToActivate = Mathf.Min(maxActiveLilies, lilyBots.Count);

        for (int i = 0; i < botsToActivate; i++)
        {
            Debug.Log("Activating Lily Bot: " + i);
            lilyBots[i].gameObject.SetActive(true);
            lilyBots[i].UpdateVisual(maxActiveLilies - 1);
            lilyBots[i].ActivateBot(spawnPoint);
            yield return new WaitForSeconds(2);
        }
    }
}
