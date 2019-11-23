using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnMouseHover : MonoBehaviour
{
    private Ray ray;
    private RaycastHit hit;
    private GameObject oldTile;
    private GameObject hoveredTile;

    // Start is called before the first frame update
    void Start()
    {
        oldTile = null;
        hoveredTile = null;
    }

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            hoveredTile = hit.collider.gameObject;
            if (oldTile != hoveredTile)
            {
                if (hoveredTile != null)
                {
                    changeTileOpacity(hoveredTile, 0.5f);
                }
                
                if (oldTile != null)
                {
                    changeTileOpacity(oldTile, 1.0f);
                }

                oldTile = hoveredTile;
            }
        }
    }

    private void changeTileOpacity(GameObject Tile, float opacity)
    {
        opacity = Mathf.Clamp(opacity, 0.0f, 1.0f);
        Color c = Tile.GetComponent<Renderer>().material.color;
        Color newColor = new Color(c.r, c.g, c.b, opacity);
        Tile.GetComponent<Renderer>().material.color = newColor;
    }
}
