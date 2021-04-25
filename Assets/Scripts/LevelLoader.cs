using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private Animator transition;
    [SerializeField] private float transitionTime;

    [SerializeField] private MusicController _mc;

    [SerializeField] private GameObject startText;

    [SerializeField] private GameObject lastScore;
    [SerializeField] private GameObject highScore;

    private bool loading;

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            if (TreasureController.score > PlayerPrefs.GetInt("HighScore", 0))
            {
                PlayerPrefs.SetInt("HighScore", TreasureController.score);
            }
            highScore.GetComponent<Text>().text = PlayerPrefs.GetInt("HighScore", 0).ToString();
            lastScore.GetComponent<Text>().text = TreasureController.score.ToString();
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D) && SceneManager.GetActiveScene().buildIndex == 0 && !loading)
        {
            StartCoroutine(_mc.FadeOut(1f));

            SFXController.instance.Play(SFX.START, 1f);

            startText.GetComponent<Animator>().SetTrigger("start");

            StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex + 1, 2f));
        }
    }

    private IEnumerator LoadScene(int levelIndex, float initDelay)
    {
        loading = true;
        yield return new WaitForSeconds(initDelay);

        transition.SetTrigger("start");

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(levelIndex);
    }

    public void PlayerDied()
    {
        StartCoroutine(_mc.FadeOut(3f));
        StartCoroutine(LoadScene(0, 4f));
    }
}

    //IEnumerator LoadYourAsyncScene()
    //{
    //    // The Application loads the Scene in the background as the current Scene runs.
    //    // This is particularly good for creating loading screens.
    //    // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
    //    // a sceneBuildIndex of 1 as shown in Build Settings.

    //    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Scene2");

    //    // Wait until the asynchronous scene fully loads
    //    while (!asyncLoad.isDone)
    //    {
    //        yield return null;
    //    }
    //}
