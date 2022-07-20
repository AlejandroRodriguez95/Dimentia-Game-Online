using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum EBoardPieces
{
    Player1,
    Player2,
    Start1,
    Start2,
    Tower,
    Pillow,
    Teleport
}

public class Piece
{
    private EBoardPieces type;
    private int player;
    private float height;
    private GameObject prefab;
    private bool blocksSlotForPlayer; // where slot is a square of the board
    private bool blocksSlotForTower;
    //private byte maxAmountPerSlot;

    public int Player
    {
        get { return player; }
        set 
        { 
            player = Mathf.Clamp(value, 1, 2); 
        }
    }

    public bool BlocksSlotForPlayer
    {
        get { return blocksSlotForPlayer; }
    }
    public bool BlocksSlotForTower
    {
        get { return blocksSlotForTower; }
    }

    //public byte MaxAmountPerSlot
    //{
    //    get { return maxAmountPerSlot; }
    //}

    public EBoardPieces Type
    {
        get { return type; }
    }
    public float Height
    {
        get { return height; }
    }

    public GameObject Prefab
    {
        get { return prefab; }
        set 
        {  
            if(type == EBoardPieces.Player1 || type == EBoardPieces.Player2)
                this.height = value.GetComponent<Renderer>().bounds.size.y;
            else
                this.height = value.transform.lossyScale.y;

            prefab = value;
        }
    }
    
    public Piece(EBoardPieces type, int owner)
    {
        this.type = type;
        this.player = owner;
        switch (type)
        {
            case EBoardPieces.Player1:
                blocksSlotForPlayer = true;
                blocksSlotForTower = true;
                break;
            case EBoardPieces.Player2:
                blocksSlotForPlayer = true;
                blocksSlotForTower = true;
                break;
            case EBoardPieces.Tower:
                blocksSlotForTower = true;
                blocksSlotForPlayer = true;
                break;
            case EBoardPieces.Teleport:
                break;
        }
    }
}
