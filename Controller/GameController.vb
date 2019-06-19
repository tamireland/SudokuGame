'
' Copyright (c) 2014 Han Hung
' 
' This program is free software; it is distributed under the terms
' of the GNU General Public License v3 as published by the Free
' Software Foundation.
'
' http://www.gnu.org/licenses/gpl-3.0.html
' 

' TODO: Save current board to My.Settings so that the next time the game
'       starts, we can load it and the user can continue where they
'       left off.
'
' TODO: When game is paused, doodle something on the panel.
'
' TODO: Allow use of arrow keys to navigate grid
'
' TODO: Keep track of the last 10 best times per level
'
' TODO: Allow user to change the game colors.  Or maybe create themes that the user can switch to.
'
' TODO: Employ more advanced puzzle ranking algorithms.

Imports System.Threading
Imports System.Text
Imports SudokuPuzzle.Model
Imports SudokuPuzzle.SharedFunctions
Imports SudokuPuzzle.Controller.GameGenerator

Namespace Controller

    Friend Class GameController
        '
        ' This class controls the game and manages all aspects of the game behind
        ' the UI.  Basically the "C" in the MVC programming model.  The UI calls
        ' methods and properties on this class.  And this class communicates 
        ' asynchronously with the UI using Events.  All the "business" logic
        ' to run the game goes in here.

        ' Let's establish some basics about the game.  We'll use Excel as a template
        ' to establish naming conventions.  The Classic Sudoku board is basically a
        ' 9x9 grid, table, or board.  But other grids can be 4x4 or 16x16 or even 
        ' larger.  But let's keep things simple and stick with a 9x9 grid.  Maybe 
        ' in the future I'll modify the game so that users can select between 
        ' different sizes.   The 9x9 grid is broken down into 3x3 boxes or mini-grids.
        ' On a 9x9 board, there are 3 mini-grids across and 3 mini-grids going down.
        '
        ' Sudoku has one simple rule: the numbers 1 through 9 appear in each row, 
        ' column, and 3x3 box only once.  The game starts with several blank cells
        ' and the object of the game is to fill in numbers in the proper cells so
        ' that the rule is followed.
        '
        ' We'll use the following naming conventions:
        '
        '   Grid = the 9x9 playing board of the game.
        '
        '   Region = the 3x3 box or mini-grid within the main grid
        '
        '   Cell = each element of the grid.
        ' 
        ' For reference, here is a diagram showing how each cell is referenced in code.
        ' Basically, it's by [col][row] much like how Excel worksheets do it.  Sure
        ' we can use zero through 8 since all arrays in .Net are zero based.  But we'll
        ' use 1 through 9 instead just to keep things simple.
        '
        '   +--------+--------+--------+
        '   |11 21 31|41 51 61|71 81 91|
        '   |12 22 32|42 52 62|72 82 92|
        '   |13 23 33|43 53 63|73 83 93|
        '   +--------+--------+--------+
        '   |14 24 34|44 54 64|74 84 94|
        '   |15 25 35|45 55 65|75 85 95|
        '   |16 26 36|46 56 66|76 86 96|
        '   +--------+--------+--------+
        '   |17 27 37|47 57 67|77 87 97|
        '   |18 28 38|48 58 68|78 88 98|
        '   |19 29 39|49 59 69|79 89 99|
        '   +--------+--------+--------+
        '
        ' Here is how we will number each region.
        '
        '   +--------+--------+--------+
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   |..  1 ..|..  2 ..|..  3 ..|
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   +--------+--------+--------+
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   |..  4 ..|..  5 ..|..  6 ..|
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   +--------+--------+--------+
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   |..  7 ..|..  8 ..|..  9 ..|
        '   |.. .. ..|.. .. ..|.. .. ..|
        '   +--------+--------+--------+
        '
        ' Here is another diagram showing how the 9x9 grid is indexed as 
        ' a single dimensional array from 0 to 80 (for a zero based array)
        '
        '   +--------+--------+--------+
        '   |00 01 02|03 04 05|06 07 08|
        '   |09 10 11|12 13 14|15 16 17|
        '   |18 19 20|21 22 23|24 25 26|
        '   +--------+--------+--------+
        '   |27 28 29|30 31 32|33 34 35|
        '   |36 37 38|39 40 41|42 43 44|
        '   |45 46 47|48 49 50|51 52 53|
        '   +--------+--------+--------+
        '   |54 55 56|57 58 59|60 61 62|
        '   |63 64 65|66 67 68|69 70 71|
        '   |72 73 74|75 76 77|78 79 80|
        '   +--------+--------+--------+
        '

        ' TODO: Expand the game to accommodate 4x4 and 16x16 games.

#Region " Variables, Constants, And other Declarations "

#Region " Variables "

        Private _View As frmMain
        Private _Model As GameModel

        Private _clsGamesManager As GamesManager
        Private _bGameInProgress As Boolean = False
        Private _bPuzzleComplete As Boolean = False
        Private _eStartButtonState As StartButtonStateEnum

        Private _lRegion(9) As List(Of CellIndex)
        Private _lLabels(9, 9) As Label

#End Region

#Region " Variables With Events "

        Private WithEvents _clsGameTimer As GameTimer

#End Region

#Region " Other Declarations "

        Friend Delegate Sub GameControllerEventHandler(sender As Object, e As GameControllerEventArgs)
        Friend Event GameControllerEvent As GameControllerEventHandler

#End Region

#End Region

#Region " Properties "

#Region " Public Properties "

        Friend WriteOnly Property SetAllCellLabels() As String
            Set(value As String)
                For iCol As Int32 = 1 To 9                          ' Loop through the lable array
                    For iRow As Int32 = 1 To 9
                        _lLabels(iCol, iRow).Text = value            ' Set the text field
                    Next
                Next
            End Set
        End Property

#End Region

#Region " Private Properties "

        Private Property CurrentCell As CellIndex
        Private Property PreviousCell As CellIndex

        Private WriteOnly Property SetStartButtonState As StartButtonStateEnum
            Set(value As StartButtonStateEnum)
                _eStartButtonState = value                          ' Save the game state
                _View.SetStartButtonText = value                    ' Set the start button text
            End Set
        End Property

#End Region

#End Region

#Region " Constructors "

        Friend Sub New(view As Form)
            _View = CType(view, frmMain)                            ' Save the View variable
            InitControllerVariables()                               ' Init any controller variables
        End Sub

#End Region

#Region " Event Handlers "

        Private Sub _GameTimer_GameTimerEvent(sender As Object, e As GameTimerEventArgs) Handles _clsGameTimer.GameTimerEvent
            RaiseGameControllerEvent(e.ElapsedTime)                 ' Pass the timer event to whoever is listening.
        End Sub

#End Region

#Region " Methods "

#Region " Public Methods "

#Region " Public Methods: UI Events "

        Friend Sub FormClickedEvent()
            FormClicked()
        End Sub

        Friend Sub FormClosing()
            StopGame()
            SaveSettings()
        End Sub

        Friend Sub CloseButtonClicked()
            ' TODO: If game is in progress, save current game as well.
            _clsGamesManager.StopGamesManager()
        End Sub

        Friend Sub NewButtonClicked()
            LoadNewGame()
        End Sub

        Friend Sub StartButtonClicked()
            StartGame()
        End Sub

        Friend Sub ResetButtonClicked()
            ResetGame()
        End Sub

        Friend Sub HintButtonClicked()
            ShowHint()
        End Sub

        Friend Sub ClearButtonClicked()
            ClearCell()
        End Sub

        Friend Sub PrintButtonClicked()
            ' TODO: implement print routine
        End Sub

        Friend Sub HelpButtonClicked()
            ShowHelp()
        End Sub

        Friend Sub ShowSolutionCheckBoxClicked(bShowSolution As Boolean)
            ShowHideSolution(bShowSolution)
        End Sub

        Friend Sub ShowHideNotesCheckBoxClicked(bShowNotes As Boolean)
            ShowHideNotes(bShowNotes)
        End Sub

        Friend Sub NumberButtonClicked(btnIndex As Int32)
            ProcessNumberButton(btnIndex)
        End Sub

        Friend Sub CellClickedEvent(iCol As Int32, iRow As Int32)
            CellClicked(iCol, iRow)
        End Sub

        Friend Sub PaintCellEvent(iCol As Int32, iRow As Int32, e As PaintEventArgs)
            PaintCell(iCol, iRow, e)
        End Sub

#End Region

#Region " Public functions: Form/Game Actions "

        Friend Sub InitLabelArray(iCol As Int32, iRow As Int32, lLabel As Label)
            If Common.IsValidIndex(iCol, iRow) Then                    ' If the iCol and iRow values are good
                _lLabels(iCol, iRow) = lLabel                           ' Save the pointer to the corresponding label
            End If
        End Sub

        Friend Sub ClearForm()
            SetAllCellLabels = ""                               ' Set all labels to blank
            _View.EnterNotes = False                            ' Clear the checkboxes as the bottom
            _View.ShowAllNotes = False
            _View.ShowSolution = False
            _View.SetStatus = ""                                ' Clear both status displays
            _View.SetStatusBar = ""
        End Sub

        Friend Sub LoadSettings()
            _View.GameLevel = My.Settings.Level
        End Sub

#End Region

#End Region

#Region " Private Methods "

        Private Sub InitControllerVariables()
            _clsGameTimer = New GameTimer                               ' Initialize some of the internal variables
            _clsGamesManager = New GamesManager
            InitRegions()                                               ' Intialize the region array
            LoadSettings()                                              ' Load settings
            SetStartButtonState = StartButtonStateEnum.StartGame        ' Set the start button state
        End Sub

        Private Sub InitRegions()
            For I As Int32 = 1 To 9                         ' Loop through all the regions
                _lRegion(I) = New List(Of CellIndex)        ' Create a new List
            Next
            AddCellsToRegion(1, 1, 3, 1, 3)                 ' For each region, add the corresponding cell coordinates
            AddCellsToRegion(2, 4, 6, 1, 3)
            AddCellsToRegion(3, 7, 9, 1, 3)
            AddCellsToRegion(4, 1, 3, 4, 6)
            AddCellsToRegion(5, 4, 6, 4, 6)
            AddCellsToRegion(6, 7, 9, 4, 6)
            AddCellsToRegion(7, 1, 3, 7, 9)
            AddCellsToRegion(8, 4, 6, 7, 9)
            AddCellsToRegion(9, 7, 9, 7, 9)
        End Sub

        Private Sub AddCellsToRegion(iRegion As Int32, iColStart As Int32, iColEnd As Int32, iRowStart As Int32, iRowEnd As Int32)
            For iCol As Int32 = iColStart To iColEnd                    ' For each column in the region
                For iRow As Int32 = iRowStart To iRowEnd                ' For each row in the region
                    _lRegion(iRegion).Add(New CellIndex(iCol, iRow))    ' Add the corresponding cell coordinates to this region
                Next
            Next
        End Sub

        Private Sub RaiseGameControllerEvent(sElapsedTime As String)
            Dim e As New GameControllerEventArgs(sElapsedTime)          ' Create an EventArg
            RaiseEvent GameControllerEvent(Me, e)                       ' Raise the event
        End Sub

        Private Sub SaveSettings()
            My.Settings.Level = _View.GameLevel
        End Sub

        Private Sub ShowHelp()
            _View.ShowHelp()
        End Sub

        Private Sub LoadNewGame()
            If _bGameInProgress Then                                ' Is there a game in progress?
                Dim iResult As MsgBoxResult = MsgBox("There is already a game in progress.  Do you want to play a new game?", MsgBoxStyle.YesNo, "Sudoku")
                If iResult = MsgBoxResult.Yes Then                  ' User really wants to start a new game?
                    GameEnded(False)                                ' Okay, reset some stuff
                    GetNewGame()                                    ' Get a new game
                End If
            Else                                                    ' There is no game in progress
                _View.EnableGameButtons(False, True)                ' Disable the view buttons
                _View.SetStatusBar = ""                             ' Clear the status bar
                GetNewGame()                                        ' Load a new game
            End If
        End Sub

        Private Sub GetNewGame()
            If _clsGamesManager.GameCount(_View.LevelSelected) > 0 Then                 ' Any games available?
                _Model = New GameModel(_clsGamesManager.GetGame(_View.LevelSelected))   ' Yes, get it and load the Model
                _View.SetStatusBar = "New game loaded."                                 ' Display a message
                If _bGameInProgress Then                                                ' Is there a game already in progress?
                    StartNewGame()                                                      ' Yes, then restart some game stuff
                Else
                    SetStartButtonState = StartButtonStateEnum.StartGame                ' No, then set the start button state
                End If
            Else                                                                        ' No games available ... tell user
                _View.SetStatusBar = "No games available for the selected level.  Please select another level."
            End If
        End Sub

        Private Sub StartGame()
            If _bGameInProgress Then                                        ' Game already in progress?
                If _eStartButtonState = StartButtonStateEnum.PauseGame Then ' Yes, is the start button state "Pause"?
                    PauseGame()                                             ' Yes, then pause the game
                Else
                    ResumeGame()                                            ' No, must be resume ... so resume the game
                End If
            Else
                StartNewGame()                                              ' No game, so start a new game
            End If
        End Sub

        Private Sub StartNewGame()
            _bGameInProgress = True                                     ' Raise the GameInProgress flag
            ShowBoard()                                                 ' Show the grid
            _clsGameTimer.StartTimer()                                  ' Start the timer
            SetStartButtonState = StartButtonStateEnum.PauseGame        ' Set the start button state to "Pause"
            _View.EnableGameButtons(True, False)                        ' Enable the game buttons plus hide the panel blocking the grid
            UpdateStatusBarCount()                                      ' Display the empty count on screen for the first time
        End Sub

        Private Sub PauseGame()
            SetStartButtonState = StartButtonStateEnum.ResumeGame       ' Set start button state to "Resume"
            _clsGameTimer.PauseTimer()                                  ' Pause the timer
            _View.EnableGameButtons(False, True)                        ' Disable the game buttons and hide the game grid
        End Sub

        Private Sub ResumeGame()
            SetStartButtonState = StartButtonStateEnum.PauseGame        ' Set the start button state to "Pause"
            _clsGameTimer.ResumeTimer()                                 ' Resume the game timer
            _View.EnableGameButtons(True, False)                        ' Enable the game buttons and hide the panel blocking the game
        End Sub

        Private Sub GameEnded(bShowDialog As Boolean)
            _clsGameTimer.StopTimer()                                       ' Stop game timer
            _bPuzzleComplete = True                                         ' Raise PuzzleComplete flag
            _bGameInProgress = False                                        ' Clear GameInProgress flag
            If bShowDialog Then                                             ' Do we show the GameComplete dialog?
                _View.EnableGameButtons(False, False)                       ' Yes, disable the game button, but don't hide the game
                SetStartButtonState = StartButtonStateEnum.DisableButton    ' Set start button state to "Disable"
                _View.SetStatusBar = "Puzzle completed!"                    ' Set status bar text
                Thread.Sleep(0)                                             ' Sleep just to allow other threads to run
                _View.DisplayGameCompletedForm(_clsGameTimer.ElapsedTime)   ' Pop up the Game Complete dialog
            Else                                                            ' Don't show dialog
                _View.EnableGameButtons(False, True)                        ' Disable game buttons and hide the game
                SetStartButtonState = StartButtonStateEnum.StartGame        ' Set start button state to "Start"
            End If
        End Sub

        Private Sub ShowBoard()
            ClearForm()                                 ' Clear the game grid
            RepaintBoard()                              ' Repaint the board with the new data
            _bPuzzleComplete = False                    ' Clear the PuzzleComplete flag
        End Sub

        Private Sub RepaintBoard()
            For Each Item As Label In _lLabels          ' Loop through the label array
                If Item IsNot Nothing Then              ' If there's a label pointer
                    Item.Invalidate()                   ' Invalidate the label to force a repaint
                End If
            Next
        End Sub

        Private Sub PaintCell(iCol As Int32, iRow As Int32, e As PaintEventArgs)
            FillCellState(iCol, iRow, e)                                            ' Fill Cell state
            Dim SelectedCell As New CellIndex(iCol, iRow)
            If SelectedCell.IsSameCell(CurrentCell) Then                            ' Is the current cell pointer same as this cell?
                If _Model.Cell(iCol, iRow).CellState <> CellStateEnum.Answer Then   ' Yes, if it's not an "Answer" cell, then draw a highlight box around it.
                    HighlightCell(SelectedCell, e)
                End If
            Else
                UnHighlightCell(SelectedCell, e)                                    ' Otherwise, undo the highlight
            End If
        End Sub

        Private Sub FillCellState(iCol As Int32, iRow As Int32, e As PaintEventArgs)
            If _Model IsNot Nothing Then
                With _Model.Cell(iCol, iRow)
                    Select Case .CellState
                        Case CellStateEnum.Blank                                    ' Cell state = blank?
                            ClearCell(iCol, iRow, e)                                ' Yes, the clear the cell contents

                        Case CellStateEnum.Hint                                     ' Cell state = Hint?
                            _lLabels(iCol, iRow).ForeColor = Color.BlueViolet       ' Yes, then set the forecolore to purple
                            _lLabels(iCol, iRow).Text = .Answer.ToString            ' Display the answer

                        Case CellStateEnum.Notes                                    ' Cell state = Notes?
                            ClearCell(iCol, iRow, e)                                ' Clear cell contents
                            DrawNotes(iCol, iRow, e)                                ' Draw notes directly on the label

                        Case CellStateEnum.Answer                                   ' Cell state = Answer?
                            _lLabels(iCol, iRow).ForeColor = Color.Black            ' Set text forecolor to Black
                            _lLabels(iCol, iRow).Text = .Answer.ToString            ' Display the answer

                        Case CellStateEnum.UserInput                                ' Cell state = user answer?
                            If .IsCorrect Then                                      ' Is the user's answer correct?
                                _lLabels(iCol, iRow).ForeColor = Color.DarkGreen    ' Yes, then set text forecolor to green
                            Else
                                _lLabels(iCol, iRow).ForeColor = Color.DarkRed      ' No, set text forecolor to dark red
                            End If
                            _lLabels(iCol, iRow).Text = .UserAnswer.ToString        ' Display the user's answer

                    End Select
                End With
            End If
        End Sub

        Private Sub ClearCell(iCol As Int32, iRow As Int32, e As PaintEventArgs)
            With _lLabels(iCol, iRow)
                .ForeColor = Color.Blue                                         ' Set the text forecolor to blue
                .Text = ""                                                      ' Set the text field to blank string
                e.Graphics.Clear(.BackColor)                                    ' Clear the label of any graphics
            End With
        End Sub

        Private Sub DrawNotes(iCol As Int32, iRow As Int32, e As PaintEventArgs)
            With _Model.Cell(iCol, iRow)
                If .HasNotes Then                                               ' Does this cell have any notes?
                    Dim drawFont As New Font("Arial", 8)                        ' Create a font element
                    For I As Int32 = 1 To 3                                     ' Loop through each number
                        For J As Int32 = 1 To 3
                            Dim noteIndex As Int32 = J + ((I - 1) * 3)          ' Compute the index into the notes array
                            If .Notes(noteIndex) Then                           ' Is the note field turned on?
                                With _lLabels(iCol, iRow)                       ' Yes, compute the location on the label to print the number
                                    Dim X As Int32 = CInt((.Width / 3) * (J - 1))
                                    Dim Y As Int32 = CInt((.Height / 3) * (I - 1))
                                    e.Graphics.DrawString(noteIndex.ToString, drawFont, Brushes.Black, X, Y)
                                End With
                            End If
                        Next
                    Next
                    drawFont.Dispose()                                          ' Get rid of the font object
                End If
            End With
        End Sub

        Private Sub ShowHideNotes()
            ShowHideNotes(_View.ShowAllNotes)                                   ' Show/Hide notes
        End Sub

        Private Sub ShowHideNotes(bShowNotes As Boolean)
            If bShowNotes Then                                                  ' Is the ShowNotes flag on?
                ShowNotes()                                                     ' Yes, then show the notes
            Else
                HideNotes()                                                     ' No, hide the notes
            End If
        End Sub

        Private Sub ShowNotes()
            For iCol As Int32 = 1 To 9                              ' Loop through all the cells
                For iRow As Int32 = 1 To 9
                    With _Model.Cell(iCol, iRow)
                        If .CellState = CellStateEnum.Blank Then    ' Is the cell state blank?
                            .CellState = CellStateEnum.Notes        ' Yes, then change the state to Notes
                            _lLabels(.Col, .Row).Invalidate()       ' Force cell to repaint
                        End If
                    End With
                Next
            Next
        End Sub

        Private Sub HideNotes()
            For iCol As Int32 = 1 To 9                              ' Loop through all the cells
                For iRow As Int32 = 1 To 9
                    With _Model.Cell(iCol, iRow)
                        If .CellState = CellStateEnum.Notes Then    ' Is the cell state Notes?
                            .CellState = CellStateEnum.Blank        ' Yes, then change the state to Blank
                            _lLabels(.Col, .Row).Invalidate()       ' Force cell to repaint
                        End If
                    End With
                Next
            Next
        End Sub

        Private Sub ShowHideSolution(bShowSolution As Boolean)
            If bShowSolution Then                               ' Show solution?
                ShowSolution()                                  ' Yes, the show it
            Else
                HideSolution()                                  ' No, hide it
            End If
        End Sub

        Private Sub ShowSolution()
            For iCol As Int32 = 1 To 9                              ' Loop through all the cells
                For iRow As Int32 = 1 To 9
                    With _Model.Cell(iCol, iRow)                    ' If it's not the answer and user did not enter anything in this cell yet.
                        If (.CellState <> CellStateEnum.Answer) AndAlso (.CellState <> CellStateEnum.UserInput) Then
                            .CellState = CellStateEnum.Hint         ' Change the cell state flag to Hint
                            _lLabels(iCol, iRow).Invalidate()       ' Force cell to repaint
                        End If
                    End With
                Next
            Next
        End Sub

        Private Sub HideSolution()
            For iCol As Int32 = 1 To 9                              ' Loop through all the cells
                For iRow As Int32 = 1 To 9
                    With _Model.Cell(iCol, iRow)
                        If .CellState = CellStateEnum.Hint Then     ' If the cell state is "Hint"
                            .CellState = CellStateEnum.Blank        ' Set it to Blank
                            _lLabels(iCol, iRow).Invalidate()       ' Force cell to repaint
                        End If
                    End With
                Next
            Next
            ShowHideNotes()
        End Sub

        Private Sub FormClicked()
            CurrentCell = Nothing                   ' User clicked outside the board.  Set current cell to nothing.
            UnHighlightPreviousCell()               ' Unhighlight the previous cell.
        End Sub

        Private Sub CellClicked(iCol As Int32, iRow As Int32)
            CurrentCell = New CellIndex(iCol, iRow)                 ' Save the current cell pointer
            UnHighlightPreviousCell()                               ' Unhighlight any previous cell
            With _Model.Cell(CurrentCell)
                _lLabels(iCol, iRow).Invalidate()                   ' Force this cell to repaint
                If (Not _bPuzzleComplete) AndAlso (.CellState <> CellStateEnum.Answer) Then         ' If the puzzle is not complete and cell state is not the answer
                    Dim iResult As Int32 = _View.ShowNumberPanel(_lLabels(iCol, iRow), iCol, iRow)  ' Show the mini input window
                    ProcessNumberButton(iResult)                                                    ' Process the results
                End If
            End With
        End Sub

        Private Sub UnHighlightPreviousCell()
            If PreviousCell IsNot Nothing Then                              ' Any previous cell selected?
                _lLabels(PreviousCell.Col, PreviousCell.Row).Invalidate()   ' Yes, force the cell to repaint
            End If
        End Sub

        Private Sub HighlightCell(uSelectedCell As CellIndex, e As PaintEventArgs)
            DrawBorder(uSelectedCell, e, True)                              ' Draw a border to highlight the cell
            PreviousCell = uSelectedCell                                    ' Save selected cell to previous pointer
        End Sub

        Private Sub UnHighlightCell(uSelectedCell As CellIndex, e As PaintEventArgs)
            DrawBorder(uSelectedCell, e, False)                             ' Unghighlight the selected cell
        End Sub

        Private Sub DrawBorder(uSelectedCell As CellIndex, e As PaintEventArgs, bHighlight As Boolean)
            With _lLabels(uSelectedCell.Col, uSelectedCell.Row)
                Dim highlightPen As Pen
                If bHighlight Then                                              ' Highlight cell?
                    highlightPen = New Pen(Brushes.CadetBlue, 4)                ' Yes, use this color
                Else
                    highlightPen = New Pen(.BackColor, 4)                       ' No, use the background color
                End If
                e.Graphics.DrawRectangle(highlightPen, 0, 0, .Width, .Height)   ' Draw a rectangle around the label
                highlightPen.Dispose()                                          ' Dispose the pen
            End With
        End Sub

        Private Sub ProcessNumberButton(value As Int32)
            If Common.IsValidIndex(value) AndAlso CurrentCell IsNot Nothing Then    ' Is the input number valid? And the current cell pointer is valid?
                With _Model.Cell(CurrentCell)                                       ' Yes
                    If .CellState <> CellStateEnum.Answer Then                      ' Is cell state not the answer?
                        If _View.EnterNotes Then                                    ' Yes, user wants to enter notes?
                            ProcessUserNotes(_Model.Cell(CurrentCell), value)       ' Process notes
                        Else                                                        ' No, user is attempting to enter a number
                            ProcessUserAnswer(_Model.Cell(CurrentCell), value)      ' Process user's input
                        End If
                    End If
                End With
            End If
        End Sub

        Private Sub ProcessUserNotes(uCell As CellStateClass, value As Int32)
            With uCell
                If .CellState = CellStateEnum.Blank Then                ' Cell state blank?
                    .ClearNotes()                                       ' Yes, clear any previous notes
                    .CellState = CellStateEnum.Notes                    ' Set cell state to notes
                    .Notes(value) = Not .Notes(value)                   ' Raise the corresponding note index
                ElseIf .CellState = CellStateEnum.Notes Then            ' Not blank, is it Notes?
                    .Notes(value) = Not .Notes(value)                   ' Yes, then toggle the corresponding note index
                End If
                _lLabels(.Col, .Row).Invalidate()                       ' Invalidate the cell
            End With
        End Sub

        Private Sub ProcessUserAnswer(uCell As CellStateClass, value As Int32)
            With uCell
                .UserAnswer = value                             ' Save user's answer
                .CellState = CellStateEnum.UserInput            ' Set cell state to user answer
                _lLabels(.Col, .Row).Invalidate()               ' Repaint the cell
                If .IsCorrect Then                              ' Is it correct?
                    ScanNotes(CurrentCell, value)               ' Yes, turn off notes in surrounding cells
                    UpdateStatusBarCount(-1)                    ' Decrement and update screen count
                    If _Model.GameComplete Then                 ' Did user fill in all the blanks?
                        GameEnded(True)                         ' Yes, then go to end game routine
                    End If
                End If
            End With
        End Sub

        Private Sub UpdateStatusBarCount(Optional iCount As Int32 = 0)
            _Model.EmptyCount += iCount                         ' Increment or decrement the Empty count
            _View.SetStatusBar = _Model.EmptyCount.ToString & " empty cells out of 81."
        End Sub

        Private Sub ScanNotes(uCellIndex As CellIndex, value As Int32)
            For iScan As Int32 = 1 To 9
                CheckNotes(_Model.Cell(iScan, uCellIndex.Row), value)       ' Scan the column
                CheckNotes(_Model.Cell(uCellIndex.Col, iScan), value)       ' Scan the rows
            Next
            For Each uItem As CellIndex In _lRegion(uCellIndex.Region)      ' Scan the region
                CheckNotes(_Model.Cell(uItem), value)                       ' Check notes in the region
            Next
        End Sub

        Private Sub CheckNotes(uCell As CellStateClass, value As Int32)
            With uCell
                If .CellState = CellStateEnum.Notes AndAlso .Notes(value) Then  ' Is cell in note mode and corresponding note flag is raised?
                    .Notes(value) = False                                       ' Yes, then turn if off
                    _lLabels(.Col, .Row).Invalidate()                           ' Force label to repaint
                End If
            End With
        End Sub

        Private Sub ClearCell()
            If CurrentCell IsNot Nothing Then                                   ' Current cell pointer valid?
                With _Model.Cell(CurrentCell)                                   ' Yes, then process it
                    If .CellState <> CellStateEnum.Answer Then                  ' Are we displaying the answer?
                        If UndoEmptyCount(_Model.Cell(CurrentCell)) Then        ' Yes, check if we need to increment the empty counter
                            UpdateStatusBarCount(1)                             ' Yes, increment and update screen count
                        End If
                        .CellState = CellStateEnum.Blank                        ' Set cell state back to "Blank"
                        .UserAnswer = 0                                         ' Clear user's answer
                        _Model.ComputeNote(CurrentCell.Col, CurrentCell.Row)    ' Recompute note for this cell
                        _lLabels(CurrentCell.Col, CurrentCell.Row).Invalidate() ' Force label to repaint
                    End If
                End With
            End If
        End Sub

        Private Function UndoEmptyCount(uCell As CellStateClass) As Boolean
            With uCell
                If .CellState = CellStateEnum.Hint Then                                 ' Is the cell state "Hint"?
                    Return True                                                         ' Yes, then return True
                ElseIf (.CellState = CellStateEnum.UserInput) AndAlso (.IsCorrect) Then ' Is the cell state "User Input" and it's correct?
                    Return True                                                         ' Yes, then return True
                End If
            End With
            Return False                                                                ' Otherwise, just return False
        End Function

        Private Sub ShowHint()
            If CurrentCell IsNot Nothing Then                                   ' Current cell pointer valid?
                With _Model.Cell(CurrentCell)                                   ' Yes, then process it
                    If .CellState <> CellStateEnum.Answer Then                  ' Are we displaying the answer?
                        .CellState = CellStateEnum.Hint                         ' No, then set it to Hint
                        _lLabels(CurrentCell.Col, CurrentCell.Row).Invalidate() ' Force label to repaint
                        UpdateStatusBarCount(-1)                                ' Decrement and update the screen count
                    End If
                End With
            End If
        End Sub

        Private Sub ResetGame()
            ClearForm()                                                 ' Clear game grid
            _clsGameTimer.ResetTimer()                                  ' Restart the timer
            For iCol As Int32 = 1 To 9                                  ' Loop through all the cells in the grid
                For iRow As Int32 = 1 To 9
                    With _Model.Cell(iCol, iRow)
                        If .CellState <> CellStateEnum.Answer Then      ' If not displaying the answer
                            .CellState = CellStateEnum.Blank            ' Set state to blank
                            .UserAnswer = 0                             ' Clear out any previous answer
                            _lLabels(iCol, iRow).Invalidate()           ' Force lable to repaint
                        End If
                    End With
                Next
            Next
        End Sub

        Private Sub StopGame()
            If _bGameInProgress Then                                ' If game is in progress ...?
                ' Do we need to do something here?
            End If
        End Sub

#End Region

#End Region

    End Class

End Namespace
