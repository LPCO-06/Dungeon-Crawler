using UnityEngine;
using Unity.Netcode;

public class NetworkDebugUI : MonoBehaviour
{
    private string inputJoinCode = "";
    private RelayManager relayManager;

    private void Start()
    {
        // Zoek automatisch de RelayManager in de scène
        relayManager = FindObjectOfType<RelayManager>();
        if (relayManager == null)
        {
            Debug.LogWarning("Geen RelayManager gevonden in de scène! Zorg ervoor dat deze op een GameObject staat.");
        }
    }

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;

        // Toon menu alleen als we nog niet verbonden/actief zijn
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.BeginArea(new Rect(20, 20, 300, 230));
            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Unity Relay Multiplayer Menu</b>", new GUIStyle(GUI.skin.label) { richText = true });

            if (GUILayout.Button("Start Relay Host", GUILayout.Height(35)))
            {
                if (relayManager != null)
                {
                    relayManager.InitializeAndStartHost();
                }
                else
                {
                    // Fallback naar gewone host als RelayManager ontbreekt
                    NetworkManager.Singleton.StartHost();
                }
            }

            GUILayout.Space(15);
            GUILayout.Label("<b>Join via Join Code:</b>");

            // Invoerveld voor de Relay Join Code
            inputJoinCode = GUILayout.TextField(inputJoinCode, 10, GUILayout.Height(30));

            if (GUILayout.Button("Connect as Client", GUILayout.Height(35)))
            {
                if (!string.IsNullOrEmpty(inputJoinCode))
                {
                    if (relayManager != null)
                    {
                        relayManager.JoinWithCode(inputJoinCode.ToUpper());
                    }
                }
                else
                {
                    Debug.LogWarning("Voer eerst een geldige Join Code in!");
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else
        {
            // Status scherm wanneer je wel verbonden bent
            GUILayout.BeginArea(new Rect(20, 20, 240, 80));
            GUILayout.BeginVertical("box");

            string status = NetworkManager.Singleton.IsHost ? "Hosting (Relay)" : "Verbonden Client";
            GUILayout.Label($"<b>Status:</b> {status}", new GUIStyle(GUI.skin.label) { richText = true });

            if (NetworkManager.Singleton.IsHost)
            {
                GUILayout.Label("<color=green>Kijk in de console voor je Join Code!</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 10 });
            }

            if (GUILayout.Button("Verbinding verbreken", GUILayout.Height(25)))
            {
                NetworkManager.Singleton.Shutdown();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}