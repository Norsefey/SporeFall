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
        ActivateBots();
    }
    private void OnDisable()
    {
        ReturnAllBots();
    }
    public void SpawnLilyBot(int spawnAmount)
    {
        while(spawnedLilyBots.Count < spawnAmount)
        {
            SpawnSingleBot();
        }
    }

    private void SpawnSingleBot()
    {
        GameObject botObj = Instantiate(lilyBotPrefab, spawnPoint.position, Quaternion.identity);
        botObj.transform.SetParent(transform);
        LilyRepairBot bot = botObj.GetComponent<LilyRepairBot>();
        if (bot != null)
        {
            bot.UpdateVisual(shopStructure.GetCurrentLevelInt());
            spawnedLilyBots.Add(bot);
            botObj.SetActive(false);
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
    public void ActivateBots()
    {
        foreach (var bot in spawnedLilyBots)
        {
            if (!bot.isActive)
            {
                bot.gameObject.SetActive(true);
                bot.ActivateBot(spawnPoint, transform);
                bot.UpdateVisual(shopStructure.GetCurrentLevelInt() - 1);
            }
        }
    }
}
