'
' Copyright (c) 2014 Han Hung
' 
' This program is free software; it is distributed under the terms
' of the GNU General Public License v3 as published by the Free
' Software Foundation.
'
' http://www.gnu.org/licenses/gpl-3.0.html
' 

Imports System.Text
Imports SudokuPuzzle.SharedFunctions

Namespace Model

    Friend Class CellStateClass

#Region " Variables "

        Private _iAnswer As Int32
        Private _uCellIndex As CellIndex
        Private _bInvalidState As Boolean
        Private _bNotes(9) As Boolean

#End Region

#Region " Public Properties "

#Region " Read/Write Properties "

        Friend Property CellState As CellStateEnum
        Friend Property UserAnswer As Int32

        Friend Property Notes(iIndex As Int32) As Boolean
            Get
                If Common.IsValidIndex(iIndex) Then
                    Return _bNotes(iIndex)
                End If
                Return False
            End Get
            Set(value As Boolean)
                If Common.IsValidIndex(iIndex) Then
                    _bNotes(iIndex) = value
                End If
            End Set
        End Property

#End Region

#Region " Readonly Properties "

        Friend ReadOnly Property Answer As Int32
            Get
                Return _iAnswer
            End Get
        End Property

        Friend ReadOnly Property Row As Int32
            Get
                Return _uCellIndex.Row
            End Get
        End Property

        Friend ReadOnly Property Col As Int32
            Get
                Return _uCellIndex.Col
            End Get
        End Property

        Friend ReadOnly Property Region As Int32
            Get
                Return _uCellIndex.Region
            End Get
        End Property

        Friend ReadOnly Property InvalidState As Boolean
            Get
                Return _bInvalidState
            End Get
        End Property

#End Region

#End Region

#Region " Constructors "

        Friend Sub New(iCol As Int32, iRow As Int32, sState As String)
            InitCell(iCol, iRow)
            LoadState(sState)
        End Sub

        Friend Sub New(iIndex As CellIndex, iAnswer As Int32)
            Initcell()
            _uCellIndex = iIndex
            _iAnswer = iAnswer
            CellState = CellStateEnum.Answer
        End Sub

#End Region

#Region " Public Methods "

        Friend Function IsCorrect() As Boolean
            Return Answer = UserAnswer                          ' Return true if user's answer matches our answer
        End Function

        Friend Function HasNotes() As Boolean
            For I As Int32 = 1 To 9                             ' Loop through the notes
                If _bNotes(I) Then                              ' If a note is raised?
                    Return True                                 ' Return true
                End If
            Next
            Return False                                        ' No notes marked, return False
        End Function

        Friend Shadows Function ToString(bFull As Boolean) As String
            Dim sTemp As New StringBuilder
            sTemp.Append(Answer.ToString)                       ' Append the answer
            sTemp.Append(CellState.GetHashCode.ToString)        ' Append the cell state
            If bFull Then                                       ' Return Full state?
                sTemp.Append(UserAnswer.ToString)               ' Yes, also include the user's answer
            End If
            Return sTemp.ToString                               ' Return the string
        End Function

        Friend Sub ClearNotes()
            For I As Int32 = 1 To 9                             ' Loop through the notes
                _bNotes(I) = False                              ' Clear the note
            Next
        End Sub

#End Region

#Region " Private Methods "

        Private Sub Initcell()
            _iAnswer = 0
            UserAnswer = 0
            CellState = CellStateEnum.Blank
            ClearNotes()
        End Sub

        Private Sub InitCell(iCol As Int32, iRow As Int32)
            Initcell()
            _uCellIndex = New CellIndex(iCol, iRow)
        End Sub

        Private Sub LoadState(sState As String)
            If (sState.Length >= 2) Then                                    ' String is at least 2 characters long?
                _iAnswer = ExtractAnswer(sState.Substring(0, 1))            ' Yes, extract the answer
                CellState = ExtractCellState(sState.Substring(1, 1))        ' Extract the cell state
                If (sState.Length >= 3) Then                                ' String is at least 3 characters long?
                    UserAnswer = ExtractAnswer(sState.Substring(2, 1))      ' Yes, also extract user's answer 
                Else                                                        ' No, make sure that the cell state is valid as well
                    ' Is the input string is 2 characters long, then the only valid cell states are Answer and Blank.
                    _bInvalidState = Not ((CellState = CellStateEnum.Answer) OrElse (CellState = CellStateEnum.Blank))
                End If
            End If
        End Sub

        Private Function ExtractAnswer(sAnswer As String) As Int32
            Try
                _bInvalidState = False                  ' Default invalid state to False
                Dim iTemp As Int32 = CInt(sAnswer)      ' Convert string to number
                If Common.IsValidIndex(iTemp) Then      ' Is the number a valid answer?
                    Return iTemp                        ' Yes, return it
                End If
            Catch ex As Exception
                ' TODO: What to do here?
            End Try
            _bInvalidState = True                       ' Errored out!  Set invalid state to True
            Return 0                                    ' Return zero
        End Function

        Private Function ExtractCellState(sState As String) As CellStateEnum
            Try
                _bInvalidState = False                      ' Default invalid state to False
                Dim iTemp As Int32 = CInt(sState)           ' Convert string to Integer
                If Common.IsValidStateEnum(iTemp) Then      ' Is the integer a valid state enum?
                    Return CType(iTemp, CellStateEnum)      ' Yes, then convert it and return it
                End If
            Catch ex As Exception
                ' TODO: What to do here?
            End Try
            _bInvalidState = True                           ' Errored out!  Set invalid state to True
            Return CellStateEnum.Blank                      ' Return state of Blank
        End Function

#End Region

    End Class

End Namespace