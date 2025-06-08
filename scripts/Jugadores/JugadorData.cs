using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;

public class JugadorData
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int SlotId { get; set; }
    public string Nombre { get; set; }
}
