using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PipeTest : MonoBehaviour {
    Tilemap m_tilemap;
    Camera m_cam;
    public Sprite TestSprite;
    private List<Vector3Int> m_usedPos = new List<Vector3Int>();

    // Use this for initialization
    void Start () {
        m_cam = Camera.main;
        m_tilemap = GetComponent<Tilemap>();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0))
        {
            PipeTile tile = GetPipeTile(m_cam.ScreenToWorldPoint(Input.mousePosition));
        }
	}

    private PipeTile GetPipeTile(Vector3 mousePos)
    {
        //mousePos.x = Mathf.RoundToInt(mousePos.x);
        //mousePos.y = Mathf.RoundToInt(mousePos.y);

        //mousePos.x += mousePos.x % 2 == 1 ? 0 : 1;
        //mousePos.y += mousePos.y % 2 == 1 ? 0 : 1;

        mousePos.z = 0;

        Vector3Int pos = Vector3Int.FloorToInt(mousePos);
        PipeTile tile = m_tilemap.GetTile(m_tilemap.WorldToCell(pos)) as PipeTile;

        if (!m_usedPos.Contains(pos))
        {
            if (tile != null)
            {
                Debug.Log("tile: " + tile.name + " @ " + pos);

                GameObject go = new GameObject();
                go.transform.position = pos;
                go.AddComponent<SpriteRenderer>().sprite = TestSprite;
                m_usedPos.Add(pos);
                tile.Drain();
            }
        }
        return tile;
    }
}
