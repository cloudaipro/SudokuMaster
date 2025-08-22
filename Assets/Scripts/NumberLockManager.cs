using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberLockManager : MonoBehaviour
{
    // Number Lock feature implementation
    [Header("Lock Settings")]
    public Color lockedBackgroundColor = new Color(70f/255f, 93f/255f, 170f/255f, 1f); // RGB(70, 93, 170)
    public Color lockedTextColor = Color.white;
    
    [Header("References")]
    public Transform numberButtonsContainer;
    
    // Lock state
    private int lockedNumber = -1;
    private NumberButton lockedButton = null;
    private Color originalBackgroundColor;
    private Color originalTextColor;
    private Color originalSubTextColor;
    
    // Auto indication integration
    private SudokuBoard sudokuBoard;
    
    // Singleton pattern
    public static NumberLockManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Find number buttons container if not assigned
        if (numberButtonsContainer == null)
        {
            numberButtonsContainer = GameObject.Find("NumberButtons")?.transform;
        }
        
        // Find SudokuBoard reference for auto indication
        sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null)
        {
            Debug.Log("NumberLockManager: SudokuBoard reference found successfully");
        }
        else
        {
            Debug.LogWarning("NumberLockManager: SudokuBoard reference NOT found!");
        }
    }
    
    private void OnEnable()
    {
        // Subscribe to game events
        GameEvents.OnSquareSelected += OnSquareSelected;
        GameEvents.onUpdateSquareNumber += OnUpdateSquareNumber;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from game events
        GameEvents.OnSquareSelected -= OnSquareSelected;
        GameEvents.onUpdateSquareNumber -= OnUpdateSquareNumber;
    }
    
    public bool IsNumberLocked(int number)
    {
        return lockedNumber == number;
    }
    
    public int GetLockedNumber()
    {
        return lockedNumber;
    }
    
    public bool HasLockedNumber()
    {
        return lockedNumber != -1;
    }
    
    public void LockNumber(int number, NumberButton button)
    {
        // Check if this is Hell Level - Fast Notes (Number Lock) are disabled
        if (sudokuBoard != null && sudokuBoard.IsHellLevel())
        {
            Debug.Log("Fast Notes (Number Lock) are disabled in Hell Level");
            return;
        }
        
        // Unlock previous number if any
        UnlockNumber();
        
        // Lock the new number
        lockedNumber = number;
        lockedButton = button;
        
        // Store original colors
        originalBackgroundColor = button.backgroundGraphic.color;
        Text numberText = button.GetComponentInChildren<Text>();
        if (numberText != null)
        {
            originalTextColor = numberText.color;
        }
        
        // Store original sub_text color
        originalSubTextColor = button.GetOriginalSubTextColor();
        
        // Apply locked appearance
        ApplyLockedAppearance();
        
        // Trigger auto indication for locked number
        TriggerAutoIndicationForLocked(number);
        
        Debug.Log($"Number {number} locked");
    }
    
    public void UnlockNumber()
    {
        if (lockedButton != null)
        {
            // Clear auto indication before unlocking
            ClearAutoIndication();
            
            // Restore original appearance
            RestoreOriginalAppearance();
            
            Debug.Log($"Number {lockedNumber} unlocked");
        }
        
        // Reset lock state
        lockedNumber = -1;
        lockedButton = null;
    }
    
    public void ToggleLock(int number, NumberButton button)
    {
        if (IsNumberLocked(number))
        {
            UnlockNumber();
        }
        else
        {
            LockNumber(number, button);
        }
    }
    
    public void SwitchLockedNumber(int newNumber, NumberButton newButton)
    {
        // Clear auto indication for current locked number
        if (HasLockedNumber())
        {
            ClearAutoIndication();
            
            // Restore appearance of previously locked button
            if (lockedButton != null)
            {
                RestoreOriginalAppearance();
            }
        }
        
        // Lock the new number
        lockedNumber = newNumber;
        lockedButton = newButton;
        
        // Store original colors of new button
        originalBackgroundColor = newButton.backgroundGraphic.color;
        Text numberText = newButton.GetComponentInChildren<Text>();
        if (numberText != null)
        {
            originalTextColor = numberText.color;
        }
        
        // Store original sub_text color of new button
        originalSubTextColor = newButton.GetOriginalSubTextColor();
        
        // Apply locked appearance to new button
        ApplyLockedAppearance();
        
        // Trigger auto indication for new locked number
        TriggerAutoIndicationForLocked(newNumber);
        
        Debug.Log($"Lock switched to {newNumber}");
    }
    
    private void ApplyLockedAppearance()
    {
        if (lockedButton != null)
        {
            // Stop any running transitions that might override the locked appearance
            lockedButton.StopTransitionsForLock();
            
            // Change background color
            lockedButton.backgroundGraphic.color = lockedBackgroundColor;
            
            // Change text color
            Text numberText = lockedButton.GetComponentInChildren<Text>();
            if (numberText != null)
            {
                numberText.color = lockedTextColor;
            }
            
            // Change sub_text color to white
            lockedButton.SetSubTextColor(Color.white);
            Debug.Log($"Set sub_text color to white for locked number {lockedNumber}");
        }
    }
    
    private void RestoreOriginalAppearance()
    {
        if (lockedButton != null)
        {
            // Restore background color
            lockedButton.backgroundGraphic.color = originalBackgroundColor;
            
            // Restore text color
            Text numberText = lockedButton.GetComponentInChildren<Text>();
            if (numberText != null)
            {
                numberText.color = originalTextColor;
            }
            
            // Restore original sub_text color
            lockedButton.SetSubTextColor(originalSubTextColor);
            Debug.Log($"Restored original sub_text color for unlocked number");
        }
    }
    
    // Auto indication methods for lock integration
    private void TriggerAutoIndicationForLocked(int number)
    {
        if (sudokuBoard != null)
        {
            Debug.Log($"Triggering auto indication for locked number {number}");
            
            // CRITICAL FIX: Clear ALL cell selections and highlights first
            // This ensures no previous cell highlighting conflicts with lock highlighting
            sudokuBoard.ClearAllCellSelections();
            
            // Highlight all cells with the locked number (this also clears existing highlights)
            sudokuBoard.HighlightAllCellsWithNumber(number);
        }
        else
        {
            Debug.LogWarning("SudokuBoard reference not found - cannot trigger auto indication");
        }
    }
    
    private void ClearAutoIndication()
    {
        if (sudokuBoard != null)
        {
            Debug.Log("Clearing auto indication for unlocked number");
            // Clear the highlighting by setting DesiredNumber to -1 for all cells
            sudokuBoard.ClearAllHighlights();
        }
        else
        {
            Debug.LogWarning("SudokuBoard reference not found - cannot clear auto indication");
        }
    }
    
    private void OnSquareSelected(int selectedIndex)
    {
        // If a number is locked and a cell is selected, try to place the locked number
        if (HasLockedNumber())
        {
            // The locked number will be automatically placed by the modified GameEvents system
            // This is handled in the SudokuCell modification
            
            // After a short delay, check if auto-unlock is needed
            // We use a coroutine to ensure the cell placement is processed first
            StartCoroutine(DelayedAutoUnlockCheck());
        }
    }
    
    private IEnumerator DelayedAutoUnlockCheck()
    {
        // Wait one frame to ensure cell placement is processed
        yield return null;
        
        // Check if the locked number is now complete
        if (HasLockedNumber())
        {
            CheckForAutoUnlock(lockedNumber);
        }
    }
    
    private void OnUpdateSquareNumber(int number)
    {
        // Check for auto-unlock when a number is placed
        CheckForAutoUnlock(number);
    }
    
    private void CheckForAutoUnlock(int placedNumber)
    {
        // Only check if we have a locked number and it's the same as the one being placed
        if (HasLockedNumber() && lockedNumber == placedNumber)
        {
            // Check if the locked number is now completely placed
            if (sudokuBoard != null && sudokuBoard.IsNumberCompletlyPlaced(lockedNumber))
            {
                // Auto-unlock the number
                Debug.Log($"Auto-unlocking number {lockedNumber} - all instances placed");
                UnlockNumber();
                
                // Optional: Add visual feedback for auto-unlock
                ShowAutoUnlockFeedback();
            }
        }
    }
    
    private void ShowAutoUnlockFeedback()
    {
        // Optional: Add subtle visual feedback for auto-unlock
        // This could be a brief color flash, animation, or other indicator
        Debug.Log("Number auto-unlocked - all instances complete");
    }
}