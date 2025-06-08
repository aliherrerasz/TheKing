using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public class DBManager : MonoBehaviour
{
    public static DBManager Instance;

    private SQLiteConnection _db;

    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            InitDB();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitDB()
    {
        string fileName = "jugadores.db";

        // Ruta donde queremos la BD: siempre en persistentDataPath
        string persistentPath = Path.Combine(Application.persistentDataPath, fileName);

#if UNITY_EDITOR
        // si no existe en StreamingAssets, la creamos directamente en persistentDataPath
        if (!File.Exists(persistentPath))
        {
            
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath);
            }
            // Si no existe en StreamingAssets, dejaremos que SQLite cree la DB en la siguiente línea
        }
#else
         if (!File.Exists(persistentPath))
        {
        
            string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);

#if UNITY_ANDROID
            // En Android, StreamingAssets está comprimido, así que usamos UnityWebRequest
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(streamingPath);
            www.SendWebRequest();
            while (!www.isDone) { }

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(persistentPath, www.downloadHandler.data);
            }
            else
            {
                Debug.LogWarning("No se encontró " + streamingPath + ", se creará nueva BD en persistentDataPath");
            }
#else
            if (File.Exists(streamingPath))
            {
                File.Copy(streamingPath, persistentPath);
            }
            // Si no existe en StreamingAssets, dejamos que SQLite lo cree
#endif
        }
#endif

        // Finalmente, abrimos (o creamos) la BD en persistentDataPath
        _db = new SQLiteConnection(persistentPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _db.CreateTable<JugadorData>();
        Debug.Log("Base de datos abierta en: " + persistentPath);
    }

    public JugadorData GetJugador(int slotId)
    {
        if (_db == null) return null;
        return _db.Table<JugadorData>()
          .Where(j => j.SlotId == slotId)
          .FirstOrDefault();

    }

    public void CrearJugador(int slotId)
    {
        if (_db == null) return;
        var jugador = new JugadorData { SlotId = slotId, Nombre = "Jugador " + slotId };
        _db.Insert(jugador);
    }
    public void ActualizarNombreJugador(int slotId, string nuevoNombre)
    {
        var jugador = GetJugador(slotId);
        if (jugador != null)
        {
            jugador.Nombre = nuevoNombre;
            _db.Update(jugador);
        }
    }
    public void ActualizarJugador(JugadorData jugador)
    {
        _db.Update(jugador);
    }

    public void CrearJugador(int slotId, string nombre)
    {
        var jugador = new JugadorData { SlotId = slotId, Nombre = nombre };
        _db.Insert(jugador);
    }


    public void BorrarJugador(int slotId)
    {
        var jugador = GetJugador(slotId);
        if (jugador != null)
        {
            _db.Delete(jugador);
        }
    }


    public SQLiteConnection GetConnection()
    {
        return _db;
    }
}
