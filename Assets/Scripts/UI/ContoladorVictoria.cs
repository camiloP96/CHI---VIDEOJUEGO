using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class VictoryScreenController : MonoBehaviour
{
    [Header("Referencias")]
    public FinalWallController paredFinal; // arrastra el GameObject "FinalPlane" en el Inspector

    private VisualElement pantallaCompleta;
    private Button botonReiniciar;
    private Button botonMenuPrincipal;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        pantallaCompleta = root.Q<VisualElement>("VictoryScreen");
        botonReiniciar = root.Q<Button>("restartVictoryB");
        botonMenuPrincipal = root.Q<Button>("mainmenuVictoryB");

        if (botonReiniciar != null)
        {
            botonReiniciar.clicked += ReiniciarNivel;
        }

        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.clicked += IrAMenuPrincipal;
        }

        // Se suscribe al evento de victoria de la pared final (FinalWallController.cs)
        if (paredFinal != null)
        {
            paredFinal.OnVictoria += MostrarPantallaVictoria;
        }
        else
        {
            Debug.LogWarning($"{name}: no se asignó 'paredFinal' en el Inspector.");
        }

        if (pantallaCompleta != null)
        {
            pantallaCompleta.style.display = DisplayStyle.None;
        }
        else
        {
            Debug.LogError("[DIAGNOSTICO] No se encontró 'VictoryScreen' en el UXML. Revisa el 'Name' en UI Builder.");
        }
    }

    private void MostrarPantallaVictoria()
    {
        if (pantallaCompleta == null) return;

        pantallaCompleta.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.name);
    }

    private void IrAMenuPrincipal()
    {
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        SceneManager.LoadScene("Main Menu");
    }

    void OnDestroy()
    {
        if (paredFinal != null)
        {
            paredFinal.OnVictoria -= MostrarPantallaVictoria;
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.clicked -= ReiniciarNivel;
        }

        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.clicked -= IrAMenuPrincipal;
        }

        Time.timeScale = 1f;
    }
}