using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpawnMenu : MonoBehaviour
{
    // global variable
    private int totalQuantity = 0;

    [Header("Spawnable Enemies")]
    [SerializeField] private EnemySelection[] spawnableEnemies;

    [SerializeField] private Transform[] spawnPoints;
    private List<GameObject> spawnedEnemies = new();

    [Header("SelectedEnemies")]
    private List<EnemySelection> selectedEnemies = new();
    [SerializeField] private SelectedPortrait[] selectedEnemyPortraits;
    [SerializeField] private Image enemyPreviewImage;
    [SerializeField] private TMP_Text enemyNameDisplay;
    private int selectedEnemyIndex = 0;

    [SerializeField]private TMP_Text enemyStatsText;

    [Header("Enemy Attribute")]
    // Attribute of the selected Enemy
    //[SerializeField] private Sprite[] attributeIcons;
    [SerializeField] private Image attributeImage;

    [Header("Enemy Strength")]
    // strength of the Selected Enemy
    [SerializeField] private TMP_Text strengthDisplay;
    [SerializeField] private TMP_InputField strengthInputField;
    private int selectedStrength = 1;

    [Header("Enemy Quantity")]
    // Quantity of the Selected Enemy
    [SerializeField] private TMP_InputField quantityInputField;
    private int selectedQuantity = 1;


    private void Start()
    {
        // Initialize the display with the current attribute and strength
/*        attributeDisplay.text = currentAttribute.ToString();
        attributeImage.sprite = attributeIcons[(int)currentAttribute];*/
        strengthDisplay.text = "LV: " + selectedStrength.ToString();
        strengthInputField.text = selectedStrength.ToString();
        quantityInputField.text = selectedQuantity.ToString();

        selectedEnemyIndex = 0;
        ChangeEnemyTypeSelection(0); // Initialize the selected enemy type
    }

    public void SpawnSelectedEnemies()
    {
        if (selectedEnemies.Count > 0 && selectedEnemies.Count < 10 - spawnedEnemies.Count)
        {
            foreach (var enemySelection in selectedEnemies)
            {
                for (int i = 0; i < enemySelection.quantity; i++)
                {
                    Vector3 spawnPoint = spawnPoints[i].position;
                    SpawnEnemy(enemySelection, spawnPoint);
                }
            }
        }
        else
        {
            Debug.LogWarning("No enemy selected to spawn.");
        }

        CloseMenu();
    }
    public void AddEnemyToSelection()
    {
        if(totalQuantity + selectedQuantity > 10)
        {
            Debug.LogWarning("Total quantity of enemies exceeds the limit of 10.");
            // make the quantity input field red to indicate the error
            quantityInputField.image.color = Color.red;
            return;
        }

        if (spawnableEnemies.Length > 0)
        {
            EnemySelection newSelection = new EnemySelection(spawnableEnemies[selectedEnemyIndex].enemyPrefab, spawnableEnemies[selectedEnemyIndex].portrait, selectedStrength, selectedQuantity);

            selectedEnemies.Add(newSelection);

            for(int i = totalQuantity; i < totalQuantity + selectedQuantity; i++)
            {
                selectedEnemyPortraits[i].portrait.sprite = spawnableEnemies[selectedEnemyIndex].portrait;
                selectedEnemyPortraits[i].strength.text = "LV:" + selectedStrength.ToString();
            }

            totalQuantity += selectedQuantity;
            Debug.Log($"Added {selectedQuantity} of {spawnableEnemies[selectedEnemyIndex].enemyPrefab.name} with strength {selectedStrength}. Total quantity: {totalQuantity}");
        }
        else
        {
            Debug.LogWarning("No spawnable enemies available.");
        }
    }
    public void ClearSelectedEnemies()
    {
        selectedEnemies.Clear();
        totalQuantity = 0;
        for (int i = 0; i < selectedEnemyPortraits.Length; i++)
        {
            selectedEnemyPortraits[i].portrait.sprite = null;
            selectedEnemyPortraits[i].strength.text = "";
        }
        Debug.Log("Cleared all selected enemies.");
    }
    private void SpawnEnemy(EnemySelection enemySelection, Vector3 spawnPoint)
    {
        Debug.Log($"Spawning {enemySelection.quantity} of {enemySelection.enemyPrefab.name} with strength {enemySelection.strength} at {spawnPoint}");
        
        if (!PoolManager.Instance.enemyPool.TryGetValue(enemySelection.enemyPrefab, out EnemyObjectPool pool))
        {
            Debug.LogError($"No pool found for enemy prefab: {enemySelection.enemyPrefab.name}");
            return;
        }

        EnemyController enemy = pool.Get(spawnPoint, Quaternion.identity);

        spawnedEnemies.Add(enemy.gameObject);
        if (enemy != null)
        {
            enemy.Initialize(enemySelection.strength);
            enemy.OnDied += (e) => { spawnedEnemies.Remove(e.gameObject); }; // Remove from list when it dies
        }
        else
        {
            Debug.LogWarning("Spawned enemy does not have an EnemyController component.");
        }
    }
    public void ChangeEnemyTypeSelection(int direction)
    {
        selectedEnemyIndex += direction;
        // loop index back if greater or less than the array length
        if(selectedEnemyIndex < 0)
        {
            selectedEnemyIndex = spawnableEnemies.Length - 1;
        }
        else if(selectedEnemyIndex >= spawnableEnemies.Length)
        {
            selectedEnemyIndex = 0;
        }

        if (selectedEnemyIndex >= 0 && selectedEnemyIndex < spawnableEnemies.Length)
        {
            enemyPreviewImage.sprite = spawnableEnemies[selectedEnemyIndex].portrait;
            
            EnemyController enemy = spawnableEnemies[selectedEnemyIndex].enemyPrefab.GetComponent<EnemyController>();
            attributeImage.sprite = enemy.statData.attributeIcon;
            enemyNameDisplay.text = enemy.statData.enemyName;

            Debug.Log($"Selected enemy type: {spawnableEnemies[selectedEnemyIndex].enemyPrefab.name}");
            UpdateStatDisplay();
        }
        else
        {
            Debug.LogWarning("Invalid enemy type index.");
        }
    }

    #region Enemy Stat Methods

   /* public void ChangeAttribute(int direction)
    {
        // 1. Convert current enum to integer index
        int currentIndex = (int)currentAttribute;

        // 2. Add direction and apply modulo to wrap around
        // (Adding totalElements prevents negative modulo bugs in C#)
        int nextIndex = (currentIndex + direction + totalElements) % totalElements;

        // 3. Cast the index back into the enum type
        currentAttribute = (EnemyAttribute)nextIndex;

        Debug.Log($"Selected: {currentAttribute}");
        attributeDisplay.text = currentAttribute.ToString();
        attributeImage.sprite = attributeIcons[nextIndex];
    }
    */
    public void ChangeStrength(int strength)
    {
        selectedStrength += strength;
        selectedStrength = Mathf.Clamp(selectedStrength, 1, 99);

        Debug.Log($"Strength change: {strength} Final:" + selectedStrength);
        strengthInputField.text = selectedStrength.ToString();
        UpdateStatDisplay();
    }
    public void UpdateStrengthInputField()
    {
        if (int.TryParse(strengthInputField.text, out int newStrength))
        {
            selectedStrength = Mathf.Clamp(newStrength, 1, 99);
        }
        else
        {
            selectedStrength = 1; // Default to 1 if input is invalid
        }
        strengthDisplay.text = "LV: " + selectedStrength.ToString();
        UpdateStatDisplay();
    }

    public void ChangeQuantity(int direction)
    {
        selectedQuantity += direction;
        selectedQuantity = Mathf.Clamp(selectedQuantity, 1, 10);

        quantityInputField.text = selectedQuantity.ToString();

        if(selectedQuantity + totalQuantity > 10)
        {
            Debug.LogWarning("Total quantity of enemies exceeds the limit of 10.");
            // make the quantity input field red to indicate the error
            quantityInputField.image.color = Color.red;
        }
        else
        {
            // reset the color to white if within limit
            quantityInputField.image.color = Color.white;
        }
    }
    public void UpdateQuantityInputField()
    {
        if (int.TryParse(quantityInputField.text, out int newQuantity))
        {
            selectedQuantity = Mathf.Clamp(newQuantity, 1, 10);
        }
        else
        {
            selectedQuantity = 1; // Default to 1 if input is invalid
        }
        quantityInputField.text = selectedQuantity.ToString();
    }

    #endregion

    private void UpdateStatDisplay()
    {
        EnemyController enemy = spawnableEnemies[selectedEnemyIndex].enemyPrefab.GetComponent<EnemyController>();
        enemy.Initialize(selectedStrength); // Initialize the enemy with the selected strength to get updated stats
        enemyStatsText.text =
                            $"Health: {enemy.Stats.MaxHealth.ToString("F0")}\n" +
                            $"Move Speed: {enemy.Stats.MoveSpeed.ToString("F0")}\n" +
                            $"Armor: {enemy.Stats.Armor}\n";

        foreach(AttackInstance attack in enemy._attacks)
        {
            enemyStatsText.text += $"Attack: {attack.Data.attackName}, Damage: {attack.ScaledDamage.ToString("F0")}\n";
        }
    }
    public void CloseMenu()
    {
        ClearSelectedEnemies();

        gameObject.SetActive(false);
        GameManager.Instance.players[0].pInput.ToggleMenu(false);
    }
    public void OpenMenu()
    {
        gameObject.SetActive(true);
    }
}

[System.Serializable]
public struct EnemySelection
{
    public GameObject enemyPrefab;
    public int strength;
    public int quantity;
    public Sprite portrait;
    public EnemySelection(GameObject prefab, Sprite pic, int str, int qty)
    {
        enemyPrefab = prefab;
        portrait = pic;
        strength = str;
        quantity = qty;

    }
}
[System.Serializable]
public class SelectedPortrait
{
    [SerializeField] public Image portrait;
    [SerializeField] public TMP_Text strength;
}
