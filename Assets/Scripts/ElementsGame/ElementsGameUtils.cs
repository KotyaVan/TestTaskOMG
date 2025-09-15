using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DefaultNamespace.InputManager;
using DG.Tweening;
using ElementsGame.Views;
using Unity.VisualScripting;
using UnityEngine;

namespace DefaultNamespace.Game
{
    public static class ElementsGameUtils
    {
        public static List<PieceView> FindMatchesForAffectedPieces(List<PieceView> pieces, FieldView fieldView)
        {
            var allMatchedSet = new HashSet<PieceView>();

            for (int i = 0; i < pieces.Count; i++)
            {
                var piece = pieces[i];
                var horizontalMatchPiece = FindMatches(
                    piece,
                    1, 0,
                    fieldView.PiecesViews,
                    fieldView._viewToCoord
                );
                var verticalMatchPiece = FindMatches(
                    piece,
                    0, 1,
                    fieldView.PiecesViews,
                    fieldView._viewToCoord
                );
                allMatchedSet.UnionWith(horizontalMatchPiece);
                allMatchedSet.UnionWith(verticalMatchPiece);
            }

            return allMatchedSet.ToList();
        }

        public static List<PieceView> FindMatchesForSwappedPieces(PieceView piece1, PieceView piece2,
            FieldView fieldView)
        {
            var horizontalMatchPiece1 = FindMatches(
                piece1,
                1, 0,
                fieldView.PiecesViews,
                fieldView._viewToCoord
            );
            var verticalMatchPiece1 = FindMatches(
                piece1,
                0, 1,
                fieldView.PiecesViews,
                fieldView._viewToCoord
            );
            var horizontalMatchPiece2 = FindMatches(
                piece2,
                1, 0,
                fieldView.PiecesViews,
                fieldView._viewToCoord
            );
            var verticalMatchPiece2 = FindMatches(
                piece2,
                0, 1,
                fieldView.PiecesViews,
                fieldView._viewToCoord
            );

            var allMatchedSet = new HashSet<PieceView>();
            allMatchedSet.UnionWith(horizontalMatchPiece1);
            allMatchedSet.UnionWith(verticalMatchPiece1);
            allMatchedSet.UnionWith(horizontalMatchPiece2);
            allMatchedSet.UnionWith(verticalMatchPiece2);

            return allMatchedSet.ToList();
        }

        private static List<PieceView> FindMatches(
            PieceView start,
            int dx,
            int dy,
            PieceView[,] pieces,
            Dictionary<PieceView, Vector2Int> viewToCoord)
        {
            if (!viewToCoord.TryGetValue(start, out Vector2Int startCoord))
            {
                Debug.LogWarning("Start piece not found in coordinate dictionary.");
                return new List<PieceView>();
            }

            List<PieceView> linearMatches = new List<PieceView>();

            if (!CanBeMatched(start)) return linearMatches;

            linearMatches.Add(start);
            PieceType type = start.PieceModel.Type;

            int x = startCoord.x + dx;
            int y = startCoord.y + dy;

            while (IsInsideBoard(x, y, pieces) && CanBeMatched(pieces[y, x]) && pieces[y, x].PieceModel.Type == type)
            {
                linearMatches.Add(pieces[y, x]);
                x += dx;
                y += dy;
            }

            x = startCoord.x - dx;
            y = startCoord.y - dy;

            while (IsInsideBoard(x, y, pieces) && CanBeMatched(pieces[y, x]) && pieces[y, x].PieceModel.Type == type)
            {
                linearMatches.Add(pieces[y, x]);
                x -= dx;
                y -= dy;
            }

            return linearMatches.Count >= 3
                ? ExpandMatches(linearMatches, type, pieces, viewToCoord)
                : new List<PieceView>();
        }

        private static List<PieceView> ExpandMatches(
            List<PieceView> baseMatches,
            PieceType type,
            PieceView[,] pieces,
            Dictionary<PieceView, Vector2Int> viewToCoord)
        {
            HashSet<PieceView> expanded = new HashSet<PieceView>(baseMatches);
            Queue<PieceView> queue = new Queue<PieceView>(baseMatches);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!viewToCoord.TryGetValue(current, out Vector2Int coord))
                    continue;
                current.PieceModel.IsMatched = true;

                var neighbors = new List<Vector2Int>
                {
                    new Vector2Int(coord.x - 1, coord.y),
                    new Vector2Int(coord.x + 1, coord.y),
                    new Vector2Int(coord.x, coord.y - 1),
                    new Vector2Int(coord.x, coord.y + 1)
                };

                foreach (var n in neighbors)
                {
                    if (IsInsideBoard(n.x, n.y, pieces))
                    {
                        var neighbor = pieces[n.y, n.x];
                        if (neighbor.PieceModel.Type == type && CanBeMatched(neighbor) && !expanded.Contains(neighbor))
                        {
                            expanded.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }

            return expanded.ToList();
        }

        public static bool IsInsideBoard(int x, int y, PieceView[,] pieces)
        {
            int height = pieces.GetLength(0);
            int width = pieces.GetLength(1);
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        private static bool CanBeMatched(PieceView pieceView)
        {
            return pieceView.PieceModel.Type != PieceType.Empty &&
                   !pieceView.PieceModel.IsSwapping &&
                   !pieceView.PieceModel.IsDestroyed &&
                   !pieceView.PieceModel.IsMatched &&
                   !pieceView.PieceModel.IsFalling;
        }

        public static List<PieceView> FindPiecesToFallWithPlaces(
            PieceView movedPiece,
            FieldView fieldView,
            FieldData fieldData)
        {
            var result = new List<PieceView>();

            if (movedPiece.PieceModel.Type == PieceType.Empty || movedPiece.PieceModel.IsDestroyed)
            {
                //Пустая фишка — ищем тех, кто может упасть на ее место
                //То есть перебираем фишки, которые выше
                CollectUpPieces(movedPiece, result, fieldView);

                if (result.Count > 0)
                {
                    movedPiece.PieceModel.IsFalling = true;
                    for (int i = 0; i < result.Count; i++)
                    {
                        var upperPiece = result[i];
                        var coord = i == 0
                            ? fieldView.GetPieceCoordinates(movedPiece)
                            : fieldView.GetPieceCoordinates(result[i - 1]);

                        upperPiece.PieceModel.IsFalling = true;
                        upperPiece.PieceModel.PlaceForFall = coord;
                    }
                }
            }
            else
            {
                // Фишка не пустая — ищем, может ли она упасть вниз
                var coordinates = fieldView.GetPieceCoordinates(movedPiece);

                if (coordinates.y < fieldData.FieldHeight - 1)
                {
                    CollectDownPieces(movedPiece, result, fieldView, fieldData);
                }

                if (result.Count > 0)
                {
                    var downestPiece = result[^1];
                    downestPiece.PieceModel.IsFalling = true;

                    var coord = fieldView.GetPieceCoordinates(downestPiece);
                    movedPiece.PieceModel.IsFalling = true;
                    movedPiece.PieceModel.PlaceForFall = coord;
                    result = new List<PieceView> { movedPiece };
                }
            }

            return result;
        }

        private static void CollectUpPieces(
            PieceView movedPiece,
            List<PieceView> pieceViews,
            FieldView fieldView)
        {
            var upPieceView = GetUpPieceView(movedPiece, fieldView);
            if (upPieceView != null && upPieceView.PieceModel.Type != PieceType.Empty)
            {
                pieceViews.Add(upPieceView);
                CollectUpPieces(upPieceView, pieceViews, fieldView);
            }
        }

        private static PieceView GetUpPieceView(PieceView currentPiece, FieldView fieldView)
        {
            var coordinates = fieldView.GetPieceCoordinates(currentPiece);
            if (coordinates.y - 1 >= 0)
            {
                return fieldView.PiecesViews[coordinates.y - 1, coordinates.x];
            }

            return null;
        }

        private static void CollectDownPieces(
            PieceView movedPiece,
            List<PieceView> emptyCells,
            FieldView fieldView,
            FieldData fieldData)
        {
            var downPieceView = GetDownPieceView(movedPiece, fieldView, fieldData);
            if (downPieceView != null &&
                (downPieceView.PieceModel.Type == PieceType.Empty || downPieceView.PieceModel.IsDestroyed))
            {
                emptyCells.Add(downPieceView);
                CollectDownPieces(downPieceView, emptyCells, fieldView, fieldData);
            }
        }

        private static PieceView GetDownPieceView(PieceView currentPiece, FieldView fieldView, FieldData fieldData)
        {
            var coordinates = fieldView.GetPieceCoordinates(currentPiece);
            if (coordinates.y + 1 < fieldData.FieldHeight)
            {
                return fieldView.PiecesViews[coordinates.y + 1, coordinates.x];
            }

            return null;
        }

        public static List<PieceView> GetAndMarkPiecesAboveMatches(
            List<PieceView> matchedPieces,
            FieldView fieldView, int fieldProcess)
        {
            var piecesAbove = new HashSet<PieceView>();

            foreach (var matchedPiece in matchedPieces)
            {
                var coord = fieldView.GetPieceCoordinates(matchedPiece);
                int x = coord.x;
                int y = coord.y;

                // Идём вверх по столбцу, собираем все не пустые и не уничтоженные фишки
                for (int currentY = y - 1; currentY >= 0; currentY--)
                {
                    var pieceAbove = fieldView.PiecesViews[currentY, x];

                    if (pieceAbove.PieceModel.Type != PieceType.Empty && !pieceAbove.PieceModel.IsDestroyed &&
                        !pieceAbove.PieceModel.IsMatched)
                    {
                        pieceAbove.PieceModel.IsFalling = true;
                        pieceAbove.PieceModel.FallingInFieldProcess = fieldProcess;
                        piecesAbove.Add(pieceAbove);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return piecesAbove.ToList();
        }

        public static List<PieceView> FindPlacesForFallenPieces(
            List<PieceView> pieces,
            FieldView fieldView,
            FieldData fieldData,
            int fieldProcess)
        {
            var allResult = new List<PieceView>();

            // Сортируем снизу вверх
            var sortedPieces = pieces.OrderByDescending(piece => fieldView.GetPieceCoordinates(piece).y).ToList();

            var occupiedPositions = new HashSet<Vector2Int>();

            foreach (var piece in sortedPieces)
            {
                var coord = fieldView.GetPieceCoordinates(piece);
                int x = coord.x;
                int y = coord.y;

                int targetY = y;

                while (targetY + 1 < fieldData.FieldHeight)
                {
                    var belowPiece = fieldView.PiecesViews[targetY + 1, x];
                    var positionBelow = new Vector2Int(x, targetY + 1);

                    bool isEmptyOrDestroyedOrFalling = belowPiece.PieceModel.Type == PieceType.Empty ||
                                                       belowPiece.PieceModel.IsDestroyed ||
                                                       belowPiece.PieceModel.IsFalling && belowPiece.PieceModel.FallingInFieldProcess == fieldProcess;

                    bool isOccupied = occupiedPositions.Contains(positionBelow);

                    if (!isEmptyOrDestroyedOrFalling || isOccupied)
                        break;

                    targetY++;
                }

                if (targetY != y)
                {
                    var newPlace = new Vector2Int(x, targetY);
                    piece.PieceModel.PlaceForFall = newPlace;
                    piece.PieceModel.IsFalling = true;
                    allResult.Add(piece);
                    occupiedPositions.Add(newPlace);
                }
            }

            return allResult;
        }
    }
}