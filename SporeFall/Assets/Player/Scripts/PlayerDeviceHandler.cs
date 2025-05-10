using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDeviceHandler : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    private PlayerInputManager inputManager;

    [SerializeField] private bool singlePlayer;
    private InputDevice keyboardDevice;
    private InputDevice mouseDevice;

    private bool usingKeyboard = false;
    //private bool usingGamepad = false;
    private bool usingXbox = false;
    //private bool usingPlaystation = false;

    private bool tutorialNeeded = true;


    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
        // Find the keyboard device
        foreach (var device in InputSystem.devices)
        {
            
            if (device is Keyboard)
            {
                keyboardDevice = device;
                //usingKeyboard = true;
                //Debug.Log("Keyboard detected");
            }

            else if (device is Mouse)
            {
                mouseDevice = device;
                //usingKeyboard = true;
                //Debug.Log("mouse detected");
            }
            

            if (keyboardDevice != null && mouseDevice != null)
            {
                break;
            }

        }
    }

    private void Update()
    {
        if (TutorialControls.Instance != null)
        {
            if (TutorialControls.Instance.playerActive == true && tutorialNeeded == true)
            {
                tutorialNeeded = false;

                if (usingXbox == true)
                {
                    Debug.Log("Telling Tutorial script xbox = true");
                    TutorialControls.Instance.usingXbox = true;
                    usingXbox = false;
                }

                else if (usingKeyboard == true)
                {
                    Debug.Log("Telling Tutorial script keyboard = true");
                    TutorialControls.Instance.usingKeyboard = true;
                    usingKeyboard = false;
                }
            }
        }
    }

    private void OnEnable()
    {
        // Subscribe to player joined and player left events
        inputManager.onPlayerJoined += OnPlayerJoined;
        inputManager.onPlayerLeft += OnPlayerLeft;
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        inputManager.onPlayerJoined -= OnPlayerJoined;
        inputManager.onPlayerLeft -= OnPlayerLeft;
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    // Called when a new player joins
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //Debug.Log($"Player {playerInput.playerIndex} joined with device {playerInput.devices[0].displayName}");
        players.Add(playerInput);

        if (players.Count == 1)
        {
            if (playerInput.devices[0].displayName == "Xbox Controller")
            {
                Debug.Log("Setting usingXbox to true");
                usingXbox = true;
            }

            else if (playerInput.devices[0].displayName == "PlayStation Controller")
            {
                Debug.Log("Setting usingPlaystation to true");
                //usingPlaystation = true;
            }

            else if (playerInput.devices[0].displayName == "Keyboard" || playerInput.devices[0].displayName == "Mouse")
            {
                Debug.Log("Setting usingKeyboard to true");
                usingKeyboard = true;
            }

            if (singlePlayer)
            {
                // In single player, disable joining after first player
                inputManager.DisableJoining();
            }
        }
        else if(players.Count > 1)
        {
            // reduce lives on first player
            players[0].GetComponent<PlayerManager>().pHealth.SetReducedLife();

            // perform set up for player two
            if (players[1] != null)
            {
                // change color, lose a HP, disable audio listener, and any other thing need for second player
                players[1].GetComponent<PlayerManager>().SetupPlayerTwo();
            }
            // max 2 players
            inputManager.DisableJoining();
        }
    }
    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (players.Contains(playerInput))
        {
            players.Remove(playerInput);
            //Debug.Log($"Player {playerInput.playerIndex} left the game.");

        }

        if (players.Count == 1)
        {
            //Debug.Log("Resetting to single player view");
            ResetToSinglePlayer();
        }

        inputManager.EnableJoining();
    }

    // Handle device disconnection
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
           // Debug.Log($"Device Connected: {device.displayName}");
            HandleDeviceAdded(device);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
           // Debug.Log($"Device Disconnected: {device.displayName}");
            HandleDeviceDisconnected(device);
        }
    }
    private void HandleDeviceAdded(InputDevice device)
    {
        if (device is Gamepad gamepad)
        {
            /*if (device is XInputController)
            {
                Debug.Log($"Single player switched to Xbox Controller: {gamepad.displayName}");

            }
            else if (device is DualShockGamepad)
            {
                Debug.Log($"Single player switched to PS Controller: {gamepad.displayName}");

            }*/


            if (singlePlayer && players.Count > 0)
            {
                // In single player, switch the existing player to the gamepad
                players[0].SwitchCurrentControlScheme("Gamepad", gamepad);
                UpdateSensitivity();
               // Debug.Log($"Single player switched to gamepad: {gamepad.displayName}");
                
            }
            else if (!singlePlayer && inputManager.joiningEnabled)
            {
                // In multiplayer, the PlayerInputManager will automatically handle creating 
                // a new player with the gamepad through its join system
               // Debug.Log($"Multiplayer: Ready for new player to join with gamepad: {gamepad.displayName}");


            }
        }

    }
    private void UpdateSensitivity()
    {
        players[0].GetComponent<PlayerManager>().SetDeviceSettings();
    }
    private void HandleDeviceDisconnected(InputDevice device)
    {
        if (device is Gamepad)
        {
            if (singlePlayer && players.Count > 0)
            {
                // In single player, switch back to keyboard/mouse
                if (keyboardDevice != null && mouseDevice != null)
                {
                    // Create an array of both keyboard and mouse devices
                    InputDevice[] keyboardMouseDevices = new InputDevice[] { keyboardDevice, mouseDevice };
                    players[0].SwitchCurrentControlScheme("Keyboard&Mouse", keyboardMouseDevices);
                    UpdateSensitivity();
                    //Debug.Log("Single player switched to Keyboard&Mouse");
                }
                else
                {
                    //Debug.LogWarning("Keyboard or mouse device not found!");
                }
            }
            else if (!singlePlayer)
            {
                //Debug.Log("Not Singleplayer need to remove player");
                // In multiplayer, find and remove the player using this device
                PlayerInput playerToRemove = FindPlayerWithoutDevice(device);
                if (playerToRemove != null)
                {
                    int playerIndex = playerToRemove.playerIndex;
                    //Debug.Log($"Found player to remove: Player {playerIndex}");

                    // Remove from our list
                    players.Remove(playerToRemove);
                   // Debug.Log($"Players remaining after removal: {players.Count}");

                    // Destroy the player GameObject
                    GameManager.Instance.RemovePlayer(playerToRemove.GetComponent<PlayerManager>());
                    // Reset to single player view if only one player remains
                    if (players.Count == 1)
                    {
                        //Debug.Log("Resetting to single player view");
                        ResetToSinglePlayer();
                    }

                    inputManager.EnableJoining();
                }
                else
                {
                    //Debug.LogWarning("Could not find player associated with disconnected device!");
                    // Fallback: try to find player by checking all active players
                    LogAllActivePlayers();
                }
            }
        
        }
    }
    private PlayerInput FindPlayerWithoutDevice(InputDevice device)
    {
        //Debug.Log($"Searching for player using device: {device.displayName}");

        foreach (var player in players)
        {
            int counter = 0;
            if (player == null) continue;

            Debug.Log($"Checking player {player.playerIndex} with devices:");
            foreach (var playerDevice in player.devices)
            {
                Debug.Log($"- {playerDevice.displayName}");
                counter++;
            }

            if(counter == 0)
                return player;
        }

        // If we get here, we didn't find a match
        //Debug.Log("No player found for disconnected device");
        return null;
    }
    private void ResetToSinglePlayer()
    {
        // Reset remaining player's position if needed
        if (players.Count > 0)
        {
            PlayerManager remainingPlayer = players[0].GetComponent<PlayerManager>();
            if (remainingPlayer != null)
            {
                // Reset any other player-specific settings
                remainingPlayer.pHealth.SetDefaultLife();
            }
        }
    }
    private void LogAllActivePlayers()
    {
        Debug.Log("=== Active Players ===");
        foreach (var player in players)
        {
            if (player != null)
            {
                Debug.Log($"Player {player.playerIndex} devices:");
                foreach (var device in player.devices)
                {
                    Debug.Log($"- {device.displayName}");
                }
            }
        }
        Debug.Log("===================");
    }
}
