Public NotInheritable Class MainPage
    Inherits Page

    Private Async Function SaveResult(sResult As String) As Task
        Dim oFile As Windows.Storage.StorageFile = Await GetLogFileYearlyAsync(Date.Now.ToString("yyyy.MM.dd_HH.mm"))
        If oFile Is Nothing Then Return

        Await oFile.AppendStringAsync(sResult)

    End Function

    Private Function GetResultText() As String
        Dim sTxt As String = vbCrLf & vbCrLf

        For Each oItem As Pytanie In moListPytania
            sTxt = sTxt & oItem.iNumer & vbTab & oItem.iOdpowiedz & vbCrLf
        Next

        sTxt &= vbCrLf

        Return sTxt
    End Function

    Private Async Function SendEmailResult(sResult As String) As Task

        Dim oMsg As Email.EmailMessage = New Windows.ApplicationModel.Email.EmailMessage()
        oMsg.Subject = GetLangString("msgEmailSubject")

        oMsg.Body = sResult

        Await Email.EmailManager.ShowComposeNewEmailAsync(oMsg)

    End Function

    Private Async Sub uiWynikuj_Click(sender As Object, e As RoutedEventArgs)
        ' sprawdz czy są jakies "0", jesli tak, to stop
        If CzyBrakujeOdpowiedzi Then
            Await DialogBoxResAsync("msgBrakAnswer")
            uiList.ItemsSource = Nothing
            uiList.ItemsSource = moListPytania
            Return
        End If

        Dim iWynik As Integer = PoliczWynik()

        Dim sSummary As String = GetResultSummaryText(iWynik)
        Await DialogBoxAsync(sSummary)

        Dim sResult As String = sSummary & GetResultText()

        ' wynik do pliku
        Await SaveResult(sResult)

        If Await DialogBoxResYNAsync("msgAskEmail") Then
            Await SendEmailResult(sResult)
        End If

    End Sub

    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        Await DialogBoxResAsync("msgInfo")
        StworzPytania()
        uiList.ItemsSource = moListPytania
    End Sub

    Private moListPytania As ObservableCollection(Of Pytanie) = New ObservableCollection(Of Pytanie)
    Private Sub StworzPytania()
        moListPytania.Clear()
        For iLoop As Integer = 1 To 50
            Dim oNew As Pytanie = New Pytanie
            oNew.sPytanie = GetLangString("q" & iLoop)
            oNew.iNumer = iLoop

            moListPytania.Add(oNew)
        Next

    End Sub

    Private Function CzyBrakujeOdpowiedzi() As Boolean

        Dim bBrak As Boolean = False
        For Each oItem As Pytanie In moListPytania
            If oItem.iOdpowiedz = 0 Then
                bBrak = True
                oItem.bError = True
            Else
                oItem.bError = False
            End If
        Next

        Return bBrak
    End Function

    Private Function PoliczWynik() As Integer
        Dim iWynik As Integer = 0

        For Each oItem As Pytanie In moListPytania
            iWynik += PunktyPytania(oItem.iNumer, oItem.iOdpowiedz)
        Next

        Return iWynik
    End Function

    Private Function PunktyPytania(iNumer As Integer, iOdpowiedz As Integer) As Integer
        ' “Definitely disagree” Or “Slightly disagree” responses to questions 1, 3, 8, 10, 11, 14, 15, 17, 24, 25, 27, 28, 29, 30, 31, 32, 34, 36, 37, 38, 40, 44, 47, 48, 49, 50 score 1 point
        If iOdpowiedz < 0 Then
            If iNumer = 1 Or iNumer = 3 Or iNumer = 8 Or iNumer = 10 Or iNumer = 11 Or iNumer = 14 Or iNumer = 15 Or iNumer = 17 Or iNumer = 24 Or iNumer = 25 Or iNumer = 27 Or iNumer = 28 Or iNumer = 29 Or iNumer = 30 Or iNumer = 31 Or iNumer = 32 Or iNumer = 34 Or iNumer = 36 Or iNumer = 37 Or iNumer = 38 Or iNumer = 40 Or iNumer = 44 Or iNumer = 47 Or iNumer = 48 Or iNumer = 49 Or iNumer = 50 Then Return 1
        End If

        ' “Definitely agree” Or “Slightly agree” responses to questions 2, 4, 5, 6, 7, 9, 12, 13, 16, 18, 19, 20, 21, 22, 23, 26, 33, 35, 39, 41, 42, 43, 45, 46 score 1 point.
        If iOdpowiedz > 0 Then
            If iNumer = 2 Or iNumer = 4 Or iNumer = 5 Or iNumer = 6 Or iNumer = 7 Or iNumer = 9 Or iNumer = 12 Or iNumer = 13 Or iNumer = 16 Or iNumer = 18 Or iNumer = 19 Or iNumer = 20 Or iNumer = 21 Or iNumer = 22 Or iNumer = 23 Or iNumer = 26 Or iNumer = 33 Or iNumer = 35 Or iNumer = 39 Or iNumer = 41 Or iNumer = 42 Or iNumer = 43 Or iNumer = 45 Or iNumer = 46 Then Return 1
        End If

        Return 0
    End Function

    Private Function GetResultSummaryText(iWynik As Integer)
        Dim sTxt As String = GetLangString("rYouGot") & " " & iWynik & " " & GetLangString("rGotPoints")
        ' "Masz " & iWynik & " punktow"
        If iWynik < 16.5 Then
            sTxt += "; " & GetLangString("rSpoko")
        ElseIf iWynik > 31 Then
            sTxt += ", " & GetLangString("rAutism")
        End If
        sTxt += "."
        Return sTxt
    End Function



End Class

Public Class Pytanie
    Public Property sPytanie As String
    Public Property iOdpowiedz As Integer = 0
    Public Property bError As Boolean = False
    Public Property iNumer As Integer
End Class

Public Class KonwersjaVisibility
    Implements IValueConverter

    Public Function Convert(ByVal value As Object,
    ByVal targetType As Type, ByVal parameter As Object,
    ByVal language As System.String) As Object _
    Implements IValueConverter.Convert

        Dim bTemp As Boolean = CType(value, Boolean)

        If bTemp Then Return Visibility.Visible

        Return Visibility.Collapsed

    End Function

    ' ConvertBack is not implemented for a OneWay binding.
    Public Function ConvertBack(ByVal value As Object,
    ByVal targetType As Type, ByVal parameter As Object,
    ByVal language As System.String) As Object _
    Implements IValueConverter.ConvertBack

        Throw New NotImplementedException

    End Function
End Class

Public Class KonwersjaKolor
        Implements IValueConverter

        Public Function Convert(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.Convert

            Dim bTemp As Double = CType(value, Double)

            If bTemp = 0 Then Return New SolidColorBrush(Windows.UI.Colors.Red)

            Return New SolidColorBrush(Windows.UI.Colors.Blue)

        End Function

        ' ConvertBack is not implemented for a OneWay binding.
        Public Function ConvertBack(ByVal value As Object,
        ByVal targetType As Type, ByVal parameter As Object,
        ByVal language As System.String) As Object _
        Implements IValueConverter.ConvertBack

            Throw New NotImplementedException

        End Function
    End Class