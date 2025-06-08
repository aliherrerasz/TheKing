using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class OpcionesMenu : MonoBehaviour
{
    public GameObject panelOpciones;

    private bool estaPausado = false;
    private bool estaMuteado = false;
    
    void Start()
    {
        panelOpciones.SetActive(false);
    }
    

    public void ToggleMute()
    {
        estaMuteado = !estaMuteado;
        AudioListener.volume = estaMuteado ? 0f : 1f;
    }


    public void TogglePanel()
    {
        estaPausado = !estaPausado;
        panelOpciones.SetActive(estaPausado);
        Time.timeScale = estaPausado ? 0f : 1f;
    }

    public void CambiarDificultad()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("menu");
    }

    public void CambiarJugador()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Seleccion");
    }
        public void Cancelar()
    {
        panelOpciones.SetActive(false);
        Time.timeScale = 1f;
        estaPausado = false;
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
