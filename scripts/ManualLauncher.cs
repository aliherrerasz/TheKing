using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ManualLauncher : MonoBehaviour
{
    const string MANUAL_FILE = "manual.html";

    void Update()
    {
        // Detecta pulsación de F11
        if (Input.GetKeyDown(KeyCode.F11))
        {
            OpenManual();
        }
    }

    void OpenManual()
    {
        // Construye la ruta completa
        #if UNITY_ANDROID && !UNITY_EDITOR
        string url = Path.Combine(Application.persistentDataPath, MANUAL_FILE);
        #else
        string url = Path.Combine(Application.streamingAssetsPath, MANUAL_FILE);
        #endif

        Application.OpenURL(url);
    }
}
