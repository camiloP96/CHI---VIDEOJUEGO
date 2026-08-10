using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class EnemyCounterController : MonoBehaviour
{
    [Header("Configuración")]
    // Total de enemigos que deben morir para considerar el nivel completo.
    // Lo dejamos como campo público en vez de calcularlo automáticamente,
    // por si en algún momento quieres controlarlo manualmente (ej. enemigos que
    // aparecen después, o si por error hay un dron de más/menos en la escena).
    public int totalEnemigos = 4;

    private int enemigosMuertos = 0;
    private Label labelContador;

    // Evento público: otros scripts (ej. el que controla la pared final) pueden
    // suscribirse a esto para saber exactamente cuándo se cumplió la condición,
    // sin necesidad de consultar constantemente en su propio Update().
    public System.Action OnTodosLosEnemigosMuertos;

    // Propiedad pública de solo lectura, por si algún script prefiere consultar
    // el estado directamente en vez de escuchar el evento.
    public bool NivelCompletado => enemigosMuertos >= totalEnemigos;

    void Start()
    {
        // Obtiene el elemento de texto del UXML por su "name", para poder modificarlo por código
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;
        labelContador = root.Q<Label>("Enemigo");

        // Busca TODOS los DroneHealth activos en la escena al iniciar (los 4 drones del nivel)
        DroneHealth[] drones = FindObjectsByType<DroneHealth>(FindObjectsSortMode.None);

        // Se suscribe al evento OnMuerte de cada uno individualmente
        foreach (DroneHealth dron in drones)
        {
            dron.OnMuerte += RegistrarMuerteEnemigo;
        }

        // Actualiza el texto una vez al inicio, para que muestre "0 / 4" desde el primer frame
        ActualizarTexto();
    }

    // Este método se ejecuta cada vez que CUALQUIERA de los drones suscritos dispara su evento OnMuerte
    private void RegistrarMuerteEnemigo()
    {
        enemigosMuertos++;
        ActualizarTexto();

        // Si ya se alcanzó el total, notifica a quien esté escuchando (ej. la pared final)
        if (enemigosMuertos >= totalEnemigos)
        {
            OnTodosLosEnemigosMuertos?.Invoke();
        }
    }

    private void ActualizarTexto()
    {
        if (labelContador != null)
        {
            labelContador.text = $"Drones: {enemigosMuertos} / {totalEnemigos}";
        }
    }

    // Buena práctica: desuscribirse de todos los eventos al destruir este objeto
    void OnDestroy()
    {
        DroneHealth[] drones = FindObjectsByType<DroneHealth>(FindObjectsSortMode.None);
        foreach (DroneHealth dron in drones)
        {
            if (dron != null) dron.OnMuerte -= RegistrarMuerteEnemigo;
        }
    }
}
