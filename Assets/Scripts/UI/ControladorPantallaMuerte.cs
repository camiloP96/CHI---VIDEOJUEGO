using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Necesario para recargar la escena actual

[RequireComponent(typeof(UIDocument))]
public class DeathScreenController : MonoBehaviour
{
    [Header("Referencias")]
    public PlayerHealth vidaJugador; // arrastra al jugador en el Inspector

    private VisualElement pantallaCompleta;
    private Button botonReiniciar;
    private Button botonMenuPrincipal;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        pantallaCompleta = root.Q<VisualElement>("PantallaMuerte");
        botonReiniciar = root.Q<Button>("restartB");
        botonMenuPrincipal = root.Q<Button>("mainmenu");

        // Conecta el evento de clic del botón con su método correspondiente.
        // La sintaxis "+= NombreMetodo" (sin paréntesis) registra el método como
        // callback, para que se ejecute automáticamente cuando el botón sea presionado.
        if (botonReiniciar != null)
        {
            botonReiniciar.clicked += ReiniciarNivel;
        }

        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.SetEnabled(true);
            botonMenuPrincipal.clicked += IrAMenuPrincipal;
            Debug.Log("[DIAGNOSTICO] botonMenuPrincipal encontrado y suscrito correctamente.");
        }
        else
        {
            Debug.LogError("[DIAGNOSTICO] No se encontró el botón 'mainmenu' en el UXML. Revisa que el 'Name' en UI Builder sea exactamente 'mainmenu'.");
        }

        // Se suscribe al evento de muerte del jugador (definido en PlayerHealth.cs)
        if (vidaJugador != null)
        {
            vidaJugador.OnMuerte += MostrarPantallaMuerte;
        }
        else
        {
            Debug.LogWarning($"{name}: no se asignó 'vidaJugador' en el Inspector.");
        }

        // Asegura que la pantalla empiece oculta, sin importar lo que diga el USS por defecto
        if (pantallaCompleta != null)
        {
            pantallaCompleta.style.display = DisplayStyle.None;
            Debug.Log("[DIAGNOSTICO] pantallaCompleta encontrada y oculta correctamente en Start().");
        }
        else
        {
            Debug.LogError("[DIAGNOSTICO] No se encontró 'death-screen-root' en el UXML. Revisa el Source Asset del UIDocument.");
        }
    }

    private void MostrarPantallaMuerte()
    {
        if (pantallaCompleta == null) return;

        pantallaCompleta.style.display = DisplayStyle.Flex;

        // Pausa el juego: al poner Time.timeScale en 0, todo lo que dependa de Time.deltaTime
        // (movimiento, animaciones, spawns) se congela, pero la UI de UI Toolkit sigue
        // funcionando con normalidad porque no depende del timeScale para renderizarse ni recibir clics.
        Time.timeScale = 0f;

        // Libera y muestra el cursor del mouse, necesario para poder hacer clic en los botones.
        // Se usa el nombre completo "UnityEngine.Cursor" (en vez de solo "Cursor") porque
        // UI Toolkit también define su propia clase "Cursor" (UnityEngine.UIElements.Cursor,
        // relacionada con estilos visuales del cursor dentro de la UI), y como este script
        // tiene "using UnityEngine.UIElements;", el nombre corto "Cursor" queda ambiguo.
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void ReiniciarNivel()
    {
        // Importante: restaurar el timeScale ANTES de cargar la nueva escena.
        // Si no se hace esto, la escena reiniciada seguiría "congelada" en 0,
        // ya que Time.timeScale es un valor global que persiste entre cargas de escena.
        Time.timeScale = 1f;

        // Recarga la escena actual por su nombre, reiniciando todo el nivel desde cero
        // (posiciones, vida, enemigos, etc., ya que todos los objetos se vuelven a instanciar).
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.name);
    }

    private void IrAMenuPrincipal()
    {
        Debug.Log("[DIAGNOSTICO] IrAMenuPrincipal() fue llamado. Intentando cargar 'Main Menu'...");

        // Igual que en ReiniciarNivel(): restaurar el timeScale es obligatorio antes
        // de cambiar de escena, para que el menú principal no cargue congelado.
        Time.timeScale = 1f;

        // También conviene restaurar el estado del cursor por si el menú lo necesita
        // configurado de otra forma (normalmente los menús SÍ quieren el cursor visible,
        // así que esto ya debería estar bien, pero lo dejamos explícito por claridad).
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        // Carga la escena del menú principal por su nombre exacto.
        // IMPORTANTE: el nombre debe coincidir EXACTAMENTE (mayúsculas incluidas) con el
        // nombre del archivo de escena, y esa escena debe estar agregada en
        // File > Build Settings > Scenes In Build, o esta línea lanzará un error en tiempo de ejecución.
        SceneManager.LoadScene("Main Menu");
    }

    // Buena práctica: desuscribirse de eventos al destruir este objeto
    void OnDestroy()
    {
        if (vidaJugador != null)
        {
            vidaJugador.OnMuerte -= MostrarPantallaMuerte;
        }

        if (botonReiniciar != null)
        {
            botonReiniciar.clicked -= ReiniciarNivel;
        }

        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.clicked -= IrAMenuPrincipal;
        }

        // Asegura que el juego no quede pausado permanentemente si este objeto
        // se destruye por alguna razón (ej. al cambiar de escena manualmente)
        Time.timeScale = 1f;
    }
}



        