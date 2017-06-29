using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls selection of tools on the robot
/// </summary>
public class ToolsController : MonoBehaviour
{
    /// Holds the tool references
    private List<IToolBase> m_tools;
    // Holds a reference to the active tool
    private IToolBase m_activeTool;
    // Tracks if the tool is ready to be cycled
    private bool m_canCycleTools;
    // Tracks if the tool controller is enabled
    private bool m_isEnabled = true;

    /// <summary>
    /// Used for initialization
    /// </summary>
	void Start()
    {
        LoadTools();
        m_canCycleTools = true;
    }

    /// <summary>
    /// Finds tools which are children of this gameobject and adds them to <see cref="m_tools"/>
    /// </summary>
    private void LoadTools()
    {
        // Capture the current tool index to restore the selection after clearing the tool list
        int currentToolIndex = m_activeTool == null ? 0 : m_tools.IndexOf(m_activeTool);

        m_tools = new List<IToolBase>();
        // Add one null entry to tools for the unequipped state
        m_tools.Add(null);
        // Iterate through the child transforms of this transform
        foreach (Transform child in transform)
        {
            IToolBase tool = child.GetComponent<IToolBase>();
            if (!m_tools.Contains(tool))
                m_tools.Add(tool);
        }

        // Restore the active tool selection if the index is still valid
        if(currentToolIndex < m_tools.Count)
            m_activeTool = m_tools[currentToolIndex];
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        if (!m_isEnabled)
            return;

        // Fire 3: Cycle between tools (Left Shift, MMB, 'X' button on Xbox 360)
        if (Input.GetButtonDown(Globals.INPUT_BUTTON_FIRE3))
            CycleTools();

        // Check if the active tool is enabled. If it's not, it's finished retracting and cycling tools can continue
        if (m_activeTool != null && !m_activeTool.GameObject.activeSelf && !m_canCycleTools)
        {
            m_canCycleTools = true;
            CycleTools();
        }
    }

    /// <summary>
    /// Cycles between the available tools in <see cref="m_tools"/>
    /// </summary>
    /// <param name="reverse">Cycle the tools in reverse</param>
    private void CycleTools(bool reverse = false)
    {
        // Disable the active tool
        if (m_activeTool != null && m_activeTool.GameObject.activeSelf)
        {
            m_activeTool.Disable();
            m_canCycleTools = false;
            return;
        }
        // Get the active tool's index
        int activeToolIndex = m_tools.IndexOf(m_activeTool);
        // Calculate the next tool's index
        int nextToolIndex = reverse ?
            (activeToolIndex - 1) >= 0 ? (activeToolIndex - 1) : m_tools.Count - 1 :
            (activeToolIndex + 1) < m_tools.Count ? (activeToolIndex + 1) : 0;
        
        // Set the active tool
        m_activeTool = m_tools[nextToolIndex];
        // Enable the active tool
        if (m_activeTool != null)
        {
            m_activeTool.GameObject.SetActive(true);
            m_activeTool.Enable();
        }
    }

    /// <summary>
    /// Enables tool selection
    /// </summary>
    public void EnableTools()
    {
        m_isEnabled = true;
    }

    /// <summary>
    /// Disables tool selection and retracts any active tool
    /// </summary>
    public void DisableTools()
    {
        if (m_activeTool != null)
        {
            m_activeTool.Disable();
            m_activeTool = null;
        }
        m_isEnabled = false;
    }
}
