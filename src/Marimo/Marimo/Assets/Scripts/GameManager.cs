using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    /// <summary>
    /// The UI Text object with the Game Over text
    /// </summary>
    public GameObject GameOverTextObject;

    /// <summary>
    /// Allows or suppresses the hint system according to a user option
    /// TODO: Add user options menu to control this (Issue #66 \m/)
    /// </summary>
    public bool HintsEnabled = true;

    /// <summary>
    /// Contains the set of hints which have already been viewed
    /// </summary>
    public HashSet<string> ViewedHints { get; private set; }

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        ViewedHints = new HashSet<string>();
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

    /// <summary>
    /// Marks the hint as viewed so it can be suppressed on a second view
    /// </summary>
    /// <param name="tool">The <see cref="Enums.ToolIcon"/> active on the <see cref="HintTrigger"/></param>
    /// <param name="interaction">The <see cref="Enums.InteractionIcon"/> active on the <see cref="HintTrigger"</param>
    public void ViewHint(Enums.ToolIcon tool, Enums.InteractionIcon interaction)
    {
        string val = string.Format("{0}:{1}", tool, interaction);
        if (!ViewedHints.Contains(val))
            ViewedHints.Add(val);
    }
}
