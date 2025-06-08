using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jugador : MonoBehaviour
{
    public GameManager gameManager;
    public float fuerzaSalto = 7f;
    public float fuerzaSaltoAire = 5f; // Fuerza del segundo salto
    public bool grounded = true;

    private Rigidbody2D rb;
    private Animator animator1;

    private int saltosRestantes = 2;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator1 = GetComponent<Animator>();
    }

    void Update()
    {
        if (gameManager == null || gameManager.gameOver) return;

        if (gameManager.start)
        {
            bool inputSalto = (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began)
                               || Input.GetMouseButtonDown(0)
                               || Input.GetKeyDown(KeyCode.Space);

            if (inputSalto && saltosRestantes > 0)
            {
                float fuerza = (saltosRestantes == 2) ? fuerzaSalto : fuerzaSaltoAire;

                rb.velocity = new Vector2(rb.velocity.x, 0); // reset vertical
                rb.AddForce(new Vector2(0, fuerza), ForceMode2D.Impulse);
                animator1.SetBool("jumping", true);

                saltosRestantes--;
                grounded = false;
            }

            // Revisa si cae por fuera de pantalla
            if (transform.position.y < -6f)
            {
                gameManager.SetGameOver();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            animator1.SetBool("jumping", false);
            grounded = true;
            saltosRestantes = 2; // Reset al tocar suelo
        }

        if (collision.gameObject.CompareTag("Obstaculo") || collision.gameObject.CompareTag("Enemigo"))
        {
            gameManager.SetGameOver();
        }
    }
}
