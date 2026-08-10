using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [RequireComponent] asegura que el dron siempre tenga su script de movimiento asociado.
// Si no lo tiene, Unity lo agrega automáticamente al añadir DroneHealth.
[RequireComponent(typeof(EnemyDroneMove))]
public class DroneHealth : MonoBehaviour, IDamageable
// La interfaz IDamageable (definida en PlayerAttack.cs) obliga a implementar RecibirDaño().
// Gracias a esto, PlayerAttack no necesita saber que existe "DroneHealth": solo sabe que
// cualquier objeto con IDamageable puede recibir daño.
{
    [Header("Vida")]
    public float vidaMaxima = 10f;     // Vida total del dron, configurable en el Inspector
    public float vidaActual;           // Vida restante en tiempo de ejecución

    [Header("Referencias")]
    private EnemyDroneMove movimiento;  // Referencia al script de movimiento, para llamar a Muerte()
    private Animator animator;          // Opcional: para animaciones de "recibir daño" (hit reaction)

    [Header("Efectos (opcional)")]
    public ParticleSystem efectoImpacto; // Partículas al recibir daño (chispas, humo, etc.)
    public AudioSource audioImpacto;     // Sonido al recibir daño

    // Evento público opcional: otros scripts (UI de vida, sistema de puntaje, etc.)
    // pueden suscribirse a esto sin que DroneHealth necesite conocerlos directamente.
    public System.Action<float, float> OnVidaCambiada; // parámetros: (vidaActual, vidaMaxima)
    public System.Action OnMuerte;

    void Start()
    {
        movimiento = GetComponent<EnemyDroneMove>(); // Obtiene el script de movimiento ya existente
        animator = GetComponent<Animator>();          // Puede ser null si el dron no tiene Animator directo

        vidaActual = vidaMaxima; // Al iniciar, el dron comienza con toda su vida
    }
    public float VidaActual => vidaActual;

    // Implementación requerida por la interfaz IDamageable.
    // Este es el método que PlayerAttack.cs llama cuando el martillo golpea al dron.
    public void RecibirDaño(float cantidad)
    {
        if (vidaActual <= 0f) return; // Si ya está muerto, ignora daño adicional (evita doble muerte)
        Debug.Log(vidaActual); 
        vidaActual -= cantidad; // Resta el daño recibido a la vida actual
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima); // Evita valores negativos

        // Notifica a quien esté escuchando (ej. una barra de vida en UI) el nuevo valor
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);

        // Reproduce efectos de impacto si están asignados
        if (efectoImpacto != null) efectoImpacto.Play();
        if (audioImpacto != null) audioImpacto.Play();

        // Si tiene Animator, dispara una reacción de golpe (opcional, requiere el trigger "Hit" en el Animator)
        if (animator != null) animator.SetTrigger("Hit");

        // Si la vida llegó a 0, ejecuta la lógica de muerte
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    // Lógica centralizada de muerte, separada de RecibirDaño para mayor claridad
    private void Morir()
    {
        OnMuerte?.Invoke(); // Notifica a otros sistemas (ej. contador de enemigos derrotados)

        // Llama al método Muerte() ya existente en EnemyDroneMove, que se encarga de:
        // desactivar colliders, detener el NavMeshAgent y disparar la animación de muerte
        if (movimiento != null)
        {
            movimiento.Muerte();
        }

        // Opcional: destruir el GameObject después de un tiempo, para dar tiempo a que
        // termine la animación de muerte antes de eliminarlo de la escena
        Destroy(gameObject, 3f);
    }
}