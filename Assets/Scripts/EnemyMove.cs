using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{

    private GameObject jugador;
    private NavMeshAgent navEnemigo;
    public Collider rangoColision;

    private bool enRango = false;
    private bool enemigoMuerto = false;
    private bool objtevioCerca;

    private float acercarse = 6f;
    public float velocidadEnemigo = 6f;
    public float velocidadOriginal;

    private Animator animator;
   // public AudioSource AudioSource;
    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player");
        navEnemigo = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        //AudioSource.enabled = false;
        velocidadOriginal = navEnemigo.speed;
    }

    /*
     
    void Update()
{
    if(enRango && !enemigoMuerto)
    {
        navEnemigo.SetDestination(jugador.transform.position);

        float distancia = Vector3.Distance(transform.position, jugador.transform.position);
        float velocidad = Mathf.Clamp(velocidadOriginal, 0f, velocidadEnemigo);

        navEnemigo.speed = velocidad;

        animator.SetFloat("Speed", navEnemigo.speed);
    }
}
      
     
     */

    void Update()
    {
        if (enRango && !enemigoMuerto)
        {
            Vector3 enemDireccion = transform.position + jugador.transform.position;
            Vector3 enemDestino = transform.position + enemDireccion.normalized * acercarse;
            navEnemigo.SetDestination(enemDestino);
            float distancia = enemDireccion.magnitude;
            float velocidad = Mathf.Clamp(velocidadOriginal * (acercarse / distancia), 0f, velocidadEnemigo);
            navEnemigo.speed = velocidad;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //AudioSource.enabled = true;
            animator.SetFloat("Speed", navEnemigo.speed);
            enRango = true;
            navEnemigo.isStopped = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
           // AudioSource.enabled = false;
            navEnemigo.speed = velocidadOriginal;
            animator.SetFloat("Speed", 0f);
            enRango = false;
            navEnemigo.isStopped = true;
        }
    }

    public void Muerte()
    {
        if (!enemigoMuerto)
        {
            enemigoMuerto = true;
            animator.SetTrigger("Dead");
            rangoColision.enabled = false;
            navEnemigo.isStopped = true;
            // AudioSource.enabled = false;
        }

        Collider colisionHijo = GetComponentInChildren<Collider>();
        if (colisionHijo != null)
        {
            colisionHijo.enabled = false;
        }
    }


}
    