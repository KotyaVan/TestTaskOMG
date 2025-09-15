using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using DefaultNamespace.Game;
using DefaultNamespace.InputManager;
using DG.Tweening;
using ElementsGame;
using ElementsGame.Views;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;

public class ElementsGameController : MonoBehaviour
{
    [SerializeField] private FieldView fieldView;
    [SerializeField] private PieceConfig pieceConfig;

    private bool _levelUpNeed;
    private FieldModel _fieldModel;
    public bool FieldIsCalm => _fieldModel.FieldCalm;

    private Action _levelCompleteHandler;
    private Action<FieldData> _stateSaveHandler;

    public void AddHandlers(Action levelCompleteHandler, Action<FieldData> stateSaveHandler)
    {
        _levelCompleteHandler = levelCompleteHandler;
        _stateSaveHandler = stateSaveHandler;
    }

    public void UpdateModel(FieldData fieldData)
    {
        _fieldModel = new FieldModel()
        {
            FieldData = fieldData
        };
        fieldView.Initialize(_fieldModel.FieldData, pieceConfig, () => { _fieldModel.PiecesLeft++; });
    }

    public async Task OnPieceSwiped(PieceView selectedPieceView, SwipeDirection direction)
    {
        if (selectedPieceView.PieceModel.Type == PieceType.Empty) return;
        if (!selectedPieceView.PieceModel.CanMove()) return;

        Vector2Int selectedPos = fieldView.GetPieceCoordinates(selectedPieceView);
        Vector2Int targetPos = selectedPos;

        switch (direction)
        {
            case SwipeDirection.SwipeRight:
                targetPos.x += 1;
                break;

            case SwipeDirection.SwipeLeft:
                targetPos.x -= 1;
                break;

            case SwipeDirection.SwipeUp:
                targetPos.y -= 1;

                var pieceView = fieldView.GetPieceAt(targetPos);
                if (pieceView && (pieceView.PieceModel.Type == PieceType.Empty || pieceView.PieceModel.IsDestroyed))
                {
                    return;
                }

                break;
            case SwipeDirection.SwipeDown:
                targetPos.y += 1;
                break;
        }

        if (!ElementsGameUtils.IsInsideBoard(targetPos.x, targetPos.y, fieldView.PiecesViews))
            return;

        var affectedBySwap = fieldView.GetPieceAt(targetPos);
        if (!affectedBySwap.PieceModel.CanMove()) return;

        _fieldModel.FieldActiveProcesses++;
        _fieldModel.FieldProcesses++;

        await fieldView.MovePiecesAsync(selectedPos, targetPos, FieldView.MoveType.Swap);

        await ProcessMatchesAndFalls(selectedPieceView, affectedBySwap);
    }

    private async Task ProcessMatchesAndFalls(PieceView piece1, PieceView piece2)
    {
        bool hasMatches;

        // Сначала ищем фишки, которые могут упасть
        var piecesToFall = new List<PieceView>();
        piecesToFall.AddRange(ElementsGameUtils.FindPiecesToFallWithPlaces(piece1, fieldView, _fieldModel.FieldData));
        piecesToFall.AddRange(ElementsGameUtils.FindPiecesToFallWithPlaces(piece2, fieldView, _fieldModel.FieldData));

        if (piecesToFall.Count > 0)
        {
            await DropFallingPieces(piecesToFall);
        }

        // Ищем заматченные фишки после падений
        var matches = ElementsGameUtils.FindMatchesForSwappedPieces(piece1, piece2, fieldView);
        matches.AddRange(ElementsGameUtils.FindMatchesForAffectedPieces(piecesToFall, fieldView));

        do
        {
            hasMatches = matches.Count > 0;

            if (hasMatches)
            {
                var fieldProcessesBeforeDestroy = _fieldModel.FieldProcesses;
                var fieldActiveProcessesBeforeDestroy = _fieldModel.FieldActiveProcesses;

                // Помечаем фишки, которые над замаченными, что им предстоит осыпание
                var piecesAboveMatches =
                    ElementsGameUtils.GetAndMarkPiecesAboveMatches(matches, fieldView,
                        fieldActiveProcessesBeforeDestroy);
                // Уничтожаем заматченные фишки
                await DestroyMatchedPieces(matches);

                //Пока шел дестрой, был еще один сваип (или закончилось уничтожение), надо пересчитать фишки, которые есть над матченными фишками
                if (_fieldModel.FieldProcesses > fieldProcessesBeforeDestroy ||
                    _fieldModel.FieldActiveProcesses < fieldActiveProcessesBeforeDestroy)
                    piecesAboveMatches =
                        ElementsGameUtils.GetAndMarkPiecesAboveMatches(matches, fieldView,
                            fieldActiveProcessesBeforeDestroy);

                // Находим фишкам для падения нужные координаты
                var piecesToFallAfterMatch =
                    ElementsGameUtils.FindPlacesForFallenPieces(piecesAboveMatches, fieldView, _fieldModel.FieldData,
                        fieldActiveProcessesBeforeDestroy);

                if (piecesToFallAfterMatch.Count > 0)
                {
                    //Осыпаем их
                    await DropFallingPieces(piecesToFallAfterMatch);

                    //Ищем, нет ли новых матчей
                    matches = new List<PieceView>();
                    matches.AddRange(
                        ElementsGameUtils.FindMatchesForAffectedPieces(piecesToFallAfterMatch, fieldView));
                }
                else
                {
                    // Если падать нечему, то просто выходим
                    break;
                }
            }
        } while (hasMatches);

        _fieldModel.FieldActiveProcesses--;
        OnFieldProcessComplete();
    }

    private async Task DropFallingPieces(List<PieceView> result)
    {
        var tasks = new List<Task>();

        foreach (var piece in result)
        {
            var coordinatesForPiece = fieldView.GetPieceCoordinates(piece);

            var task = fieldView.MovePiecesAsync(coordinatesForPiece, piece.PieceModel.PlaceForFall,
                FieldView.MoveType.Fall);
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
    }

    private async Task DestroyMatchedPieces(List<PieceView> markedAsMatched)
    {
        var uniquePieces = new HashSet<PieceView>(markedAsMatched);

        var tasks = new List<Task>();
        foreach (var view in uniquePieces)
        {
            var tcs = new TaskCompletionSource<bool>();

            view.PlayDestroyAnimation(() =>
            {
                view.PieceModel.IsMatched = false;
                view.PieceModel.IsDestroyed = true;
                _fieldModel.PiecesLeft--;
                tcs.SetResult(true);
            });

            tasks.Add(tcs.Task);
        }

        await Task.WhenAll(tasks);

        if (_fieldModel.PiecesLeft == 0)
        {
            LevelComplete();
        }
    }

    private void OnFieldProcessComplete()
    {
        if (_fieldModel.FieldActiveProcesses == 0)
        {
            if (_levelUpNeed)
            {
                _levelUpNeed = false;
                LevelComplete();
            }
            else
            {
                UpdateFieldDataFromViews();
                _stateSaveHandler.Invoke(_fieldModel.FieldData);
            }
        }
    }

    private void LevelComplete()
    {
        if (_fieldModel.FieldActiveProcesses == 0)
        {
            _levelCompleteHandler.Invoke();
        }
        else
        {
            _levelUpNeed = true;
        }
    }

    private void UpdateFieldDataFromViews()
    {
        for (int i = 0; i < _fieldModel.FieldData.FieldHeight; i++)
        {
            for (int j = 0; j < _fieldModel.FieldData.FieldWidth; j++)
            {
                var pieceView = fieldView.PiecesViews[i, j];

                if (pieceView == null || pieceView.PieceModel.IsDestroyed)
                {
                    _fieldModel.FieldData.PieceData[i, j].PieceType = PieceType.Empty;
                }
                else
                {
                    _fieldModel.FieldData.PieceData[i, j].PieceType = pieceView.PieceModel.Type;
                }
            }
        }
    }
}