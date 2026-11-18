using PurrNet;
using PurrNet.Transports;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobbyUI : MonoBehaviour
{
    [Header("PurrNet References")]
    [SerializeField] private NetworkManager networkManager;

    [Header("Pre-Game Lobby & Class Selection")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button[] classButtons;
    [SerializeField] private TMP_Text selectedClassText;

    [Header("Connection Controls")]
    [SerializeField] private TMP_InputField addressOrRoomInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text statusText;

    private int selectedClassIndex = -1;
    private bool hasAppliedInitialClass = false;

    private void Awake()
    {
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<NetworkManager>();
        }
    }

    private void Start()
    {
        if (hostButton != null)
        {
            hostButton.onClick.AddListener(OnHostClicked);
            hostButton.interactable = false; // Locked until a class is picked
        }

        if (joinButton != null)
        {
            joinButton.onClick.AddListener(OnJoinClicked);
            joinButton.interactable = false; // Locked until a class is picked
        }

        for (int i = 0; i < classButtons.Length; i++)
        {
            int index = i;
            if (classButtons[i] != null)
            {
                classButtons[i].onClick.AddListener(() => OnSelectClassClicked(index));
            }
        }

        if (lobbyPanel != null) lobbyPanel.SetActive(true);
        if (selectedClassText != null) selectedClassText.text = "Select a Class to Continue";
        UpdateStatus("Choose your class");
    }

    private void Update()
    {
        // Once connected and our local player spawns, apply the class we picked in the lobby
        if (!hasAppliedInitialClass && selectedClassIndex >= 0)
        {
            PlayerClassManager[] managers = FindObjectsByType<PlayerClassManager>(FindObjectsSortMode.None);
            foreach (PlayerClassManager manager in managers)
            {
                if (manager.isOwner)
                {
                    manager.RequestClassChange(selectedClassIndex);
                    hasAppliedInitialClass = true;
                    break;
                }
            }
        }
    }

    private void OnSelectClassClicked(int classIndex)
    {
        selectedClassIndex = classIndex;

        // Highlight the chosen button visually by disabling it while keeping others clickable
        for (int i = 0; i < classButtons.Length; i++)
        {
            if (classButtons[i] != null)
            {
                classButtons[i].interactable = (i != classIndex);
            }
        }

        // Unlock Host and Join buttons now that a class is chosen
        if (hostButton != null) hostButton.interactable = true;
        if (joinButton != null) joinButton.interactable = true;

        // Display the button's label (or fallback to slot number)
        string className = $"Class #{classIndex + 1}";
        if (classButtons[classIndex] != null)
        {
            TMP_Text btnLabel = classButtons[classIndex].GetComponentInChildren<TMP_Text>();
            if (btnLabel != null && !string.IsNullOrWhiteSpace(btnLabel.text))
            {
                className = btnLabel.text;
            }
        }

        if (selectedClassText != null) selectedClassText.text = $"Selected: {className}";
        UpdateStatus("Enter Code; Host or Join");
    }

    private void OnHostClicked()
    {
        if (networkManager == null || selectedClassIndex < 0) return;

        ApplyAddressOrRoom();
        UpdateStatus("Starting Host...");

        networkManager.StartHost();
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void OnJoinClicked()
    {
        if (networkManager == null || selectedClassIndex < 0) return;

        ApplyAddressOrRoom();
        UpdateStatus($"Joining {GetTargetAddress()}...");

        networkManager.StartClient();
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private void ApplyAddressOrRoom()
    {
        string target = GetTargetAddress();
        if (string.IsNullOrWhiteSpace(target)) return;

        if (networkManager.TryGetComponent<UDPTransport>(out UDPTransport udp))
        {
            udp.address = target;
        }
        else if (networkManager.TryGetComponent<PurrTransport>(out PurrTransport purrRelay))
        {
            purrRelay.roomName = target;
        }
    }

    private string GetTargetAddress()
    {
        if (addressOrRoomInput != null && !string.IsNullOrWhiteSpace(addressOrRoomInput.text))
        {
            return addressOrRoomInput.text.Trim();
        }
        return "127.0.0.1";
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null) statusText.text = message;
    }
}