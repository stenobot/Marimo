/// <summary>
/// Contains non-localizable app strings
/// </summary>
public class Globals
{
    // Animation state names
    public const string ANIMSTATE_GAMEOVER = "gameover";
    public const string ANIMSTATE_IDLE = "idle";
    public const string ANIMSTATE_IDLE_UP = "idleup";
    public const string ANIMSTATE_IDLE_DOWN = "idledown";
    public const string ANIMSTATE_MOVE_RIGHT = "moveright";
    public const string ANIMSTATE_MOVE_RIGHT_UP = "moverightup";
    public const string ANIMSTATE_MOVE_RIGHT_DOWN = "moverightdown";
    public const string ANIMSTATE_ROBOT_RAISE = "raise";
    public const string ANIMSTATE_ROBOT_BUMP = "bump";
    public const string ANIMSTATE_ROBOT_JUMP = "jump";
	public const string ANIMSTATE_WRENCH_TURN = "turn_wrench";
	public const string ANIMSTATE_CONVEYOR_RIGHT = "moveright";
	public const string ANIMSTATE_CONVEYOR_LEFT = "moveleft";
    public const string ANIMSTATE_HINT_APPEAR = "appear";
    public const string ANIMSTATE_HINT_IDLE_OPEN = "idle_open";
    public const string ANIMSTATE_HINT_INTERACTION_SWITCH = "switch";
    public const string ANIMSTATE_HINT_INTERACTION_ELEVATOR = "elevator";
    public const string ANIMSTATE_HINT_TOOL_WRENCH = "wrench";
    public const string ANIMSTATE_HINT_TOOL_UPDOWNARROWS = "updownarrows";
	public const string ANIMSTATE_DISPENSOR_ON = "dispensor_on";

    // Animation parameters
    public const string ANIM_PARAM_SPEED = "AnimSpeed";

    // Axes for input
    public const string INPUT_AXIS_HORIZONTAL = "Horizontal";
    public const string INPUT_AXIS_VERTICAL = "Vertical";

    // Buttons for input
    public const string INPUT_BUTTON_JUMP = "Jump";
    public const string INPUT_BUTTON_FIRE0 = "Fire0";
    public const string INPUT_BUTTON_FIRE1 = "Fire1";
    public const string INPUT_BUTTON_FIRE2 = "Fire2";
    public const string INPUT_BUTTON_FIRE3 = "Fire3";
    public const string INPUT_BUTTON_SELECT = "Select";
    public const string INPUT_BUTTON_START = "Start";

    // Tags
    public const string TAG_GAMEMANAGER = "GameManager";
    public const string TAG_PLAYER = "Player";
    public const string TAG_MUCKLE = "Muckle";
    public const string TAG_GROUND = "Ground";
    public const string TAG_TREADS = "Treads";
	public const string TAG_TRASH = "Trash";
    public const string TAG_TRASH_PART = "TrashPart";
    public const string TAG_CONVEYOR = "Conveyor";
    public const string TAG_BOMB = "Bomb";
    public const string TAG_LEAF = "Leaf";
	
	// Global values
    public const float PIXELS_PER_UNIT = 6f;
    public const float PIXEL_SIZE = 1f / PIXELS_PER_UNIT;
}