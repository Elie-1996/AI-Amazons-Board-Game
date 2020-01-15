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
    private const float estimatedTileCameraSize = 0.65f;

    private PlayerLogic player1;
    private PlayerLogic player2;

    // Start is executed once
    void Start()
    {
        // TODO: rows and columns should be input later on
        int rows = InitializingParameters.rows;
        int columns = InitializingParameters.columns;

        // TODO: WhiteQueens and BlackQueens should be input later on
        List<Indices> WhiteQueens = InitializingParameters.WhiteQueens;
        List<Indices> BlackQueens = InitializingParameters.BlackQueens;
        GameBoardInformation.InitializeBoard(rows, columns, WhiteQueens, BlackQueens);
        generateAndPlaceTiles();

        // initialize players as Humans/AIs as requested
        InitializePlayersAsHumansOrAIs();

        // Players identities are finally recognized, we can start playing!
        StartCoroutine(Play());
    }

    private void InitializePlayersAsHumansOrAIs()
    {
        if (InitializingParameters.numberOfAIs == 2)
        {
            player1 = gameObject.AddComponent<AILogic>();
            player2 = gameObject.AddComponent<AILogic>();
        }
        else if (InitializingParameters.numberOfAIs == 1)
        {
            player1 = gameObject.AddComponent<HumanLogic>();
            player2 = gameObject.AddComponent<AILogic>();
        }
        else //InitializingParameters.numberOfAIs should be zero!
        {
            if (InitializingParameters.numberOfAIs != 0) throw new System.Exception("Expected number of AIs to be zero, instead = " + InitializingParameters.numberOfAIs);
            player1 = gameObject.AddComponent<HumanLogic>();
            player2 = gameObject.AddComponent<HumanLogic>();
        }
    }
    
    IEnumerator Play()
    {
        StartCoroutine(player1.PlayTurn());
        StartCoroutine(player2.PlayTurn());
        yield return new WaitUntil(() => GameBoardInformation.GetWinner() != Piece.EMPTY);
        Debug.Log("Winner is = " + GameBoardInformation.GetWinner().ToString());
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
            if (player1 is HumanLogic)
                (player1 as HumanLogic).SelectIndices(i, j);
        }
        else
        {
            if (player2 is HumanLogic)
                (player2 as HumanLogic).SelectIndices(i, j);
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
                Vector3 tilePosition = new Vector3(j, -i, 0.0f);
                if (GameBoardInformation.getPieceIntensity(i, j) == MaterialIntensity.DARK)
                {   
                    newTile = Instantiate(DarkTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                else
                {
                    newTile = Instantiate(LightTilePrefab, tilePosition, Quaternion.identity, transform);
                }
                newTile.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
                newTile.AddComponent<BoxCollider>();
                newTile.name = "Index (" + i + ", " + j + ")";
                boardUITiles[i, j] = newTile;
            }
        }
    }
}
