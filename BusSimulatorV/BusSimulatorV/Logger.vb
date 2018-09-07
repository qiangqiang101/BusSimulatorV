Public NotInheritable Class Logger

    Private Sub New()

    End Sub

    Public Shared Sub Log(message As Object)
        System.IO.File.AppendAllText(".\BusSimulatorV.log", DateTime.Now & ":" & message & Environment.NewLine)
    End Sub

End Class