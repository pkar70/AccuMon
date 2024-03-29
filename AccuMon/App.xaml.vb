﻿NotInheritable Class App
    Inherits Application

#Region "wizard"
    ''' <summary>
    ''' Invoked when the application is launched normally by the end user.  Other entry points
    ''' will be used when the application is launched to open a specific file, to display
    ''' search results, and so forth.
    ''' </summary>
    ''' <param name="e">Details about the launch request and process.</param>
    Protected Overrides Sub OnLaunched(e As Windows.ApplicationModel.Activation.LaunchActivatedEventArgs)
        Dim rootFrame As Frame = TryCast(Window.Current.Content, Frame)

        ' Do not repeat app initialization when the Window already has content,
        ' just ensure that the window is active

        If rootFrame Is Nothing Then
            ' Create a Frame to act as the navigation context and navigate to the first page
            rootFrame = New Frame()

            AddHandler rootFrame.NavigationFailed, AddressOf OnNavigationFailed

            If e.PreviousExecutionState = ApplicationExecutionState.Terminated Then
                ' TODO: Load state from previously suspended application
            End If
            ' Place the frame in the current Window
            Window.Current.Content = rootFrame
        End If

        If e.PrelaunchActivated = False Then
            If rootFrame.Content Is Nothing Then
                ' When the navigation stack isn't restored navigate to the first page,
                ' configuring the new page by passing required information as a navigation
                ' parameter
                rootFrame.Navigate(GetType(MainPage), e.Arguments)
            End If

            ' Ensure the current window is active
            Window.Current.Activate()
        End If
    End Sub

    ''' <summary>
    ''' Invoked when Navigation to a certain page fails
    ''' </summary>
    ''' <param name="sender">The Frame which failed navigation</param>
    ''' <param name="e">Details about the navigation failure</param>
    Private Sub OnNavigationFailed(sender As Object, e As NavigationFailedEventArgs)
        Throw New Exception("Failed to load Page " + e.SourcePageType.FullName)
    End Sub

    ''' <summary>
    ''' Invoked when application execution is being suspended.  Application state is saved
    ''' without knowing whether the application will be terminated or resumed with the contents
    ''' of memory still intact.
    ''' </summary>
    ''' <param name="sender">The source of the suspend request.</param>
    ''' <param name="e">Details about the suspend request.</param>
    Private Sub OnSuspending(sender As Object, e As SuspendingEventArgs) Handles Me.Suspending
        Dim deferral As SuspendingDeferral = e.SuspendingOperation.GetDeferral()
        ' TODO: Save application state and stop any background activity
        deferral.Complete()
    End Sub
#End Region

    Private moTimerDeferal As Background.BackgroundTaskDeferral = Nothing

    Protected Overrides Async Sub OnBackgroundActivated(args As BackgroundActivatedEventArgs)
        moTimerDeferal = args.TaskInstance.GetDeferral()

        Await SprawdzStanBateryjki()

        moTimerDeferal.Complete()
    End Sub

    Public Shared Async Function SprawdzStanBateryjki() As Task
        Dim sTxt As String
        Dim oBattRep As Windows.Devices.Power.BatteryReport = Windows.Devices.Power.Battery.AggregateBattery.GetReport

        sTxt = oBattRep.Status.ToString
        sTxt = sTxt & " " & oBattRep.RemainingCapacityInMilliwattHours & "/" & oBattRep.FullChargeCapacityInMilliwattHours &
            " (design: " & oBattRep.DesignCapacityInMilliwattHours & ")"

        Await AppendLogFile(sTxt)

    End Function

    Private Async Function AppServiceLocalCommand(sCommand As String) As Task(Of String)
        Return ""
    End Function

    Public Shared gLogData As String = ""

    Public Shared Async Function AppendLogFile(sLogLine As String) As Task

        sLogLine = DateTime.Now.ToString("yyyy.MM.dd HH:mm ") & sLogLine & vbCrLf
        gLogData = gLogData & sLogLine

        ' log w App ma tylko 100 entries
        Dim aLog As String() = gLogData.Split("vbCrLf")
        If aLog.Length > 100 Then
            Dim iInd As Integer = gLogData.IndexOf(vbCrLf)
            gLogData = gLogData.Substring(iInd + 2)
        End If

        Dim oFile As Windows.Storage.StorageFile = Await GetLogFileDailyAsync("accumon", "txt")
        If oFile Is Nothing Then Return

        Await Windows.Storage.FileIO.AppendTextAsync(oFile, sLogLine)

    End Function


End Class
