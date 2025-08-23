using System.Collections;
using System.Collections.Generic;
using GNPXcore;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public delegate void UpdateSquareNumber(int number);
    public static event UpdateSquareNumber onUpdateSquareNumber; 
    public static void UpdateSquareNumberMethod(int number) => onUpdateSquareNumber?.Invoke(number);

    public delegate void SquareSelected(int square_index);
    public static event SquareSelected OnSquareSelected;
    public static void SquareSelectedMethod(int square_index) => OnSquareSelected?.Invoke(square_index);

    public delegate void willSetNumber(int square_index, int value);
    public static event willSetNumber OnWillSetNumber;
    public static void willSetNumberMethod(int square_index, int value) => OnWillSetNumber?.Invoke(square_index, value);

    public delegate void didSetNumber(int square_index);
    public static event didSetNumber OnDidSetNumber;
    public static void didSetNumberMethod(int square_index) => OnDidSetNumber?.Invoke(square_index);

    public delegate void WrongNumber();
    public static event WrongNumber OnWrongNumber;
    public static void OnWrongNumberMethod() =>  OnWrongNumber?.Invoke();

    public delegate void NumberUsed(int number);
    public static event NumberUsed OnNumberUsed;
    public static void OnNumberUsedMethod(int number) => OnNumberUsed?.Invoke(number);

    public delegate void GameOver();
    public static event GameOver OnGameOver;
    public static void OnGameOverMethod() => OnGameOver?.Invoke();

    public delegate void NotesActive(bool active);
    public static event NotesActive OnNotesActive;
    public static void OnNotesActiveMethod(bool active) => OnNotesActive?.Invoke(active);

    public delegate void ClearNumber();
    public static event ClearNumber OnClearNumber;
    public static void OnClearNumberMethod() => OnClearNumber?.Invoke();

    public delegate void NumberCleared(int number);
    public static event NumberCleared OnNumberCleared;
    public static void OnNumberClearedMethod(int number) => OnNumberCleared?.Invoke(number);

    public delegate void BoardCompleted();
    public static event BoardCompleted OnBoardCompleted;
    public static void OnBoardCompletedMethod() => OnBoardCompleted?.Invoke();

    public delegate void CheckBoardCompleted();
    public static event CheckBoardCompleted OnCheckBoardCompleted;
    public static void CheckBoardCompletedMethod() => OnCheckBoardCompleted?.Invoke();

    public delegate void GiveAHint();
    public static event GiveAHint OnGiveAHint;
    public static void GiveAHintMethod() => OnGiveAHint?.Invoke();

    public delegate void DidFinishHintAd();
    public static event DidFinishHintAd OnDidFinishHintAd;
    public static void DidFinishHintAdMethod() => OnDidFinishHintAd?.Invoke();

    public delegate void GiveFastNote();
    public static event GiveFastNote OnGiveFastNote;
    public static void GiveFastNoteMethod() => OnGiveFastNote?.Invoke();

    public delegate void DidFinishLiveRewardAd();
    public static event DidFinishLiveRewardAd OnDidFinishLiveRewardAd;
    public static void DidFinishLiveRewardAdMethod() => OnDidFinishLiveRewardAd?.Invoke();

    public delegate void RewardAdFail();
    public static event RewardAdFail OnRewardAdFail;
    public static void RewardAdFailMethod() => OnRewardAdFail?.Invoke();

    public delegate void Pause(bool bPause);
    public static event Pause OnPause;
    public static void OnPauseMethod(bool bPause) => OnPause?.Invoke(bPause);

    public delegate void SDKsolverCompleted(UPuzzle pGP);
    public static event SDKsolverCompleted OnSDKsolverCompleted;
    public static void OnSDKsolverCompletedMethod(UPuzzle pGP) => OnSDKsolverCompleted?.Invoke(pGP);

    public delegate void ApplyHint();
    public static event ApplyHint OnApplyHint;
    public static void ApplyHintMethod() => OnApplyHint?.Invoke();

    public delegate void UpdateHintBadge();
    public static event UpdateHintBadge OnUpdateHintBadge;
    public static void OnUpdateHintBadgeMethod() => OnUpdateHintBadge?.Invoke();

    public delegate void ChangeWaittingState(bool bWaitting);
    public static event ChangeWaittingState OnChangeWaittingState;
    public static void OnChangeWaittingStateMethod(bool bWaitting) => OnChangeWaittingState?.Invoke(bWaitting);

    public delegate void SaveProgressData();
    public static event SaveProgressData OnSaveProgressData;
    public static void OnSaveProgressDataMethod() => OnSaveProgressData?.Invoke();

}
