using System;
using System.Collections.Generic;
using DefaultNamespace;
using DefaultNamespace.InputManager;
using ElementsGame.Views;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PieceConfig", menuName = "Scriptable Objects/PieceConfig")]
public class PieceConfig : ScriptableObject
{
    public List<PieceInfo> pieces;

    public PieceView GetPieceViewByType(PieceType type)
    {
        return pieces.Find(piece => piece.pieceType == type).pieceView;
    }
}

[Serializable]
public class PieceInfo
{
    public PieceType pieceType;
    public PieceView pieceView;
}
