'
' Copyright (c) 2014 Han Hung
' 
' This program is free software; it is distributed under the terms
' of the GNU General Public License v3 as published by the Free
' Software Foundation.
'
' http://www.gnu.org/licenses/gpl-3.0.html
' 

Namespace Controller.GameGenerator

    Friend Class GamesManager
        ' This class manages all the different games that are generated
        ' When the UI requests a game for a particular level, the request
        ' comes here.

#Region " Variables, Constants, And other Declarations "

#Region " Constants "

        ' Maximum number of levels that we will manage.
        Private Const _cMaxLevels As Int32 = 4              ' This should match PuzzleLevelEnum

#End Region

#Region " Variables "

        Private _Games(_cMaxLevels) As GameCollection       ' Fixed array of games

#End Region

#End Region

#Region " Public Properties "

        Friend ReadOnly Property GameCount(Level As GameLevelEnum) As Int32
            Get
                Return _Games(Level).GameCount              ' Return the game count for specified level
            End Get
        End Property

#End Region

#Region " Constructors "

        Friend Sub New()
            InitializeClass()                               ' Init any class level stuff
        End Sub

#End Region

#Region " Methods "

#Region " Public Methods "

        Friend Sub StopGamesManager()
            StopBackgroundTasks()                                           ' Stop all background tasks
            SaveGames()                                                     ' Save all the games
        End Sub

        Friend Function GetGame(eLevel As GameLevelEnum) As Model.CellStateClass(,)
            Return _Games(eLevel).GetGame                                   ' Grab a new game for the specified level
        End Function

#End Region

#Region " Private Methods "

        Private Sub InitializeClass()
            For I As Int32 = 0 To _cMaxLevels                               ' For each level
                _Games(I) = New GameCollection(CType(I, GameLevelEnum))     ' Initialize a new GameCollection class to generate games
            Next
            LoadGames()                                                     ' Load games.
            StartBackgroundTasks()                                          ' Start the background tasks.
        End Sub

        Private Sub StopBackgroundTasks()
            For Each Item As GameCollection In _Games                       ' Loop through the array of Games
                Item.StopThread()                                           ' Stop each background thread
            Next
        End Sub

        Private Sub StartBackgroundTasks()
            For Each Item As GameCollection In _Games                       ' Loop through each array of games
                Item.StartThread()                                          ' Start the background thread
            Next
        End Sub

        Private Sub SaveGames()
            My.Settings.GamesLevel0 = _Games(0).SaveGames()                 ' For each level, save all the games created
            My.Settings.GamesLevel1 = _Games(1).SaveGames()
            My.Settings.GamesLevel2 = _Games(2).SaveGames()
            My.Settings.GamesLevel3 = _Games(3).SaveGames()
            My.Settings.GamesLevel4 = _Games(4).SaveGames()
        End Sub

        Private Sub LoadGames()
            _Games(0).LoadGames(My.Settings.GamesLevel0)                    ' For each level, load any pre-created games
            _Games(1).LoadGames(My.Settings.GamesLevel1)
            _Games(2).LoadGames(My.Settings.GamesLevel2)
            _Games(3).LoadGames(My.Settings.GamesLevel3)
            _Games(4).LoadGames(My.Settings.GamesLevel4)
        End Sub

#End Region

#End Region

    End Class

End Namespace