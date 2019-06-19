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

Namespace Controller

    Friend Class GameControllerEventArgs
        Inherits EventArgs

#Region " Variables "

        ' Assign default values to all the variables.
        Private _eGameControllerEventType As GameControllerEventType = Controller.GameControllerEventType.Invalid
        Private _sElapsedTime As String = "00:00:00"

#End Region

#Region " Public Properties "

        Friend ReadOnly Property GameControllerEventType As GameControllerEventType
            Get
                Return _eGameControllerEventType
            End Get
        End Property

        Friend ReadOnly Property GameElapsedTime As String
            Get
                Return _sElapsedTime
            End Get
        End Property

#End Region

#Region " Constructors "

        Friend Sub New(sElapsedTime As String)
            MyBase.New()
            _eGameControllerEventType = Controller.GameControllerEventType.GameTimer
            _sElapsedTime = sElapsedTime
        End Sub

#End Region

    End Class

End Namespace
