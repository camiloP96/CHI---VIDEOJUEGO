using UnityEngine;

[RequireComponent(typeof(EnemyDroneMove))]
public class DroneAttack : MonoBehaviour
{
    [Header("Referencias")]
    public Transform puntoDisparo;        // Empty GameObject ubicado en el "cañón" del dron
    public GameObject prefabProyectil;    // Opcional: si se deja vacío, se dispara en modo hitscan
    private EnemyDroneMove movimiento;
    private Animator animator;
    private GameObject jugador;

    [Header("Configuración de disparo")]
    public float dañoPorDisparo = 10f;
    public float cadenciaDisparo = 1.5f;  // segundos entre disparos
    public float alcanceMaximo = 15f;
    public LayerMask capaImpacto;         // capas que puede golpear el disparo (Player, paredes, etc.)
    private float temporizadorDisparo;

    [Header("Efectos (opcional)")]
    public ParticleSystem efectoDisparo;
    public AudioSource audioDisparo;

    [Header("Corrección de orientación del modelo")]
    // Algunos modelos importados (especialmente desde Blender/FBX) tienen su "frente" visual
    // desalineado respecto al eje Z (forward) que usa Unity internamente.
    // Si el dron parece darte la espalda al apuntarte, ajusta este valor a 180.
    // Si mira de lado, prueba con 90 o -90 hasta que el cañón quede alineado hacia el jugador.
    public float offsetRotacionY = 0f;

    void Start()
    {
        movimiento = GetComponent<EnemyDroneMove>();
        animator = GetComponent<Animator>();
        jugador = GameObject.FindGameObjectWithTag("Player");

        if (jugador == null) Debug.LogWarning($"{name}: no se encontró un objeto con tag 'Player'.");
    }

    void Update()
    {
        if (jugador == null) return;

        temporizadorDisparo -= Time.deltaTime;

        bool puedeDisparar =
            movimiento.TieneLineaDeVision &&
            //!movimiento.EstaHuyendo &&
            Vector3.Distance(transform.position, jugador.transform.position) <= alcanceMaximo;

       /* Debug.Log($"LOS: {movimiento.TieneLineaDeVision} | Huyendo: {movimiento.EstaHuyendo} | " +
              $"Distancia: {Vector3.Distance(transform.position, jugador.transform.position):F1} | " +
              $"PuedeDisparar: {puedeDisparar} | Timer: {temporizadorDisparo:F2}");
       */

        if (puedeDisparar && temporizadorDisparo <= 0f)
        {
            Disparar();
            temporizadorDisparo = cadenciaDisparo;
        }

        // Orienta el dron hacia el jugador mientras lo tenga a la vista
        if (movimiento.TieneLineaDeVision)
        {
            Vector3 direccion = jugador.transform.position - transform.position;
            direccion.y = 0f;
            if (direccion.sqrMagnitude > 0.01f)
            {
                // Se calcula la rotación "natural" hacia el jugador, y luego se le suma
                // el offset de corrección para compensar la orientación del modelo importado.
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion) * Quaternion.Euler(0f, offsetRotacionY, 0f);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * 5f);
            }
        }
    }

    private void Disparar()
    {
        if (animator != null) animator.SetTrigger("Shoot");
        if (efectoDisparo != null) efectoDisparo.Play();
        if (audioDisparo != null) audioDisparo.Play();

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        Vector3 destino = jugador.transform.position + Vector3.up * 1f;
        Vector3 direccion = (destino - origen).normalized;

        if (prefabProyectil != null)
        {
            // Modo proyectil físico: se instancia y se le indica la dirección hacia la que debe volar
            GameObject instancia = Instantiate(prefabProyectil, origen, Quaternion.LookRotation(direccion));
            // Busca el script Proyectil.cs en el objeto recién creado y le pasa la dirección
            Proyectil scriptProyectil = instancia.GetComponent<Proyectil>();
            if (scriptProyectil != null)
            {
                scriptProyectil.Inicializar(direccion);
            }
            else
            {
                Debug.LogWarning($"{name}: el prefab '{prefabProyectil.name}' no tiene el script Proyectil.cs asignado.");
            }
        }
        else
        {
            //modo hitscan: disparo instantaneo por raycast
            if (Physics.Raycast(origen,direccion,out RaycastHit hitInfo, alcanceMaximo, capaImpacto))
            {
                if (hitInfo.collider.CompareTag("Player"))
                {
                    IDamageable objetivoJugador = hitInfo.collider.GetComponent<IDamageable>();
                    objetivoJugador?.RecibirDaño(dañoPorDisparo);
                }
            }
        }
    }
}
