using UnityEngine;

/// <summary>
/// Provides access to the current game manager's public methods, for use by objects which need to invoke it from their animators
/// </summary>
public class GameManagerClient : MonoBehaviour
{
    private GameManager m_gameManager;

    /// <summary>
    /// Initialization
    /// </summary>
    void Start()
    {
        m_gameManager = GameObject.FindGameObjectWithTag(Globals.TAG_GAMEMANAGER).GetComponent<GameManager>();
    }

    /// <summary>
    /// Triggers the GameOver function of the game manager. Has no arguments so it can be called from the animator.
    /// </summary>
    public void GameOver()
    {
        m_gameManager.GameOver();
    }

    /// <summary>
    /// Triggers the RestartScene function of the game manager. Has no arguments so it can be called from the animator.
    /// </summary>
    public void RestartScene()
    {
        m_gameManager.RestartScene();
    }

}
