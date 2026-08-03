using UnityEngine;

// Este script va sobre el PREFAB del proyectil ("disparo"), no sobre el dron.
public class Proyectil : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 20f;           // Qué tan rápido viaja el proyectil
    public float tiempoDeVida = 5f;         // Segundos antes de autodestruirse si no golpea nada

    [Header("Daño")]
    public float daño = 10f;                // Daño que aplica al impactar al jugador
    public LayerMask capaImpacto;           // Qué capas puede golpear (Player, paredes, etc.)

    [Header("Efectos (opcional)")]
    public GameObject efectoImpactoPrefab;  // Partículas o explosión al chocar (opcional)

    // Guarda la dirección de movimiento, asignada justo después de instanciar el proyectil
    private Vector3 direccion;

    // Bandera para saber si Inicializar() fue realmente llamado desde DroneAttack
    private bool inicializado = false;

    void Awake()
    {
        // Si el prefab tiene un Rigidbody (necesario para que OnTriggerEnter funcione),
        // lo forzamos a modo kinemático para que NO controle la física del objeto.
        // Así evitamos que el motor de física sobreescriba el movimiento manual
        // que hacemos con transform.Translate() en Update(), que es la causa más común
        // de que un proyectil "se quede quieto en el aire" a pesar de tener código de movimiento.
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // desactiva la simulación física, pero mantiene la detección de colisión
            rb.useGravity = false; // evita que caiga por gravedad si por alguna razón se activara
        }
    }

    // Método público para que DroneAttack le indique hacia dónde debe volar
    public void Inicializar(Vector3 direccionDisparo)
    {
        Debug.Log($"Inicializar() SÍ fue llamado. Dirección recibida: {direccionDisparo}"); // LOG TEMPORAL DE DIAGNÓSTICO
        direccion = direccionDisparo.normalized;
        inicializado = true;
    }

    void Start()
    {
        // Autodestrucción por tiempo: si no golpea nada, desaparece solo después de X segundos.
        // Esto evita que proyectiles perdidos (que fallaron el disparo) queden acumulándose en el mapa.
        Destroy(gameObject, tiempoDeVida);
    }

    void Update()
    {
        // Si por alguna razón Inicializar() nunca fue llamado (ej. DroneAttack no encontró
        // el componente Proyectil, o hay un error en la cadena de instanciación),
        // direccion se queda en Vector3.zero y el proyectil no se movería nunca.
        // Esta advertencia ayuda a detectar ese caso específico en la consola.
        if (!inicializado)
        {
            Debug.LogWarning($"{name}: el proyectil nunca fue inicializado con una dirección (Inicializar() no fue llamado).");
            return; // evita mover el objeto con dirección (0,0,0), que no haría nada de todos modos
        }

        // Mueve el proyectil en línea recta cada frame, en la dirección asignada.
        // Transform.Translate con Space.World asegura que se mueva en coordenadas globales,
        // sin importar la rotación del propio proyectil.
        transform.Translate(direccion * velocidad * Time.deltaTime, Space.World);
    }

    // Se ejecuta automáticamente cuando el Collider de este proyectil
    // (debe ser Trigger) toca otro Collider en la escena.
    private void OnTriggerEnter(Collider other)
    {
        // Filtra: solo reacciona a colliders que estén dentro de la capaImpacto configurada.
        // El operador de bits compara si la capa del objeto golpeado está incluida en la máscara.
        if (((1 << other.gameObject.layer) & capaImpacto) == 0)
        {
            return; // Si no es una capa relevante (ej. otro dron, un trigger de detección), lo ignora
        }

        // Si golpeó al jugador específicamente, le aplica daño
        if (other.CompareTag("Player"))
        {
            IDamageable objetivo = other.GetComponent<IDamageable>();
            if (objetivo != null)
            {
                objetivo.RecibirDaño(daño);
            }
        }

        // Instancia el efecto de impacto (chispas/explosión) en el punto de colisión, si está asignado
        if (efectoImpactoPrefab != null)
        {
            Instantiate(efectoImpactoPrefab, transform.position, Quaternion.identity);
        }

        // Destruye el proyectil al impactar, sin importar qué golpeó (jugador o pared),
        // para que no atraviese obstáculos indefinidamente.
        Destroy(gameObject);
    }
}