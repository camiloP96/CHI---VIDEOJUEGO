using UnityEngine;

// Este script va en el GameObject "FinalPlane" ya existente en la escena.
// Requiere que ese objeto tenga un Collider marcado como Trigger.
public class FinalWallController : MonoBehaviour
{
    [Header("Referencias")]
    public EnemyCounterController contadorEnemigos; // arrastra el objeto que tiene este script en el Inspector

    // Evita que la victoria se dispare más de una vez si el jugador toca la pared repetidamente
    private bool victoriaActivada = false;

    // Evento público: el script de la pantalla de victoria se suscribe a esto,
    // siguiendo el mismo patrón que ya usamos con OnMuerte y OnTodosLosEnemigosMuertos.
    public System.Action OnVictoria;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return; // solo reacciona al jugador, ignora cualquier otra cosa
        if (victoriaActivada) return; // ya se activó antes, no hace nada de nuevo

        if (contadorEnemigos == null)
        {
            Debug.LogWarning($"{name}: no se asignó 'contadorEnemigos' en el Inspector.");
            return;
        }

        // La condición central: solo se considera victoria si TODOS los drones ya murieron.
        // Usamos la propiedad pública NivelCompletado que ya dejamos preparada en EnemyCounterController.
        if (contadorEnemigos.NivelCompletado)
        {
            victoriaActivada = true;
            OnVictoria?.Invoke();
        }
        else
        {
            // Opcional: feedback en consola si el jugador toca la pared antes de tiempo.
            // Podrías reemplazar esto por un mensaje en pantalla más adelante si quieres.
            Debug.Log("Todavía quedan drones con vida. Elimínalos antes de tocar la pared.");
        }
    }
}