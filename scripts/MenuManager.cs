
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void CargarNivelFacil()
    {
        SceneManager.LoadScene("NivelFacil");
    }

    public void CargarNivelIntermedio()
    {
        SceneManager.LoadScene("NivelIntermedio");
    }

    public void CargarNivelDificil()
    {
        SceneManager.LoadScene("NivelDificil");
    }
}