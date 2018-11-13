Imports System.Xml.Serialization
Imports GTA.Math
Imports System.IO

Public Structure BusRoute

    Private Shared _instance As BusRoute

    Public ReadOnly Property Instance As BusRoute
        Get
            Return ReadFromFile()
        End Get
    End Property

    <XmlIgnore>
    Public Property RouteFileName() As String
    <XmlIgnore>
    Public ReadOnly Property TotalStation() As Integer
        Get
            Return Stations.Count
        End Get
    End Property

    Public RouteName As String
    Public RouteNumber As Integer
    Public PlayerSpawnPoint As Vector3
    Public BusSpawnPoint As Vector3
    Public BusHeading As Single
    Public BusModel As String
    Public TurnOnExtra As Integer
    Public RouteFare As Integer
    Public Stations As List(Of Station)

    Public Sub New(_FileName As String, _Name As String, _Number As Integer, PlayerPosition As Vector3, BusPosition As Vector3, BusHead As Single, Model As String, _Stations As List(Of Station), Optional Fares As Integer = 2)
        RouteFileName = _FileName
        RouteName = _Name
        RouteNumber = _Number
        PlayerSpawnPoint = PlayerPosition
        BusSpawnPoint = BusPosition
        BusModel = Model
        RouteFare = Fares
        Stations = _Stations
    End Sub

    Public Sub New(_FileName As String)
        RouteFileName = _FileName
    End Sub

    Public Sub Save()
        Try
            Dim ser = New XmlSerializer(GetType(BusRoute))
            Dim writer As TextWriter = New StreamWriter(RouteFileName)
            ser.Serialize(writer, Me)
            writer.Close()
        Catch ex As Exception
            Logger.Log(String.Format("{0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Function ReadFromFile() As BusRoute
        If Not File.Exists(RouteFileName) Then
            Logger.Log(String.Format("Unable to load Route File {0}{1}{0}, file not found.", """", RouteFileName))
            Return New BusRoute(RouteFileName, RouteName, RouteNumber, PlayerSpawnPoint, BusSpawnPoint, BusHeading, BusModel, Stations, RouteFare)
        End If

        Try
            Dim ser = New XmlSerializer(GetType(BusRoute))
            Dim reader As TextReader = New StreamReader(RouteFileName)
            Dim instance = CType(ser.Deserialize(reader), BusRoute)
            reader.Close()
            Return instance
        Catch ex As Exception
            Logger.Log(String.Format("(ReadfromFile): {0} {1}", ex.Message, ex.StackTrace))
            Return New BusRoute(RouteFileName, RouteName, RouteNumber, PlayerSpawnPoint, BusSpawnPoint, BusHeading, BusModel, Stations, RouteFare)
        End Try
    End Function
End Structure

Public Structure Station
    Public StationIndex As Integer
    Public StationName As String
    Public StationCoords As Vector3

    Public Sub New(index As Integer, name As String, coords As Vector3)
        StationIndex = index
        StationName = name
        StationCoords = coords
    End Sub
End Structure
