using SQLite4Unity3d;

public class RecordData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Dificultad { get; set; }
    public float Distancia { get; set; }
}
