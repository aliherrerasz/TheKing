
using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System.Linq;

public class RecordManager : MonoBehaviour
{
    private SQLiteConnection _connection;

    void Awake()
    {
        string dbPath = Path.Combine(Application.persistentDataPath, "records.db");
        _connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        _connection.CreateTable<RecordData>();
    }

    public void GuardarRecord(string dificultad, float distancia)
    {
        var record = _connection.Table<RecordData>().FirstOrDefault(r => r.Dificultad == dificultad);
        if (record == null)
        {
            _connection.Insert(new RecordData { Dificultad = dificultad, Distancia = distancia });
        }
        else if (distancia > record.Distancia)
        {
            record.Distancia = distancia;
            _connection.Update(record);
        }
    }

    public float ObtenerRecord(string dificultad)
    {
        var record = _connection.Table<RecordData>().FirstOrDefault(r => r.Dificultad == dificultad);
        return record?.Distancia ?? 0f;
    }
}
