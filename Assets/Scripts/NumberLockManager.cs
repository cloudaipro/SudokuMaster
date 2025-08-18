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
        
        // Apply locked appearance
        ApplyLockedAppearance();
        
        Debug.Log($"Number {number} locked");
    }
    
    public void UnlockNumber()
    {
        if (lockedButton != null)
        {
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
    
    private void ApplyLockedAppearance()
    {
        if (lockedButton != null)
        {
            // Change background color
            lockedButton.backgroundGraphic.color = lockedBackgroundColor;
            
            // Change text color
            Text numberText = lockedButton.GetComponentInChildren<Text>();
            if (numberText != null)
            {
                numberText.color = lockedTextColor;
            }
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
        }
    }
    
    private void OnSquareSelected(int selectedIndex)
    {
        // If a number is locked and a cell is selected, try to place the locked number
        if (HasLockedNumber())
        {
            // The locked number will be automatically placed by the modified GameEvents system
            // This is handled in the SudokuCell modification
        }
    }
    
    private void OnUpdateSquareNumber(int number)
    {
        // This handles the normal number input through number buttons
        // If a number is locked, we don't interfere with normal operation
    }
}