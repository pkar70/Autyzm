
Public Class MainPage

    Public Shared moListPytania As New List(Of Pytanie)

    Public Shared Sub SaveResult(sResult As String)
        Dim sFile As String = GetLogFileYearly(Date.Now.ToString("yyyy.MM.dd_HH.mm"))
        If sFile = "" Then Return

        IO.File.AppendAllText(sFile, sResult)

    End Sub

    Public Shared Function GetResultText() As String
        Dim sTxt As String = vbCrLf & vbCrLf

        For Each oItem As Pytanie In moListPytania
            sTxt = sTxt & oItem.iNumer & vbTab & oItem.iOdpowiedz & vbCrLf
        Next

        sTxt &= vbCrLf

        Return sTxt
    End Function

    Public Shared Sub StworzPytania()
        moListPytania.Clear()
        For iLoop As Integer = 1 To 50
            Dim oNew As New Pytanie With {
                .sPytanie = GetLangString("q" & iLoop),
                .iNumer = iLoop
            }

            moListPytania.Add(oNew)
        Next

    End Sub

    Public Shared Function CzyBrakujeOdpowiedzi() As Boolean

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

    Public Shared Function PoliczWynik() As Integer
        Dim iWynik As Integer = 0

        For Each oItem As Pytanie In moListPytania
            iWynik += PunktyPytania(oItem.iNumer, oItem.iOdpowiedz)
        Next

        Return iWynik
    End Function

    Private Shared Function PunktyPytania(iNumer As Integer, iOdpowiedz As Integer) As Integer
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

    Public Shared Function GetResultSummaryText(iWynik As Integer) As String
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

