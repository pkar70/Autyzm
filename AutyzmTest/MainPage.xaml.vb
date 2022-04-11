Imports VBlib.MainPage
Imports vb14 = VBlib.pkarlibmodule14


Public NotInheritable Class MainPage
    Inherits Page

    Private Async Function SendEmailResult(sResult As String) As Task
        Dim oMsg As New Windows.ApplicationModel.Email.EmailMessage With {
            .Subject = VBlib.GetLangString("msgEmailSubject"),
            .Body = sResult
        }

        Await Email.EmailManager.ShowComposeNewEmailAsync(oMsg)

    End Function

    <CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="<Pending>")>
    Private Async Sub uiWynikuj_Click(sender As Object, e As RoutedEventArgs)
        ' sprawdz czy są jakies "0", jesli tak, to stop
        If CzyBrakujeOdpowiedzi() Then
            Await vb14.DialogBoxResAsync("msgBrakAnswer")
            uiList.ItemsSource = Nothing
            uiList.ItemsSource = moListPytania
            Return
        End If

        Dim iWynik As Integer = PoliczWynik()

        Dim sSummary As String = GetResultSummaryText(iWynik)
        Await vb14.DialogBoxAsync(sSummary)

        Dim sResult As String = sSummary & GetResultText()

        ' wynik do pliku
        SaveResult(sResult)

        If Await vb14.DialogBoxResYNAsync("msgAskEmail") Then
            Await SendEmailResult(sResult)
        End If

    End Sub

    <CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification:="<Pending>")>
    Private Async Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        Await vb14.DialogBoxResAsync("msgInfo")
        StworzPytania()
        uiList.ItemsSource = moListPytania

    End Sub


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

'Public Class KonwersjaKolor
'        Implements IValueConverter

'        Public Function Convert(ByVal value As Object,
'        ByVal targetType As Type, ByVal parameter As Object,
'        ByVal language As System.String) As Object _
'        Implements IValueConverter.Convert

'            Dim bTemp As Double = CType(value, Double)

'            If bTemp = 0 Then Return New SolidColorBrush(Windows.UI.Colors.Red)

'            Return New SolidColorBrush(Windows.UI.Colors.Blue)

'        End Function

'        ' ConvertBack is not implemented for a OneWay binding.
'        Public Function ConvertBack(ByVal value As Object,
'        ByVal targetType As Type, ByVal parameter As Object,
'        ByVal language As System.String) As Object _
'        Implements IValueConverter.ConvertBack

'            Throw New NotImplementedException

'        End Function
'    End Class