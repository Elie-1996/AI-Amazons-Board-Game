using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic : MonoBehaviour
{
    public Camera _camera;
    public GameObject DarkTilePrefab;
    public GameObject LightTilePrefab;

    public Material DarkCubeMaterialEmpty;
    public Material LightCubeMaterialEmpty;
    public Material DarkTileFire;
    public Material LightTileFire;
    public Material DarkTileBlackQueen;
    public Material LightTileBlackQueen;
    public Material DarkTileWhiteQueen;
    public Material LightTileWhiteQueen;

    private GameBoardInformation info;
    private GameObject[,] boardTiles;
    private const float estimatedTileCameraSize = 0.6f;

    // Start is executed once
    void Start()
    {
        // TODO: rows and columns should be input later on
        int rows = 10;
        int columns = 10;

        // TODO: WhiteQueens and BlackQueens should be input later on
        List<Vector2> WhiteQueens = new List<Vector2>();
        List<Vector2> BlackQueens = new List<Vector2>();
        WhiteQueens.Add(new Vector2(0, 0));
        WhiteQueens.Add(new Vector2(9, 9));
        BlackQueens.Add(new Vector2(0, 9));
        BlackQueens.Add(new Vector2(9, 0));


        info = new GameBoardInformation(rows, columns, WhiteQueens, BlackQueens);
        generateAndPlaceTiles();
        LoadTilePieces();
    }

    private void Update()
    {
        OnMouseClick();
    }

    private void OnMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.transform.gameObject.name);
            }
        }
    }

    // TODO: Should involve which piece to burn.
    private void movePiece(int i, int j, int destination_i, int destination_j)
    {
        if (info.isMoveLegal(i, j, destination_i, destination_j) == false)
        {
            Debug.Log("Illegal move, Please make a legal move.");
            return;
        }
        bool didMove = info.movePiece(i, j, destination_i, destination_j);
        if (didMove == false) return;
        UpdateTileMaterial(i, j, info.getPieceAt(i,j));
        UpdateTileMaterial(destination_i, destination_j, info.getPieceAt(destination_i, destination_j));
    }

    private void UpdateTileMaterial(int i, int j, Piece piece)
    {
        MaterialIntensity intensity = info.getPieceIntensity(i, j);

        Material updatedMaterial = null;
        if (intensity == MaterialIntensity.DARK)
        {
            switch (piece)
            {
                case Piece.BLACKQUEEN:
                    updatedMaterial = DarkTileBlackQueen;
                    break;
                case Piece.DESTROYEDTILE:
                    updatedMaterial = DarkTileFire;
                    break;
                case Piece.EMPTY:
                    updatedMaterial = DarkCubeMaterialEmpty;
                    break;
                case Piece.WHITEQUEEN:
                    updatedMaterial = DarkTileWhiteQueen;
                    break;
            }
        }
        else
        {
            switch (piece)
            {
                case Piece.BLACKQUEEN:
                    updatedMaterial = LightTileBlackQueen;
                    break;
                case Piece.DESTROYEDTILE:
                    updatedMaterial = LightTileFire;
                    break;
                case Piece.EMPTY:
                    updatedMaterial = LightCubeMaterialEmpty;
                    break;
                case Piece.WHITEQUEEN:
                    updatedMaterial = LightTileWhiteQueen;
                    break;
            }
        }

        boardTiles[i, j].GetComponent<Renderer>().material = updatedMaterial;
        if (updatedMaterial == null)
        {
            Debug.LogError("GameLogic.cs, updateTileMaterial: updatedMaterial is null.");
        }
    }

    private void generateAndPlaceTiles()
    {
        int rows = info.rows;
        int columns = info.columns;
        boardTiles = new GameObject[rows, columns];

        int maximum = rows > columns ? rows : columns;
        Vector3 cameraPosition = new Vector3(rows / 2.0f, -columns / 2.0f, -10.0f);
        _camera.orthographicSize = estimatedTileCameraSize * maximum;
        _camera.transform.position = cameraPosition;

        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                GameObject newTile;
                Vector3 tilePosition = new Vector3(i, -j, 0.0f);
                if (info.getPieceIntensity(i, j) == MaterialIntensity.DARK)
                {   
                    newTile = Instantiate(DarkTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                else
                {
                    newTile = Instantiate(LightTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                newTile.AddComponent<BoxCollider>();
                newTile.name = "Index (" + i + ", " + j + ")";
                boardTiles[i, j] = newTile;
            }
        }
    }

    private void LoadTilePieces()
    {
        int rows = info.rows;
        int columns = info.columns;
        for (int i = 0; i < rows; ++i)
        {
            for (int j = 0; j < columns; ++j)
            {
                UpdateTileMaterial(i, j, info.getPieceAt(i, j));
            }
        }
    }
}
