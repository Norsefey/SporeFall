using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

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
    private bool controllerTookOver = false;
    private bool firstPlayerJoined = false;


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
                    //Debug.Log("Telling Tutorial script xbox = true");
                    TutorialControls.Instance.usingXbox = true;
                    usingXbox = false;
                }

                else if (usingKeyboard == true)
                {
                    //Debug.Log("Telling Tutorial script keyboard = true");
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
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        inputManager.onPlayerJoined -= OnPlayerJoined;
        inputManager.onPlayerLeft -= OnPlayerLeft;
        InputSystem.onDeviceChange -= OnDeviceChange;
        InputSystem.onEvent -= OnInputEvent;

    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Only process actual user input events (not device discovery etc)
        if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>())
            return;

        // === CASE 1: First player not joined yet ===
        if (!firstPlayerJoined)
        {
            if (device is Gamepad gamepad)
            {
                inputManager.JoinPlayer(0, -1, "Gamepad", gamepad);
                firstPlayerJoined = true;
                Debug.Log("First player joined with Gamepad.");

            }
            else if (device is Keyboard || device is Mouse)
            {
                // Wait for both keyboard and mouse to be present
                if (keyboardDevice == null || mouseDevice == null)
                    return;

                if (device is Mouse mouse)
                {
                    // Only join if left-click or right-click is pressed
                    if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
                    {
                        InputDevice[] combo = new InputDevice[] { keyboardDevice, mouseDevice };
                        inputManager.JoinPlayer(0, -1, "Keyboard&Mouse", combo);
                        firstPlayerJoined = true;
                        Debug.Log("First player joined with Mouse.");
                        return;
                    }
                }
            }
        }
        // === Allow Player 1 to switch back to keyboard ===
        else if (device is (Keyboard or Mouse) && players.Count >= 1)
        {
            var player1 = players.FirstOrDefault(p => p.playerIndex == 0);

            if (player1 != null && player1.currentControlScheme == "Gamepad")
            {
                InputDevice[] combo = new InputDevice[] { keyboardDevice, mouseDevice };
                player1.SwitchCurrentControlScheme("Keyboard&Mouse", combo);
                controllerTookOver = false;
                UpdateSensitivity();
                Debug.Log("Player 1 switched control scheme back to Keyboard & Mouse.");
            }

            // Block keyboard input for non-Player 1 (e.g., mouse jiggle or input routed to Player 2)
            foreach (var player in players)
            {
                if (player.playerIndex != 0 && player.devices.Any(d => d is Keyboard || d is Mouse))
                {
                    Debug.Log($"Blocked keyboard input for Player {player.playerIndex}");
                    return;
                }
            }
        }
        // === Prevent others from using keyboard ===
        else if (players.Count > 1 && device is (Keyboard or Mouse))
        {
            Debug.Log("Blocked keyboard input for non-Player 1.");
            return;
        }

        // === Gamepad input received — show join prompt if not already shown ===
        else if (device is Gamepad gamepad && players.Count == 1)
        {
            // Check if this gamepad isn't already paired to Player 1
            var player1 = players[0];
            if (!player1.devices.Contains(gamepad))
            {
                Debug.Log("Second gamepad input detected — prompting for second player join.");
                ShowDeviceConnectionPrompt(gamepad);
            }
        }
    }

    // Called when a new player joins
    private void OnPlayerJoined(PlayerInput playerInput)
    {
        //Debug.Log($"Player {playerInput.playerIndex} joined with device {playerInput.devices[0].displayName}");
        players.Add(playerInput);

        if (playerInput.playerIndex != 0)
        {
            // Force-unpair keyboard & mouse for non-Player 1
            var user = playerInput.user;
            if (user != null)
            {
                foreach (var dev in user.pairedDevices.ToList())
                {
                    if (dev is Keyboard || dev is Mouse)
                    {
                        user.UnpairDevice(dev);
                    }
                }
            }
        }

        if (players.Count == 1)
        {
            if (playerInput.devices[0].displayName == "Xbox Controller")
            {
                //Debug.Log("Setting usingXbox to true");
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                usingXbox = true;
            }

            else if (playerInput.devices[0].displayName == "PlayStation Controller")
            {
                //Debug.Log("Setting usingPlaystation to true");
                //usingPlaystation = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            else if (playerInput.devices[0].displayName == "Keyboard" || playerInput.devices[0].displayName == "Mouse")
            {
               // Debug.Log("Setting usingKeyboard to true");
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
            bool alreadyInUse = players.Any(p => p.devices.Contains(gamepad));
            if (alreadyInUse) return;

            if (players.Count == 1)
            {
                ShowDeviceConnectionPrompt(gamepad);
            }
        }

        else if (device is Keyboard || device is Mouse)
        {
            if (controllerTookOver)
            {
                Debug.Log("Keyboard/Mouse prevented from joining: controller already took over.");
                return;
            }

            if (players.Count == 0)
            {
                // Manually join player with keyboard+mouse
                InputDevice[] keyboardMouseDevices = new InputDevice[] { keyboardDevice, mouseDevice };
                inputManager.JoinPlayer(0, -1, "Keyboard&Mouse", keyboardMouseDevices);
            }
        }

    }
    private void ShowDeviceConnectionPrompt(Gamepad gamepad)
    {
        Debug.Log($"Device Connected: {gamepad.displayName}. Waiting for player decision...");

        GameManager.Instance.players[0].pUI.ToggleConnectionPrompt(true);

        StartCoroutine(WaitForDeviceDecision(gamepad));
    }
    private IEnumerator WaitForDeviceDecision(Gamepad gamepad)
    {
        bool decisionMade = false;
        float timeout = 10f;

        while (!decisionMade && timeout > 0)
        {
            if (gamepad.buttonNorth.wasPressedThisFrame) // A
            {
                // Controller takes over Player 1
                players[0].SwitchCurrentControlScheme("Gamepad", gamepad);
                UpdateSensitivity();
                controllerTookOver = true;
                decisionMade = true;
            }
            else if (gamepad.buttonSouth.wasPressedThisFrame) // B
            {
                bool alreadyInUse = players.Any(p => p.devices.Contains(gamepad));
                if (!alreadyInUse)
                {
                    inputManager.JoinPlayer(players.Count, -1, "Gamepad", gamepad);
                    decisionMade = true;
                    Debug.Log("Player 2 joined with new Gamepad.");
                }
                else
                {
                    Debug.Log("Gamepad already in use — ignoring Player 2 join input.");
                }
            }

            timeout -= Time.unscaledDeltaTime;
            yield return null;
        }

      /*  if (!decisionMade)
        {
            Debug.Log("No input detected. Defaulting to add second player.");
            inputManager.JoinPlayer(players.Count, -1, "Gamepad", gamepad);
        }
*/
        GameManager.Instance.players[0].pUI.ToggleConnectionPrompt(false);
    }
    private void UpdateSensitivity()
    {
        players[0].GetComponent<PlayerManager>().SetDeviceSettings();
    }
    private void HandleDeviceDisconnected(InputDevice device)
    {
        if (device is Gamepad)
        {
            if (players.Count == 1)
            {
                if (keyboardDevice != null && mouseDevice != null)
                {
                    InputDevice[] keyboardMouseDevices = new InputDevice[] { keyboardDevice, mouseDevice };

                    players[0].SwitchCurrentControlScheme("Keyboard&Mouse", keyboardMouseDevices);
                    UpdateSensitivity(); // keep your sensitivity switch logic
                    controllerTookOver = false; // allow keyboard to take control again if needed
                    Debug.Log("Controller disconnected — switched back to keyboard & mouse.");
                }
                else
                {
                    Debug.LogWarning("Keyboard or Mouse device missing. Cannot switch control scheme.");
                }
            }
            else if (players.Count > 1)
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
