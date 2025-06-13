using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Canvas fadeInCanvas;
    public GameObject panel; // Use GameObject if you want to reference a UI panel prefab or object
    public TMP_Text textIntro;
    public Canvas menuCanvas;
    public Camera mainCamera;
    public static GameManager Instance;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        fadeInCanvas.enabled = true;
        StartCoroutine(TextIntro());
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menuCanvas.enabled)
            {
                menuCanvas.enabled = false;
                Time.timeScale = 1f; // Resume the game
            }
            else
            {
                menuCanvas.enabled = true;
                Time.timeScale = 0f; // Pause the game
            }
        }


    }


    public IEnumerator TextIntro()
    {
        Color c = textIntro.color;
        c.a = 0f;
        textIntro.color = c;
        while (c.a < 1f)
        {
            c.a += Time.deltaTime * 0.5f;
            textIntro.color = c;
            yield return null;
        }

        yield return new WaitForSeconds(4f); // Wait for 2 seconds before starting the fade-in

        yield return new WaitUntil(() => ManaCore.Instance != null);
        mainCamera.transform.position = ManaCore.Instance.transform.position + new Vector3(0f, 0f, -10f);
        StartCoroutine(FadeIn());
        yield break;
    }

    public IEnumerator FadeIn()
    {
        Color p = panel.GetComponent<Image>().color;
        Color c = textIntro.color;
        c.a = 1f;
        p.a = 1f;
        panel.GetComponent<Image>().color = p;
        textIntro.color = c;
        while (p.a > 0f)
        {
            p.a -= Time.deltaTime * 0.2f;
            c.a -= Time.deltaTime;
            panel.GetComponent<Image>().color = p;
            textIntro.color = c;
            yield return null;
        }
        fadeInCanvas.enabled = false;
        yield break;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ResumeGame()
    {
        menuCanvas.enabled = false;
        Time.timeScale = 1f; // Resume the game
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume the game
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void LooseGame()
    {
        Time.timeScale = 1f; // Resume the game
        UnityEngine.SceneManagement.SceneManager.LoadScene("EcranTitre");
    }
}
