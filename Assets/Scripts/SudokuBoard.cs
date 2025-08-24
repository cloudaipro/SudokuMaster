using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GNPXcore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TransitionEffection;

public class SudokuBoard : MonoBehaviour
{
    public GameObject canvas;
    public GameObject back_panel;
    public GameObject level;
    public GameObject gameOverDialog;
    public List<GameObject> number_buttons;
    public float top_margin = 500f;
    public float bottom_margin = 400f;
    public float min_LR_margin = 10f;
    public GameObject ProgressInfo;
    public float progressInfoHeight = 100f;
    [SerializeField] AudioSource Correct_Audio = null;
    [SerializeField] AudioSource Wrong_Audio = null;
    public GameObject AppliedMethod;
    public GameObject unavilableNow;
    public GameObject HintMessagePanel;
    public Text HintTitle;
    public Text HintMessage;
    public Button NextHint;
    public Button PrevHint;
    
    [Header("Hell Level Support")]
    private ValidationContext validationContext;

    public int cols = 0;
    public int rows = 0;
    public GameObject cellPrefab; // prefab
    private Vector2 start_position = new Vector2(0.0f, 0.0f);
    public float boarder_size = 10f;
    public float square_scale = 1.0f;
    public float cell_line_size = 0.0f;
    public float square_gap = 0.1f;
    public Color line_heighlight_color = Color.red;

    public List<Color> cell_gradient_colors = new List<Color> { Color.grey, Color.grey, Color.grey };
    public List<Color> hint_gradient_colors = new List<Color> { Color.grey, Color.grey, Color.grey };
    public List<Color> board_gradient_colors = new List<Color> { Color.grey, Color.grey, Color.grey, Color.grey, Color.grey, Color.grey };
    private List<GameObject> grid_squares_ = new List<GameObject>();
    private int selected_dataIdx = -1;
    private float cellSize = 122f;
    private bool fastNoteMode = false;
    private Dictionary<int, List<int>> backupGridNote = null;
    private Dictionary<int, List<int>> currFastNotesData = null;
    private bool bHintMode = false;
    private UPuzzle pHintGP;
    public NuPz_Win nuPzWin;

    private int stepHintMode = 0;
    private string skillName = "";
    private bool noteHintMode = false;
    class HintStep
    {
        public string Name;
        public List<string> stepDesc;
    }

    private void Awake()
    {
        if (!GameSettings.Instance.ContinuePreviousGame && Setting.Instance.FreeHints <= 0)
            Setting.Instance.UpdateFreeHints(1);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogFormat("screen size:(w,h) = ({0}, {1}", Screen.currentResolution.width, Screen.currentResolution.height);

        // cell width (Screen.currentResolution.width - board_left_right_margin * 2 - square_gap * 2 - square_offset * 6) / 9;
        if (cellPrefab.GetComponent<SudokuCell>() == null)
        {
            Debug.LogError("This Game Object need to have SudokuCell script attached!");
        }

        setBoardPosition();
        CreateGrid();
        
        // Initialize ValidationContext for Hell Level support
        InitializeValidationContext();
        
        // Initialize Hell Level UI components
        InitializeHellLevelUI();

        if (GameSettings.Instance.ContinuePreviousGame)
        {
            SetGridFromFile();
            if (Lives.Instance.lives <= 0)
                Lives.Instance.DoGameOverProcess();
        }
        else
        {            
            SetGridNumber(GameSettings.Instance.GameMode.GetDescription());//GetGameMode());
            GameEvents.SquareSelectedMethod(grid_squares_.Select(x => x.GetComponent<SudokuCell>()).Where(x => x.Has_default_value == false).First()?.Cell_index ?? 0);

            BoardGardientTransiton(true);
        }

        level.GetComponent<Text>().text = GameSettings.Instance.GameMode.GetDescription() + (GameSettings.Instance.GameMode) switch
        {
            EGameMode.EASY => "",
            EGameMode.MEDIUM => "", // $" - level {Setting.Instance.MediumLevel}",
            EGameMode.HARD => "", // $" - level {Setting.Instance.HardLevel}",
            EGameMode.EXTREME => "", // $" - level {Setting.Instance.ExtremeLevel}",
            EGameMode.HELL => "",
            _ => ""
        };

        if (AdManager.Instance != null)
        {
            AdManager.Instance.ShowBanner();
            Debug.Log("Show banner ... done!");
        }

        GameSettings.Instance.bBoardInteractable = true;
        
    }

    private void BoardGardientTransiton(bool FadeIn)
    {
        var vLines = new List<SudokuCell[]>();
        var vGLines = new List<Graphic[]>();
        SudokuCell[] vLine;
        for (int i = 0; i < 9; i++)
        {
            vLine = LineIndicator.Instance.GetVerticalLine(i)
                                          .Select(idx => grid_squares_[idx].GetComponent<SudokuCell>()).ToArray();
            vLines.Add(vLine);
        }
        if (FadeIn)
        {
            Graphic[] vGLine;
            for (int i = 0; i < 9; i++)
            {
                vGLine = vLines[i].Select(cell => cell.number_text.GetComponent<Text>() as Graphic).ToArray();
                vGLines.Add(vGLine);
            }
            StartCoroutine(BlockFadeInGradientTransition<SudokuCell>(vLines, vGLines, board_gradient_colors, 0.6f));
        }
        else
            StartCoroutine(BlockGradientTransition<SudokuCell>(vLines, board_gradient_colors, 0.6f));
    }

    private void OnEnable()
    {
        GameEvents.OnSquareSelected += OnSquareSelected;
        GameEvents.OnCheckBoardCompleted += CheckBoardCompleted;
        GameEvents.OnWillSetNumber += OnWillSetNumber;
        GameEvents.OnDidSetNumber += OnDidSetNumber;
        GameEvents.OnGiveAHint += OnGiveAHint;
        GameEvents.OnGiveFastNote += OnGiveFastNote;
        GameEvents.OnWrongNumber += WrongNumber;
        GameEvents.OnSDKsolverCompleted += SDKsolverCompleted;
        GameEvents.OnRewardAdFail += RewardAdFail;
        GameEvents.OnDidFinishLiveRewardAd += DidFinisLiveRewardAd;
        GameEvents.OnSaveProgressData += SaveData;
        GameEvents.OnNumberCleared += OnNumberCleared;
        NextHint.onClick.AddListener(NextHintClick);
        PrevHint.onClick.AddListener(PrevHintClick);
    }
    private void OnDisable()
    {
        GameEvents.OnSquareSelected -= OnSquareSelected;
        GameEvents.OnCheckBoardCompleted -= CheckBoardCompleted;
        GameEvents.OnWillSetNumber -= OnWillSetNumber;
        GameEvents.OnDidSetNumber -= OnDidSetNumber;
        GameEvents.OnGiveAHint -= OnGiveAHint;
        GameEvents.OnGiveFastNote -= OnGiveFastNote;
        GameEvents.OnWrongNumber -= WrongNumber;
        GameEvents.OnSDKsolverCompleted -= SDKsolverCompleted;
        GameEvents.OnRewardAdFail -= RewardAdFail;
        GameEvents.OnDidFinishLiveRewardAd -= DidFinisLiveRewardAd;
        GameEvents.OnSaveProgressData -= SaveData;
        GameEvents.OnNumberCleared -= OnNumberCleared;
        NextHint.onClick.RemoveAllListeners();
        PrevHint.onClick.RemoveAllListeners();

        if (GameSettings.Instance.ExistAfterWon || Lives.Instance.lives <= 0)
            Config.DeleteDataFile();
        else
            // save data before exit
            SaveData();

        AdManager.Instance.HideBanner();
        GameSettings.Instance.ExistAfterWon = false;
    }


    private void SaveData()
    {
        var solved_data = grid_squares_.Select(x => x.GetComponent<SudokuCell>().Correct_number).ToArray();
        var unsolved_data = grid_squares_.Select(x => x.GetComponent<SudokuCell>().Number).ToArray();
        Config.SaveBoardData(new SudokuData.SudokuBoardData(unsolved_data, solved_data),
                            GameSettings.Instance.GameMode, GameSettings.Instance.Level,
                           selected_dataIdx, Lives.Instance.error_number,
                           (fastNoteMode && backupGridNote != null) ? backupGridNote : GetGridNotes(),
                           grid_squares_.Select(x => x.GetComponent<SudokuCell>().Has_default_value).ToArray(), noteHintMode);
    }


    private void setBoardPosition()
    {
        canvas.GetComponent<RectTransform>().rect.Also(canvasSize =>
        {
            var board_panel_size = System.Math.Min(canvasSize.width, canvasSize.height - (top_margin + progressInfoHeight) - bottom_margin) - min_LR_margin * 2;
            //Debug.Log("Board panel size:" + board_panel_size.ToString());
            //(back_panel.transform as RectTransform).anchoredPosition = new Vector2(board_panel_size / 2 * -1, screen.height / 2 - ((screen.height - top_margin - bottom_margin) / 2 + top_margin));

            var yOfBackPanel = (bottom_margin - (top_margin + progressInfoHeight)) / 2;
            (back_panel.transform as RectTransform).Also(rt =>
            {
                rt.sizeDelta = new Vector2(board_panel_size, board_panel_size);
                rt.anchoredPosition = new Vector2(0, yOfBackPanel);
                this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (bottom_margin - (top_margin + progressInfoHeight)) / 2);
            });
            cellSize = (board_panel_size - boarder_size * 2 - square_gap * 2 - cell_line_size * 6) / 9;
            start_position.x = board_panel_size / 2 * -1 + boarder_size + cellSize / 2;
            start_position.y = back_panel.transform.position.y + board_panel_size / 2 - boarder_size - cellSize / 2;

            (ProgressInfo.transform as RectTransform).Also(rt =>
            {
                rt.sizeDelta = new Vector2(board_panel_size, progressInfoHeight);
                rt.anchoredPosition = new Vector2(0, (bottom_margin - (top_margin + progressInfoHeight)) / 2 + board_panel_size / 2 + progressInfoHeight / 2);
            });

            (HintMessagePanel.transform as RectTransform).Also(rt =>
            {
                var height = canvasSize.height / 2 - 300 - (board_panel_size / 2 - yOfBackPanel) - min_LR_margin;
                rt.sizeDelta = new Vector2(0, height); // 300 for ad
                rt.anchoredPosition = new Vector2(0, (300.0f + (height / 2)));
            });
        });
        /*
        Screen.currentResolution.Also(screen =>
        {
            var max_width = screen.width - min_LR_margin * 2;
            var max_height = screen.height - top_margin - bottom_margin - min_LR_margin * 2;
            var board_panel_size = System.Math.Min(max_width, max_height);
            //(back_panel.transform as RectTransform).anchoredPosition = new Vector2(board_panel_size / 2 * -1, screen.height / 2 - ((screen.height - top_margin - bottom_margin) / 2 + top_margin));
            (back_panel.transform as RectTransform).Also(rt =>
            {
                rt.anchoredPosition = new Vector2(0, screen.height / 2 - ((screen.height - top_margin - bottom_margin) / 2 + top_margin));
                rt.sizeDelta = new Vector2(board_panel_size, board_panel_size);
            });
            cellSize = (board_panel_size - board_left_right_margin * 2 - square_gap * 2 - cell_line_size * 6) / 9;
            start_position.x = board_panel_size / 2 * -1 + board_left_right_margin;
            start_position.y = back_panel.transform.position.y + board_panel_size / 2 - board_left_right_margin;
        });
        */
        //Debug.Log("Cell size=" + cellSize);
    }

    private void SetGridFromFile()
    {
        var gameProgress = Config.LoadBoardData();
        GameSettings.Instance.GameMode = gameProgress.game_Mode;
        selected_dataIdx = gameProgress.selected_index_at_dataOfLevel;
        setCellData(new SudokuData.SudokuBoardData(gameProgress.unsolved, gameProgress.solved));
        SetGridNotes(gameProgress.grid_notes);
        gameProgress.hasDefaultFlags.ForEachWithIndex((hasDefaultFlag, idx) =>
        {
            grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
            {
                cell.Has_default_value = hasDefaultFlag;
                cell.UpdateSquareColor();
            });
        });
        noteHintMode = gameProgress.noteHintMode;
    }
    private void SetGridNotes(Dictionary<int, List<int>> notes)
    {
        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ToList().Also(cells =>
        {
            cells.ForEach(cell => cell.ClearupAllNotes());
            foreach (var note in notes)
                cells[note.Key].SetCellNotes(note.Value);
        });
        //foreach (var note in notes)
        //    grid_squares_[note.Key].GetComponent<SudokuCell>().Also(cell =>
        //    {
        //        cell.ClearupAllNotes();
        //        cell.SetCellNotes(note.Value);
        //    });
    }
    private Dictionary<int, List<int>> GetGridNotes()
    {
        var grid_notes = new Dictionary<int, List<int>>();
        IList<int> notes;
        grid_squares_.Select(x => x.GetComponent<SudokuCell>())
                     .Where(x => x.IsCorrectNumberSet() == false)
                     .ForEach(x =>
                     {
                         notes = x.GetSquareNotes();
                         if (notes.Count() > 0)
                             grid_notes.Add(x.Cell_index, notes.ToList());
                     });
        return grid_notes;
    }
    private void CreateGrid()
    {
        SpawnGridSquare();
        SetSquarePosition();
    }
    private void SpawnGridSquare()
    {
        int square_index = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                //var grid_cell = Instantiate(cellPrefab, this.transform) as GameObject;
                var grid_cell = Instantiate(cellPrefab) as GameObject;
                grid_cell.GetComponent<SudokuCell>().Also(x =>
                {
                    x.Cell_index = square_index++;
                    x.setCellSize(cellSize);
                });
                //grid_cell.transform.parent = this.transform;
                grid_cell.transform.SetParent(this.transform, false);
                grid_squares_.Add(grid_cell);
                grid_cell.transform.localScale = new Vector3(square_scale, square_scale, square_scale);
            }
        }
    }
    private void SetSquarePosition()
    {
        float x = start_position.x;
        float y = start_position.y;
        for (int iRow = 0; iRow < rows; iRow++)
        {
            for (int iCol = 0; iCol < cols; iCol++)
            {
                var square = grid_squares_[iRow * cols + iCol];

                square.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
                //Debug.LogFormat("({0},{1}) = ({2},{3})", iRow, iCol, x, y);
                square.GetComponent<SudokuCell>().Also(cell =>
                {
                    cell.setCellSize(cellSize);
                    cell.sizeScaleForFocus = (cellSize + square_gap) / cellSize;
                });

                x += cellSize + (((iCol + 1) % 3) == 0 ? square_gap : cell_line_size);
            }
            x = start_position.x;
            y -= cellSize + (((iRow + 1) % 3) == 0 ? square_gap : cell_line_size);
        }
    }

    private void SetGridNumber(string level)
    {

        if (level == "Easy" || level == "Hell")
        {
            var data = Generator.Sudoku_Generator("Easy");
            setCellData(new SudokuData.SudokuBoardData(data.unsolved, data.solved));
        }
        else
        {
            var dataOfLevel = SudokuData.GetData(level);
            selected_dataIdx = Random.Range(0, dataOfLevel.Count);
            var data = dataOfLevel[selected_dataIdx];
            data.ShuffleNumber();
            setCellData(data);
        }
    }
    private void setCellData(SudokuData.SudokuBoardData data)
    {
        SudokuCell cell;
        var sub_text_value = new int[9] { 9, 9, 9, 9, 9, 9, 9, 9, 9 };
        (int r, int c, int b) pos;
        for (var i = 0; i < grid_squares_.Count; i++)
        {
            cell = grid_squares_[i].GetComponent<SudokuCell>();
            cell.SetNumber(data.unsolved_data[i]);
            //cell.Correct_number = data.solved_data[i];
            cell.SetCorrectNumber(data.solved_data[i]);
            cell.Has_default_value = data.unsolved_data[i] != 0;
            if (cell.Number >0)
                sub_text_value[cell.Number - 1] -= 1;

            pos = LineIndicator.Instance.GetCellPosition(i);
            cell.Row_index = pos.r;
            cell.Column_index = pos.c;
            cell.Block_index = pos.b;
        }
        number_buttons.Select(x => x.GetComponent<NumberButton>()).ForEachWithIndex((x, idx) =>
        {
            x.SetSubText(sub_text_value[idx]);
        });
    }

    private void prepareAnalyzerPuzzle()
    {
        nuPzWin = new NuPz_Win();

        var BDL = grid_squares_.Select(x => x.GetComponent<SudokuCell>()).Select(x => (x.IsCorrectNumberSet() ? x.Correct_number : 0)).Select((n, idx) => new UCell(idx, n)).ToList();
        var uPuzzle = new UPuzzle(BDL);
        nuPzWin.GNPX_000.SDKProbLst.Clear();
        nuPzWin.GNPX_000.SDKProbLst.Add(uPuzzle);
        nuPzWin.GNPX_000.CurrentPrbNo = 0;
        nuPzWin.GNPX_000.pGNPX_Eng.AnMan.ResetAnalysisResult(true); //Clear analysis result only
        nuPzWin.GNPX_000.pGNPX_Eng.AnalyzerCounterReset();
    }

    public void OnSquareSelected(int square_index)
    {
        foreach (var cell in grid_squares_)
        {
            cell.GetComponent<SudokuCell>().Also(cell =>
            {
                cell.OnSquareSelected(square_index);
                cell.Indicated = false;
                cell.DesiredNumnber = -1; //HightlightedSameNumber = false;
                //cell.UpdateSquareColor();
            });
        }

        // set related cells: Indicated = true
        LineIndicator.Instance.GetAllRelatedSudokuCell(square_index).Also(cells =>
            cells.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ForEach(x => x.Indicated = true));

        grid_squares_[square_index].GetComponent<SudokuCell>().Also(x =>
        {
            if (bHintMode)
            {
                if (!x.Has_default_value && !x.IsCorrectNumberSet())
                {
                    pHintGP.BDL[square_index].FixedNo = x.Correct_number;
                    ApplyHint();
                }
            }
            else if (x.Has_default_value || x.IsCorrectNumberSet())
                HighlightAllCellsWithNumber(x.Number);
        });

        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
    }

    public void HighlightAllCellsWithNumber(int number)
    {
        // First, clear all DesiredNumnber values
        grid_squares_.Select(y => y.GetComponent<SudokuCell>())
                          .ForEach(y => y.DesiredNumnber = -1);
        
        // Then, set DesiredNumnber only for cells that contain the target number
        grid_squares_.Select(y => y.GetComponent<SudokuCell>())
                          .Where(y => y.Number == number && (y.Has_default_value || y.IsCorrectNumberSet()))
                          .ForEach(y => y.DesiredNumnber = number);
                          
        // CRITICAL: Update visual appearance after setting highlight flags
        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
    }
    
    public void ClearAllHighlights()
    {
        grid_squares_.Select(y => y.GetComponent<SudokuCell>())
                          .ForEach(y => y.DesiredNumnber = -1);
                          
        // CRITICAL: Update visual appearance after clearing highlight flags
        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
    }
    
    public void ClearAllCellSelections()
    {
        // Clear all cell selections, highlights, and indicators
        foreach (var cell in grid_squares_)
        {
            cell.GetComponent<SudokuCell>().Also(cellComponent =>
            {
                cellComponent.IsSelected = false;
                cellComponent.Indicated = false;
                cellComponent.DesiredNumnber = -1;
            });
        }
        
        // Update visual appearance after clearing all selection states
        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
    }
    
    public bool IsNumberCompletlyPlaced(int number)
    {
        // Count how many times the number appears in the grid
        int count = 0;
        foreach (var square in grid_squares_)
        {
            var cell = square.GetComponent<SudokuCell>();
            if (cell != null && cell.Number == number && (cell.Has_default_value || cell.IsCorrectNumberSet()))
            {
                count++;
            }
        }
        
        // In a standard 9x9 Sudoku, each number should appear exactly 9 times
        return count >= 9;
    }
    
    public void OnWillSetNumber(int square_index, int value)
    {
        if (fastNoteMode && backupGridNote != null)
        {
            //grid_squares_[square_index].GetComponent<SudokuCell>().Also(cell =>
            //{
            //    if (cell.Has_default_value || cell.Number > 0) return;
            SetGridNotes(backupGridNote);
            fastNoteMode = false;
            backupGridNote = null;
            //});
        }
    }
    public void OnDidSetNumber(int square_index)
    {
        var cell = grid_squares_[square_index].GetComponent<SudokuCell>();
        if (!cell.IsCorrectNumberSet()) return;

        HighlightAllCellsWithNumber(cell.Number);

        var hLine = LineIndicator.Instance.GetHorizontalLine(square_index);
        var vLine = LineIndicator.Instance.GetVerticalLine(square_index);
        var block = LineIndicator.Instance.GetBlockFlat(square_index);

        block.Union(hLine).Union(vLine).Distinct().ToArray()
              .Also(cells => cells.Select(x => grid_squares_[x].GetComponent<SudokuCell>())
                                  .ForEach(x => x.RemoveNote(cell.Number)));

        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());

        Correct_Audio.Play();
        if (!DoCheckCompleted())
        {
            //Correct_Audio.Play();
            List<SudokuCell[]> finishCells = new List<SudokuCell[]>();
            var hLineObjs = hLine.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            var vLineObjs = vLine.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            var squareObjs = block.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            if (DoCheckCompleted(hLineObjs))
                finishCells.Add(hLineObjs);
            if (DoCheckCompleted(vLineObjs))
                finishCells.Add(vLineObjs);
            if (DoCheckCompleted(squareObjs))
                finishCells.Add(squareObjs);
            if (finishCells.Count > 0)
                StartCoroutine(BlockGradientTransition<SudokuCell>(finishCells, board_gradient_colors, 0.6f));//cell_gradient_colors, 0.5f));
                                                                                                              //StartCoroutine(Transition(finishCells, cell_gradient_colors, 0.5f));

            SaveData();
        }
    }

    private bool DoCheckCompleted(SudokuCell[] cells)
    {
        foreach (var comp in cells)
        {
            if (!comp.IsCorrectNumberSet())
                return false;
        }
        return true;

    }

    private bool DoCheckCompleted()
    {
        SudokuCell comp;
        foreach (var square in grid_squares_)
        {
            comp = square.GetComponent<SudokuCell>();
            if (!comp.IsCorrectNumberSet())
                return false;
        }
        return true;
    }

    public void OnNumberCleared(int clearedNumber)
    {
        // Handle number restoration for all number buttons, including inactive ones
        foreach (var buttonObj in number_buttons)
        {
            if (buttonObj != null)
            {
                var numberButton = buttonObj.GetComponent<NumberButton>();
                if (numberButton != null && numberButton.value == clearedNumber)
                {
                    // Increase sub_value when this number is cleared
                    if (numberButton.sub_value< 9) // Don't exceed max count
                    {
                        numberButton.SetSubText(numberButton.sub_value + 1);
                        
                        // Re-enable the button if it was disabled due to sub_value being 0
                        if (!buttonObj.activeInHierarchy)
                        {
                            buttonObj.SetActive(true);
                        }
                    }
                    break; // Found the matching button, exit loop
                 }
            }
        }
    }

    private void CheckBoardCompleted()
    {
        if (DoCheckCompleted())
        {
            BoardGardientTransiton(false);
            StartCoroutine(didCompletedCoroutine());
        }
    }
    private IEnumerator didCompletedCoroutine()
    {
        //yield on a new YieldInstruction that waits for 0.6 seconds.
        yield return new WaitForSeconds(0.65f);
        GameEvents.OnBoardCompletedMethod();
    }

    private void WrongNumber()
    {
        Wrong_Audio.Play();
        SaveData();
    }

    private List<int> generatePossibleNotes(SudokuCell cell)
    {
        var allNumberShowedInRelatedCells = LineIndicator.Instance.GetAllRelatedSudokuCell(cell.Cell_index)
                                                  .Select(x => grid_squares_[x].GetComponent<SudokuCell>())
                                                  .Where(x => x.Number > 0 && x.IsCorrectNumberSet())
                                                  .Select(x => x.Number).Distinct();
        return Enumerable.Range(1, 9).ToArray().Except(allNumberShowedInRelatedCells).ToList();
    }

    public void FastNote()
    {
        if (fastNoteMode) return;

        AdManager.Instance.ShowFastNoteRewardAd();
        //GameEvents.GiveFastNoteMethod();
    }

    private Dictionary<int, List<int>> GetAllPossibleNotes()
    {
        Dictionary<int, List<int>> notes = new Dictionary<int, List<int>>();
        grid_squares_.Select(x => x.GetComponent<SudokuCell>())
                     .Where(x => x.IsCorrectNumberSet() == false) //x.Number == 0)
                     .ForEach(x =>
                     {
                         notes.Add(x.Cell_index, generatePossibleNotes(x));
                     });
        return notes;
    }

    private void OnGiveFastNote()
    {
        Dispatcher.RunOnMainThread(() =>
        {
            if (noteHintMode)
            {
                foreach (var x in grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ToList())
                {
                    if (x.IsCorrectNumberSet()) continue;
                    var note = (x.number_notes[x.Correct_number - 1]).GetComponent<SudokuNote>();
                    if (!note.bSetted)
                    {
                        note.bSetted = true;
                    }
                }
            }
            else
            {
                // backup note
                backupGridNote = GetGridNotes();

                currFastNotesData = GetAllPossibleNotes();
                SetGridNotes(currFastNotesData);

                fastNoteMode = true;

                if (NoteButton.Instance.Active)
                    NoteButton.Instance.ToggleActive();
            }
        });
    }

    private void DidFinisLiveRewardAd()
    {
        Dispatcher.RunOnMainThread(() =>
        {
            gameOverDialog.SetActive(false);
            SetGridFromFile();
            Clock.Instance.StartClock();
        });
    }
    private void RewardAdFail()
    {
        var xxx = unavilableNow.GetComponent<ToastMessage>();
        xxx?.showToast(2);
        GameSettings.Instance.Pause = false;
        //StartCoroutine(UnavilableNowTransition(0.5f));
    }
    public void SolveSudoku()
    {

        prepareAnalyzerPuzzle();

        /*
        if (!nuPzWin.pGNPX_Eng.IsSolved())
        {
            nuPzWin.SolveUp();
            var x = nuPzWin.Get_MethodCounter();
            AppliedMethod.GetComponent<Text>().text = x.Last().methodName + ":" + x.Last().difficulty;
        }
        */

        StartCoroutine(tryAllSkillToSolve());

        /*
        grid_squares_.Select(x => x.GetComponent<SudokuCell>()).Where(x => x.IsCorrectNumberSet() == false)
                     .ToList()
                     .Also(x =>
                     {
                         for (int i = 0; i < x.Count - 1; i++)
                         {
                             var target = x[i];
                             GameEvents.SquareSelectedMethod(target.Cell_index);
                             GameEvents.UpdateSquareNumberMethod(target.Correct_number);
                             //x[i].SetCorrectNumber();
                         }
                     });
        */
        //foreach (var square in grid_squares_)
        //{
        //    var comp = square.GetComponent<SudokuCell>();
        //    comp.SetCorrectNumber();
        //}
        //CheckBoardCompleted();
    }
    private IEnumerator tryAllSkillToSolve()
    {
        float timer = 0;
        int loopCnt = 0;
        var tmp = grid_squares_.Select(x => x.GetComponent<SudokuCell>()).Where(x => x.IsCorrectNumberSet() == false);
        int count = tmp.Count();
        Debug.Log($"Total count={tmp.Count()}");
        //foreach (var x in tmp)
        for (; count > 0; count--)
        {
            timer += Time.deltaTime;
            yield return new WaitForSeconds(1f);

            OnGiveAHint();
            if (pHintGP.Sol_ResultLong == NO_SOLUTION || pHintGP.pMethod.MethodName == FINNED_XWING || pHintGP.pMethod.MethodName == XWING || pHintGP.pMethod.MethodName == XYWING || pHintGP.pMethod.MethodName == HIDDEN_SINGLE) //pHintGP.pMethod.DifLevel > 1)//MethodName == "HiddenSingle")
                break;
            if (hintSteps.Keys.Contains(pHintGP.pMethod.MethodName) == false)
                break;
            Debug.Log($"{(++loopCnt)} {pHintGP.pMethod.MethodName}");

            ApplyHint();
        }
        Debug.Log("Finish.");
    }

    #region Hint

    private bool isMissingNote()
    {
        if (noteHintMode == false) return true;
        foreach (var x in grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ToList())
        {
            if (x.IsCorrectNumberSet()) continue;
            var note = (x.number_notes[x.Correct_number - 1]).GetComponent<SudokuNote>();
            if (!note.bSetted)
                return true;
        }
        return false;
    }

    private Dictionary<int, List<int>> backupGridNoteForHintMode = null;
    private void OnGiveAHint()
    {
        try
        {
            if (GNPZ_Engin.SolverBusy) return;
            prepareAnalyzerPuzzle();
            if (!nuPzWin.pGNPX_Eng.IsSolved())
            {
                nuPzWin.GNPX_000.AnalyzerMode = "Solve";
                AppliedMethod.GetComponent<Text>().text = "";
                GNPZ_Engin.SolverBusy = true;

                if (noteHintMode)
                {
                    if (isMissingNote())
                    {
                        skillName = TAKE_SOME_NOTES;
                        HintMessagePanel.SetActive(true);
                        bHintMode = true;
                        AppliedMethod.GetComponent<Text>().text = $"{skillName}";
                        beginHint();
                        return;
                    }
                    else
                    {
                        int freeB = 0;
                        SudokuCell cell;
                        nuPzWin.pGNPX_Eng.pGP_Initial.BDL.ForEachWithIndex((p, idx) =>
                        {
                            cell = grid_squares_[idx].GetComponent<SudokuCell>();
                            if (cell.IsCorrectNumberSet()) return;

                            freeB = 0;
                            cell.number_notes.ForEachWithIndex((n, noteIdx) =>
                            {
                                if (n.GetComponent<SudokuNote>().bSetted)
                                    freeB |= 1 << noteIdx;
                            });
                            p.FreeB = freeB;
                        });
                        nuPzWin.pGNPX_Eng.Set_NextStage(setAllCandidates: false);
                    }
                }
                else
                    nuPzWin.pGNPX_Eng.Set_NextStage();

                //OnGiveAHint();
                nuPzWin.MySolver();
            }
        }
        catch (System.Exception) { }
        finally
        {
            GNPZ_Engin.SolverBusy = false;
        }
        /*
        var allPossibleNotes = (fastNoteMode) ? currFastNotesData : GetAllPossibleNotes();
        SudokuCell target = null;
        foreach(var key in allPossibleNotes.Keys)
        {
            if (allPossibleNotes[key].Count == 1)
            {
                target = grid_squares_[key].GetComponent<SudokuCell>();                
                break;
            }
        }
        if (target == null)
        {
            grid_squares_.Select(x => x.GetComponent<SudokuCell>())
                         .Where(x => !x.Has_default_value && !x.IsCorrectNumberSet())
                         .ToArray().Also(unsolved_cells =>
                         {
                             target = unsolved_cells[Random.Range(0, unsolved_cells.Count())];
                         });
        }
        GameEvents.SquareSelectedMethod(target.Square_index);
        GameEvents.UpdateSquareNumberMethod(target.Correct_number);
        //target.FocusDueToGetAHint();
        StartCoroutine(BlockGradientTransition<SudokuCell>(new List<SudokuCell[]> { new SudokuCell[] { target } }, board_gradient_colors, 1f));//hint_gradient_colors, 1f));
        */
    }

    private static string NO_SOLUTION { get; } = "no solution";
    private static string HIDDEN_SINGLE { get; } = "HiddenSingle";
    private static string TAKE_SOME_NOTES { get; } = "TakeSomeNotes";
    private static string LOCKED_CANDIDATE { get; } = "LockedCandidate";
    private static string LOCKED_CANDIDATE_TYPE2 { get; } = "LockedCandidate2";
    private static string LOCKED_SET_2D { get; } = "LockedSet(2D)";
    private static string LOCKED_SET_2D_HIDDEN { get; } = "LockedSet(2D)Hidden";
    private static string LOCKED_SET_3D { get; } = "LockedSet(3D)";
    private static string LOCKED_SET_3D_HIDDEN { get; } = "LockedSet(3D)Hidden";
    private static string XWING { get; } = "XWing";
    private static string FINNED_XWING { get; } = "Finned XWing";
    private static string XYWING { get; } = "XY-Wing";

    Dictionary<string, HintStep> hintSteps = new Dictionary<string, HintStep>
    {
        { "LastDigit", new HintStep { Name = "Last digit", stepDesc = new List<string> { "Look at the #type ", "the final free cell can only be #no because all other numbers are already used" } } },
        { "TwoDirSingle", new HintStep { Name = "Last remaining cell", stepDesc = new List<string> { "Note these #no and the highlighed areas", "In this block, there is only one cell left for candidate #no", "Since this cell must be #no" } } },
        { "NakedSingle", new HintStep { Name = "Last possible number", stepDesc = new List<string> { "Note the selected cell and the highlighed areas", "The numbers #nos have already been assigned in either the row, column or block.", "Since this cell must be #no" } } },
        { HIDDEN_SINGLE, new HintStep { Name = "Apparent single", stepDesc = new List<string> { "Look at the #type ", "As there is no other viable choice, the #no only can be assigned to this cell", "Since this cell must be #no" } } },
        { TAKE_SOME_NOTES, new HintStep { Name = "Take some notes", stepDesc = new List<string> { "The notes entered in these cells are missing or incorrect", "To apply advanced solving techniques, it's necessary to fill all cells with possible notes" } } },
        { NO_SOLUTION, new HintStep { Name = "Free cell", stepDesc = new List<string> { "Choose a particular cell that you wish to solve"} } },
        { LOCKED_CANDIDATE, new HintStep { Name = "Pointing Numbers", stepDesc = new List<string> { "Note the #type and the highlighed block", "The cells in this block that may contain the number #no are in only one #type ",
            "As #no is to be added to these cells within this block, it implies that none of the other cells in the same #type can have #no" } } },
        { LOCKED_CANDIDATE_TYPE2, new HintStep { Name = "Pointing Numbers", stepDesc = new List<string> { "Note the two #types and the highlighed block", "The cells in this block that may contain the number #no are in only one #type ",
            "As #no is to be added to these cells within this block, it implies that none of the other cells in the same block can have #no" } } },
        { LOCKED_SET_2D, new HintStep { Name = "Naked Pair", stepDesc = new List<string> { "Note the #type and the highlighed cells",
            "These highlighed cells contain only either the numbers #no1 or #no2, indicating that one of these numbers must belong to them",
            "In these indicated cells, the following numbers #no1 and #no2 are not possible candidates" } } },
        { LOCKED_SET_2D_HIDDEN, new HintStep { Name = "Hidden Pair", stepDesc = new List<string> { "Note the #type and the highlighed cells",
            "The numbers #no1 or #no2 are in only 2 cells, indicating that one of these numbers must belong to them",
            "Therefore, in these two cells the remaining #nos can be excluded from the candidate" } } },
        { LOCKED_SET_3D, new HintStep { Name = "Naked Triple", stepDesc = new List<string> { "Note the #type and the highlighed cells",
            "These highlighed cells contain the numbers #no1, #no2 or #no3, indicating that one of these numbers must belong to them",
            "In these indicated cells, the following numbers #no1, #no2 and #no3 are not possible candidates" } } },
        { LOCKED_SET_3D_HIDDEN, new HintStep { Name = "Hidden triple", stepDesc = new List<string> { "Note the #type and the highlighed cells",
            "The numbers #no1, #no2 or #no3 are in only 3 cells, indicating that one of these numbers must belong to them",
            "Therefore, in these three cells the remaining #nos can be excluded from the candidate" } } },
        { XWING, new HintStep { Name = "XWing", stepDesc = new List<string> { "Note the #types and the highlighed cells",
            "Two #types have two cells with a note #no each, diagonally placed in the same two #othertypes, creating a X-Wing pattern.",
            "Because the candidates are diagonal, each corresponding #othertype will already have one of them, allowing us to remove candidate #no from other cells in those #othertypes" } } },
        { FINNED_XWING, new HintStep { Name = "XWing", stepDesc = new List<string> { "Note the #types and the highlighed cells",
            "Two #types have cells with a note #no each, diagonally placed in the two #othertypes, creating a X-Wing pattern with Fish's Fin.",
            "Because the candidates are diagonal, each corresponding #othertype will already have one of them, allowing us to remove candidate #no from other cells in those #othertypes" } } },
        { XYWING, new HintStep { Name = "XY Wing", stepDesc = new List<string> { "Note these highlighed cells",
            "The highlighted cells must contain either #nos", "#no should be present in one of the other two cells",
            "Therefore, #no can be excluded from the candidate in the common area of these two cells." } } },

    };
    private string NoteDataToString(Dictionary<int, List<int>> notes)
    {
        string str = "";
        notes.Keys.ForEach(k =>
        {
            str += $"{k}[";
            notes[k].Select(x => x.ToString().Aggregate(str, (a, b) => $"{a}{b}"));
            str += "]";
        });
        return str;
    }
    private void SDKsolverCompleted(UPuzzle pGP)
    {
        HintMessagePanel.SetActive(true);
        bHintMode = true;
        pHintGP = pGP;

        if (pHintGP.pMethod == null || pGP.Sol_ResultLong == NO_SOLUTION)
        {
            skillName = NO_SOLUTION;
            AppliedMethod.GetComponent<Text>().text = NO_SOLUTION;
        }
        else
        {
            GameSettings.Instance.bBoardInteractable = false;
            skillName = pHintGP.pMethod.MethodName;
            if (pGP.DifLevel > 1 || pHintGP.pMethod.MethodName == HIDDEN_SINGLE)
            {
                if (noteHintMode == false)
                {
                    if (fastNoteMode)
                    {
                        fastNoteMode = false;
                        backupGridNote = null;
                        noteHintMode = true;
                    }
                    else
                    {
                        var tmpNotes = GetGridNotes();
                        var allNotes = GetAllPossibleNotes();
                        if (tmpNotes.Keys.Count() != allNotes.Keys.Count() || NoteDataToString(tmpNotes) != NoteDataToString(allNotes))
                            skillName = TAKE_SOME_NOTES;
                        else
                            noteHintMode = true;
                    }
                }
                else if (isMissingNote())
                    skillName = TAKE_SOME_NOTES;
            }
            var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).FirstOrDefault();
            AppliedMethod.GetComponent<Text>().text = $"{skillName}";
            if (uCell != null)
                AppliedMethod.GetComponent<Text>().text += $" ({uCell.r + 1},{uCell.c + 1})";
        }
        beginHint();
    }

    private void beginHint()
    {
        stepHintMode = 0;
        // cleanup all color
        foreach (var cell in grid_squares_)
        {
            cell.GetComponent<SudokuCell>().Also(cell =>
            {
                cell.IsSelected = false;
                cell.Indicated = false;
                cell.DesiredNumnber = -1; //HightlightedSameNumber = false;
                cell.bHintMode = true;
                cell.bFocusCellInHintMode = false;
                cell.bFocusWholeColInHintMode = false;
                cell.bFocusWholeRowInHintMode = false;
                cell.bFocusBlockInHintMode = false;
                cell.bHilightedCellInHintMode = false;
                cell.Impossible_Cell = false;
                cell.UpdateSquareColor();
            });
        }

        if (!hintSteps.Keys.Contains(skillName))
            return;
        hintSteps[skillName]?.Also(hintStep =>
        {
            HintTitle.text = hintStep.Name;
            //HintMessage.text = hintStep.stepDesc[0].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString());
            if (skillName == NO_SOLUTION)
            {
                NextHint.gameObject.SetActive(false);
                PrevHint.gameObject.SetActive(false);
                HintMessage.text = hintStep.stepDesc[0];
            }
            else
            {
                if (skillName == TAKE_SOME_NOTES && GetGridNotes().Keys.Count() == 0)
                    stepHintMode = 1;
                NextHint.gameObject.SetActive(true);
                PrevHint.gameObject.SetActive((skillName == TAKE_SOME_NOTES) ? false : true);
                if (stepHintMode == hintStep.stepDesc.Count - 1)
                    NextHint.GetComponentInChildren<Text>().text = "Apply";
                else
                    NextHint.GetComponentInChildren<Text>().text = "Next";
            }

            DoHintStepAction();
        });
        //PrevHint.gameObject.SetActive(true);
    }
    private void NextHintClick()
    {
        stepHintMode++;
        hintSteps[skillName].Also(hintStep =>
        {
            if (hintStep.stepDesc.Count == stepHintMode)
                ApplyHint();
            else
            {
                //HintMessage.text = hintStep.stepDesc[stepHintMode].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString());
                if (stepHintMode == hintStep.stepDesc.Count - 1)
                    NextHint.GetComponentInChildren<Text>().text = "Apply";
                //PrevHint.gameObject.SetActive(true);
                DoHintStepAction();
            }
        });

    }
    private void DoHintStepAction()
    {
        if (skillName == "LastDigit")
            LastDigit(stepHintMode);
        else if (skillName == "TwoDirSingle")
            TwoDirSingle(stepHintMode);
        else if (skillName == "NakedSingle")
            NakedSingle(stepHintMode);
        else if (skillName == HIDDEN_SINGLE)
            HiddenSingle(stepHintMode);
        else if (skillName == TAKE_SOME_NOTES)
            TakeSomeNotes(stepHintMode);
        else if (skillName == LOCKED_CANDIDATE)
            LockedCandidate(stepHintMode);
        else if (skillName == LOCKED_SET_2D || skillName == LOCKED_SET_2D_HIDDEN)
            LockedSet2D(stepHintMode);
        else if (skillName == LOCKED_SET_3D || skillName == LOCKED_SET_3D_HIDDEN)
            LockedSet3D(stepHintMode);
        else if (skillName == XWING || skillName == FINNED_XWING)
            XWing(stepHintMode);
        else if (skillName == XYWING)
            XYWing(stepHintMode);
    }
    private void PrevHintClick()
    {
        ApplyHint();
    }

    private void ApplyHint()
    {
        // cleanup all color
        bHintMode = false;
        GameSettings.Instance.bBoardInteractable = true;

        foreach (var cell in grid_squares_)
        {
            cell.GetComponent<SudokuCell>().Also(cell =>
            {
                cell.IsSelected = false;
                cell.Indicated = false;
                cell.DesiredNumnber = -1; //HightlightedSameNumber = false;
                cell.bHintMode = false;
                cell.bFocusCellInHintMode = false;
                cell.bFocusWholeColInHintMode = false;
                cell.bFocusWholeRowInHintMode = false;
                cell.bFocusBlockInHintMode = false;
                cell.bHilightedCellInHintMode = false;
                cell.Impossible_Cell = false;
                if (cell.Number == 0)
                    cell.number_text.GetComponent<Text>().text = " ";
                //cell.UpdateSquareColor();
            });
        }

        if (skillName == TAKE_SOME_NOTES)
        {
            HintMessagePanel.SetActive(false);
            if (!noteHintMode)
                SetGridNotes(GetAllPossibleNotes());
            noteHintMode = true;
            backupGridNote = null;
            fastNoteMode = false;
            grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
            SaveData();
        }
        else if (skillName == LOCKED_CANDIDATE ||
                 skillName == LOCKED_SET_2D || skillName == LOCKED_SET_2D_HIDDEN ||
                 skillName == LOCKED_SET_3D || skillName == LOCKED_SET_3D_HIDDEN ||
                 skillName == XWING || skillName == FINNED_XWING ||
                 skillName == XYWING)
        {
            var cancelB = pHintGP.BDL.Where(x => x.CancelB > 0).First().CancelB.BitToNum() + 1;
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.CancelB > 0)
                {
                    uCell.CancelB.BitToNumList().ForEach(cancelB =>
                    {
                        grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell => cell.RemoveNote(cancelB));
                    });
                }
            });

            HintMessagePanel.SetActive(false);
            grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ForEach(x => x.UpdateSquareColor());
            SaveData();
        }
        else
        {
            SudokuCell target = null;
            var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).FirstOrDefault();
            if (uCell != null)
            {
                target = grid_squares_[uCell.rc].GetComponent<SudokuCell>();
            }
            else
            {
                grid_squares_.Select(x => x.GetComponent<SudokuCell>())
                             .Where(x => !x.Has_default_value && !x.IsCorrectNumberSet())
                             .ToArray().Also(unsolved_cells =>
                             {
                                 target = unsolved_cells[Random.Range(0, unsolved_cells.Count())];
                             });
            }
            HintMessagePanel.SetActive(false);
            if (NoteButton.Instance.Active)
            {
                NoteButton.Instance.ToggleActive();
            }
            GameEvents.SquareSelectedMethod(target.Cell_index);
            GameEvents.UpdateSquareNumberMethod(target.Correct_number);
            StartCoroutine(BlockGradientTransition<SudokuCell>(new List<SudokuCell[]> { new SudokuCell[] { target } }, board_gradient_colors, 1f));
        }

        AppliedMethod.GetComponent<Text>().text = "";
    }

    private void LastDigit(int step)
    {
        var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).First();
        grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
        {
            if (cell.Number > 0)
                cell.SetNumber(0);
        });
        if (step == 0)
        {
            var bFind = false;
            var type = "block";
            LineIndicator.Instance.GetVerticalLine(uCell.rc).Also(indices =>
            {
                indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).Also(cells =>
                {
                    if (cells.Where(x => x.IsCorrectNumberSet() == false).Count() == 1)
                    {
                        cells.ForEach(c =>
                        {
                            c.bHintMode = true;
                            c.Indicated = true;
                            c.bFocusWholeColInHintMode = true;
                            c.UpdateSquareColor();
                        });
                        bFind = true;
                        type = "column";
                    }
                });
            });

            if (bFind) goto SetDesc;
            LineIndicator.Instance.GetHorizontalLine(uCell.rc).Also(indices =>
            {
                indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).Also(cells =>
                {
                    if (cells.Where(x => x.IsCorrectNumberSet() == false).Count() == 1)
                    {
                        cells.ForEach(c =>
                        {
                            c.bHintMode = true;
                            c.Indicated = true;
                            c.bFocusWholeRowInHintMode = true;
                            c.UpdateSquareColor();
                        });
                        bFind = true;
                        type = "row";
                    }
                });
            });
            if (bFind) goto SetDesc;
            LineIndicator.Instance.GetBlockFlat(uCell.rc).Also(indices =>
            {
                indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ForEach(c =>
                {
                    c.bHintMode = true;
                    c.Indicated = true;
                    c.bFocusBlockInHintMode = true;
                    c.UpdateSquareColor();
                });
            });
        SetDesc:
            hintSteps[skillName].Also(hintStep =>
                HintMessage.text = hintStep.stepDesc[step].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString())
                                                                  .Replace("#type", type)
                                     );
        }
        else if (step == 1)
        {
            hintSteps[skillName].Also(hintStep => HintMessage.text = hintStep.stepDesc[stepHintMode].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString()));
            grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
            {
                cell.number_text.GetComponent<Text>().text = cell.Correct_number.ToString();
                cell.IsSelected = true;
                cell.UpdateSquareColor();
            });
        }
    }
    private void updateHouseForTwoDirSingle(int h, _HouseType houseType, UCell refCell)
    {
        LineIndicator.Instance.GetHouse(h)
            .Also(indices =>
            {
                indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>()).Also(cells =>
                {
                    if (cells.Select(x => x.Number).Contains(refCell.FixedNo))
                    {
                        cells.ForEach(y =>
                        {
                            y.bHintMode = true;
                            y.Indicated = true;
                            y.bHilightedCellInHintMode = (y.Number == refCell.FixedNo);
                            if (y.Number == 0 &&
                                ((houseType == _HouseType.block && y.Block_index == refCell.b) ||
                                 (houseType == _HouseType.row && y.Row_index == refCell.r) ||
                                 (houseType == _HouseType.column && y.Column_index == refCell.c)
                                )
                            )
                                y.Impossible_Cell = true;
                            y.UpdateSquareColor();
                        });
                    }
                });
            });

    }
    private void TwoDirSingle(int step)
    {
        hintSteps[skillName].Also(hintStep => HintMessage.text = hintStep.stepDesc[step].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString()));
        var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).First();
        _HouseType houseType = uCell.TypeOfHouse();
        grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
        {
            if (cell.Number > 0)
                cell.SetNumber(0);
        });

        if (step == 0)
        {
            var excludeHouseIndex = MyStaticSA.GetExcludeHouse(uCell.houseIndex, uCell.nx);
            var house = LineIndicator.Instance.GetHouse(uCell.houseIndex);
            var cellsInHouse = house.Select(x => grid_squares_[x].GetComponent<SudokuCell>());
            if (houseType == _HouseType.block)
            {
                updateHouseForTwoDirSingle(excludeHouseIndex.h1, houseType, uCell);
                updateHouseForTwoDirSingle(excludeHouseIndex.h2, houseType, uCell);
                updateHouseForTwoDirSingle(excludeHouseIndex.h3, houseType, uCell);
                updateHouseForTwoDirSingle(excludeHouseIndex.h4, houseType, uCell);
            }
            else
            {
                updateHouseForTwoDirSingle(excludeHouseIndex.h3, houseType, uCell);
                updateHouseForTwoDirSingle(excludeHouseIndex.h4, houseType, uCell);
                if (houseType == _HouseType.row)
                {
                    cellsInHouse.Where(x => x.IsCorrectNumberSet() == false && x.Cell_index != uCell.rc)
                        .ForEach(x =>
                        {
                            updateHouseForTwoDirSingle(x.Column_index + 9, houseType, uCell);
                        });
                }
                else // _HouseType.column
                {
                    cellsInHouse.Where(x => x.IsCorrectNumberSet() == false && x.Cell_index != uCell.rc)
                        .ForEach(x =>
                        {
                            updateHouseForTwoDirSingle(x.Row_index, houseType, uCell);
                        });
                }
            }
            house.ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                y.bHintMode = true;
                y.Indicated = true;
                y.UpdateSquareColor();
            }));
        }
        else if (step == 1)
        {
            LineIndicator.Instance.GetHouse(uCell.houseIndex).ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                if (y.Cell_index != uCell.rc && y.Number == 0)
                {
                    if (y.Impossible_Cell)
                        y.number_text.GetComponent<Text>().text = "X";
                }
                switch (uCell.TypeOfHouse())
                {
                    case _HouseType.row:
                        y.bFocusWholeRowInHintMode = true;
                        break;
                    case _HouseType.column:
                        y.bFocusWholeColInHintMode = true;
                        break;
                    case _HouseType.block:
                        y.bFocusBlockInHintMode = true;
                        break;
                }
                y.UpdateSquareColor();
            }));
        }
        else if (step == 2)
        {
            LineIndicator.Instance.GetHouse(uCell.houseIndex).ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                if (y.Cell_index == uCell.rc)
                {
                    //y.Notes_panel.SetActive(false);
                    y.number_text.GetComponent<Text>().text = y.Correct_number.ToString();
                    y.IsSelected = true;
                }
                //switch (uCell.TypeOfHouse())
                //{
                //    case _HouseType.row:
                //        y.bFocusWholeRowInHintMode = false;
                //        break;
                //    case _HouseType.column:
                //        y.bFocusWholeColInHintMode = false;
                //        break;
                //    case _HouseType.block:
                //        y.bFocusBlockInHintMode = false;
                //        break;
                //}
                y.UpdateSquareColor();
            }));
        }
    }
    /*
    private void TwoDirSingle(int step)
    {
        hintSteps[skillName].Also(hintStep => HintMessage.text = hintStep.stepDesc[step].Replace("#no", pHintGP.BDL.Where(x => x.FixedNo != 0).First().FixedNo.ToString()));
        var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).First();
        if (step == 0)
        {
            var ExcludeLineIndics = LineIndicator.Instance.GetExcludeLineIndices(uCell.rc);
            LineIndicator.Instance.GetHorizontalLine(ExcludeLineIndics.hLineIdx1).Also(indices =>
            {
                if (indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>().Number).Contains(uCell.FixedNo))
                    indices.ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
                    {
                        y.bHintMode = true;
                        y.Indicated = true;
                        y.bHilightedCellInHintMode = (y.Number == uCell.FixedNo);
                        if (y.Block_index == uCell.b && y.Number == 0)
                            y.Impossible_Cell = true;
                        y.UpdateSquareColor();
                    }));
            });

            LineIndicator.Instance.GetHorizontalLine(ExcludeLineIndics.hLineIdx2).Also(indices =>
            {
                if (indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>().Number).Contains(uCell.FixedNo))
                    indices.ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
                    {
                        y.bHintMode = true;
                        y.Indicated = true;
                        y.bHilightedCellInHintMode = (y.Number == uCell.FixedNo);
                        if (y.Block_index == uCell.b && y.Number == 0)
                            y.Impossible_Cell = true;
                        y.UpdateSquareColor();
                    }));
            });
            LineIndicator.Instance.GetVerticalLine(ExcludeLineIndics.vLineIdx1).Also(indices =>
            {
                if (indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>().Number).Contains(uCell.FixedNo))
                    indices.ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
                    {
                        y.bHintMode = true;
                        y.Indicated = true;
                        y.bHilightedCellInHintMode = (y.Number == uCell.FixedNo);
                        if (y.Block_index == uCell.b && y.Number == 0)
                            y.Impossible_Cell = true;
                        y.UpdateSquareColor();
                    }));
            });
            LineIndicator.Instance.GetVerticalLine(ExcludeLineIndics.vLineIdx2).Also(indices =>
            {
                if (indices.Select(x => grid_squares_[x].GetComponent<SudokuCell>().Number).Contains(uCell.FixedNo))
                    indices.ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
                    {
                        y.bHintMode = true;
                        y.Indicated = true;
                        y.bHilightedCellInHintMode = (y.Number == uCell.FixedNo);
                        if (y.Block_index == uCell.b && y.Number == 0)
                            y.Impossible_Cell = true;
                        y.UpdateSquareColor();
                    }));
            });
            LineIndicator.Instance.GetBlockFlat(uCell.rc).ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                y.bHintMode = true;
                y.Indicated = true;                
                y.UpdateSquareColor();
            }));
        }
        else if (step == 1)
        {
            LineIndicator.Instance.GetBlockFlat(uCell.rc).ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                if (y.Cell_index != uCell.rc && y.Number == 0)
                {
                    if (y.Impossible_Cell)
                        y.number_text.GetComponent<Text>().text = "X";
                }
                y.bFocusBlockInHintMode = true;
                y.UpdateSquareColor();
            }));
        }
        else if (step == 2)
        {
            LineIndicator.Instance.GetBlockFlat(uCell.rc).ForEach(x => grid_squares_[x].GetComponent<SudokuCell>().Also(y =>
            {
                if (y.Cell_index == uCell.rc)
                {
                    y.number_text.GetComponent<Text>().text = y.Correct_number.ToString();
                    y.IsSelected = true;
                }
                y.bFocusBlockInHintMode = false;
                y.UpdateSquareColor();
            }));            
        }
    }
    */
    private void NakedSingle(int step)
    {
        var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).First();

        var hintStep = hintSteps[skillName];
        var shownNums = LineIndicator.Instance.GetAllRelatedSudokuCell(uCell.rc)
                                    .Select(x => grid_squares_[x].GetComponent<SudokuCell>())
                                    .Where(x => x.IsCorrectNumberSet())
                                    .Select(x => x.Correct_number)
                                    .Distinct().OrderBy(x => x)
                                    .ToList();//Enumerable.Range(1, 9).Except(new int[] { uCell.FixedNo}).ToList();

        grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
        {
            if (cell.Number > 0)
                cell.SetNumber(0);
        });

        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step];
            LineIndicator.Instance.GetAllRelatedSudokuCell(uCell.rc).Select(x => grid_squares_[x].GetComponent<SudokuCell>())
                .ForEach(x =>
                {
                    x.bHintMode = true;
                    x.Indicated = true;
                    x.bFocusCellInHintMode = (x.Cell_index == uCell.rc);
                    x.UpdateSquareColor();
                });
        }
        else if (step == 1)
        {
            if (shownNums.Count < 8)
            {
                HintMessage.text = $"the free cell can only be {uCell.FixedNo}";//hintStep.stepDesc[step].Replace("#nos", tmpStr);
                grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(x =>
                {
                    x.HighLightNote(uCell.FixedNo);
                });
            }
            else
            {
                var tmpStr = shownNums.Take(shownNums.Count - 1).Select(x => x.ToString()).Aggregate((a, b) => $"{a}, {b}") + " and " + shownNums.Last().ToString();
                HintMessage.text = hintStep.stepDesc[step].Replace("#nos", tmpStr);
                var tmpNums = new List<int>(shownNums);
                foreach (var cell in LineIndicator.Instance.GetAllRelatedSudokuCell(uCell.rc).Select(x => grid_squares_[x].GetComponent<SudokuCell>()))
                {
                    if (cell.IsCorrectNumberSet() && tmpNums.Contains(cell.Number))
                    {
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        tmpNums.Remove(cell.Number);
                        if (tmpNums.Count == 0)
                            break;
                    }
                }
            }
        }
        else
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no", uCell.FixedNo.ToString());
            grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
            {
                //cell.ClearupAllNotes();
                cell.number_text.GetComponent<Text>().text = cell.Correct_number.ToString();
                cell.IsSelected = true;
                cell.UpdateSquareColor();
            });
        }
    }

    private Dictionary<int, List<int>> GetPossibleNotesInHouse(SudokuCell[] house)
    {
        Dictionary<int, List<int>> notes = new Dictionary<int, List<int>>();
        grid_squares_.Select(x => x.GetComponent<SudokuCell>())
                     .Where(x => x.IsCorrectNumberSet() == false)
                     .ForEach(x =>
                     {
                         notes.Add(x.Cell_index, x.GetSquareNotes());  //generatePossibleNotes(x));
                     });
        house.Where(x => x.IsCorrectNumberSet() == false)
            .ForEach(x =>
            {
                notes[x.Cell_index] = generatePossibleNotes(x);
            });
        return notes;
    }
    private void HiddenSingle(int step)
    {
        var uCell = pHintGP.BDL.Where(x => x.FixedNo != 0).First();
        var hintStep = hintSteps[skillName];

        grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
        {
            if (cell.Number > 0)
                cell.SetNumber(0);
        });
        if (step == 0)
        {
            //// set note for explaination
            //// backup note
            //if (!noteHintMode)
            //{
            //    backupGridNote = GetGridNotes();
            //    fastNoteMode = true;
            //    if (NoteButton.Instance.Active)
            //        NoteButton.Instance.ToggleActive();
            //}
            var houseType = uCell.TypeOfHouse();
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] house;
            if (houseType == _HouseType.row)
            {
                house = LineIndicator.Instance.GetHorizontalLine(uCell.rc).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                house.ForEach(x => x.bFocusWholeRowInHintMode = true);
            }
            else if (houseType == _HouseType.column)
            {
                house = LineIndicator.Instance.GetVerticalLine(uCell.rc).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                house.ForEach(x => x.bFocusWholeColInHintMode = true);
            }
            else
            {
                house = LineIndicator.Instance.GetBlockFlat(uCell.rc).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                house.ForEach(x => x.bFocusBlockInHintMode = true);
            }

            house.ForEach(cell =>
            {
                cell.bHintMode = true;
                cell.Indicated = true;
                cell.UpdateSquareColor();
            });

            //SetGridNotes(GetPossibleNotesInHouse(house));
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no", uCell.FixedNo.ToString());
            grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
            {
                cell.bHilightedCellInHintMode = true;
                cell.UpdateSquareColor();
            });
        }
        else if (step == 2)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no", uCell.FixedNo.ToString());
            grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
            {
                //cell.ClearupAllNotes();
                cell.number_text.GetComponent<Text>().text = cell.Correct_number.ToString();
                cell.IsSelected = true;
                cell.UpdateSquareColor();
            });
        }
    }

    private void LockedCandidate(int step)
    {
        if (pHintGP.SolData < 200000)
            LockedCandidateType1(step);
        else
            LockedCandidateType2(step);
    }

    private void LockedCandidateType1(int step)
    {
        var hintStep = hintSteps[skillName];
        var b0 = pHintGP.SolData % 10;
        var hs = ((pHintGP.SolData - b0) / 10) % 100;
        var houseType = (hs < 9) ? _HouseType.row :
                        (hs < 18) ? _HouseType.column : _HouseType.block;
        var cancelB = pHintGP.BDL.Where(x => x.CancelB > 0).First().CancelB.BitToNum() + 1;

        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] house;

            house = LineIndicator.Instance.GetHouse(hs).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();

            SudokuCell[] blockHouse;
            blockHouse = LineIndicator.Instance.GetHouse(b0 + 18).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            blockHouse.ForEach(x => x.bFocusBlockInHintMode = true);

            house.Union(blockHouse).ToArray().ForEach(cell =>
            {
                cell.bHintMode = true;
                cell.Indicated = true;
                cell.UpdateSquareColor();
            });
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription()).Replace("#no", cancelB.ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.ECrLst != null && uCell.ECrLst.Count() > 0)
                {
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        cell.HighLightNote(cancelB);
                    });
                }
            });
        }
        else if (step == 2)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription()).Replace("#no", cancelB.ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.CancelB > 0)
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        cell.AlarmNote(cancelB);
                    });
            });
        }
    }

    private void LockedCandidateType2(int step)
    {
        var hintStep = hintSteps[LOCKED_CANDIDATE_TYPE2];
        var cancelB = pHintGP.BDL.Where(x => x.CancelB > 0).First().CancelB.BitToNum() + 1;
        var houseType = (pHintGP.SolData < 300000) ? _HouseType.row : _HouseType.column;

        if (step == 0)
        {
            var b0 = pHintGP.SolData % 10;
            var otherHRC1 = ((pHintGP.SolData - b0) / 10) % 100;
            var otherHRC2 = ((pHintGP.SolData - b0 - otherHRC1 * 10) / 1000) % 100;

            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] b1House, b2House;

            b1House = LineIndicator.Instance.GetHouse(otherHRC1).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            b2House = LineIndicator.Instance.GetHouse(otherHRC2).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();

            SudokuCell[] blockHouse;
            blockHouse = LineIndicator.Instance.GetHouse(b0 + 18).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            blockHouse.ForEach(x => x.bFocusBlockInHintMode = true);

            b1House.Union(b2House).Union(blockHouse).ToArray().ForEach(cell =>
            {                
                cell.bHintMode = true;
                cell.Indicated = true;
                cell.UpdateSquareColor();
            });
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription()).Replace("#no", cancelB.ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.ECrLst != null && uCell.ECrLst.Count() > 0)
                {
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        cell.HighLightNote(cancelB);
                    });
                }
            });
        }
        else if (step == 2)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription()).Replace("#no", cancelB.ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.CancelB > 0)
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        cell.AlarmNote(cancelB);
                    });
            });
        }
    }

    private void TakeSomeNotes(int step)
    {
        var hintStep = hintSteps[skillName];
        HintMessage.text = hintStep.stepDesc[step];
        if (noteHintMode)
        {
            foreach (var x in grid_squares_.Select(x => x.GetComponent<SudokuCell>()).ToList())
            {
                if (x.IsCorrectNumberSet()) continue;
                var note = (x.number_notes[x.Correct_number - 1]).GetComponent<SudokuNote>();
                if (!note.bSetted)
                {
                    note.bSetted = true;
                    note.bHighlight = true;
                }
            };
        }
    }

    private void LockedSet2D(int step)
    {
        var hintStep = hintSteps[skillName];
        var hs = pHintGP.SolData % 100;
        var houseType = (hs < 9) ? _HouseType.row :
                        (hs < 18) ? _HouseType.column : _HouseType.block;
        var b = (int)(pHintGP.SolData / 100);
        var cellPair = pHintGP.BDL.Where(x => x.ECrLst != null && x.ECrLst.Count() > 0).ToList();
        int[] notePair;
        if (skillName == LOCKED_SET_2D)
            notePair = cellPair[0].ECrLst[0].noB.BitToNumList().ToArray();
        else //if (skillName == LOCKED_SET_2D_HIDDEN)
            notePair = cellPair[0].ECrLst[0].noB.BitToNumList().Except(cellPair[0].CancelB.BitToNumList()).Except(cellPair[1].CancelB.BitToNumList()).ToArray();

        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] house;

            house = LineIndicator.Instance.GetHouse(hs).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            house.ForEach(cell =>
            {
                switch (houseType)
                {
                    case _HouseType.row:
                        cell.bFocusWholeRowInHintMode = true;
                        break;
                    case _HouseType.column:
                        cell.bFocusWholeColInHintMode = true;
                        break;
                    case _HouseType.block:
                        cell.bFocusBlockInHintMode = true;
                        break;
                }
                cell.bHintMode = true;
                cell.Indicated = true;
                cell.UpdateSquareColor();
            });
            cellPair.Select(x => x.rc).ForEach(idx =>
            {
                grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
                {
                    cell.bHilightedCellInHintMode = true;
                    cell.UpdateSquareColor();
                });
            });

            if (b > 0)
            {
                SudokuCell[] blockHouse;
                blockHouse = LineIndicator.Instance.GetHouse(b).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                blockHouse.ForEach(cell => {
                    cell.bHintMode = true;
                    cell.Indicated = true;
                    cell.UpdateSquareColor();
                });
            }
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no1", notePair[0].ToString()).Replace("#no2", notePair[1].ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.ECrLst != null && uCell.ECrLst.Count() > 0)
                {
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        //cell.bHilightedCellInHintMode = true;
                        //cell.UpdateSquareColor();
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        cell.HighLightNote(notePair[0]);
                        cell.HighLightNote(notePair[1]);
                    });
                }
            });
        }
        else if (step == 2)
        {
            if (skillName == LOCKED_SET_2D)
                HintMessage.text = hintStep.stepDesc[step].Replace("#no1", notePair[0].ToString()).Replace("#no2", notePair[1].ToString());
            else if (skillName == LOCKED_SET_2D_HIDDEN)
            {
                var cancelList = cellPair[0].CancelB.BitToNumList().Union(cellPair[1].CancelB.BitToNumList()).Select(x => x.ToString()).ToList();
                string nos = cancelList[0];
                if (cancelList.Count > 1)
                    nos = cancelList.GetRange(0, cancelList.Count - 1).Aggregate((a, b) => $"{a},{b}") + " and " + cancelList.Last();
                HintMessage.text = hintStep.stepDesc[step].Replace("#nos", nos);
            }
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                {
                    if (uCell.CancelB > 0)
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        uCell.CancelB.BitToNumList().ForEach(n => cell.AlarmNote(n));
                        if (skillName == LOCKED_SET_2D_HIDDEN)
                        {
                            cell.HighLightNote(notePair[0]);
                            cell.HighLightNote(notePair[1]);
                        }
                    }
                });
            });
        }

    }

    private void LockedSet3D(int step)
    {
        var hintStep = hintSteps[skillName];
        var hs = pHintGP.SolData % 100;
        var houseType = (hs < 9) ? _HouseType.row :
                        (hs < 18) ? _HouseType.column : _HouseType.block;
        var b = (int)(pHintGP.SolData / 100);
        var cellTriple = pHintGP.BDL.Where(x => x.ECrLst != null && x.ECrLst.Count() > 0).ToList();

        int[] noteTriple;
        var allNoB = cellTriple.Select(x => x.ECrLst[0].noB.BitToNumList().ToArray()).ToArray();
        var allCancelB = cellTriple.Select(x => x.CancelB.BitToNumList().ToArray()).ToArray();
        var unionOfAllNoB = allNoB.Aggregate((a, b) => a.Union(b).ToArray());
        var unionOfAlCancelB = allCancelB.Aggregate((a, b) => a.Union(b).ToArray());

        noteTriple = unionOfAllNoB.Except(unionOfAlCancelB).ToArray();

        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] house;

            house = LineIndicator.Instance.GetHouse(hs).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
            house.ForEach(cell =>
            {
                switch (houseType)
                {
                    case _HouseType.row:
                        cell.bFocusWholeRowInHintMode = true;
                        break;
                    case _HouseType.column:
                        cell.bFocusWholeColInHintMode = true;
                        break;
                    case _HouseType.block:
                        cell.bFocusBlockInHintMode = true;
                        break;
                }
                cell.bHintMode = true;
                cell.Indicated = true;
                cell.UpdateSquareColor();
            });
            cellTriple.Select(x => x.rc).ForEach(idx =>
            {
                grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
                {
                    if (cell.Number > 0)
                        cell.SetNumber(0);
                    cell.bHilightedCellInHintMode = true;
                    cell.UpdateSquareColor();
                });
            });

            if (b > 0)
            {
                SudokuCell[] blockHouse;
                blockHouse = LineIndicator.Instance.GetHouse(b).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                blockHouse.ForEach(cell => {
                    cell.bHintMode = true;
                    cell.Indicated = true;
                    cell.UpdateSquareColor();
                });
            }
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no1", noteTriple[0].ToString()).Replace("#no2", noteTriple[1].ToString()).Replace("#no3", noteTriple[2].ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.ECrLst != null && uCell.ECrLst.Count() > 0)
                {
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        //cell.bHilightedCellInHintMode = true;
                        //cell.UpdateSquareColor();
                        cell.HighLightNote(noteTriple[0]);
                        cell.HighLightNote(noteTriple[1]);
                        cell.HighLightNote(noteTriple[2]);
                    });
                }
            });
        }
        else if (step == 2)
        {
            if (skillName == LOCKED_SET_3D)
                HintMessage.text = hintStep.stepDesc[step].Replace("#no1", noteTriple[0].ToString()).Replace("#no2", noteTriple[1].ToString()).Replace("#no3", noteTriple[2].ToString());
            else if (skillName == LOCKED_SET_3D_HIDDEN)
            {
                //var xxx = cellTriple[0].CancelB.BitToNumList()
                //    .Union(cellTriple[1].CancelB.BitToNumList())
                //    .Union(cellTriple[2].CancelB.BitToNumList())
                //    .Select(x => x.ToString()).ToList();
                var cancelList = unionOfAlCancelB.Select(x => x.ToString()).ToList();//cellTriple[0].CancelB.BitToNumList().Union(cellTriple[1].CancelB.BitToNumList()).Select(x => x.ToString()).ToList();
                string nos = cancelList[0];
                if (cancelList.Count > 1)
                    nos = cancelList.GetRange(0, cancelList.Count - 1).Aggregate((a, b) => $"{a},{b}") + " and " + cancelList.Last();
                HintMessage.text = hintStep.stepDesc[step].Replace("#nos", nos);
            }
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                {
                    if (uCell.CancelB > 0)
                    {
                        cell.bHilightedCellInHintMode = true;
                        cell.UpdateSquareColor();
                        uCell.CancelB.BitToNumList().ForEach(n => cell.AlarmNote(n));
                    }
                });
            });
        }

    }

    private void XWing(int step)
    {
        var hintStep = hintSteps[skillName];
        var baseSet = pHintGP.BaseSet.HouseToNumList().Select(x => x - 1).ToArray();
        var coverSet = pHintGP.CoverSet.HouseToNumList().Select(x => x - 1).ToArray();
        var houseType = baseSet[0].Let<int, _HouseType>(hs => (hs < 9) ? _HouseType.row :
                                                              (hs < 18) ? _HouseType.column : _HouseType.block);

        var cellPair = pHintGP.BDL.Where(x => x.ECrLst != null && x.ECrLst.Count() > 0 && x.ECrLst[0].ClrCellBkg.r > 0).ToList();
        int[] notePair;
        notePair = cellPair[0].ECrLst[0].noB.BitToNumList().Except(cellPair[0].CancelB.BitToNumList()).Except(cellPair[1].CancelB.BitToNumList()).ToArray();
        var finSet = pHintGP.PFinSet?.ToList() ?? new List<int>();
        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription());
            SudokuCell[] house;

            baseSet.ForEach(hs =>
            {
                house = LineIndicator.Instance.GetHouse(hs).Select(x => grid_squares_[x].GetComponent<SudokuCell>()).ToArray();
                house.ForEach(cell =>
                {
                    cell.bHintMode = true;
                    cell.Indicated = true;
                    cell.UpdateSquareColor();
                });
            });
            cellPair.Select(x => x.rc).ForEach(idx =>
            {
                grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
                {
                    if (cell.Number > 0)
                        cell.SetNumber(0);
                    cell.bHilightedCellInHintMode = true;
                    cell.UpdateSquareColor();
                });
            });
            if (skillName == FINNED_XWING)
            {
                finSet.ForEach(x =>
                {
                    grid_squares_[x].GetComponent<SudokuCell>().Also(cell =>
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        notePair.ForEach(n => cell.HighLightNote(n));
                    });
                });
            }
        }
        else if (step == 1)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription())
                                                      .Replace("#no", pHintGP.SolData.ToString())
                                                      .Replace("#othertype", houseType == _HouseType.row ? _HouseType.column.GetDescription() : _HouseType.row.GetDescription());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                if (uCell.ECrLst != null && uCell.ECrLst.Count() > 0 && uCell.ECrLst[0].ClrCellBkg.r > 0)
                {
                    grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                    {
                        //cell.bHilightedCellInHintMode = true;
                        //cell.UpdateSquareColor();
                        notePair.ForEach(n => cell.HighLightNote(n));
                    });
                }
            });
        }
        else if (step == 2)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#type", houseType.GetDescription())
                                          .Replace("#no", pHintGP.SolData.ToString())
                                          .Replace("#othertype", houseType == _HouseType.row ? _HouseType.column.GetDescription() : _HouseType.row.GetDescription());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                {
                    //cell.bHilightedCellInHintMode = true;
                    //cell.UpdateSquareColor();
                    if (uCell.CancelB > 0)
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        uCell.CancelB.BitToNumList().ForEach(n => cell.AlarmNote(n));
                    }
                    if (skillName == FINNED_XWING && finSet.Contains(cell.Cell_index))
                        notePair.ForEach(n => cell.HighLightNote(n));
                });
            });
        }

    }

    private void XYWing(int step)
    {
        var hintStep = hintSteps[skillName];
        var no = pHintGP.SolData >> 24;
        var UCeStart = pHintGP.SolData >> 16 & 0xFF;
        var UCeA2 = (pHintGP.SolData >> 8) & 0xFF;
        var UCeB2 = pHintGP.SolData & 0xFF;

        if (step == 0)
        {
            HintMessage.text = hintStep.stepDesc[step];
            SudokuCell[] house;

            new List<int> { UCeStart, UCeA2, UCeB2 }.ForEach(idx =>
                 grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
                 {
                     if (cell.Number > 0)
                         cell.SetNumber(0);
                     cell.bHintMode = true;
                     cell.Indicated = true;
                     cell.bHilightedCellInHintMode = true;
                     cell.UpdateSquareColor();
                 })
                );
        }
        else if (step == 1)
        {
            var nos = pHintGP.BDL[UCeStart].FreeB.ToBitStringNZ(9);
            HintMessage.text = hintStep.stepDesc[step].Replace("#nos", $"{nos[0]} or {nos[1]}");
            grid_squares_[UCeStart].GetComponent<SudokuCell>().Also(cell =>
            {
                cell.bFocusCellInHintMode = true;
                cell.UpdateSquareColor();
            });
        }
        else if (step == 2)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no", no.ToString());
            new List<int> { UCeStart, UCeA2, UCeB2 }.ForEach(idx =>
                 grid_squares_[idx].GetComponent<SudokuCell>().Also(cell =>
                 {
                     if (cell.Cell_index == UCeStart)
                     {
                         cell.bFocusCellInHintMode = false;
                         cell.UpdateSquareColor();
                     }
                     else
                     {
                         cell.bFocusCellInHintMode = true;
                         cell.UpdateSquareColor();
                         cell.HighLightNote(no);
                     }
                 })
                );
        }
        else if (step == 3)
        {
            HintMessage.text = hintStep.stepDesc[step].Replace("#no", no.ToString());
            pHintGP.BDL.ForEachWithIndex((uCell, idx) =>
            {
                grid_squares_[uCell.rc].GetComponent<SudokuCell>().Also(cell =>
                {
                    //cell.bHilightedCellInHintMode = true;
                    //cell.UpdateSquareColor();
                    if (uCell.CancelB > 0)
                    {
                        if (cell.Number > 0)
                            cell.SetNumber(0);
                        uCell.CancelB.BitToNumList().ForEach(n => cell.AlarmNote(n));
                    }
                });
            });
        }

    }

    #endregion
    
    #region Hell Level Support
    
    private void InitializeValidationContext()
    {
        // Add or get ValidationContext component
        validationContext = GetComponent<ValidationContext>();
        if (validationContext == null)
        {
            validationContext = gameObject.AddComponent<ValidationContext>();
        }
        
        // Initialize with current game mode and grid squares
        validationContext.Initialize(GameSettings.Instance.GameMode, grid_squares_);
        
        Debug.Log($"ValidationContext initialized for {GameSettings.Instance.GameMode} mode");
    }
    
    // Public accessor for ValidationContext (useful for UI components)
    public ValidationContext GetValidationContext()
    {
        return validationContext;
    }
    
    // Check if current game mode is Hell Level
    public bool IsHellLevel()
    {
        return GameSettings.Instance.GameMode == EGameMode.HELL;
    }
    
    private void InitializeHellLevelUI()
    {
        // Create Hell Level UI components only if they don't already exist
        GameObject uiContainer = GameObject.Find("HellLevelUIContainer");
        if (uiContainer == null)
        {
            uiContainer = new GameObject("HellLevelUIContainer");
            uiContainer.transform.SetParent(canvas.transform, false);
            
            // Add RectTransform to UI container
            RectTransform containerRect = uiContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 1f);
            containerRect.anchorMax = new Vector2(0.5f, 1f);
            containerRect.anchoredPosition = new Vector2(0, -425);
            containerRect.sizeDelta = new Vector2(0, 850);
        }
        
        // Initialize Hell Level Mode Indicator
        // if (GameObject.FindObjectOfType<HellLevelModeIndicator>() == null)
        // {
        //     GameObject modeIndicatorObj = new GameObject("HellLevelModeIndicator");
        //     modeIndicatorObj.transform.SetParent(uiContainer.transform, false);
        //     modeIndicatorObj.AddComponent<HellLevelModeIndicator>();
        // }
        
        // Initialize Manual Validation Button
        if (GameObject.FindObjectOfType<ManualValidationButton>() == null)
        {
            GameObject validationButtonObj = new GameObject("ManualValidationButton");
            validationButtonObj.transform.SetParent(uiContainer.transform, false);
            validationButtonObj.AddComponent<ManualValidationButton>();
        }
        
        // Initialize Solution Progress Indicator
        if (GameObject.FindObjectOfType<SolutionProgressIndicator>() == null)
        {
            GameObject progressIndicatorObj = new GameObject("SolutionProgressIndicator");
            progressIndicatorObj.transform.SetParent(uiContainer.transform, false);
            progressIndicatorObj.AddComponent<SolutionProgressIndicator>();
        }
        
        // Initialize Visual Feedback Manager
        if (GameObject.FindObjectOfType<VisualFeedbackManager>() == null)
        {
            GameObject feedbackManagerObj = new GameObject("VisualFeedbackManager");
            feedbackManagerObj.transform.SetParent(uiContainer.transform, false);
            feedbackManagerObj.AddComponent<VisualFeedbackManager>();
        }
        
        //// Initialize Hell Level Introduction Modal
        //if (GameObject.FindObjectOfType<HellLevelIntroModal>() == null)
        //{
        //    GameObject introModalObj = new GameObject("HellLevelIntroModal");
        //    introModalObj.transform.SetParent(uiContainer.transform, false);
        //    introModalObj.AddComponent<HellLevelIntroModal>();
        //}
        
        // Initialize Hell Level Tutorial - DISABLED
        // if (GameObject.FindObjectOfType<HellLevelTutorial>() == null)
        // {
        //     GameObject tutorialObj = new GameObject("HellLevelTutorial");
        //     tutorialObj.transform.SetParent(uiContainer.transform, false);
        //     tutorialObj.AddComponent<HellLevelTutorial>();
        // }
        
        // Initialize Performance Optimizer
        //if (GameObject.FindObjectOfType<HellLevelPerformanceOptimizer>() == null)
        //{
        //    GameObject optimizerObj = new GameObject("HellLevelPerformanceOptimizer");
        //    optimizerObj.transform.SetParent(uiContainer.transform, false);
        //    optimizerObj.AddComponent<HellLevelPerformanceOptimizer>();
        //}
        
        // Initialize Testing Suite (only in development/debug builds)
        
        Debug.Log("Hell Level UI components initialized");
    }
    
    #endregion
}

