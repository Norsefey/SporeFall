using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunUpgrades : MonoBehaviour
{
    public static PlayerGunUpgrades Instance;

    [Header("Upgrade Values")]
    public float damageIncreasePercentage = .25f;
    public float fireRateIncreasePercentage = 0.1f;
    public float reloadSpeedIncreasePercentage = 0.1f;
    public float magazineSizeIncreasePercentage = .25f;

    [Header("Upgrade Costs")]
    public int damageUpgradeCost = 50;
    public int fireRateUpgradeCost = 50;
    public int reloadSpeedUpgradeCost = 50;
    public int magazineSizeUpgradeCost = 50;

    [Header("Upgrade Cost Increase")]
    public float damageUpgradeCostIncrease = 0.50f;
    public float fireRateUpgradeCostIncrease = 0.50f;
    public float reloadSpeedUpgradeCostIncrease = 0.50f;
    public float magazineSizeUpgradeCostIncrease = 0.50f;

/*    private float damageIncreaseAmount;
    private float fireRateIncreaseAmount;
    private float magazineSizeIncreaseAmount;
    private float reloadSpeedIncreaseAmount;*/

    private PlayerManager activePlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetActivePlayer(PlayerManager player)
    {
        activePlayer = player;
    }

    public void ApplyUpgradesToPlayerWeapons()
    {
        ApplyModifiersToWeapon(activePlayer.defaultWeapon);
        if (activePlayer.equippedWeapon != null)
        {
            ApplyModifiersToWeapon(activePlayer.equippedWeapon);
        }

        activePlayer.pUI.AmmoDisplay(activePlayer.currentWeapon);
    }
    public void ApplyModifiersToWeapon(Weapon weapon)
    {
        weapon.damage = Mathf.RoundToInt(weapon.damage * (1 + damageIncreasePercentage));
        weapon.fireRate = Mathf.RoundToInt(weapon.fireRate * (1 + fireRateIncreasePercentage));
        weapon.bulletCapacity = Mathf.RoundToInt(weapon.bulletCapacity * (1 + magazineSizeIncreasePercentage));
        weapon.reloadTime = Mathf.RoundToInt(weapon.reloadTime * (1 + reloadSpeedIncreasePercentage));
    }
    public void UpgradeDamage()
    {
        //damageIncreaseAmount += damageIncreasePercentage;
        damageUpgradeCost = Mathf.RoundToInt(damageUpgradeCost * (1 + damageUpgradeCostIncrease));
        ApplyUpgradesToPlayerWeapons();
    }
    public void UpgradeFireRate()
    {
        //fireRateIncreaseAmount += fireRateIncreasePercentage;
        fireRateUpgradeCost = Mathf.RoundToInt(fireRateUpgradeCost * (1 + fireRateUpgradeCostIncrease));
        ApplyUpgradesToPlayerWeapons();

    }
    public void UpgradeReloadSpeed()
    {
        //reloadSpeedIncreaseAmount += reloadSpeedIncreasePercentage;
        reloadSpeedUpgradeCost = Mathf.RoundToInt(reloadSpeedUpgradeCost * (1 + reloadSpeedUpgradeCostIncrease));

        ApplyUpgradesToPlayerWeapons();
    }
    public void UpgradeMagazineSize()   
    {
        //magazineSizeIncreaseAmount += magazineSizeIncreasePercentage;
        magazineSizeUpgradeCost = Mathf.RoundToInt(magazineSizeUpgradeCost * (1 + magazineSizeUpgradeCostIncrease));

        ApplyUpgradesToPlayerWeapons();
    }
}
