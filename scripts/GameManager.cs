using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 2;
    public float scrollFondo = 0.015f;

    [Header("Suelo")]
    [Tooltip("Lista de prefabs que forman el suelo")]
    public List<GameObject> columnaPrefabs;

    [Header("Obstáculos")]
    public List<GameObject> obstaculoPrefabs;
    public int cantidadObstaculos = 2;
    public Vector2 rangoSpawnX = new Vector2(10, 18);

    [Header("UI")]
    public GameObject menuPrincipal;
    public GameObject menuGameOver;
    public Text cronometro;
    public Text marcadorFinal;
    public Text recordTexto;

    [Header("Fondo")]
    public Renderer bg;

    [Header("Nivel")]
    public string nivelActual = "Fácil";
    [Tooltip("En Fácil: desmarca. En Intermedio/Difícil: marca")]
    public bool generarHuecos = false;

    [HideInInspector] public bool start = false;
    [HideInInspector] public bool gameOver = false;
    private float tiempo;

    // Suelo y obstáculos estáticos
    private List<GameObject> suelo = new List<GameObject>();
    private List<GameObject> obstaculos = new List<GameObject>();
    private Queue<GameObject> columnasPendientes = new Queue<GameObject>();

    // Patrón “naranja” y “azul” (solo se usan cuando generarHuecos == true)
    private readonly int[] patronNaranja = { 0, 1, 2, 3 };
    private readonly int[] patronAzul    = { 4, 5, 6 };

    private int ultimaX = -10;
    private RecordManager recordManager;
    private bool ultimoFueHueco = false;
    private List<GameObject> enemigos = new List<GameObject>();

    [SerializeField]
    private LayerMask sueloLayer; 

    void Start()
    {
        recordManager = FindObjectOfType<RecordManager>();
        if (recordManager != null)
        {
            float mejorDistancia = recordManager.ObtenerRecord(nivelActual);
            recordTexto.text = $"Record: {mejorDistancia:F0} m";
        }

        // Generar suelo inicial (21 columnas o huecos seguidos)
        for (int i = 0; i < 21; i++)
        {
            if (columnasPendientes.Count == 0)
                GenerarNuevoPatron();

            GameObject prefabCol = columnasPendientes.Dequeue();
            if (prefabCol != null)
            {
                GameObject nuevaCol = Instantiate(prefabCol, new Vector2(ultimaX, -3), Quaternion.identity);
                suelo.Add(nuevaCol);
            }
            else
            {
                suelo.Add(null); // hueco
            }
            ultimaX++;
        }

        GenerarObstaculosIniciales();

        menuPrincipal.SetActive(true);
        menuGameOver.SetActive(false);
        cronometro.text = "0";
        marcadorFinal.text = "";
    }

    public void SetGameOver()
    {
        gameOver = true;
    }

    void Update()
    {
        if (!start)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X) ||
                (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began))
            {
                start = true;
            }
        }

        if (start && gameOver)
        {
            menuGameOver.SetActive(true);
            marcadorFinal.text = $"Has recorrido: {tiempo:F0} m";

            if (recordManager != null)
            {
                recordManager.GuardarRecord(nivelActual, tiempo);
                float mejor = recordManager.ObtenerRecord(nivelActual);
                recordTexto.text = $"Record: {mejor:F0} m";
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.X) ||
                (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        if (start && !gameOver)
        {
            menuPrincipal.SetActive(false);
            // Mover background
            bg.material.mainTextureOffset += new Vector2(scrollFondo, 0) * velocidad * Time.deltaTime;
            tiempo += Time.deltaTime;
            cronometro.text = tiempo.ToString("F0");

            // 1) Mover y regenerar suelo
            for (int i = 0; i < suelo.Count; i++)
            {
                var s = suelo[i];
                if (s == null) continue;

                s.transform.position += Vector3.left * velocidad * Time.deltaTime;
                if (s.transform.position.x <= -10f)
                {
                    Destroy(s);
                    if (columnasPendientes.Count == 0)
                        GenerarNuevoPatron();

                    GameObject siguiente = columnasPendientes.Dequeue();
                    if (siguiente != null)
                        suelo[i] = Instantiate(siguiente, new Vector3(10f, -3f, 0f), Quaternion.identity);
                    else
                        suelo[i] = null;
                }
            }

            // 2) Mover obstáculos estáticos
            for (int i = obstaculos.Count - 1; i >= 0; i--)
            {
                var o = obstaculos[i];
                if (o == null)
                {
                    obstaculos.RemoveAt(i);
                    continue;
                }

                if (o.CompareTag("Obstaculo"))
                {
                    o.transform.position += Vector3.left * velocidad * Time.deltaTime;
                    if (o.transform.position.x <= -10f)
                    {
                        Destroy(o);
                        obstaculos.RemoveAt(i);
                    }
                }
            }

            // 3) Regenerar obstáculos estáticos (evitar bucle infinito)
            int maxReintentos = 30;
            while (obstaculos.Count < cantidadObstaculos && maxReintentos-- > 0)
            {
                int intentos = 10;
                bool generado = false;
                while (intentos-- > 0)
                {
                    float x = Mathf.Round(Random.Range(rangoSpawnX.x, rangoSpawnX.y));
                    if (HaySueloBajo(x) && !HayObstaculoCerca(x) && !HayEnemigoCerca(x))
                    {
                        var prefab = obstaculoPrefabs[Random.Range(0, obstaculoPrefabs.Count)];
                        GameObject instancia = Instantiate(prefab);
                        AjustarAlturaSobreSuelo(instancia, x);

                        if (instancia.CompareTag("Obstaculo"))
                        {
                            obstaculos.Add(instancia);
                            generado = true;
                        }
                        else if (instancia.CompareTag("Enemigo"))
                        {
                            enemigos.Add(instancia);
                            generado = true;
                        }
                        break;
                    }
                }
                if (!generado) break;
            }
            enemigos.RemoveAll(e => e == null);

            // 4) Verificar caída del jugador
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null && jugador.transform.position.y < -6f)
            {
                SetGameOver();
            }
        }
    }

    private bool HayObstaculoCerca(float x, float umbral = 1.5f)
    {
        foreach (var o in obstaculos)
        {
            if (o == null) continue;
            if (Mathf.Abs(o.transform.position.x - x) < umbral)
                return true;
        }
        return false;
    }

    private bool HayEnemigoCerca(float x, float umbral = 1.5f)
    {
        foreach (var e in enemigos)
        {
            if (e == null) continue;
            if (Mathf.Abs(e.transform.position.x - x) < umbral)
                return true;
        }
        return false;
    }

    public bool HaySueloEn(float x)
    {
        foreach (var c in suelo)
        {
            if (c != null && Mathf.Abs(c.transform.position.x - x) < 0.6f)
                return true;
        }
        return false;
    }

    void GenerarObstaculosIniciales()
    {
        int gen = 0, mx = 30;
        while (gen < cantidadObstaculos && mx-- > 0)
        {
            float x = Mathf.Round(Random.Range(rangoSpawnX.x, rangoSpawnX.y));
            if (HaySueloBajo(x))
            {
                var p = obstaculoPrefabs[Random.Range(0, obstaculoPrefabs.Count)];
                GameObject instancia = Instantiate(p, new Vector2(x, -2), Quaternion.identity);

                if (instancia.CompareTag("Obstaculo"))
                {
                    obstaculos.Add(instancia);
                    gen++;
                }
                // Los enemigos móviles (tag="Enemigo") se quedan y los mueve su propio script
            }
        }
    }

    bool HaySueloBajo(float x)
    {
        foreach (var c in suelo)
        {
            if (c != null && Mathf.Abs(c.transform.position.x - x) < 0.6f)
                return true;
        }
        return false;
    }

    void AjustarAlturaSobreSuelo(GameObject obj, float x)
    {
        float yRayStart = 10f;
        Vector2 origen = new Vector2(x, yRayStart);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, 20f, sueloLayer);
        if (hit.collider != null)
        {
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            float halfH = (sr != null) ? sr.bounds.extents.y : 0.5f;
            if (sr == null)
            {
                Collider2D col = obj.GetComponent<Collider2D>();
                if (col != null) halfH = col.bounds.extents.y;
            }
            obj.transform.position = new Vector2(x, hit.point.y + halfH);
        }
        else
        {
            obj.transform.position = new Vector2(x, -2f);
        }
    }

    void GenerarNuevoPatron()
    {
        if (!generarHuecos)
        {
            // MODO FÁCIL: no se generan huecos, solo una columna aleatoria cada vez.
            if (columnaPrefabs.Count == 0)
            {
                // Si no hay ningún prefab en la lista
                columnasPendientes.Enqueue(null);
            }
            else
            {
                int idx = Random.Range(0, columnaPrefabs.Count);
                columnasPendientes.Enqueue(columnaPrefabs[idx]);
            }
            return;
        }

        // MODO INTERMEDIO/DIFÍCIL: usamos patrones con posibles huecos
        int tipo;
        if (ultimoFueHueco)
            tipo = Random.Range(0, 2);  // Solo naranja o azul
        else
            tipo = Random.Range(0, 3);  // 0 = naranja, 1 = azul, 2 = hueco

        if (tipo == 0)
        {
            // Patrón naranja: índices {0,1,2,3}
            foreach (var idx in patronNaranja)
            {
                if (idx >= 0 && idx < columnaPrefabs.Count)
                    columnasPendientes.Enqueue(columnaPrefabs[idx]);
            }
            ultimoFueHueco = false;
        }
        else if (tipo == 1)
        {
            // Patrón azul: índices {4,5,6}
            foreach (var idx in patronAzul)
            {
                if (idx >= 0 && idx < columnaPrefabs.Count)
                    columnasPendientes.Enqueue(columnaPrefabs[idx]);
            }
            ultimoFueHueco = false;
        }
        else
        {
            // Hueco de 1 o 2 columnas
            int anchoHueco = Random.Range(1, 3);
            for (int i = 0; i < anchoHueco; i++)
                columnasPendientes.Enqueue(null);
            ultimoFueHueco = true;
        }
    }
}
