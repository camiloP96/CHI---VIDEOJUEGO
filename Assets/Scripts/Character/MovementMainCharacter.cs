using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementMainCharacter : MonoBehaviour
{
    
    // VARIABLES PÚBLICAS (editables en Inspector)
    

    [Header("Velocidades de movimiento")]
    public float velocidadMovimiento = 3f;
    public float velocidadCorrer = 5f;

    [Header("Física")]
    public float gravedad = -9.81f;
    public float altitudSalto = 5f;

    [Header("Dash")]
    public float velocidadDash = 20f;
    public float tiempoDash = 0.25f;
    public float cooldownDash = 1f;

    
    // VARIABLES PRIVADAS (uso interno)

    private bool isDashing = false;  // true mientras el dash está activo
    private bool canDash = true;     // false durante el cooldown

    private CharacterController controller;
    private Vector3 velocity;        // almacena la velocidad acumulada (especialmente la vertical)
    private Animator animator;
    private PlayerHealth vida;

    // START
    void Start()
    {
        // Obtenemos los componentes del mismo GameObject
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        vida = GetComponent<PlayerHealth>();

    }
    //// corta el dash (u otras corrutinas) inmediatamente, sin esperar al siguiente frame
    void DetenerMovimientoPorMuerte()
    {
        StopAllCoroutines();
        isDashing = false;
    }
    // desuscribirse al destruir el objeto, para evitar referencias colgante
    void OnDestroy()
    {
        if (vida != null)
        {
            vida.OnMuerte -= DetenerMovimientoPorMuerte;
        }
    }

    // UPDATE — se ejecuta cada frame
    void Update()
    {

        if (vida != null && vida.EstaMuerto) return;
        {
            // Actualizamos animaciones siempre y antes  del return para que la animación de dash funcione
            ActualizarAnimaciones();

            // Si estamos dasheando, bloqueamos todo el movimiento normal
            if (isDashing) return;

            // MOVIMIENTO HORIZONTAL 

            // Si se mantiene Fire3, usamos velocidad de correr; si no, la normal
            float velocidadActual = Input.GetButton("Fire3") ? velocidadCorrer : velocidadMovimiento;

            // GetAxisRaw devuelve -1, 0 o 1 sin suavizado (respuesta inmediata)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            // Construimos el vector de movimiento relativo a la orientación del personaje
            Vector3 move = transform.right * moveX + transform.forward * moveZ;

            // Normalizamos para evitar que la diagonal sea más rápida
            if (move.magnitude > 1f) move.Normalize();

            controller.Move(move * velocidadActual * Time.deltaTime);

            // GRAVEDAD 

            // Si estamos en el suelo, reseteamos la velocidad vertical
            // (usamos -2 en vez de 0 para mantener al personaje pegado al suelo)
            if (controller.isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            // Acumulamos gravedad frame a frame (simula caída libre)
            velocity.y += gravedad * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // SALTO 

            // Solo puede saltar si está en el suelo
            if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded)
            {
                // Fórmula física: v = sqrt(h * -2 * g)
                velocity.y = Mathf.Sqrt(altitudSalto * -5f * gravedad);
            }

            //  DASH 

            // Solo puede dashear si: pulsa E, el cooldown terminó, y se está moviendo
            if (Input.GetKeyDown(KeyCode.E) && canDash && move.magnitude > 0.1f)
            {
                StartCoroutine(DashMechanic(move));
            }
        }
    }

    // ANIMACIONES — método separado para mayor claridad
    // Se llama desde Update() siempre, antes del return del dash
    
    void ActualizarAnimaciones()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        bool estaMoviendo = Mathf.Abs(moveX) > 0.1f || Mathf.Abs(moveZ) > 0.1f;
        bool estaCorriendo = Input.GetButton("Fire3") && estaMoviendo; // Shift + movimiento

        if (isDashing)
        {
            // Estado: DASH — tiene prioridad sobre todo lo demás
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("Dash", true);
        }
        else if (estaCorriendo)
        {
            // Estado: CORRIENDO (Shift + movimiento)
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsRunning", true);
            animator.SetBool("Dash", false);
        }
        else if (estaMoviendo)
        {
            // Estado: CAMINANDO
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("Dash", false);
        }
        else
        {
            // Estado: IDLE (sin input de movimiento)
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
            animator.SetBool("IsRunning", false);
            animator.SetBool("Dash", false);
        }
    }

    // CORRUTINA DEL DASH
 
    IEnumerator DashMechanic(Vector3 dashDirection)
    {
        isDashing = true;  // Bloqueamos el movimiento normal en Update
        canDash = false; // Iniciamos el cooldown

        // Guardamos la velocidad vertical para restaurarla después
        float originalGravity = velocity.y;
        velocity.y = 0; // Sin gravedad durante el dash (movimiento recto)

        float startTime = Time.time;

        // Se agrega la condición "controller.enabled" al while:
        // si el jugador muere a mitad del dash, el bucle se corta inmediatamente
        while (Time.time < startTime + tiempoDash && controller.enabled)
        {
            // Cada frame del dash empujamos al personaje en la dirección guardada
            controller.Move(dashDirection.normalized * velocidadDash * Time.deltaTime);
            yield return null; // Esperamos al siguiente frame y continuamos el bucle
        }

        // Fin del dash 
        isDashing = false;         // Devolvemos el control al jugador
        velocity.y = originalGravity; // Restauramos la gravedad
                                      // Si murió durante el dash, no tiene sentido iniciar el cooldown normal
        if (!controller.enabled) yield break;

        // Cooldown 
        // yield return new WaitForSeconds pausa la corrutina sin congelar el juego
        yield return new WaitForSeconds(cooldownDash);
        canDash = true; // Ya se puede volver a dashear
    }
}


