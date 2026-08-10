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

    // Referencia al Rigidbody que se usa para mover el proyectil de forma compatible con la física.
    private Rigidbody rb;

    void Awake()
    {
        // Se agrega un Rigidbody por código si el prefab no tiene uno ya asignado manualmente.
        // Esto es OBLIGATORIO para que la física de Unity detecte correctamente el movimiento
        // del Collider del proyectil: mover un Collider únicamente con Transform.Translate(),
        // sin Rigidbody, puede hacer que el motor de física nunca registre la nueva posición
        // a tiempo para generar OnTriggerEnter, dependiendo de la configuración del proyecto.
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Kinematic = true: el Rigidbody NO reacciona a fuerzas externas (gravedad, empujones),
        // pero SÍ permite que Unity detecte correctamente los triggers cuando lo movemos
        // manualmente con MovePosition(), que es la forma recomendada de mover un objeto
        // "a mano" sin que se comporte como un objeto físico normal.
        rb.isKinematic = true;
        rb.useGravity = false;

        // Collision Detection en modo Continuous ayuda a evitar que, a alta velocidad,
        // el proyectil "atraviese" al jugador sin detectar la colisión entre un frame y otro.
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    // Método público para que DroneAttack le indique hacia dónde debe volar
    public void Inicializar(Vector3 direccionDisparo)
    {
        direccion = direccionDisparo.normalized;
        inicializado = true;
    }

    void Start()
    {
        // Autodestrucción por tiempo: si no golpea nada, desaparece solo después de X segundos.
        // Esto evita que proyectiles perdidos (que fallaron el disparo) queden acumulándose en el mapa.
        Destroy(gameObject, tiempoDeVida);
    }

    void FixedUpdate()
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

        // Rigidbody.MovePosition mueve el objeto de forma "física", garantizando que el motor
        // de colisiones registre correctamente la nueva posición del Collider en cada paso.
        // Se usa FixedUpdate() en vez de Update() porque es el ciclo correcto para todo
        // movimiento relacionado con física en Unity (sincronizado con el motor de físicas).
        Vector3 nuevaPosicion = rb.position + direccion * velocidad * Time.fixedDeltaTime;
        rb.MovePosition(nuevaPosicion);
    }

    // Se ejecuta automáticamente cuando el Collider de este proyectil
    // (debe ser Trigger) toca otro Collider en la escena.
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DIAGNOSTICO] Proyectil tocó: {other.gameObject.name} | Layer: {LayerMask.LayerToName(other.gameObject.layer)} | Tag: {other.tag}");

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
            else
            {
                Debug.LogWarning("El objeto con tag 'Player' no tiene un componente IDamageable (ej. PlayerHealth).");
            }
        }

        // Instancia el efecto de impacto (chispas/explosión) en el punto de colisión, si está asignado
        if (efectoImpactoPrefab != null)
        {
            Instantiate(efectoImpactoPrefab, transform.position, Quaternion.identity);
        }

        Debug.Log($"[DIAGNOSTICO] Proyectil se destruye por colisión con: {other.gameObject.name}");

        // Destruye el proyectil al impactar, sin importar qué golpeó (jugador o pared),
        // para que no atraviese obstáculos indefinidamente.
        Destroy(gameObject);
    }
}