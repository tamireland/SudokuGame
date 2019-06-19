'
' Copyright (c) 2014 Han Hung
' 
' This program is free software; it is distributed under the terms
' of the GNU General Public License v3 as published by the Free
' Software Foundation.
'
' http://www.gnu.org/licenses/gpl-3.0.html
' 

Imports SudokuPuzzle.Model

Namespace Controller.GameGenerator

    Friend Class MaskPuzzle
        ' After playing many Sudoku puzzles over the years, some of the more 
        ' difficult puzzles can have multiple solutions.  I find that these kinds of
        ' puzzles are not true Sudoku puzzles because at some point, you have to guess
        ' what number goes into a cell to complete the puzzle.  I feel that guessing
        ' takes away from the core essense of the game which is to solve the puzzle 
        ' by pure logic.  Of course, this is debatable among Sudoku game enthusiates.
        ' But for this program, each game that is generated can have one and only one
        ' solution.
        '
        ' After a Sudoku game is generated, we need to mask some cells based on the
        ' level of difficulty requested.  But we need to make sure that after
        ' masking, the puzzle has one and only one solution.
        '
        ' At first, I thought of the following logic:
        '   
        '   CreateGame()
        '   Do
        '       MaskPuzzle()
        '   Loop Until number of solution to puzzle = 1
        '
        ' But after looking at that, I realize that after creating a puzzle, there
        ' really is one and only one solution.  So, in the process of masking the
        ' puzzle, we should try to solve the puzzle before masking the next cell.
        ' This turns out to be much faster since we can backtrack if masking the
        ' next cell creates a puzzle with multiple solutions.  So the code for
        ' the logic.
        '
        ' TODO: Mask on a vertical mirror ... meaning the mask patter is symmetrical
        '       on the vertical axis.
        '

#Region " Variables, Constants, And other Declarations "

#Region " Constants "

        ' Maximum number of iterations we will try before declaring the puzzle unsolvable.
        Private Const _cMaxIterations As Int32 = 10

#End Region

#Region " Variables "

        Private _clsRandom As RandomClass
        Private _eLevel As GameLevelEnum
        Private _eNewLevel As GameLevelEnum
        Private _bNotGood As Boolean

#End Region

#End Region

#Region " Public Properties "

        Friend ReadOnly Property Level As GameLevelEnum
            Get
                Return _eLevel
            End Get
        End Property

        Friend ReadOnly Property NewLevel As GameLevelEnum
            Get
                Return _eNewLevel
            End Get
        End Property

        Friend ReadOnly Property LevelChanged As Boolean
            Get
                Return _eLevel <> _eNewLevel
            End Get
        End Property

        Friend ReadOnly Property NotGood As Boolean
            Get
                Return _bNotGood
            End Get
        End Property

#End Region

#Region " Constructors "

        Friend Sub New(Level As GameLevelEnum)
            _eLevel = Level
            _eNewLevel = Level
            _clsRandom = RandomClass.GetInstance
        End Sub

#End Region

#Region " Methods "

#Region " Public Methods "

        Friend Sub MaskPuzzle(uCells(,) As CellStateClass)
            Dim iMaskValue As Int32 = GetMaskValue()                        ' Figure out number of squares to mask
            Dim cSolve As New SolveGame(uCells)                             ' Instantiate the Solve class
            Dim iNumIterations As Int32 = 0                                 ' Init the iteration counter
            _bNotGood = False                                               ' Clear the NotGood flag
            Do
                Dim Index As Int32 = _clsRandom.GetRandomInt(80)            ' Find a random cell to mask
                Dim Cell As CellIndex = New CellIndex(Index)                ' Convert the one dimensional index to a row and column
                With uCells(Cell.Col, Cell.Row)
                    If .CellState = CellStateEnum.Answer Then               ' Is that cell(col, row) already masked?
                        .CellState = CellStateEnum.Blank                    ' No, then mark it as masked
                        cSolve.SolvePuzzle()                                ' Try to solve the puzzle
                        If cSolve.Solvable Then                             ' Is it solvable?
                            iMaskValue -= 1                                 ' Yes, then find next square to mask
                            iNumIterations = 0                              ' Reset iteration counter
                        Else
                            .CellState = CellStateEnum.Answer               ' No, put cell state back to unmasked and try again
                            iNumIterations += 1                             ' Increment the iteration counter
                            If iNumIterations > _cMaxIterations Then        ' Did we hit the max number of iterations?
                                _eNewLevel = GetLevel(81 - iMaskValue)      ' Yes, figure out the new level
                                _bNotGood = True                            ' Raise flag
                                Exit Do                                     ' Then exit Do loop
                            End If
                        End If
                    End If
                End With
            Loop While iMaskValue >= 0                                      ' Keep looping until we unmask the necessary number of squares to match the given level
        End Sub

#End Region

#Region " Private Methods "

        ' Level             Given
        '=========================
        ' Very Easy         50+     
        ' Easy              36-49
        ' Medium            32-35
        ' Hard              28-31
        ' Expert            22-27

        Private Function GetMaskValue() As Int32
            Dim iMin As Int32
            Dim iMax As Int32
            Select Case Me.Level
                Case GameLevelEnum.VeryEasy
                    ' Very easy puzzles have between 50 and 60 given values
                    iMin = 50
                    iMax = 60

                Case GameLevelEnum.Easy
                    ' Easy puzzles have between 36 and 49 given values
                    iMin = 36
                    iMax = 49

                Case GameLevelEnum.Medium
                    ' Medium puzzles between 32 and 35 given values
                    iMin = 32
                    iMax = 35

                Case GameLevelEnum.Hard
                    ' Hard puzzles have between 28 and 31 given values
                    iMin = 28
                    iMax = 31

                Case Else   ' Expert level
                    ' Expert puzzles have between 22 and 27 given values
                    iMin = 22
                    iMax = 27

            End Select
            Return 81 - _clsRandom.GetRandomInt(iMin, iMax)
        End Function

        Private Shared Function GetLevel(iNumMasked As Int32) As GameLevelEnum
            Select Case iNumMasked
                Case Is < 22
                    Return GameLevelEnum.Invalid

                Case Is <= 27
                    Return GameLevelEnum.VeryEasy

                Case Is <= 31
                    Return GameLevelEnum.Easy

                Case Is <= 35
                    Return GameLevelEnum.Medium

                Case Is <= 49
                    Return GameLevelEnum.Hard

                Case Else
                    Return GameLevelEnum.Expert

            End Select
        End Function

#End Region

#End Region

    End Class

End Namespace