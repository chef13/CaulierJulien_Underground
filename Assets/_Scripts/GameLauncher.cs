using UnityEngine;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviour
{
    public Button startButton;
    public Button exitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void StartGame()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
    public void ExitGame()
    {
        // Exit the application
        Application.Quit();
    }
}
