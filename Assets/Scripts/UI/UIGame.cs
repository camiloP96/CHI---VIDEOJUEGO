using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    private Button playButton;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        // El nombre debe coincidir con el "name" que le pusiste en el UXML
        playButton = root.Q<Button>("btn_start");

        playButton.clicked += OnPlayButtonClicked;
    }

    private void OnDisable()
    {
        // Importante desuscribirse para evitar errores al destruir el objeto
        playButton.clicked -= OnPlayButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("TestScene");
    }
}