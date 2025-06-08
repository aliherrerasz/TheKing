using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemigoMovil : MonoBehaviour
{
    public float velocidad = 2f;
    public float amplitud = 0.5f;
    public float velocidadOscilacion = 2f;
    public bool oscilarEnY = false;

    private Rigidbody2D rb;
    private Vector3 posicionInicial;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // aseguramos que no caiga
        rb.freezeRotation = true; // evitamos que gire al chocar
        posicionInicial = transform.position;
    }

    void FixedUpdate() // Usamos FixedUpdate para físicas
    {
        float offset = Mathf.Sin(Time.time * velocidadOscilacion) * amplitud;

        Vector3 nuevaPosicion = transform.position + Vector3.left * velocidad * Time.fixedDeltaTime;

        if (oscilarEnY)
            nuevaPosicion.y = posicionInicial.y + offset;
        else
            nuevaPosicion.y = posicionInicial.y; // aseguramos eje Y estable
            nuevaPosicion.x += offset; // aplica el zigzag solo si quieres

        rb.MovePosition(nuevaPosicion);

        if (transform.position.x <= -10f)
            Destroy(gameObject);
    }
}
