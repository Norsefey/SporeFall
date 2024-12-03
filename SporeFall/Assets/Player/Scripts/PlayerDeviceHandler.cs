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
    private bool usingGamepad = false;
    private bool usingXbox = false;
    private bool usingPlaystation = false;


    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
        // Find the keyboard device
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard)
            {
                keyboardDevice = device;
                usingKeyboard = true;
                Debug.Log("Keyboard detected");
            }
            else if (device is Mouse)
            {
                mouseDevice = device;
                usingKeyboard = true;
                Debug.Log("mouse detected");
            }
            if (device is XInputController)
            {
                usingXbox = true;
                usingKeyboard = false;
                Debug.Log("xbox controller detected");
            }
            if (device is DualShockGamepad)
            {
                usingPlaystation = true;
                usingKeyboard = false;
                Debug.Log("xbox controller detected");
            }

            if (keyboardDevice != null && mouseDevice != null)
            {
                break;
            }

        }
    }

    private void Start()
    {
        

        if (usingXbox == true)
        {
            Tutorial.Instance.usingXbox = true;
            Tutorial.Instance.usingKeyboard = false;
            usingXbox = false;
        }

        if (usingPlaystation == true)
        {
            Tutorial.Instance.usingPlaystation = true;
            Tutorial.Instance.usingKeyboard = false;
            usingPlaystation = false;
        }

        else if (usingKeyboard == true)
        {
            Debug.Log("Telling Tutorial script keyboard = true");
            if (Tutorial.Instance != null)
                Tutorial.Instance.usingKeyboard = true;
            usingKeyboard = false;
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
            usingGamepad = true;

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
