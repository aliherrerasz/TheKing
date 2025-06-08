using UnityEngine;

public class ObstaculoMovil : MonoBehaviour
{
    [Header("Oscilación")]
    [Tooltip("Si está activado, oscila en Y (nivel Difícil). Si está desactivado, oscila en X (nivel Intermedio).")]
    public bool moverVertical = false;

    [Tooltip("Distancia máxima (amplitud) de la oscilación.")]
    public float amplitud = 1.5f;

    [Tooltip("Velocidad angular de la oscilación (en rad/s).")]
    public float velocidadOscilacion = 2f;

    [Tooltip("Velocidad a la que se desplaza con el suelo (eje X hacia la izquierda).")]
    public float velocidadAvance = 2f;

    [Header("Sprites")]
    [Tooltip("Lista de posibles sprites (se escoge uno al azar en Start).")]
    public Sprite[] posiblesSprites;

    [Header("Detección")]
    [Tooltip("Capa que marca otros obstáculos (para invertir dirección si chocan).")]
    public LayerMask obstaculoMask;

    // -------------------------------------------------
    //  Parámetros de chequeo (si no hay suelo o hay obstáculo delante).
    private float chequeoSueloDistancia = 0.6f;
    private float chequeoObstaculoDistancia = 0.6f;
    private float chequeoObstaculoRadio = 0.3f;

    // -------------------------------------------------
    private Vector3 posicionBase;   // Punto central de oscilación (se actualiza con el avance del suelo)
    private int direccion = 1;      // +1 = va “hacia la derecha” en la parte oscilatoria, o -1 = revertir eje oscilación
    private float tiempoOffset = 0f; // Para reiniciar cuando cambie dirección

    private GameManager gameManager;

    void Start()
    {
        // Guardar la posición inicial (mundo) para comenzar a oscilar
        posicionBase = transform.position;

        // Referencia al GameManager para leer si el juego empezó / terminó
        gameManager = FindObjectOfType<GameManager>();

        // Elegir sprite aleatorio al crear
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && posiblesSprites != null && posiblesSprites.Length > 0)
        {
            sr.sprite = posiblesSprites[Random.Range(0, posiblesSprites.Length)];
        }
    }

    void Update()
    {
        // Si GameManager no existe o aún no se empezó, o ya terminó, no hacemos nada
        if (gameManager == null || !gameManager.start || gameManager.gameOver)
            return;

        // -------------------------------------------------
        //  1) Chequear si necesitamos invertir dirección:
        //     - No hay suelo justo delante (solo importa en modo horizontal)
        //     - Hay un obstáculo delante (independiente de modo)
    
        //
        Vector2 posActual = transform.position;
        Vector2 puntoChequeo = moverVertical
            // Si oscila en Y, “adelante” significa en Y, chequeamos suelo abajo/arriba?
           
            ? new Vector2(posActual.x, posActual.y + direccion * chequeoSueloDistancia)
            : new Vector2(posActual.x + direccion * chequeoSueloDistancia, posActual.y);

        bool sinSuelo = false;
        if (!moverVertical)
        {
        
            sinSuelo = !gameManager.HaySueloEn(puntoChequeo.x);
        }

        // Siempre chequeamos obstáculo próximo (mismo eje)
        Vector2 origenObst = moverVertical
            ? new Vector2(posActual.x, posActual.y + direccion * chequeoObstaculoDistancia)
            : new Vector2(posActual.x + direccion * chequeoObstaculoDistancia, posActual.y);

        bool obstDelante = Physics2D.OverlapCircle(origenObst, chequeoObstaculoRadio, obstaculoMask) != null;

        if (sinSuelo || obstDelante)
        {
            // Invertimos la dirección reiniciamos
            direccion *= -1;
            tiempoOffset = Time.time;
            posicionBase = transform.position;
        }

        // -------------------------------------------------
        //  2) Calcular desplazamiento oscilatorio
        float fase = (Time.time - tiempoOffset) * velocidadOscilacion;
        float desplazOscil = Mathf.Sin(fase) * amplitud * direccion;

        // -------------------------------------------------
        //  3) Avance con el suelo hacia la izquierda
        Vector3 avanceIzq = Vector3.left * gameManager.velocidad * Time.deltaTime;

        // -------------------------------------------------
        //  4) Posicionar en función de modo:
        Vector3 nuevaPos = posicionBase;
        if (moverVertical)
        {
            // Oscilación en Y:
            nuevaPos += new Vector3(0, desplazOscil, 0);
        }
        else
        {
            // Oscilación en X:
            nuevaPos += new Vector3(desplazOscil, 0, 0);
        }

        // Luego aplicar el avance hacia la izquierda (sincroniza con el suelo)
        transform.position = nuevaPos + avanceIzq;

        // -------------------------------------------------
        //  5) Actualizar la posiciónBase para la próxima:
        posicionBase += avanceIzq;

        // -------------------------------------------------
        //  6) Destruir si sale de pantalla (por la izquierda)
        if (transform.position.x <= -10f)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Dibuja el radio de chequeo de obstáculo
        Gizmos.color = Color.red;
        Vector2 posActual = Application.isPlaying ? (Vector2)transform.position : (Vector2)transform.position;
        Vector2 origenObst = moverVertical
            ? new Vector2(posActual.x, posActual.y + direccion * chequeoObstaculoDistancia)
            : new Vector2(posActual.x + direccion * chequeoObstaculoDistancia, posActual.y);
        Gizmos.DrawWireSphere(origenObst, chequeoObstaculoRadio);
    }
}
