using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LilyRepairShop : MonoBehaviour
{
    [SerializeField] private Structure shopStructure;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private GameObject lilyBotPrefab;

    public List<LilyRepairBot> spawnedLilyBots = new();

    private void OnEnable()
    {
        StartCoroutine(ActivateLilyBots());
    }
    private void OnDisable()
    {
        ReturnAllBots();
    }
    public void SpawnLilyBot(int spawnAmount)
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnSingleBot();
        }
    }

    private void SpawnSingleBot()
    {
        GameObject botObj = Instantiate(lilyBotPrefab, spawnPoint.position, Quaternion.identity);
        LilyRepairBot bot = botObj.GetComponent<LilyRepairBot>();
        if (bot != null)
        {
            bot.gameObject.SetActive(false);
            bot.UpdateVisual(shopStructure.GetCurrentLevelInt());
            spawnedLilyBots.Add(bot);
        }
        else
        {
            Debug.LogError("Spawned object does not have a LilyRepairBot component.");
            Destroy(botObj);
        }
    }
    public void ReturnAllBots()
    {
        foreach (var bot in spawnedLilyBots)
        {
            if (bot.isActive)
            {
                bot.DeactivateBot();
            }
        }
    }
    public IEnumerator ActivateLilyBots()
    {
        if(spawnedLilyBots.Count == 0)
        {

            yield break;
        }

        foreach (var bot in spawnedLilyBots)
        {
            bot.gameObject.SetActive(true);
            bot.ActivateBot(spawnPoint, transform);
            bot.UpdateVisual(shopStructure.GetCurrentLevelInt());
            yield return new WaitForSeconds(2);
        }

    }
}
