using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceInfo : MonoBehaviour
{
    public float height;
    public EBoardPieces type;
    public bool blocksSlotForPlayer; // where slot is a square of the board
    public bool blocksSlotForTower;
    public byte maxAmountPerSlot;
}
