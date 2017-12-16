using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Used for game management, including scene management, global states, and character selection
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Public editor variables

    /// <summary>
    /// The UI Text object with the Game Over text
    /// </summary>
    public GameObject GameOverTextObject;

    /// <summary>
    /// Allows or suppresses the hint system according to a user option
    /// TODO: Add user options menu to control this (Issue #66 \m/)
    /// </summary>
    public bool HintsEnabled = true;

    #endregion

    #region Public script-only variables

    /// <summary>
    /// Contains the set of hints which have already been viewed
    /// </summary>
    public HashSet<string> ViewedHints { get; private set; }

    /// <summary>
    /// Holds a reference to the robot's controller which other scripts can use to retrieve it
    /// </summary>
    public RobotController Robot { get; private set; }

    /// <summary>
    /// Holds a reference to muckle's controller which other scripts can use to retrieve it
    /// </summary>
    public MuckleController Muckle { get; private set; }

    /// <summary>
    /// Holds the pause state
    /// </summary>
    public bool IsPaused { get; private set; }
    #endregion

    #region Private variables

    // Gets or sets the active character
	private Enums.PlayableCharacter m_activeCharacter;

    #endregion

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        Robot = GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER) != null
            ? GameObject.FindGameObjectWithTag(Globals.TAG_PLAYER).GetComponent<RobotController>() :
            null;
        Muckle = GameObject.FindGameObjectWithTag(Globals.TAG_MUCKLE) != null ?
            GameObject.FindGameObjectWithTag(Globals.TAG_MUCKLE).GetComponent<MuckleController>() :
            null;
        ViewedHints = new HashSet<string>();
        // Default to selecting the robot as the playable character
        SelectRobot();
        // Set programmatic sorting layers to meshes
        SetMeshSortingLayers();
    }

    private void SetMeshSortingLayers()
    {
        // Set sorting layer on all pipes
        List<GameObject> pipes = GameObject.FindGameObjectsWithTag(Globals.TAG_PIPES).ToList();
        foreach (GameObject pipe in pipes)
            pipe.GetComponent<Renderer>().sortingLayerName = Globals.SORTING_LAYER_PIPES;
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Processes user input for the switching character or pausing the game
    /// </summary>
    private void HandleInput()
    {
        // Switch player
        if (!IsPaused && Input.GetButtonDown(Globals.INPUT_BUTTON_SELECT))
        {
            m_activeCharacter = m_activeCharacter == Enums.PlayableCharacter.Robot ? Enums.PlayableCharacter.Muckle : Enums.PlayableCharacter.Robot;
            if (m_activeCharacter == Enums.PlayableCharacter.Robot)
                SelectRobot();
            else
                SelectMuckle();
        }

        // Pause
        if (Input.GetButtonDown(Globals.INPUT_BUTTON_START))
            Pause();
    }

    /// <summary>
    /// Selects Muckle and disables the robot
    /// </summary>
    private void SelectMuckle()
    {
        if (Muckle != null)
        {
            m_activeCharacter = Enums.PlayableCharacter.Muckle;
            Muckle.CanControl(true);
            if (Robot != null)
                Robot.CanControl(false);
        }
        else
        {
            // Fallback to robot if Muckle doesn't exist in the scene
            // If neither character exists, none will be selected (for instance, in menus or cut scenes)
            if (Robot != null)
                SelectRobot();
        }
    }

    /// <summary>
    /// Selects the robot and disables Muckle
    /// </summary>
    private void SelectRobot()
    {
        if (Robot != null)
        {
            m_activeCharacter = Enums.PlayableCharacter.Robot;
            Robot.CanControl(true);
            if (Muckle != null)
                Muckle.CanControl(false);
        }
        else
        {
            // Fallback to Muckle if the robot doesn't exist in the scene
            // If neither character exists, none will be selected (for instance, in menus or cut scenes)
            if (Muckle != null)
                SelectMuckle();
        }
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
        IsPaused = !IsPaused;
    }

    /// <summary>
    /// Marks a hint as viewed so it can be suppressed the second time the character tries to view it
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
