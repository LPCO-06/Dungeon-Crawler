using UnityEngine;
using Unity.Netcode;

public class NetworkDebugUI : MonoBehaviour
{
    private void OnGUI()
    {
        // Safety check: If NetworkManager doesn't exist in the scene yet, don't run GUI logic
        if (NetworkManager.Singleton == null) return;

        // Only show buttons if we aren't already connected/listening
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            GUILayout.BeginArea(new Rect(20, 20, 250, 150));

            GUILayout.BeginVertical("box");

            GUILayout.Label("<b>Multiplayer Controls</b>", new GUIStyle(GUI.skin.label) { richText = true });

            if (GUILayout.Button("Start Host (Player 1)", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Start Client (Player 2)", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUILayout.Button("Start Server (Dedicated)", GUILayout.Height(30)))
            {
                NetworkManager.Singleton.StartServer();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        else
        {
            GUILayout.BeginArea(new Rect(20, 20, 200, 60));
            GUILayout.BeginVertical("box");

            string status = NetworkManager.Singleton.IsHost ? "Hosting" : (NetworkManager.Singleton.IsServer ? "Server" : "Connected Client");
            GUILayout.Label($"<b>Status:</b> {status}", new GUIStyle(GUI.skin.label) { richText = true });

            if (GUILayout.Button("Disconnect", GUILayout.Height(25)))
            {
                NetworkManager.Singleton.Shutdown();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}