using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("GUI")]
    [SerializeField] private GameObject uiFadeOut;
    
    [Space]

    [Header("Events")]
    [SerializeField] private UnityEvent OnStart;
    [SerializeField] private UnityEvent OnQuit;

    [HideInInspector] public bool isQuitting;
    [HideInInspector] public bool isLoadingScene;

    private void Start()
    {
        OnStart.Invoke();

        uiFadeOut.SetActive(false);
    }

    private void Update()
    {
        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Delete)) && !isQuitting && !isLoadingScene)
        {
            Time.timeScale = 1.0f;

            QuitGame();
        }
    }

    public void StartGame(bool isSolo)
    {
        if (!isLoadingScene && !isQuitting)
        {
            Time.timeScale = 1.0f;

            uiFadeOut.SetActive(true);

            StartCoroutine(CR_StartGame());

            Settings.isSolo = isSolo;

            isLoadingScene = true;
        }
    }

    public IEnumerator CR_StartGame()
    {
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(1);
    }

    public void QuitGame()
    {
        if (!isQuitting)
        {
            OnQuit.Invoke();

            StartCoroutine(CR_QuitGame());     // Quit Game shortcut : Lshift + Del

            isQuitting = true;
        }
    }

    public IEnumerator CR_QuitGame()
    {
        uiFadeOut.SetActive(true);

        yield return new WaitForSeconds(1.0f);

        Debug.Log("Quit");

        Application.Quit();
    }
}
