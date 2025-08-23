using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SudokuCell : Selectable, IPointerDownHandler //IPointerClickHandler, ISubmitHandler, IPointerUpHandler, IPointerExitHandler
{
    public GameObject number_text;
 
    public GameObject Notes_panel;
    public List<GameObject> number_notes;
    private bool note_active = false;
    public int Number { get; set; } = 0;
    public int Correct_number { get; set; } = 0;
    public int Cell_index { get; set; } = -1;
    public int Row_index { get; set; } = -1;
    public int Column_index { get; set; } = -1;
    public int Block_index { get; set; } = -1;

    public bool Has_default_value { get; set; } = false;
    public bool Has_Wrong_value { get; set; } = false;
    public bool IsCorrectNumberSet() => Number == Correct_number;
    public bool Impossible_Cell = false;    

    public Color Selected_Color = Color.grey;
    public Color Indicated_Color = Color.grey;
    public Color HintMode_Indicated_Color = Color.grey;
    public Color Hilghted_SameNumber_Color = Color.grey;
    public Color Default_Color = Color.blue;
    public Color Correct_Color = Color.blue;
    public Color Wrong_Color = Color.red;
    public Color Focus_Color = Color.cyan;
    public Color HintMode_Focus_Color = Color.cyan;
    //public Color Hypothesis_Color = new Color(0.8f, 0.6f, 0.2f, 1f); // Orange for hypothesis numbers
    public Color Hypothesis_Text_Color = new Color(0.736f, 0.64f, 0.156f, 1f);//Color(1f, 0.9f, 0.7f, 1f); // Light orange text   

    public float sizeScaleForFocus = 1.1f;

    public bool bHintMode = false;
    public bool bFocusBlockInHintMode = false;
    public bool bFocusWholeRowInHintMode = false;
    public bool bFocusWholeColInHintMode = false;
    public bool bFocusCellInHintMode = false;
    public bool bHilightedCellInHintMode = false;

    public GameObject TopLine;
    public GameObject BottomLine;
    public GameObject RightLine;
    public GameObject LeftLine;

    private bool _IsSelect = false;
    public bool IsSelected { get => _IsSelect;
        set
        {
            _IsSelect = value;
            //Debug.Log("IsSelect= " + value.ToString());
        }
    }
    public bool Indicated { get; set; } = false;
    //public bool HightlightedSameNumber { get; set; } = false;
    public int DesiredNumnber { get; set; } = -1;
    
    // Number Lock functionality
    private NumberLockManager lockManager;
    
    // Hell Level support
    private ValidationContext validationContext;
    private bool isHypothesisNumber = false;

    void Start()
    {
        note_active = false;

        //(transform as RectTransform).Also( x => image.rectTransform.sizeDelta = new Vector2(x.rect.width, x.rect.height));

        if (GameSettings.Instance.ContinuePreviousGame == false)
        //    SetClearEmptyNotes();
        //else
            ClearupAllNotes();
            
        // Find NumberLockManager reference
        lockManager = FindObjectOfType<NumberLockManager>();
        
        // Find ValidationContext for Hell Level support
        var sudokuBoard = FindObjectOfType<SudokuBoard>();
        if (sudokuBoard != null)
        {
            validationContext = sudokuBoard.GetValidationContext();
        }
    }

    private void OnEnable()
    {
        GameEvents.onUpdateSquareNumber += OnSetNumber;
        GameEvents.OnSquareSelected += OnSquareSelected;
        GameEvents.OnNotesActive += OnNotesActive;
        GameEvents.OnClearNumber += OnClearNumber;
        GameEvents.OnGameOver += OnGameOver;
        GameEvents.OnPause += OnPause;
    }
    private void OnDisable()
    {
        GameEvents.onUpdateSquareNumber -= OnSetNumber;
        GameEvents.OnSquareSelected -= OnSquareSelected;
        GameEvents.OnNotesActive -= OnNotesActive;
        GameEvents.OnClearNumber -= OnClearNumber;
        GameEvents.OnGameOver -= OnGameOver;
        GameEvents.OnPause -= OnPause;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (GameSettings.Instance.bBoardInteractable)
        {
            GameEvents.SquareSelectedMethod(Cell_index);
            
            // Check for locked number after selection
            if (lockManager != null && lockManager.HasLockedNumber() && !Has_default_value)
            {
                int lockedNumber = lockManager.GetLockedNumber();
                
                // Only input if the cell is empty or has the wrong value
                if (Number == 0 || Has_Wrong_value)
                {
                    GameEvents.UpdateSquareNumberMethod(lockedNumber);
                }
            }
        }
    }

    public void setCellSize(float size)
    {
        (transform as RectTransform).Also(rt =>
        {
            rt.sizeDelta = new Vector2(size, size);
            //image.rectTransform.sizeDelta = new Vector2(size, size);
        });
    }
    
    public List<int> GetSquareNotes()
    {
        //number_notes.ForEachWithIndex((n, idx) =>
        //{
        //    if (n.GetComponent<SudokuNote>().bSetted)
        //        Debug.Log($"{idx + 1},{n.activeInHierarchy.ToString()}");
        //});        
        return number_notes.Select((x, idx) => new { bSetted = x.GetComponent<SudokuNote>().bSetted, value = idx + 1})
                           .Where(x => x.bSetted)
                           .Select(x => x.value).ToList();
        //List<int> notes = new List<int>();
        //foreach (var number in number_notes)
        //{
        //    string note = number.GetComponent<Text>().text;
        //    notes.Add(note == " " ? 0 : int.Parse(note));
        //}
        //return notes;
    }

    public void SetCorrectNumber(int number)
    {
        Correct_number = number;
        Has_Wrong_value = false;
        if (Number != 0 && Number != Correct_number) {
            Has_Wrong_value = true;
            UpdateSquareColor();
        }
    }
    public void SetCorrectNumber()
    {
        Number = Correct_number;
        ClearupAllNotes();
        DisplayText();
    }    
    private void SetClearEmptyNotes()
    {
        number_notes.ForEach(x =>
        {
            x.GetComponent<SudokuNote>().Also(note =>
            {
                note.bHighlight = false;
                note.bSetted = false;
                note.bAlarm = false;
            });
        });
        //number_notes.ForEach(x =>
        //{
        //    var textObj = x.GetComponent<Text>();
        //    if (textObj.text == "0")
        //        textObj.text = " ";
        //});
    }
    public void ClearupAllNotes()
    {
        number_notes.ForEach(x =>
        {
            x.GetComponent<SudokuNote>().Also(note =>
            {
                note.bHighlight = false;
                note.bAlarm = false;
                note.bSetted = false;
            });
        });
        //number_notes.ForEach(x =>
        //{
        //    var textObj = x.GetComponent<Text>();
        //    textObj.text = " ";
        //});
    }
    private void SetNoteSingleNumberValue(int value, bool force_update = false)
    {
        if (!note_active && !force_update) return;
        if (value <= 0) return;

        number_notes[value - 1].GetComponent<SudokuNote>().Also(note =>
        {
            if (note.bSetted == false || force_update)
                note.bSetted = true;
            else
                note.bSetted = false;  // clear when press note again
        });

        //var textObj = number_notes[value - 1].GetComponent<Text>();

        //if (textObj.text == " " || force_update)
        //    textObj.text = value.ToString();
        //else // clear when press note again
        //    textObj.text = " ";
    }
    public void RemoveNote(int value) => number_notes[value - 1].GetComponent<SudokuNote>().bSetted = false; //GetComponent<Text>().text = " ";
    public void SetCellNotes(List<int> notes) => notes.ForEach(value => SetNoteSingleNumberValue(value, true));
    public void HighLightNote(int value) => number_notes[value - 1].GetComponent<SudokuNote>().bHighlight = true;
    public void AlarmNote(int value) => number_notes[value - 1].GetComponent<SudokuNote>().bAlarm = true;
    //public void SetCellNotes(List<int> notes) => notes.ForEachWithIndex((value, idx) =>
    //{
    //    if (value == 0)
    //        RemoveNote(idx + 1);
    //    else
    //        SetNoteSingleNumberValue(value, true);
    //});
    public void OnNotesActive(bool active) => note_active = active;
    public void DisplayText()
    {
        if (Number <= 0)
            number_text.GetComponent<Text>().text = " ";
        else
            number_text.GetComponent<Text>().text = Number.ToString();
        Notes_panel.SetActive(Number <= 0);
    }
    public void SetNumber(int number)
    {
        Number = number;
        DisplayText();
    }
        
    private void OnGameOver()
    {
        if (Number != 0 && Number != Correct_number)
        {
            Has_Wrong_value = false;            
            Number = 0;
            DisplayText();
        }
        UpdateSquareColor();
    }

    public void OnClearNumber()
    {
        if (IsSelected && !Has_default_value)
        {
            if (isHypothesisNumber || !IsCorrectNumberSet()) {
                int clearedNumber = Number; // Capture the number before clearing
                Number = 0;
                Has_Wrong_value = false;
                isHypothesisNumber = false; // Clear hypothesis state
                UpdateSquareColor();
                DisplayText();
                
                // Notify about the cleared number if it was not empty
                if (clearedNumber > 0)
                {
                    GameEvents.OnNumberClearedMethod(clearedNumber);
                }
            }
        }
    }

    public void OnSetNumber(int number)
    {
        if (IsSelected && !Has_default_value)
        {
            if (note_active && !Has_Wrong_value)
            {
                SetNoteSingleNumberValue(number);
            }
            else if (!note_active)
            {
                if (IsCorrectNumberSet())
                    return;
                
                GameEvents.willSetNumberMethod(Cell_index, number);
                SetNumber(number);
                
                // Use ValidationContext if available, otherwise use traditional logic
                if (validationContext != null && validationContext.IsInitialized)
                {
                    // Process move through ValidationContext (Strategy Pattern)
                    ValidationResult result = validationContext.ProcessMove(Cell_index, number);
                    
                    // Handle result based on strategy
                    if (validationContext.IsHellLevel)
                    {
                        // Hell Level: Mark as hypothesis number
                        isHypothesisNumber = true;
                        Has_Wrong_value = false; // Don't mark as wrong in hypothesis mode
                        
                        // Still track number usage for UI updates (sub_value decrements)
                        GameEvents.OnNumberUsedMethod(number);
                        
                        // Don't clear notes or trigger completion in hypothesis mode
                        // Visual feedback will distinguish hypothesis vs confirmed numbers
                    }
                    else if (result.Message == "Legacy validation active")
                    {
                        // ValidationContext not fully initialized - use legacy logic
                        if (number == Correct_number)
                        {
                            Has_Wrong_value = false;
                            isHypothesisNumber = false;
                            ClearupAllNotes();
                            GameEvents.didSetNumberMethod(Cell_index);
                            GameEvents.OnNumberUsedMethod(number);
                            GameEvents.CheckBoardCompletedMethod();
                        }
                        else
                        {
                            Has_Wrong_value = true;
                            isHypothesisNumber = false;
                            GameEvents.OnNumberUsedMethod(number);
                            GameEvents.OnWrongNumberMethod();
                        }
                    }
                    else
                    {
                        // Normal levels: Use immediate validation result
                        if (result.Type == ValidationResultType.Success)
                        {
                            Has_Wrong_value = false;
                            isHypothesisNumber = false;
                            ClearupAllNotes();
                            GameEvents.didSetNumberMethod(Cell_index);
                            GameEvents.OnNumberUsedMethod(number);
                            GameEvents.CheckBoardCompletedMethod();
                        }
                        else
                        {
                            Has_Wrong_value = true;
                            isHypothesisNumber = false;
                            GameEvents.OnNumberUsedMethod(number);
                            GameEvents.OnWrongNumberMethod();
                        }
                    }
                }
                else
                {
                    // Fallback to original logic when ValidationContext is not available
                    if (number == Correct_number)
                    {
                        Has_Wrong_value = false;
                        isHypothesisNumber = false;
                        ClearupAllNotes();
                        GameEvents.didSetNumberMethod(Cell_index);
                        GameEvents.OnNumberUsedMethod(number);
                        GameEvents.CheckBoardCompletedMethod();
                    }
                    else
                    {                    
                        Has_Wrong_value = true;
                        isHypothesisNumber = false;
                        GameEvents.OnNumberUsedMethod(number);
                        GameEvents.OnWrongNumberMethod();
                    }
                }
            }            
        }
        UpdateSquareColor();
    }
    public void OnSquareSelected(int selectedIndex)
    {
        IsSelected = (selectedIndex == Cell_index);
    }

    public void UpdateSquareColor()
    {
        var colors = this.colors;

        colors.disabledColor = Color.white;        

        if (GameSettings.Instance.Pause)
        {
            colors.disabledColor = Color.white;
        }
        else if (IsSelected) {            
            colors.disabledColor = Selected_Color;            
        }
        else if (DesiredNumnber > 0 && DesiredNumnber == Number) //HightlightedSameNumber)
        {
            colors.disabledColor = Hilghted_SameNumber_Color;
        }
        else if (Indicated) {
            colors.disabledColor = (bHintMode) ? (bHilightedCellInHintMode) ? HintMode_Focus_Color : HintMode_Indicated_Color
                                               : Indicated_Color;
        }
        // else if (isHypothesisNumber && Number != 0) {
        //     colors.disabledColor = Hypothesis_Color; // Orange background for hypothesis numbers
        // }
        this.colors = colors;

        // note color
        if (number_text.GetComponent<Text>().text != " ")
            Notes_panel.SetActive(false);
        else
            Notes_panel.SetActive(Number <= 0);
        number_notes.Select(x => x.GetComponent<SudokuNote>()).ForEachWithIndex((x, idx) => x.bHighlight = (DesiredNumnber == idx + 1));

        number_text.GetComponent<Text>().Also(x => {
            var forground_color = x.color;
            if (bHintMode)
            {
                if (Has_Wrong_value)
                    forground_color = number_text.GetComponent<Text>().text == Number.ToString() ? Wrong_Color : Default_Color;
                else
                    forground_color = (Impossible_Cell) ? Color.grey :
                                      (Indicated) ? Default_Color : Color.grey;
            }
            else
            {
                if (Has_default_value)
                    forground_color = Default_Color;
                else if (isHypothesisNumber)
                    forground_color = Hypothesis_Text_Color; // Orange text for hypothesis numbers
                else if (Has_Wrong_value)
                    forground_color = Wrong_Color;
                else
                    forground_color = Correct_Color;
            }
            x.color = forground_color;
         });
        //if (Cell_index == 0) {
        //    Debug.LogFormat("normal color:{0}, Highlight color: {1}", colors.normalColor, colors.highlightedColor);
        //}

        UpdateLine();
        
    }

    public void UpdateLine()
    {        
        if (bHintMode)
        {
            TopLine.SetActive(false);
            BottomLine.SetActive(false);
            LeftLine.SetActive(false);
            RightLine.SetActive(false);
            int iRow = Cell_index / 9;
            int iCol = Cell_index % 9;
            int iBlockRow = iRow % 3;
            int iBlockCol = iCol % 3;
            if (bFocusBlockInHintMode)
            {                
                if (iBlockRow == 0)
                    TopLine.SetActive(true);                    
                if (iBlockRow == 2)
                    BottomLine.SetActive(true);
                if (iBlockCol == 0)
                    LeftLine.SetActive(true);
                if (iBlockCol == 2)
                    RightLine.SetActive(true);
            }
            else if (bFocusWholeRowInHintMode)
            {                
                TopLine.SetActive(true);
                BottomLine.SetActive(true);                
                if (iCol == 0)
                    LeftLine.SetActive(true);
                if (iCol == 8)
                    RightLine.SetActive(true);
            }
            else if (bFocusWholeColInHintMode)
            {
                LeftLine.SetActive(true);
                RightLine.SetActive(true);
                if (iRow == 0)
                    TopLine.SetActive(true);
                if (iRow == 8)
                    BottomLine.SetActive(true);
            }
            else if (bFocusCellInHintMode)
            {
                LeftLine.SetActive(true);
                RightLine.SetActive(true);
                TopLine.SetActive(true);
                BottomLine.SetActive(true);
            }
        }
        else
        {
            if (IsSelected)
            {
                TopLine.SetActive(true);
                BottomLine.SetActive(true);
                LeftLine.SetActive(true);
                RightLine.SetActive(true);
            }
            else
            {
                TopLine.SetActive(false);
                BottomLine.SetActive(false);
                LeftLine.SetActive(false);
                RightLine.SetActive(false);
            }
        }
    }

    public void FocusDueToGetAHint()
    {
        StartCoroutine(HintTransition(new Vector3(sizeScaleForFocus, sizeScaleForFocus, sizeScaleForFocus), sizeScaleForFocus));
    }
    public IEnumerator HintTransition(Vector3 newSize, float transitionTime)
    {
        float timer = 0;
        Vector3 startSize = transform.localScale;
        var flashColors = this.colors;
        var defaultColor = this.colors.disabledColor;

        while (timer < transitionTime * 4)
        {
            timer += Time.deltaTime;

            yield return null;
            if (timer <= transitionTime || (timer > transitionTime * 2 && timer <= transitionTime * 3))
            {
                transform.localScale = Vector3.Lerp(startSize, newSize, timer / transitionTime);
                flashColors.disabledColor = Color.Lerp(defaultColor, Focus_Color, timer / transitionTime);
                this.colors = flashColors;
            }
            else
            {
                transform.localScale = Vector3.Lerp(newSize, startSize, timer / transitionTime);
                flashColors.disabledColor = Color.Lerp(Focus_Color, defaultColor, timer / transitionTime);
                this.colors = flashColors;
            }
        }
    }
    /*
    public IEnumerator HintTransition(Vector3 newSize, float transitionTime)
    {
        float timer = 0;
        Vector3 startSize = transform.localScale;
        var flashColors = this.colors;
        var defaultColor = this.colors.disabledColor;
        
        while (timer < transitionTime * 4)
        {
            timer += Time.deltaTime;

            yield return null;
            if (timer <= transitionTime || (timer > transitionTime * 2 && timer <= transitionTime * 3))
            {
                transform.localScale = Vector3.Lerp(startSize, newSize, timer / transitionTime);
                flashColors.disabledColor = Color.Lerp(defaultColor, Focus_Color, timer / transitionTime);
                this.colors = flashColors;
            }
            else
            {
                transform.localScale = Vector3.Lerp(newSize, startSize, timer / transitionTime);
                flashColors.disabledColor = Color.Lerp(Focus_Color, defaultColor, timer / transitionTime);
                this.colors = flashColors;
            }
        }
    }
    */
    private void OnPause(bool bPause)
    {
        number_text.SetActive(bPause == false);
        Notes_panel.SetActive(bPause == false && Number == 0);
        UpdateSquareColor();
        if (bPause)
        {
            TopLine.SetActive(false);
            BottomLine.SetActive(false);
            LeftLine.SetActive(false);
            RightLine.SetActive(false);
        }
    }
    
    // Hell Level support methods
    public bool IsHypothesisNumber() 
    {
        return isHypothesisNumber && Number != 0;
    }
    
    public void SetAsHypothesis(bool hypothesis)
    {
        isHypothesisNumber = hypothesis && Number != 0;
        UpdateSquareColor();
    }
    
    public void ConfirmHypothesis()
    {
        if (isHypothesisNumber && Number == Correct_number)
        {
            isHypothesisNumber = false;
            Has_Wrong_value = false;
            UpdateSquareColor();
        }
    }
    
    public void ResetHypothesis()
    {
        if (isHypothesisNumber && !Has_default_value)
        {
            Number = 0;
            isHypothesisNumber = false;
            Has_Wrong_value = false;
            DisplayText();
            UpdateSquareColor();
        }
    }

}
