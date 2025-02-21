using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

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
    private bool usingPlaystation = false;

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
                Debug.Log("Keyboard detected");
            }

            else if (device is Mouse)
            {
                mouseDevice = device;
                //usingKeyboard = true;
                Debug.Log("mouse detected");
            }
            

            if (keyboardDevice != null && mouseDevice != null)
            {
                break;
            }

        }
    }

    private void Update()
    {
        if (Tutorial.Instance.playerActive == true && tutorialNeeded == true)
        {
            tutorialNeeded = false;

            if (usingXbox == true)
            {
                Debug.Log("Telling Tutorial script xbox = true");
                if (Tutorial.Instance != null)
                {
                    Tutorial.Instance.usingXbox = true;
                }
                usingXbox = false;
            }

            else if (usingPlaystation == true)
            {
                Debug.Log("Telling Tutorial script playstation = true");
                if (Tutorial.Instance != null)
                {
                    Tutorial.Instance.usingPlaystation = true;
                }
                usingPlaystation = false;
            }

            else if (usingKeyboard == true)
            {
                Debug.Log("Telling Tutorial script keyboard = true");
                if (Tutorial.Instance != null)
                {
                    Tutorial.Instance.usingKeyboard = true;
                } 
                usingKeyboard = false;
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
        Debug.Log($"Player {playerInput.playerIndex} joined with device {playerInput.devices[0].displayName}");
        players.Add(playerInput);



        if (singlePlayer && players.Count == 1)
        {
            // In single player, disable joining after first player
            inputManager.DisableJoining();

            if (playerInput.devices[0].displayName == "Xbox Controller")
            {
                Debug.Log("Setting usingXbox to true");
                usingXbox = true;
            }

            else if (playerInput.devices[0].displayName == "PlayStation Controller")
            {
                Debug.Log("Setting usingPlaystation to true");
                usingPlaystation = true;
            }

            else if (playerInput.devices[0].displayName == "Keyboard" || playerInput.devices[0].displayName == "Mouse")
            {
                Debug.Log("Setting usingKeyboard to true");
                usingKeyboard = true;
            }

        }
        else if(players.Count > 1)
        {
            // reduce lives on first player
            //players[0].GetComponent<PlayerManager>().pHealth.SetReducedLife(2);

            // perform set up for player two
            if (players[1] != null)
            {
                // change color, lose a HP, disable audio listener, and any other thing need for second player
                players[1].GetComponent<PlayerManager>().SetupPlayerTwo();
            }
        }
    }
    private void OnPlayerLeft(PlayerInput playerInput)
    {
        if (players.Contains(playerInput))
        {
            players.Remove(playerInput);
            Debug.Log($"Player {playerInput.playerIndex} left the game.");
        }
    }

    // Handle device disconnection
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            Debug.Log($"Device Connected: {device.displayName}");
            HandleDeviceAdded(device);
        }
        else if (change == InputDeviceChange.Disconnected)
        {
            Debug.Log($"Device Disconnected: {device.displayName}");
            HandleDeviceDisconnected(device);
        }
    }
    private void HandleDeviceAdded(InputDevice device)
    {
        if (device is Gamepad gamepad)
        {
            //usingGamepad = true;

            if (device is XInputController)
            {
                Debug.Log($"Single player switched to Xbox Controller: {gamepad.displayName}");

            }
            else if (device is DualShockGamepad)
            {
                Debug.Log($"Single player switched to PS Controller: {gamepad.displayName}");

            }


            if (singlePlayer && players.Count > 0)
            {
                // In single player, switch the existing player to the gamepad
                players[0].SwitchCurrentControlScheme("Gamepad", gamepad);
                UpdateSensitivity();
                Debug.Log($"Single player switched to gamepad: {gamepad.displayName}");
                
            }
            else if (!singlePlayer && inputManager.joiningEnabled)
            {
                // In multiplayer, the PlayerInputManager will automatically handle creating 
                // a new player with the gamepad through its join system
                Debug.Log($"Multiplayer: Ready for new player to join with gamepad: {gamepad.displayName}");


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
                    Debug.Log("Single player switched to Keyboard&Mouse");
                }
                else
                {
                    Debug.LogWarning("Keyboard or mouse device not found!");
                }
            }
            else if (!singlePlayer)
            {
                // In multiplayer, find and remove the player using this device
                PlayerInput playerToRemove = FindPlayerByDevice(device);
                if (playerToRemove != null)
                {
                    Debug.Log($"Removing player {playerToRemove.playerIndex} due to disconnected device");
                    Destroy(playerToRemove.gameObject);
                }
            }
        }
    }

    private PlayerInput FindPlayerByDevice(InputDevice device)
    {
        foreach (var player in players)
        {
            if (player.devices.Contains(device))
            {
                return player;
            }
        }
        return null;
    }
}
