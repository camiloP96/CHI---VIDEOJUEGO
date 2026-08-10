using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

public class UIInGame : MonoBehaviour
{

    [Header("Referencias de vida")]
    public PlayerHealth vidaJugador; // arrastra al jugador en el Inspector

    [Header("referencias al documento XML")]// trae las referencias del documento XML
    UIDocument VidaChi;
    ProgressBar barra; 

    // Start is called before the first frame update
    void OnEnable()
    {
        VidaChi = GetComponent<UIDocument>(); // traer el UI Document
        VisualElement root = VidaChi.rootVisualElement; // traer la raiz del UI Document. 

        barra = root.Q<ProgressBar>("barraVida");// traer el progress bar
        Debug.Log($"barra es {(barra == null ? "NULL" : "encontrada")}");

        if (vidaJugador != null)
        {
            ActualizarBarraJugador(vidaJugador.VidaActual, vidaJugador.vidaMaxima);
        }
    }

    void Start()
    {
        Debug.Log($"vidaJugador es {(vidaJugador == null ? "NULL" : "válido")}");
        // Se suscribe al evento de cambio de vida del jugador (definido en PlayerHealth.cs)
        if (vidaJugador != null)
        {
            vidaJugador.OnVidaCambiada += ActualizarBarraJugador;
            // Llama una vez de entrada para que la barra empiece en el valor correcto (vida completa)
            ActualizarBarraJugador(vidaJugador.VidaActual, vidaJugador.vidaMaxima);
        }
        else
        {
            Debug.LogWarning($"{name}: no se asignó 'vidaJugador' en el Inspector.");
        }

    }

    private void ActualizarBarraJugador(float vidaActual, float vidaMaxima)
    {
        float porcentaje = Mathf.Clamp01(vidaActual / vidaMaxima) * 100f;
        if (barra != null)
        {
            barra.value = porcentaje;
            barra.MarkDirtyRepaint(); 
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
