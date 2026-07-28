using System.Collections;
using System.Collections.Generic;
using UnityEngine; // Espacio de nombres principal de Unity: contiene MonoBehaviour, Vector3, Collider, etc.

// [RequireComponent] obliga a que el GameObject tenga un CharacterController.
// Si no lo tiene, Unity lo agrega automáticamente al añadir este script.
[RequireComponent(typeof(CharacterController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Referencias")]
    private CharacterController controller; // Referencia al CharacterController del jugador (para saber orientación/posición)
    private Animator animator;               // Referencia al Animator, para disparar la animación de ataque

    [Header("Punto de ataque")]
    public Transform puntoAtaque;            // Empty GameObject ubicado frente al personaje (ej. a la altura de la mano/arma)
    public float radioAtaque = 1.2f;         // Radio de la esfera que detecta enemigos golpeados
    public LayerMask capaEnemigos;           // Capa(s) que identifican a los enemigos golpeables (se configura en el Inspector)

    [Header("Configuración de combate")]
    public float dañoAtaque = 20f;           // Cantidad de daño que inflige cada golpe
    public float cadenciaAtaque = 0.6f;      // Tiempo mínimo (segundos) entre un ataque y el siguiente
    private float temporizadorAtaque = 0f;   // Cuenta regresiva interna para controlar la cadencia

    [Header("Efectos (opcional)")]
    public ParticleSystem efectoGolpe;       // Partículas opcionales al conectar el golpe
    public AudioSource audioAtaque;          // Sonido opcional al atacar

    // Start se ejecuta una sola vez, al iniciar el objeto en la escena
    void Start()
    {
        controller = GetComponent<CharacterController>(); // Obtiene el CharacterController ya existente en este GameObject
        animator = GetComponent<Animator>();               // Obtiene el Animator (puede ser null si no existe, por eso se valida antes de usarlo)

        // Advertencia en consola si falta el punto de ataque, para detectar el error rápido en desarrollo
        if (puntoAtaque == null)
            Debug.LogWarning($"{name}: no se asignó 'puntoAtaque' en el Inspector.");
    }

    // Update se ejecuta una vez por frame; aquí se revisa el input del jugador
    void Update()
    {
        // Reduce el temporizador de cadencia con el tiempo transcurrido desde el último frame
        if (temporizadorAtaque > 0f)
            temporizadorAtaque -= Time.deltaTime;

        // Input.GetMouseButtonDown(0) detecta el clic IZQUIERDO del mouse (0 = izquierdo, 1 = derecho, 2 = central)
        // Solo se activa en el frame exacto en que se presiona (no se repite si se mantiene presionado)
        if (Input.GetMouseButtonDown(0) && temporizadorAtaque <= 0f)
        {
            RealizarAtaque(); // Ejecuta la lógica del golpe
            temporizadorAtaque = cadenciaAtaque; // Reinicia el temporizador para bloquear ataques hasta que pase la cadencia
        }
    }

    // Método privado que contiene toda la lógica de ejecutar el ataque
    private void RealizarAtaque()
    {
        // Si hay Animator asignado, dispara el trigger "Attack" para reproducir la animación correspondiente
        if (animator != null)
            animator.SetTrigger("Ataque");

        // Si hay un AudioSource asignado, reproduce el sonido de ataque
        if (audioAtaque != null)
            audioAtaque.Play();

        // Determina desde dónde se origina la detección: el punto de ataque si existe, o la posición del jugador como respaldo
        Vector3 origen = puntoAtaque != null ? puntoAtaque.position : transform.position;

        // Physics.OverlapSphere devuelve un arreglo con todos los colliders dentro del radio indicado, filtrados por la capa de enemigos
        Collider[] enemigosGolpeados = Physics.OverlapSphere(origen, radioAtaque, capaEnemigos);

        // Recorre cada collider encontrado dentro del radio de ataque
        foreach (Collider enemigo in enemigosGolpeados)
        {
            // Intenta obtener un componente que implemente la interfaz IDamageable (ver más abajo)
            IDamageable objetivo = enemigo.GetComponent<IDamageable>();

            // Si el objeto encontrado implementa IDamageable, se le aplica daño
            if (objetivo != null)
            {
                objetivo.RecibirDaño(dañoAtaque); // Llama al método de daño definido en el enemigo
            }

            // Si hay efecto de partículas asignado, se reproduce en la posición del enemigo golpeado
            if (efectoGolpe != null)
            {
                // Instantiate crea una copia del sistema de partículas en el punto de impacto
                Instantiate(efectoGolpe, enemigo.transform.position, Quaternion.identity);
            }
        }
    }

    // OnDrawGizmosSelected dibuja una ayuda visual en el Editor (no afecta el juego en tiempo de ejecución)
    // Permite ver el radio de ataque como una esfera amarilla al seleccionar el objeto en la escena
    private void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return; // Si no hay punto de ataque asignado, no dibuja nada

        Gizmos.color = Color.yellow;                       // Define el color del gizmo
        Gizmos.DrawWireSphere(puntoAtaque.position, radioAtaque); // Dibuja una esfera de alambre del radio configurado
    }
}

// Interfaz que cualquier objeto "dañable" (enemigos, objetos destructibles, etc.) debe implementar.
// Usar una interfaz permite que PlayerAttack no necesite saber si golpeó un dron, un zombie u otra cosa:
// solo le importa que el objeto sepa "recibir daño".
public interface IDamageable
{
    void RecibirDaño(float cantidad); // Método que cada clase que implemente esta interfaz debe definir
}
