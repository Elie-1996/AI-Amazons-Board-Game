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
    
    private GameObject[,] boardUITiles; // holds the visible GUI tiles.
    private const float estimatedTileCameraSize = 0.6f;

    private PlayerLogic player1;
    private PlayerLogic player2;

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
        WhiteQueens.Add(new Vector2(rows - 1, columns - 1));
        BlackQueens.Add(new Vector2(0, columns - 1));
        BlackQueens.Add(new Vector2(rows - 1, 0));
        GameBoardInformation.InitializeBoard(rows, columns, WhiteQueens, BlackQueens);
        generateAndPlaceTiles();
        player1 = gameObject.AddComponent<PlayerLogic>(); // initializing player1
        player2 = gameObject.AddComponent<PlayerLogic>(); // initializing player2
        StartCoroutine(Play());
    }
    
    IEnumerator Play()
    {
        StartCoroutine(player1.PlayTurn());
        StartCoroutine(player2.PlayTurn());
        yield return new WaitUntil(() => GameBoardInformation.playAgain == true);
        resetGame(); // TODO: Complete implementation
        yield return Play();
    }

    private void resetGame()
    {
        GameBoardInformation.playAgain = false;
        PlayerLogic.reset();
        GameBoardInformation.reset(); // TODO: Do not forget to implement reset!
    }


    private void Update()
    {
        UpdateTilesUI();
        UpdatePlayerSelection();
    }

    private void UpdatePlayerSelection()
    {
        GetTileClicked(out int i, out int j);
        if (i == -1 && j == -1) return;
        if (PlayerLogic.globalTurn == (int)Piece.WHITEQUEEN)
        {
            player1.SelectIndices(i, j);
        }
        else
        {
            player2.SelectIndices(i, j);
        }
    }

    private void UpdateTilesUI()
    {
        while (GameBoardInformation.UIUpdatesQueue.Count > 0) // if the queue isn't empty, O(1) operation
        {
            UpdatedTile changes = GameBoardInformation.UIUpdatesQueue.Dequeue();
            int i = changes.i;
            int j = changes.j;
            Piece newPiece = changes.piece;
            UpdateTileMaterial(i, j, newPiece);
        }
    }


    // returns the indices of clicked Tile
    private void GetTileClicked(out int i, out int j)
    {
        i = -1; j = -1;
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                GameObject clickedTile = hit.transform.gameObject;
                TileIndices(clickedTile.name, out i, out j);
            }
        }
    }

    private void TileIndices(string name, out int i, out int j)
    {
        int start_1 = name.IndexOf('(') + 1;
        int end_1 = name.IndexOf(',') - 1;
        i = int.Parse(name.Substring(start_1, end_1 - start_1 + 1));

        int start_2 = name.IndexOf(',') + 1;
        int end_2 = name.IndexOf(')') - 1;
        j = int.Parse(name.Substring(start_2, end_2 - start_2 + 1));
    }

    private void UpdateTileMaterial(int i, int j, Piece piece)
    {
        MaterialIntensity intensity = GameBoardInformation.getPieceIntensity(i, j);

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
        
        boardUITiles[i, j].GetComponent<Renderer>().material = updatedMaterial;
        if (updatedMaterial == null)
        {
            Debug.LogError("GameLogic.cs, updateTileMaterial: updatedMaterial is null.");
        }
    }

    private void generateAndPlaceTiles()
    {
        int rows = GameBoardInformation.rows;
        int columns = GameBoardInformation.columns;
        boardUITiles = new GameObject[rows, columns];

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
                if (GameBoardInformation.getPieceIntensity(i, j) == MaterialIntensity.DARK)
                {   
                    newTile = Instantiate(DarkTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                else
                {
                    newTile = Instantiate(LightTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                newTile.AddComponent<BoxCollider>();
                newTile.name = "Index (" + i + ", " + j + ")";
                boardUITiles[i, j] = newTile;
            }
        }
    }
}
