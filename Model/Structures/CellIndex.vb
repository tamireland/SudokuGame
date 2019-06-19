'
' Copyright (c) 2014 Han Hung
' 
' This program is free software; it is distributed under the terms
' of the GNU General Public License v3 as published by the Free
' Software Foundation.
'
' http://www.gnu.org/licenses/gpl-3.0.html
' 

Namespace Model

    Friend Class CellIndex

#Region " Variables "

        Private _iRow As Int32
        Private _iCol As Int32
        Private _iRegion As Int32

#End Region

#Region " Public Properties "

        Friend ReadOnly Property Row As Int32
            Get
                Return _iRow
            End Get
        End Property
        Friend ReadOnly Property Col As Int32
            Get
                Return _iCol
            End Get
        End Property
        Friend ReadOnly Property Region As Int32
            Get
                Return _iRegion
            End Get
        End Property

#End Region

#Region " Constructors "

        Friend Sub New(iCol As Int32, iRow As Int32)
            _iCol = iCol                                        ' Save column value
            _iRow = iRow                                        ' Save row value
            SetRegion()                                         ' Set the region value
        End Sub

        Friend Sub New(iIndex As Int32)
            iIndex += 1                                         ' Increment index
            _iCol = ComputeColumn(iIndex)                       ' Compute Column
            _iRow = ComputeRow(Me.Col, iIndex)                  ' Compute Row
            SetRegion()                                         ' Compute Region
        End Sub

#End Region

#Region " Public Methods "

        Friend Function IsSameCell(uIndex As CellIndex) As Boolean
            If uIndex IsNot Nothing Then                                ' If the input value is not null
                Return ((Row = uIndex.Row) AndAlso (Col = uIndex.Col))  ' Return True if the Row And Col values are the same
            End If
            Return False                                                ' Otherwise, just return False
        End Function

#End Region

#Region " Private Methods "

        Private Shared Function ComputeColumn(ByVal iIndex As Int32) As Int32
            Dim iRet As Integer = iIndex Mod 9
            If iRet = 0 Then
                Return 9
            End If
            Return iRet
        End Function

        Private Shared Function ComputeRow(iCol As Int32, ByVal iIndex As Int32) As Int32
            If iCol = 9 Then
                Return iIndex \ 9
            End If
            Return (iIndex \ 9) + 1
        End Function


        ' For reference, here is a diagram showing how each cell
        ' is referenced in code.  Basically, it's [col][row]
        ' much like how Excel worksheets do it.
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
        ' And these are how the regions are defined
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

        Private Sub SetRegion()
            Select Case _iRow
                Case 1 To 3
                    Select Case _iCol
                        Case 1 To 3
                            _iRegion = 1

                        Case 4 To 6
                            _iRegion = 2

                        Case Else
                            _iRegion = 3

                    End Select

                Case 4 To 6
                    Select Case _iCol
                        Case 1 To 3
                            _iRegion = 4

                        Case 4 To 6
                            _iRegion = 5

                        Case Else
                            _iRegion = 6

                    End Select

                Case 7 To 9
                    Select Case _iCol
                        Case 1 To 3
                            _iRegion = 7

                        Case 4 To 6
                            _iRegion = 8

                        Case Else
                            _iRegion = 9

                    End Select

                Case Else
                    _iRegion = 0

            End Select
        End Sub

#End Region

    End Class

End Namespace