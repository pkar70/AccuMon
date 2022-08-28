
' 2022.04.02 aktualizacja do nowego pkarmodule

Public NotInheritable Class MainPage
    Inherits Page

    Private Sub uiClearLog_Click(sender As Object, e As RoutedEventArgs)
        uiLog.Text = ""
        App.gLogData = ""
    End Sub

    Private Async Function PokazLog() As Task
        uiLog.Text = App.gLogData

        If App.gLogData.Length > 200 Then Return

        ' jesli jest krótkie, spróbuj odczytać z pliku - może będzie dłuższe
        Dim oFile As Windows.Storage.StorageFile = Await GetLogFileDailyAsync("accumon", "txt")
        If oFile Is Nothing Then Return

        App.gLogData = Await Windows.Storage.FileIO.ReadTextAsync(oFile)
        uiLog.Text = App.gLogData

    End Function

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)
        uiVers.ShowAppVers()

        If Not Await CanRegisterTriggersAsync() Then
            DialogBox("Nie umiem bez triggerów!")
            Return
        End If

        Dim bRunning = GetSettingsBool("MonitorRunnig")
        uiOnOff.IsOn = bRunning

        If bRunning AndAlso (Not IsTriggersRegistered("accumon")) Then
            RegisterTimerTrigger("AccuMon_Timer", 30)
        End If

        Await App.SprawdzStanBateryjki

        Await PokazLog()

    End Sub

    Private Sub UiOnOff_Toggled(sender As Object, e As RoutedEventArgs) Handles uiOnOff.Toggled
        If uiOnOff.IsOn Then
            If Not IsTriggersRegistered("accumon") Then
                RegisterTimerTrigger("AccuMon_Timer", 30)
            End If
        Else
            UnregisterTriggers("accumon")
        End If
        SetSettingsBool(uiOnOff, "MonitorRunnig")
    End Sub

    Private Async Sub uiOpenLog_Click(sender As Object, e As RoutedEventArgs)
        Dim oFold As Windows.Storage.StorageFolder = Await GetLogFolderMonthAsync()

        oFold.OpenExplorer

    End Sub

    Private Sub Page_GotFocus(sender As Object, e As RoutedEventArgs)
        uiLog.Text = App.gLogData
    End Sub
End Class
