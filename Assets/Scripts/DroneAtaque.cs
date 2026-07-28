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
            !movimiento.EstaHuyendo &&
            Vector3.Distance(transform.position, jugador.transform.position) <= alcanceMaximo;

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
                Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);
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
            // Modo proyectil físico: el prefab debe tener su propio script de movimiento y daño
            Instantiate(prefabProyectil, origen, Quaternion.LookRotation(direccion));
        }
        else
        {
            // Modo hitscan: disparo instantáneo por raycast
            if (Physics.Raycast(origen, direccion, out RaycastHit hitInfo, alcanceMaximo, capaImpacto))
            {
                if (hitInfo.collider.CompareTag("Player"))
                {
                    // Aquí se conectaría con el script de vida del jugador, por ejemplo:
                    // hitInfo.collider.GetComponent<PlayerHealth>()?.RecibirDaño(dañoPorDisparo);
                    Debug.Log($"Dron golpeó al jugador por {dañoPorDisparo} de daño.");
                }
            }
        }
    }
}
