using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using TMPro;


enum ETurnStage
{
    SelectPlayer,
    MovePlayer,
    GameOverCheck,
    SelectPiece,
    MovePiece,
    GameOverCheck2,
    GameOver,
}


public class GameManager : MonoBehaviourPunCallbacks
{
    // variables marked with m_ must be updated during RPCs
    #region Internal Variables

    private List<Piece>[,] boardArray;
    PhotonView pv;
    private List<(short, short)> currentPieceAvailablePositions;
    private List<GameObject>[,] instanciatedPieces;
    private (short, short) currentSelectorPosition;
    private (short, short) currentShadowPosition;
    private (short, short) m_player1BoardPosition;
    private (short, short) m_player2BoardPosition;
    private (short, short) m_targetPos = (-1, -1); 
    private (short, short) m_oldPos = (-1, -1);
    private (short, short) defaultPos = (-1, -1);
    private bool m_isGameOver;
    private string player1Name;
    private string player2Name;
    [Range(1, 2)][SerializeField] private byte m_playerTurn;
    private GameObject player1Instance;
    private GameObject player2Instance;
    [SerializeField] GameObject currentPlayerInstanceReference;
    [SerializeField] private bool isPlayer2;
    private GameObject selectorInstance;
    [SerializeField] private GameObject selectedPiece;
    [SerializeField] private GameObject selectedShadow;
    private List<GameObject> highlightList;
    private SelectorLogic selectorComponent;
    private int playerModifier;
    [SerializeField] private ETurnStage e_turnStage;


    #endregion



    #region Serialized Fields

    [Header("Board Settings")]
    [SerializeField] private Transform board;
    [SerializeField] private Transform piecesContainer;
    [SerializeField] private Transform highlightContainer;
    [SerializeField][Range(4, 8)] private short xSize;
    [SerializeField][Range(4, 8)] private short ySize;
    [SerializeField] float boardWallThickness;
    [SerializeField] float boardWallHeight;

    #endregion



    #region Prefabs
    [Header("Board prefabs")]
    [SerializeField] private GameObject verticalBoardWall;
    [SerializeField] private GameObject horizontalBoardWall;
    [SerializeField] private GameObject horizontalDivider;
    [SerializeField] private GameObject verticalDivider;
    [SerializeField] private GameObject availableHighlight;
    [SerializeField] private GameObject unavailableHighlight;
    [SerializeField] private GameObject selector;

    [Header("Pieces")]
    [SerializeField] private GameObject tower1;
    [SerializeField] private GameObject tower2;
    [SerializeField] private GameObject pillow1;
    [SerializeField] private GameObject pillow2;
    [SerializeField] private GameObject player1;
    [SerializeField] private GameObject player2;
    [SerializeField] private GameObject start1;
    [SerializeField] private GameObject start2;
    [SerializeField] private GameObject teleport;
    [SerializeField] private GameObject playerShadow;
    [SerializeField] private GameObject towerShadow;
    [SerializeField] private GameObject pillowShadow;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI player1UI;
    [SerializeField] private TextMeshProUGUI player2UI;
    [SerializeField] private TextMeshProUGUI currentPlayerName;

    #endregion  



    #region Getters and Setters
    public (short, short) CurrentSelectorPosition
    {
        get { return currentSelectorPosition; }
        set
        {
            currentSelectorPosition.Item1 = (short)Mathf.Clamp(value.Item1, 0, xSize - 1);
            currentSelectorPosition.Item2 = (short)Mathf.Clamp(value.Item2, 0, ySize - 1);
        }
    }
    public (short, short) CurrentShadowPosition
    {
        get { return currentShadowPosition; }
        set
        {
            currentShadowPosition.Item1 = (short)Mathf.Clamp(value.Item1, 0, xSize - 1);
            currentShadowPosition.Item2 = (short)Mathf.Clamp(value.Item2, 0, ySize - 1);
        }
    }
    #endregion



    #region Unity Callbacks

    private void Awake()
    {
        InitializeVariables();
        GenerateBoard(xSize, ySize);
        FillBoardArrayWithPieces(xSize, ySize);
        AssignPrefabsToPieces();
        InstantiateBoard();
        SetPlayerNumber();
        FixCameraPosition();
        AssignPlayerNamesToUI();
        SpawnSelector();
    }

    void Update()
    {
        TurnManager();
        Controls();
    }

    #endregion



    #region Custom Methods

    private void GenerateBoard(short _xSize, short _ySize)
    {
        board.localScale = new Vector3(_xSize, 0.3f, _ySize);

        GameObject tempWall;
        tempWall = Instantiate(horizontalBoardWall, new Vector3(0, 0f, -ySize / 2f), horizontalBoardWall.transform.rotation);
        tempWall.transform.localScale = new Vector3(xSize + 0.1f, boardWallThickness, boardWallHeight);
        tempWall.transform.SetParent(board);

        tempWall = Instantiate(horizontalBoardWall, new Vector3(0, 0f, ySize / 2f), horizontalBoardWall.transform.rotation);
        tempWall.transform.localScale = new Vector3(xSize + 0.1f, boardWallThickness, boardWallHeight);
        tempWall.transform.SetParent(board);

        tempWall = Instantiate(verticalBoardWall, new Vector3(-xSize / 2f, 0f, 0), verticalBoardWall.transform.rotation);
        tempWall.transform.localScale = new Vector3(ySize + 0.1f, boardWallThickness, boardWallHeight);
        tempWall.transform.SetParent(board);

        tempWall = Instantiate(verticalBoardWall, new Vector3(xSize / 2f, 0f, 0), verticalBoardWall.transform.rotation);
        tempWall.transform.localScale = new Vector3(ySize + 0.1f, boardWallThickness, boardWallHeight);
        tempWall.transform.SetParent(board);


        board.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(xSize / 4f, ySize / 4f);

        for (int i = 1; i < ySize; i++)
        {
            var wall1 = Instantiate(horizontalDivider, new Vector3(0, 0f, (-ySize / 2f) + i), horizontalDivider.transform.rotation);
            wall1.transform.localScale = new Vector3(xSize, boardWallThickness - 0.01f, boardWallHeight);
            wall1.transform.SetParent(board);
        }
        for (int i = 1; i < xSize; i++)
        {
            var wall2 = Instantiate(verticalDivider, new Vector3((-xSize / 2f) + i, 0f, 0), verticalDivider.transform.rotation);
            wall2.transform.localScale = new Vector3(ySize, boardWallThickness - 0.01f, boardWallHeight);
            wall2.transform.SetParent(board);

        }

        boardArray = new List<Piece>[xSize, ySize];
        instanciatedPieces = new List<GameObject>[xSize, ySize];

        for (short i = 0; i < xSize; i++)
        {
            for (short j = 0; j < ySize; j++)
            {
                boardArray[i, j] = new List<Piece>();
                instanciatedPieces[i, j] = new List<GameObject>();
            }
        }

    }

    private void FillBoardArrayWithPieces(short _xSize, short _ySize)
    {
        // Which pieces will be put where?
        // In the future, pieces should be placed manually by the player at the start of the match
        // For now, we will place all the pieces through code using this function, made for a 4x4 board.

        // T = Tower
        // P = Pillow
        // P1 = Player1
        // S1 = Start1
        // S2 = Start2
        // TP = Teleport


        // TP  P   -   S2
        // T   T   P   -
        // -   T   T+P -
        // T   T   T   P
        // -   -   -   T
        // S1  P   P   TP


        boardArray[0, 0].Add(new Piece(EBoardPieces.Start1, 1));
        boardArray[0, 0].Add(new Piece(EBoardPieces.Player1, 1));
        m_player1BoardPosition = (0, 0);

        boardArray[1, 0].Add(new Piece(EBoardPieces.Pillow, 1));
        boardArray[2, 0].Add(new Piece(EBoardPieces.Pillow, 1));
        boardArray[3, 0].Add(new Piece(EBoardPieces.Teleport, 1));
        boardArray[3, 1].Add(new Piece(EBoardPieces.Tower, 1));
        boardArray[0, 2].Add(new Piece(EBoardPieces.Tower, 1));
        boardArray[1, 2].Add(new Piece(EBoardPieces.Tower, 1));
        boardArray[2, 2].Add(new Piece(EBoardPieces.Tower, 1));
        boardArray[3, 2].Add(new Piece(EBoardPieces.Pillow, 1));



        boardArray[1, 3].Add(new Piece(EBoardPieces.Tower, 2));
        boardArray[3, 3].Add(new Piece(EBoardPieces.Tower, 2));
        boardArray[3, 3].Add(new Piece(EBoardPieces.Pillow, 2));
        boardArray[1, 4].Add(new Piece(EBoardPieces.Tower, 2));
        boardArray[2, 4].Add(new Piece(EBoardPieces.Tower, 2));
        boardArray[3, 4].Add(new Piece(EBoardPieces.Pillow, 2));
        boardArray[0, 5].Add(new Piece(EBoardPieces.Teleport, 2));
        boardArray[1, 5].Add(new Piece(EBoardPieces.Pillow, 2));



        boardArray[3, 5].Add(new Piece(EBoardPieces.Start2, 2));
        boardArray[3, 5].Add(new Piece(EBoardPieces.Player2, 2));
        m_player2BoardPosition = (3, 5);
    }

    private void AssignPrefabsToPieces()
    {
        foreach (var pieceList in boardArray)
        {
            foreach (var piece in pieceList)
            {
                switch (piece.Type)
                {
                    case EBoardPieces.Tower:
                        if (piece.Player == 1)
                            piece.Prefab = tower1;
                        else
                            piece.Prefab = tower2;
                        break;

                    case EBoardPieces.Pillow:
                        if (piece.Player == 1)
                            piece.Prefab = pillow1;
                        else
                            piece.Prefab = pillow2;
                        break;

                    case EBoardPieces.Player1:
                        piece.Prefab = player1;
                        break;

                    case EBoardPieces.Player2:
                        piece.Prefab = player2;
                        break;

                    case EBoardPieces.Start1:
                        piece.Prefab = start1;
                        break;

                    case EBoardPieces.Start2:
                        piece.Prefab = start2;
                        break;

                    case EBoardPieces.Teleport:
                        piece.Prefab = teleport;
                        break;
                }
            }
        }
    }

    private void InstantiateBoard()
    {
        for (int i = 0; i < boardArray.GetLength(0); i++)
        {
            for (int j = 0; j < boardArray.GetLength(1); j++)
            {
                short pillowStack = 0;
                float totalHeight = 0.15f;

                foreach (var piece in boardArray[i, j])
                {
                    totalHeight += piece.Height / 2;


                    var pos = new Vector3(i - xSize / 2 + 0.5f, totalHeight, j - ySize / 2 + 0.5f);

                    totalHeight += piece.Height / 2;

                    var rotation = Quaternion.Euler(new Vector3(0, 0, 0));

                    if (piece.Type == EBoardPieces.Pillow && pillowStack > 0)
                        rotation = Quaternion.Euler(new Vector3(0, 45 * pillowStack, 0));


                    var newPiece = Instantiate(piece.Prefab, pos, rotation);
                    newPiece.transform.SetParent(piecesContainer);

                    instanciatedPieces[i, j].Add(newPiece);
                    PieceInfo pieceInfo = newPiece.GetComponent<PieceInfo>();
                    pieceInfo.height = piece.Height;
                    pieceInfo.blocksSlotForPlayer = piece.BlocksSlotForPlayer;
                    //pieceInfo.maxAmountPerSlot = piece.MaxAmountPerSlot;
                    pieceInfo.blocksSlotForTower = piece.BlocksSlotForTower;

                    if (pieceInfo.type == EBoardPieces.Player1)
                        player1Instance = newPiece;

                    if (pieceInfo.type == EBoardPieces.Player2)
                        player2Instance = newPiece;

                    if (piece.Type == EBoardPieces.Pillow)
                        pillowStack++;

                    currentPlayerInstanceReference = player1Instance;
                }
            }
        }
    }

    private void FixCameraPosition()
    {
        if (isPlayer2)
        {
            var newCameraPos = new Vector3(0, 3.5f, 4);
            var newCameraAngle = Quaternion.Euler(50, 180, 0);
            Camera.main.transform.position = newCameraPos;
            Camera.main.transform.rotation = newCameraAngle;
        }
    }

    private void AssignPlayerNamesToUI()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        player1UI.text = PhotonNetwork.MasterClient.NickName;
        player2UI.text = PhotonNetwork.PlayerList[1].NickName;
        player1Name = player1UI.text;
        player2Name = player2UI.text;
    }

    private void SpawnSelector()
    {
        float y = CalculateSpawnHeightOfSelector(CurrentSelectorPosition);

        Vector3 spawnPos = new Vector3(CurrentSelectorPosition.Item1 - xSize / 2 + 0.5f, y, CurrentSelectorPosition.Item2 - ySize / 2 + 0.5f);


        selectorInstance = Instantiate(selector, piecesContainer);
        selectorComponent = selectorInstance.GetComponent<SelectorLogic>();

        selectorInstance.transform.position = spawnPos;
    }

    private float CalculateSpawnHeightOfSelector((short, short) position)
    {
        var list = instanciatedPieces[position.Item1, position.Item2];
        var spawnHeight = 0.16f;
        for (int i = 0; i < list.Count; i++)
        {
            if (i != list.Count - 1)
                spawnHeight += list[i].GetComponent<PieceInfo>().height;
            else
                spawnHeight += list[i].GetComponent<PieceInfo>().height / 2;
        }
        return spawnHeight;
    }

    private float CalculateSpawnHeight((short, short) position)
    {
        var list = instanciatedPieces[position.Item1, position.Item2];
        var spawnHeight = 0.15f;
        for (int i = 0; i < list.Count; i++)
        {
            spawnHeight += list[i].GetComponent<PieceInfo>().height;
        }
        return spawnHeight;
    }

    private void MoveSelector((short, short) newPosition)
    {
        var val1 = newPosition.Item1;
        var val2 = newPosition.Item2;

        newPosition.Item1 = (short)Mathf.Clamp(val1, 0, xSize - 1);
        newPosition.Item2 = (short)Mathf.Clamp(val2, 0, ySize - 1);

        CurrentSelectorPosition = newPosition;
        var y = CalculateSpawnHeightOfSelector(newPosition);
        var newPos = new Vector3(newPosition.Item1 - xSize / 2 + 0.5f, y, newPosition.Item2 - ySize / 2 + 0.5f);
        selectorInstance.transform.position = newPos;
    }

    private void Controls()
    {
        if ((e_turnStage == ETurnStage.SelectPiece || e_turnStage == ETurnStage.SelectPlayer) || !IsMyTurn())
        {

            if (Input.GetKeyDown(KeyCode.A))
            {
                var newPos = ((short)(CurrentSelectorPosition.Item1 - playerModifier), CurrentSelectorPosition.Item2);
                MoveSelector(newPos);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                var newPos = ((short)(CurrentSelectorPosition.Item1 + playerModifier), CurrentSelectorPosition.Item2);
                MoveSelector(newPos);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                var newPos = (CurrentSelectorPosition.Item1, (short)(CurrentSelectorPosition.Item2 + playerModifier));
                MoveSelector(newPos);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                var newPos = (CurrentSelectorPosition.Item1, (short)(CurrentSelectorPosition.Item2 - playerModifier));
                MoveSelector(newPos);
            }
        }

        if ((e_turnStage == ETurnStage.MovePiece || e_turnStage == ETurnStage.MovePlayer) && IsMyTurn())
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                var newPos = ((short)(CurrentShadowPosition.Item1 - playerModifier), CurrentShadowPosition.Item2);
                MoveShadow(selectedShadow, newPos);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                var newPos = ((short)(CurrentShadowPosition.Item1 + playerModifier), CurrentShadowPosition.Item2);
                MoveShadow(selectedShadow, newPos);
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                var newPos = (CurrentShadowPosition.Item1, (short)(CurrentShadowPosition.Item2 + playerModifier));
                MoveShadow(selectedShadow, newPos);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                var newPos = (CurrentShadowPosition.Item1, (short)(CurrentShadowPosition.Item2 - playerModifier));
                MoveShadow(selectedShadow, newPos);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) && IsMyTurn())
        {
            //PhotonView pv = PhotonView.Get(this);
            //int x1 = currentSelection.Item1;
            //int y1 = currentSelection.Item2;
            //pv.RPC("MovePiece", RpcTarget.All, x1, y1, 1, 3);
            ////MovePiece(CurrentSelection, (1, 3));
            ///

            if (e_turnStage == ETurnStage.SelectPiece || e_turnStage == ETurnStage.SelectPlayer)
                SelectPiece(CurrentSelectorPosition);

            else if (e_turnStage == ETurnStage.MovePiece || e_turnStage == ETurnStage.MovePlayer)
            {
                if (!NewPosIsValid())
                    return;

                ChooseNewPositionForPiece();
                pv.RPC("MovePieceRPC", RpcTarget.All, (int)m_targetPos.Item1, (int)m_targetPos.Item2, (int)m_oldPos.Item1, (int)m_oldPos.Item2);

                //MovePiece(m_targetPos, m_oldPos);
            }


        }


        // Debugging input
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DestroyHighlights();
        }

    }

    private void SetPlayerNumber()
    {
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected)
        {
            isPlayer2 = false;
            CurrentSelectorPosition = (0, 0);
        }
        else
        {
            isPlayer2 = true;
            CurrentSelectorPosition = ((short)(xSize - 1), (short)(ySize - 1));
        }
        if (!PhotonNetwork.IsConnected) // for debugging
        {
            CurrentSelectorPosition = (0, 0);
            isPlayer2 = false;
        }

        playerModifier = 1;
        if (isPlayer2)
        {
            playerModifier = -1;
        }


    }

    private bool IsMyTurn()
    {
        if (isPlayer2 && m_playerTurn == 2)
            return true;
        if (!isPlayer2 && m_playerTurn == 1)
            return true;
        else
            return false;
    }

    #region Select and move piece Logic
    private void SelectPiece((short, short) currentPosition)
    {
        if (e_turnStage == ETurnStage.SelectPiece)
        {
            if (instanciatedPieces[currentPosition.Item1, currentPosition.Item2] == null)
                return;

            var currentPositionPieces = instanciatedPieces[currentPosition.Item1, currentPosition.Item2];

            if (currentPositionPieces.Count == 0)
                return;

            selectedPiece = currentPositionPieces[currentPositionPieces.Count - 1];

            if (selectedPiece == player1Instance || selectedPiece == player2Instance)
                return;

            if (!CheckValidMovesAndDisplayHighlight(currentSelectorPosition))
                return;

            selectorComponent.isHoldingPiece = true;
            var type = selectedPiece.GetComponent<PieceInfo>().type;
            switch (type)
            {
                case EBoardPieces.Tower:
                    selectedShadow = towerShadow;
                    break;
                case EBoardPieces.Pillow:
                    selectedShadow = pillowShadow;
                    break;
                default:
                    Debug.Log("Piece not movable");
                    break;
            }

            e_turnStage = ETurnStage.MovePiece;
            currentShadowPosition = currentSelectorPosition;
        }

        else if (e_turnStage == ETurnStage.SelectPlayer)
        {
            if (instanciatedPieces[currentPosition.Item1, currentPosition.Item2] == null)
                return;

            var currentPositionPieces = instanciatedPieces[currentPosition.Item1, currentPosition.Item2];

            if (currentPositionPieces.Count == 0)
                return;

            selectedPiece = currentPositionPieces[currentPositionPieces.Count - 1];

            if (selectedPiece != currentPlayerInstanceReference)
                return;

            if (selectedPiece == player1Instance || selectedPiece == player2Instance)
            {
                if (!CheckValidMovesAndDisplayHighlight(currentSelectorPosition))
                    return;

                selectorComponent.isHoldingPiece = true;
                var type = selectedPiece.GetComponent<PieceInfo>().type;
                switch (type)
                {
                    case EBoardPieces.Player1:
                        selectedShadow = playerShadow;
                        break;
                    case EBoardPieces.Player2:
                        selectedShadow = playerShadow;
                        break;
                }

                currentShadowPosition = currentSelectorPosition;
                e_turnStage = ETurnStage.MovePlayer;
            }
        }
    }

    private void ChooseNewPositionForPiece()
    {
        // Spawn dotted line
        // Move piece to new slot

        m_targetPos = CurrentShadowPosition;
        m_oldPos = CurrentSelectorPosition;
        selectorComponent.isHoldingPiece = false;
        DestroyHighlights();
    }

    private bool CheckValidMovesAndDisplayHighlight((short, short) currentPos)
    {
        EBoardPieces currentPiece = selectedPiece.GetComponent<PieceInfo>().type;
        List<(short, short)> availablePositions = new List<(short, short)>();
        availablePositions = ScanAvailablePositions(currentPos, currentPiece);

        if (availablePositions == null)
            return false;

        if (availablePositions.Count == 0)
            return false;

        currentPieceAvailablePositions = availablePositions;
        HighlightAvailablePositions(availablePositions);

        return true;

    }

    private List<(short, short)> ScanAvailablePositions((short, short) position, EBoardPieces pieceType)
    {
        List<(short, short)> list = new List<(short, short)>();

        if (pieceType == EBoardPieces.Player1 || pieceType == EBoardPieces.Player2)
        {
            for (int i = 0, x = position.Item1 - 1; i < 3; i++, x++)
            {
                for (int j = 0, y = position.Item2 - 1; j < 3; j++, y++)
                {
                    if (x < 0 || y < 0 || x > xSize - 1 || y > ySize - 1)
                        continue;

                    bool blocked = false;

                    foreach (var piece in instanciatedPieces[x, y])
                    {
                        if (piece.GetComponent<PieceInfo>().blocksSlotForPlayer)
                        {
                            //Debug.Log($"Blocked {x}, {y} because of {piece.name}");
                            blocked = true;
                        }
                    }
                    if (!blocked)
                    {
                        list.Add(((short)x, (short)y));
                        //Debug.Log($"{x}, {y}");
                    }
                }
            }
        }

        else if (pieceType == EBoardPieces.Tower)
        {
            (short, short) player;
            if (m_playerTurn == 1)
                player = m_player1BoardPosition;
            else
                player = m_player2BoardPosition;

            bool isInContactWithPlayer = false;

            List<(short, short)> tempList2 = new List<(short, short)>();
            for (int i = 0, x = player.Item1 - 2; i < 5; i++, x++)
            {
                for (int j = 0, y = player.Item2 - 2; j < 5; j++, y++)
                {
                    if (x < 0 || y < 0 || x > xSize - 1 || y > ySize - 1)
                        continue;

                    bool blocked = false;

                    if (x > player.Item1 - 2 && y > player.Item2 - 2 && x < player.Item1 + 2 && y < player.Item2 + 2)
                    {
                        (short, short) temp = ((short)x, (short)y);
                        if (temp == position)
                            isInContactWithPlayer = true;
                    }

                    foreach (var piece in instanciatedPieces[x, y])
                    {
                        if (piece.GetComponent<PieceInfo>().blocksSlotForTower)
                        {
                            blocked = true;
                        }
                    }
                    if (!blocked)
                    {
                        list.Add(((short)x, (short)y));
                    }
                }
            }
            if (!isInContactWithPlayer)
                return null;
        }

        else if (pieceType == EBoardPieces.Pillow)
        {
            (short, short) player;
            if (m_playerTurn == 1)
                player = m_player1BoardPosition;
            else
                player = m_player2BoardPosition;

            bool isInContactWithPlayer = false;

            List<(short, short)> tempList2 = new List<(short, short)>();
            for (int i = 0, x = player.Item1 - 2; i < 5; i++, x++)
            {
                for (int j = 0, y = player.Item2 - 2; j < 5; j++, y++)
                {
                    if (x < 0 || y < 0 || x > xSize - 1 || y > ySize - 1)
                        continue;

                    bool blocked = false;

                    if (x > player.Item1 - 2 && y > player.Item2 - 2 && x < player.Item1 + 2 && y < player.Item2 + 2)
                    {
                        (short, short) temp = ((short)x, (short)y);
                        if (temp == position)
                            isInContactWithPlayer = true;
                    }

                    var pillowCountInStack = 0;
                    foreach (var piece in instanciatedPieces[x, y])
                    {

                        if (piece.GetComponent<PieceInfo>().type == EBoardPieces.Pillow)
                            pillowCountInStack++;

                        if (pillowCountInStack == 3)
                        {
                            blocked = true;
                        }
                    }
                    if (!blocked)
                    {
                        list.Add(((short)x, (short)y));
                    }
                }
            }
            if (!isInContactWithPlayer)
                return null;
        }

        //foreach (var item in list)
        //    //Debug.Log(item);
        return list;
    }

    private bool NewPosIsValid()
    {
        var targetPosition = CurrentShadowPosition;

        if (!currentPieceAvailablePositions.Contains(targetPosition))
        {
            Debug.Log("Invalid Position");
            return false;
        }

        return true;
    }

    private void MoveShadow(GameObject piece, (short, short) destination)
    {
        if (!selectedShadow.activeSelf)
            selectedShadow.SetActive(true);

        var val1 = destination.Item1;
        var val2 = destination.Item2;

        destination.Item1 = (short)Mathf.Clamp(val1, 0, xSize - 1);
        destination.Item2 = (short)Mathf.Clamp(val2, 0, ySize - 1);

        var newHeight = CalculateSpawnHeight(destination) + selectedPiece.GetComponent<PieceInfo>().height / 2;

        if (selectedPiece.GetComponent<PieceInfo>().type == EBoardPieces.Pillow)
        {
            if (destination == m_player1BoardPosition || destination == m_player2BoardPosition)
                newHeight -= 0.575f; // must change to make cleaner code
        }



        var newPos = new Vector3(destination.Item1 - xSize / 2 + 0.5f, newHeight, destination.Item2 - ySize / 2 + 0.5f);

        CurrentShadowPosition = destination;

        piece.transform.position = newPos;
    }


    private void MovePiece((short, short) targetPosition, (short, short) oldPosition)
    {
        //int x1, int y1, int x2, int y2 -> args
        //(short, short) selectedPosition = ((short)x1, (short)y1);
        //(short, short) targetPosition = ((short)x2, (short)y2);

        var index = instanciatedPieces[oldPosition.Item1, oldPosition.Item2].Count - 1;
        GameObject pieceToMove = instanciatedPieces[oldPosition.Item1, oldPosition.Item2][index];
        var pieceType = pieceToMove.GetComponent<PieceInfo>().type;



        var newHeight = CalculateSpawnHeight(targetPosition) + pieceToMove.GetComponent<PieceInfo>().height / 2;

        if (pieceType == EBoardPieces.Pillow)
        {
            if (targetPosition == m_player1BoardPosition || targetPosition == m_player2BoardPosition)
            {
                newHeight -= 0.6f; // must change to make cleaner code
                var newPosition = new Vector3(targetPosition.Item1 - xSize / 2 + 0.5f, newHeight, targetPosition.Item2 - ySize / 2 + 0.5f);
                pieceToMove.transform.position = newPosition;
                var lastIndex = instanciatedPieces[targetPosition.Item1, targetPosition.Item2].Count - 1;
                instanciatedPieces[targetPosition.Item1, targetPosition.Item2][lastIndex].transform.position += new Vector3(0, 0.05f, 0);

                instanciatedPieces[oldPosition.Item1, oldPosition.Item2].Remove(pieceToMove);
                var temp = instanciatedPieces[targetPosition.Item1, targetPosition.Item2][lastIndex];
                instanciatedPieces[targetPosition.Item1, targetPosition.Item2].RemoveAt(lastIndex);

                instanciatedPieces[targetPosition.Item1, targetPosition.Item2].Add(pieceToMove);
                instanciatedPieces[targetPosition.Item1, targetPosition.Item2].Add(temp);

                return;
            }
        }

        var newPos = new Vector3(targetPosition.Item1 - xSize / 2 + 0.5f, newHeight, targetPosition.Item2 - ySize / 2 + 0.5f);

        pieceToMove.transform.position = newPos;

        instanciatedPieces[oldPosition.Item1, oldPosition.Item2].Remove(pieceToMove);
        instanciatedPieces[targetPosition.Item1, targetPosition.Item2].Add(pieceToMove);

        if (pieceType == EBoardPieces.Player1)
            m_player1BoardPosition = targetPosition;

        if (pieceType == EBoardPieces.Player2)
            m_player2BoardPosition = targetPosition;

        if (pieceType == EBoardPieces.Player1 || pieceType == EBoardPieces.Player2)
            e_turnStage = ETurnStage.GameOverCheck;
        else 
            e_turnStage = ETurnStage.GameOverCheck2;

        m_oldPos = defaultPos;
        m_targetPos = defaultPos;
        selectedShadow.SetActive(false);
    }

    private void HighlightAvailablePositions(List<(short, short)> positions)
    {
        if (positions == null)
            return;
        foreach (var position in positions)
        {
            var y = 0.16f;
            var pos = new Vector3(position.Item1 - xSize / 2 + 0.5f, y, position.Item2 - ySize / 2 + 0.5f);
            var newInstance = Instantiate(availableHighlight, pos, Quaternion.Euler(90, 0, 0));
            newInstance.transform.SetParent(highlightContainer);
            highlightList.Add(newInstance);

        }
    }

    private void DestroyHighlights()
    {
        foreach (var highlight in highlightList)
        {
            Destroy(highlight);
        }
    }
    #endregion SelectPiece Logic

    private void InitializeVariables()
    {
        pv = PhotonView.Get(this);
        m_playerTurn = 1;
        if(PhotonNetwork.IsConnected)
        {
            currentPlayerName.text = PhotonNetwork.MasterClient.NickName;
        }

        highlightList = new List<GameObject>();
        e_turnStage = ETurnStage.SelectPlayer;
        currentPieceAvailablePositions = new List<(short, short)>();
    }


    private void TurnManager()
    {
        switch (e_turnStage)
        {
            case ETurnStage.SelectPlayer:
                if (!IsMyTurn())
                    e_turnStage = ETurnStage.MovePlayer;
                break;

            case ETurnStage.MovePlayer:
                if (m_oldPos != defaultPos && m_targetPos != defaultPos)
                {
                    MovePiece(m_targetPos, m_oldPos);
                }
                break;

            case ETurnStage.GameOverCheck:
                Debug.Log("Running first GameOver check");
                pv.RPC("FirstGameOverCheckRPC", RpcTarget.All);
                e_turnStage = ETurnStage.SelectPiece;
                break;

            case ETurnStage.SelectPiece:
                if (!IsMyTurn())
                    e_turnStage = ETurnStage.MovePiece;
                // Managed by SelectPiece function
                break;

            case ETurnStage.MovePiece:
                if (m_oldPos != defaultPos && m_targetPos != defaultPos)
                {
                    MovePiece(m_targetPos, m_oldPos);
                }
                break;

            case ETurnStage.GameOverCheck2:
                Debug.Log("Running second GameOver check");
                pv.RPC("FirstGameOverCheckRPC", RpcTarget.All);
                e_turnStage = ETurnStage.SelectPlayer;
                // New turn

                if (m_playerTurn == 1)
                {
                    m_playerTurn = 2;
                    currentPlayerName.text = player2Name;
                    currentPlayerInstanceReference = player2Instance;
                }
                else
                {
                    currentPlayerInstanceReference = player1Instance;
                    currentPlayerName.text = player1Name;
                    m_playerTurn = 1;
                }
                // Send move piece/GameOver info RPC
                break;


            case ETurnStage.GameOver:
                Debug.Log("Game has ended");
                break;
        }
    }

    #endregion



    #region Photon Callbacks


    #endregion



    #region Remote Procedure Calls

    [PunRPC]
    private void FirstGameOverCheckRPC()
    {
        if (ScanAvailablePositions(m_player1BoardPosition, EBoardPieces.Player1).Count == 0 || ScanAvailablePositions(m_player2BoardPosition, EBoardPieces.Player2).Count == 0)
        {
            m_isGameOver = true;
            Debug.Log("Game over");
        }
    }

    [PunRPC]
    private void MovePieceRPC(int targetPosX, int targetPosY, int oldPosX, int oldPosY)
    {
        m_targetPos = ((short)targetPosX, (short)targetPosY);
        m_oldPos = ((short)oldPosX, (short)oldPosY);
    }
    #endregion
}
