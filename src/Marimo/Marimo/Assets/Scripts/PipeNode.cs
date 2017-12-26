using UnityEngine;

public class PipeNode : MonoBehaviour
{
    // Node properties
    public PipeNode NodeLeft;
    public PipeNode NodeRight;
    public PipeNode NodeTop;
    public PipeNode NodeBottom;
    public Enums.PipeType PipeType { get; set; }
}
