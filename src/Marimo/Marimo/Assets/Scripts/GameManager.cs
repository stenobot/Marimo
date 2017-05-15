using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    // The UI Text object with the Game Over text
    public GameObject GameOverTextObject;

    // Use this for initialization
    void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /// <summary>
    /// Shows the game over screen
    /// </summary>
    public void GameOver()
    {
        // Show the game over text. It will call RestartScene() from the animator when it's complete
        GameOverTextObject.GetComponent<Text>().enabled = true;
        GameOverTextObject.GetComponent<Animator>().Play(Globals.ANIMSTATE_GAMEOVER);
    }

    /// <summary>
    /// Loads the specified scene
    /// </summary>
    /// <param name="sceneName">The scene to load</param>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Restarts the current scene
    /// </summary>
    public void RestartScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Pauses or unpauses the game
    /// </summary>
    public void Pause()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }
}
