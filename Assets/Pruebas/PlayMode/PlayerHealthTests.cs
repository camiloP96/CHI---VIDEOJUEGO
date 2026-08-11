using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// Las pruebas de PlayMode heredan comportamiento especial: usan [UnityTest] en vez de [Test]
// cuando necesitan esperar frames (por ejemplo, para simular el paso del tiempo).
public class PlayerHealthTests
{
    // [UnityTest] permite usar "yield return" para esperar frames/tiempo real dentro de la prueba,
    // a diferencia de [Test], que se ejecuta de forma instantánea y sincrónica.
    [UnityTest]
    public IEnumerator RecibirDaño_ReduceVidaActual()
    {
        // ARRANGE: prepara el escenario de prueba.
        // Se crea un GameObject temporal con los componentes mínimos necesarios
        // para que PlayerHealth funcione (requiere CharacterController por el [RequireComponent]).
        GameObject jugador = new GameObject("JugadorDePrueba");
        jugador.AddComponent<CharacterController>();
        PlayerHealth vida = jugador.AddComponent<PlayerHealth>();
        vida.vidaMaxima = 100f;

        // Se espera un frame para que Start() del componente se ejecute correctamente,
        // ya que Unity no llama a Start() de forma inmediata al usar AddComponent en tiempo de ejecución.
        yield return null;

        // ACT: ejecuta la acción que se quiere probar.
        vida.RecibirDaño(30f);

        // ASSERT: verifica que el resultado sea el esperado.
        // Se espera que la vida haya bajado exactamente en la cantidad de daño aplicado.
        Assert.AreEqual(70f, vida.VidaActual, "La vida debería reducirse en la cantidad exacta de daño recibido.");

        // Limpieza: destruye el objeto de prueba para no dejar residuos entre pruebas
        Object.Destroy(jugador);
    }

    [UnityTest]
    public IEnumerator RecibirDaño_DuranteInvulnerabilidad_NoReduceVidaDeNuevo()
    {
        GameObject jugador = new GameObject("JugadorDePrueba");
        jugador.AddComponent<CharacterController>();
        PlayerHealth vida = jugador.AddComponent<PlayerHealth>();
        vida.vidaMaxima = 100f;
        vida.duracionInvulnerabilidad = 1f; // ventana de invulnerabilidad amplia para la prueba

        yield return null;

        // Primer golpe: sí debe aplicar daño y activar la invulnerabilidad temporal
        vida.RecibirDaño(20f);

        // Segundo golpe INMEDIATO: como todavía está dentro de la ventana de invulnerabilidad,
        // este golpe debería ser ignorado por completo.
        vida.RecibirDaño(20f);

        // Se espera que la vida solo haya bajado UNA vez (20), no dos veces (40),
        // confirmando que la invulnerabilidad temporal funciona como se diseñó.
        Assert.AreEqual(80f, vida.VidaActual, "El segundo golpe durante la invulnerabilidad no debería aplicar daño.");

        Object.Destroy(jugador);
    }

    [UnityTest]
    public IEnumerator RecibirDaño_VidaLlegaACero_MarcaEstaMuertoComoTrue()
    {
        GameObject jugador = new GameObject("JugadorDePrueba");
        jugador.AddComponent<CharacterController>();
        PlayerHealth vida = jugador.AddComponent<PlayerHealth>();
        vida.vidaMaxima = 10f;

        yield return null;

        // Se aplica daño suficiente para agotar toda la vida de una sola vez
        vida.RecibirDaño(15f);

        // Confirma que el estado de muerte se activó correctamente cuando la vida llega a 0
        Assert.IsTrue(vida.EstaMuerto, "El jugador debería estar marcado como muerto tras recibir daño letal.");

        Object.Destroy(jugador);
    }
}
