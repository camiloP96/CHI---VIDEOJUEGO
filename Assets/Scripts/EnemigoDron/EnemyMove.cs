using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyDroneMove : MonoBehaviour
{
    [Header("Referencias")]
    private GameObject jugador;
    private NavMeshAgent navEnemigo;
    private Animator animator;

    [Header("Detección")]
    public Collider rangoColision;
    public bool enRango = false;
    private bool enemigoMuerto = false;

    [Header("Distancias de combate")]
    public float distanciaMinima = 6f;   // si el jugador está más cerca que esto, el dron huye
    public float distanciaOptima = 10f;  // distancia ideal para disparar
    public float distanciaMaxima = 15f;  // si el jugador está más lejos, el dron se acerca

    [Header("Movimiento")]
    public float velocidadEnemigo = 8f;
    public float intervaloReposicion = 2f; // cada cuánto busca un nuevo punto de vantage
    private float temporizadorReposicion;

    [Header("Line of Sight")]
    public LayerMask capaObstaculos; // capas que bloquean la visión (paredes, etc.)
    private bool tieneLineaDeVision;

    // Propiedades públicas para que DroneAttack.cs consulte el estado del movimiento
    public bool TieneLineaDeVision => tieneLineaDeVision;
    public bool EstaHuyendo { get; private set; }

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        navEnemigo = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (jugador == null) Debug.LogWarning($"{name}: no se encontró un objeto con tag 'Player'.");
        if (navEnemigo == null) Debug.LogWarning($"{name}: falta el componente NavMeshAgent.");

        navEnemigo.speed = velocidadEnemigo;
    }

    void Update()
    {
        if (!enRango || enemigoMuerto || jugador == null) return;

        ActualizarLineaDeVision();

        Vector3 haciaJugador = jugador.transform.position - transform.position; // dirección correcta
        float distancia = haciaJugador.magnitude;

        temporizadorReposicion -= Time.deltaTime;

        if (distancia < distanciaMinima)
        {
            Huir(haciaJugador);
        }
        else if (distancia > distanciaMaxima)
        {
            Acercarse();
        }
        else if (temporizadorReposicion <= 0f || !tieneLineaDeVision)
        {
            BuscarPuntoDeVantage();
        }
        else
        {
            // buena distancia y línea de visión: se detiene a disparar
            navEnemigo.isStopped = true;
            EstaHuyendo = false;
        }

        if (animator != null)
            animator.SetFloat("Speed", navEnemigo.velocity.magnitude);
    }

    private void Huir(Vector3 haciaJugador)
    {
        EstaHuyendo = true;
        Vector3 direccionHuida = -haciaJugador.normalized;
        Vector3 destinoHuida = transform.position + direccionHuida * distanciaOptima;

        if (NavMesh.SamplePosition(destinoHuida, out NavMeshHit hit, distanciaOptima, NavMesh.AllAreas))
        {
            navEnemigo.isStopped = false;
            navEnemigo.SetDestination(hit.position);
        }
    }

    private void Acercarse()
    {
        EstaHuyendo = false;
        navEnemigo.isStopped = false;
        navEnemigo.SetDestination(jugador.transform.position);
    }

    private void BuscarPuntoDeVantage()
    {
        EstaHuyendo = false;
        temporizadorReposicion = intervaloReposicion;

        for (int intento = 0; intento < 8; intento++)
        {
            float angulo = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angulo), 0f, Mathf.Sin(angulo)) * distanciaOptima;
            Vector3 puntoCandidato = jugador.transform.position + offset;

            if (NavMesh.SamplePosition(puntoCandidato, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (HayLineaDeVisionDesde(hit.position))
                {
                    navEnemigo.isStopped = false;
                    navEnemigo.SetDestination(hit.position);
                    return;
                }
            }
        }
        // si no encontró un punto con visión, al menos se acerca un poco
        Acercarse();
    }

    private void ActualizarLineaDeVision()
    {
        tieneLineaDeVision = HayLineaDeVisionDesde(transform.position);
    }

    private bool HayLineaDeVisionDesde(Vector3 origen)
    {
        if (jugador == null) return false;
        Vector3 destino = jugador.transform.position + Vector3.up * 1f;
        Vector3 dir = destino - origen;
        if (Physics.Raycast(origen, dir.normalized, out RaycastHit hitInfo, dir.magnitude, capaObstaculos))
        {
            return false; // algo bloquea la visión
        }
        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enRango = true;
            navEnemigo.isStopped = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enRango = false;
            navEnemigo.isStopped = true;
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    public void Muerte()
    {
        if (enemigoMuerto) return;
        enemigoMuerto = true;

        if (animator != null) animator.SetTrigger("Dead");
        if (rangoColision != null) rangoColision.enabled = false;
        navEnemigo.isStopped = true;

        Collider colisionHijo = GetComponentInChildren<Collider>();
        if (colisionHijo != null) colisionHijo.enabled = false;
    }
}
