using System.Collections;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterGenerator : MonoBehaviour
{
    public float Width = 10f;
    public int WaveNumber = 0;
    public float StepsPerUnit = 3f;
    private Vector2[] m_points;
    private Mesh m_waterMesh;
    private MeshFilter m_filter;
    private MeshRenderer m_renderer;
    private float m_startX;

    // Use this for initialization
    void Start()
    {
        m_startX = transform.position.x;
        m_waterMesh = new Mesh();
        m_filter = GetComponent<MeshFilter>();
        m_renderer = GetComponent<MeshRenderer>();
        m_renderer.sortingOrder = WaveNumber;
        InvokeRepeating("Refresh", 0, .05f);
    }

    void Refresh()
    {
        StartCoroutine(GenerateNoise());
    }

    private IEnumerator GenerateNoise()
    {
        int numPoints = Mathf.RoundToInt(StepsPerUnit * (Width - m_startX));
        m_points = new Vector2[numPoints];
        Vector2 bottomPoint = new Vector2(m_startX, transform.position.y - 10f);
        m_points[0] = bottomPoint;

        for (int x = 1; x < (numPoints - 1); x++)
        {
            float noise = Mathf.PerlinNoise(Time.time + (x / 3), WaveNumber);
            Vector2 point = new Vector2(m_startX + (x / StepsPerUnit), transform.position.y + noise);
            if (x == (numPoints - 2))
                point.x = Width;
            m_points[x] = point;
        }

        bottomPoint = new Vector2(Width, transform.position.y - 10f);
        m_points[numPoints - 1] = bottomPoint;

        Triangulator tr = new Triangulator(m_points);
        int[] indices = tr.Triangulate();

        // Create the Vector3 vertices
        Vector3[] vertices = new Vector3[m_points.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(m_points[i].x, m_points[i].y, 0);
        }

        m_waterMesh.vertices = vertices;
        m_waterMesh.triangles = indices;
        m_waterMesh.RecalculateNormals();
        m_waterMesh.RecalculateBounds();
        if (m_filter.mesh != null)
            Destroy(m_filter.mesh);
        m_filter.mesh = m_waterMesh;
        
        yield return null;
    }
}
