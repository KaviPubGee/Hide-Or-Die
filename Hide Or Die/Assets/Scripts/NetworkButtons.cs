using Unity.Netcode;
using UnityEngine;

public class NetworkButtons : MonoBehaviour
{
    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 300, 300));

        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 24;
        buttonStyle.fixedHeight = 60;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 22;

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host", buttonStyle))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUILayout.Button("Client", buttonStyle))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUILayout.Button("Server", buttonStyle))
            {
                NetworkManager.Singleton.StartServer();
            }
        }
        else
        {
            GUILayout.Label("Connected", labelStyle);

            if (GUILayout.Button("Shutdown", buttonStyle))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}