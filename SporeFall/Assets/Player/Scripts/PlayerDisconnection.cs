using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDisconnection : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    private PlayerInputManager inputManager;
    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
    }
    private void OnEnable()
    {
        // Subscribe to player joined and player left events
        inputManager.onPlayerJoined += OnPlayerJoined;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        inputManager.onPlayerJoined -= OnPlayerJoined;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Called when a new player joins
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log($"Player {playerInput.playerIndex} joined with device {playerInput.devices[0].displayName}.");
        players.Add(playerInput);
    }

    // Handle device disconnection
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device disconnected: {device.displayName}");

            // Find the player associated with the disconnected device
            PlayerInput playerInput = FindPlayerByDevice(device);
            if (playerInput != null)
            {
                Debug.Log($"Player {playerInput.playerIndex} was using the disconnected device. Removing player.");
                RemovePlayer(playerInput);
            }
        }
    }

    // Removes the player from the game
    private void RemovePlayer(PlayerInput playerInput)
    {
        if (players.Contains(playerInput))
        {
            players.Remove(playerInput);
            Destroy(playerInput.gameObject); // Destroy the player's game object
            Debug.Log($"Player {playerInput.playerIndex} removed.");
        }
    }

    // Utility function to find player by their input device
    private PlayerInput FindPlayerByDevice(InputDevice device)
    {
        foreach (var player in players)
        {
            if(player.devices.Count == 0)
            {
                Debug.Log(player.name + " Has No Devices");
                return player;
            }
        }
        Debug.Log("No Match: " );
        return null;
    }
}
