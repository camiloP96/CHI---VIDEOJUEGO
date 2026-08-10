using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// El jugador también implementa IDamageable, la misma interfaz que usa DroneHealth.
// Esto es lo que permite que TANTO el ataque del dron (Proyectil.cs) COMO cualquier
// otro enemigo futuro puedan dañar al jugador usando exactamente el mismo método RecibirDaño(),
// sin que esos scripts necesiten saber que existe específicamente un "PlayerHealth".
[RequireComponent(typeof(CharacterController))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    public float vidaMaxima = 100f;
    public float vidaActual;

    [Header("Invulnerabilidad temporal")]
    // Tiempo durante el cual el jugador no puede recibir más daño después de un golpe.
    // Evita que un solo ataque continuo (ej. un proyectil atravesando, o varios enemigos
    // pegados) le quite toda la vida de golpe en un solo frame o segundo.
    public float duracionInvulnerabilidad = 0.5f;
    private float temporizadorInvulnerabilidad = 0f;
    private bool esInvulnerable = false;

    [Header("Estado")]
    private bool estaMuerto = false;

    [Header("Referencias")]
    private Animator animator; // Opcional: para animaciones de "recibir daño" y "morir"
    private CharacterController controller; // Referencia al CharacterController, para desactivarlo al morir

    [Header("Efectos (opcional)")]
    public ParticleSystem efectoDaño;
    public AudioSource audioDaño;
    public AudioSource audioMuerte;

    // Eventos públicos para que la UI (barra de vida, pantalla de muerte, etc.)
    // se entere de los cambios sin que PlayerHealth necesite conocer esa UI directamente.
    public System.Action<float, float> OnVidaCambiada; // (vidaActual, vidaMaxima)
    public System.Action OnMuerte;
    public System.Action OnRecibirDaño; // útil para, por ejemplo, activar un flash rojo en pantalla

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        vidaActual = vidaMaxima; // El jugador inicia con toda su vida 
    }
  
    void Update()
    {
        // Cuenta regresiva de la invulnerabilidad temporal, si está activa
        if (esInvulnerable)
        {
            temporizadorInvulnerabilidad -= Time.deltaTime;
            if (temporizadorInvulnerabilidad <= 0f)
            {
                esInvulnerable = false; // termina el período de invulnerabilidad
            }
        }
    }

    // Implementación requerida por IDamageable. Este es el método que los enemigos
    // (Proyectil.cs del dron, o cualquier otro script futuro) llaman para hacer daño.
    public void RecibirDaño(float cantidad)
    {

        Debug.Log($"[DIAGNOSTICO] RecibirDaño() llamado con {cantidad} de daño. EstaMuerto: {estaMuerto} | EsInvulnerable: {esInvulnerable}");
        // Si ya está muerto o actualmente es invulnerable, ignora el daño entrante
        if (estaMuerto || esInvulnerable) return;

        vidaActual -= cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima); // evita valores negativos

        // Notifica a la UI (o cualquier sistema suscrito) el nuevo valor de vida
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
        OnRecibirDaño?.Invoke();

        // Activa la ventana de invulnerabilidad temporal
        esInvulnerable = true;
        temporizadorInvulnerabilidad = duracionInvulnerabilidad;

        // Reproduce efectos de haber sido golpeado
        if (efectoDaño != null) efectoDaño.Play();
        if (audioDaño != null) audioDaño.Play();
        if (animator != null) animator.SetTrigger("Hit");

        // Si la vida llegó a 0, ejecuta la lógica de muerte
        if (vidaActual <= 0f)
        {
            Morir();
        }
    }

    private void Morir()
    {
        if (estaMuerto) return; // evita ejecutar la muerte más de una vez
        estaMuerto = true;

        OnMuerte?.Invoke(); // notifica a la UI/GameManager para mostrar pantalla de derrota, etc.

        if (animator != null) animator.SetTrigger("Dead");
        if (audioMuerte != null) audioMuerte.Play();

        // Desactiva el CharacterController para que el jugador ya no pueda moverse ni recibir más daño físico
  
        if (controller != null) controller.enabled = false;

        // Aquí normalmente se activaría un GameManager para pausar el juego,
        // mostrar un menú de "Game Over", o reiniciar el nivel después de un delay.
        // Ejemplo (si tuvieras un GameManager con un método público Reiniciar()):
        // GameManager.instancia.MostrarPantallaDerrota();
    }

    // Método público opcional, útil para power-ups de curación o checkpoints
    public void Curar(float cantidad)
    {
        if (estaMuerto) return;

        vidaActual += cantidad;
        vidaActual = Mathf.Clamp(vidaActual, 0f, vidaMaxima);
        OnVidaCambiada?.Invoke(vidaActual, vidaMaxima);
    }

    // Propiedades públicas de solo lectura, por si otros scripts necesitan consultar el estado
    public float VidaActual => vidaActual;
    public bool EstaMuerto => estaMuerto;
}