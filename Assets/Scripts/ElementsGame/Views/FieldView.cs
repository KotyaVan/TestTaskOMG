using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Game;
using DefaultNamespace.InputManager;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

namespace ElementsGame.Views
{
    public class FieldView : MonoBehaviour
    {
        public enum MoveType
        {
            Fall,
            Swap
        }

        private const float CellSize = 2f;
        private const float BottomY = -7f;

        public PieceView[,] PiecesViews { get; set; }
        public Dictionary<PieceView, Vector2Int> _viewToCoord = new();
        private FieldData _fieldData;
        
        public Vector2Int GetPieceCoordinates(PieceView view)
        {
            return _viewToCoord[view];
        }

        public void Initialize(FieldData fieldData, PieceConfig config, Action onPieceCreate)
        {
            CleanAll();

            int height = fieldData.FieldHeight;
            int width = fieldData.FieldWidth;
            PiecesViews = new PieceView[height, width];
            _fieldData = fieldData;

            float startX = -(width - 1) / 2f;
            float startY = BottomY + (height - 1) * CellSize;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var data = fieldData.PieceData[y, x];
                    var prefab = config.GetPieceViewByType(data.PieceType);

                    var view = Instantiate(prefab, transform);
                    view.transform.localPosition = new Vector3((startX + x) * CellSize, startY - y * CellSize, 0);
                    view.Initialize(new PieceModel(data.PieceType));
                    view.SetSpriteOrder(x, y, _fieldData.FieldHeight);

                    PiecesViews[y, x] = view;
                    _viewToCoord[view] = new Vector2Int(x, y);

                    if (data.PieceType != PieceType.Empty)
                    {
                        onPieceCreate.Invoke();
                    }
                }
            }
        }

        private Vector3 GetTransformPositionByCoord(int x, int y)
        {
            float startX = -(_fieldData.FieldWidth - 1) / 2f;
            float startY = BottomY + (_fieldData.FieldHeight - 1) * CellSize;

            var position = new Vector3(
                (startX + x) * CellSize,
                startY - y * CellSize,
                0
            );

            return position;
        }

        public PieceView GetPieceAt(Vector2Int pos)
        {
            if (!ElementsGameUtils.IsInsideBoard(pos.x, pos.y, PiecesViews)) return null;
            return PiecesViews[pos.y, pos.x];
        }

        public async Task MovePiecesAsync(Vector2Int pos1, Vector2Int pos2, MoveType moveType)
        {
            SwapPieceView(pos1, pos2, out var view1, out var view2);

            var tcs1 = new TaskCompletionSource<bool>();
            var tcs2 = new TaskCompletionSource<bool>();

            var view1Pos = moveType == MoveType.Swap
                ? view1.transform.position
                : GetTransformPositionByCoord(pos1.x, pos1.y);
            var view2Pos = moveType == MoveType.Swap
                ? view2.transform.position
                : GetTransformPositionByCoord(pos2.x, pos2.y);

            if (moveType == MoveType.Fall)
            {
                view1.Fall(view2Pos, tcs1);
                view2.Fall(view1Pos, tcs2);
            }
            else if (moveType == MoveType.Swap)
            {
                view1.PieceModel.IsSwapping = true;
                view2.PieceModel.IsSwapping = true;

                view1.Swap(view2Pos, tcs1);
                view2.Swap(view1Pos, tcs2);
            }

            view1.SetSpriteOrder(pos2.x, pos2.y, _fieldData.FieldHeight);
            view2.SetSpriteOrder(pos1.x, pos1.y, _fieldData.FieldHeight);

            await Task.WhenAll(tcs1.Task, tcs2.Task);
        }

        private void SwapPieceView(Vector2Int pos1, Vector2Int pos2, out PieceView view1, out PieceView view2)
        {
            view1 = PiecesViews[pos1.y, pos1.x];
            view2 = PiecesViews[pos2.y, pos2.x];

            PiecesViews[pos1.y, pos1.x] = view2;
            PiecesViews[pos2.y, pos2.x] = view1;

            _viewToCoord[view1] = pos2;
            _viewToCoord[view2] = pos1;
        }

        private void CleanAll()
        {
            if (PiecesViews == null) return;

            for (int i = 0; i < PiecesViews.GetLength(0); i++)
            {
                for (int j = 0; j < PiecesViews.GetLength(1); j++)
                {
                    var piece = PiecesViews[i, j];
                    Destroy(piece.gameObject);
                }
            }

            PiecesViews = null;
            _viewToCoord = new Dictionary<PieceView, Vector2Int>();
            _fieldData = null;
        }
    }
}