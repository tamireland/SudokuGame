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

    Friend Class RandomClass
        ' I created this Random class to generate Random numbers.  In computing,
        ' there is really no concept of 'random' numbers.  It's actually a
        ' mathematical formula that generates a fixed sequence of numbers that
        ' to a casual observer, looks random.  So, if two random number generators
        ' were instantiated in the same run, it will generate an identical
        ' sequence of numbers.  To test this hypothesis, place the following
        ' code in a console application and run it.
        '
        ' Sub MySub()
        '     Dim rnd1 As New Random
        '     Console.Write("First sequence :")
        '     For I As Int32 = 1 To 10
        '         Console.Write("{0, 5}", rnd1.Next(100))
        '     Next
        '     Console.WriteLine()
        '     Dim rnd2 As New Random
        '     Console.Write("Second sequence:")
        '     For I As Int32 = 1 To 10
        '         Console.Write("{0, 5}", rnd2.Next(100))
        '     Next
        ' End Sub
        '
        ' 
        ' You'll see that it will generate an identical sequence of numbers.
        ' So, to counter this, all random numbers generated within this application
        ' will be channeled through this class.
        ' 
        ' In writing this class, there were many ways I could have gone.
        '
        '   1) make use of Shared variables and functions to maintain a single instance
        '   2) use the singleton pattern to enforce a single instance
        '   3) combine both patterns
        '
        ' I chose to implement the second pattern.  There really is no right or wrong 
        ' way to code this issue.

#Region " Variables "

        Private Shared _instance As RandomClass
        Private Shared _objInstanceLock As New Object

        Private _rnd As Random
        Private _objRndLock As New Object

#End Region

#Region " Constructors "

        Private Sub New()
            ' Declared private so that no one can instantiate this class.
        End Sub

#End Region

#Region " Public Properties "

        Friend Shared ReadOnly Property GetInstance As RandomClass
            ' Use the double check locking here so that once the instance is created,
            ' any subsequent calls do not need to be blocked.
            Get
                If _instance Is Nothing Then                ' Is the instance variable nothing?
                    SyncLock _objInstanceLock               ' Yes, obtain a lock on the singleton object.
                        If _instance Is Nothing Then        ' Is it nothing?
                            _instance = New RandomClass     ' Yes, then instantiate an instance and save the pointer.
                            _instance.InitInstance()        ' Init the instance.
                        End If
                    End SyncLock                            ' Release the lock.
                End If
                Return _instance                            ' Return a pointer to the instance.
            End Get
        End Property

#End Region

#Region " Public Methods "

        Friend Function GetRandomInt(iMax As Int32) As Int32
            Return GetRandomInt(0, iMax)                            ' Return a random number from 0 to iMax
        End Function

        Friend Function GetRandomInt(iMin As Int32, iMax As Int32) As Int32
            SyncLock _objRndLock                                    ' Obtain a lock on the random object
                Return _rnd.Next(iMin, iMax)                        ' Return a random number from iMin to iMax
            End SyncLock                                            ' Release the lock
        End Function

#End Region

#Region " Private Methods "

        Private Sub InitInstance()
            SyncLock _objRndLock                                    ' Obtain a lock on the random object.
                If (_rnd Is Nothing) Then                           ' Is it nothing?
                    Dim tsp As New TimeSpan(DateTime.Now.Ticks)     ' Create a seed value using the current time.
                    Dim iSeed As Int32 = CInt((CLng(tsp.TotalMilliseconds * 10000) Mod Int32.MaxValue) Mod 10000)

                    _rnd = New Random(iSeed)                        ' Initialize the Random object using the seed value above.
                End If
            End SyncLock                                            ' Release the lock.
        End Sub

#End Region

    End Class

End Namespace