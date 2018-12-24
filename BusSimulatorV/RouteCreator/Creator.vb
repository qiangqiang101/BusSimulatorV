Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI
Imports System.Windows.Forms

Public Class Creator
    Inherits Script

    Public WithEvents MainMenu, RouteMenu, CreatorMenu, StationMenu, StationInfoMenu, Vector3Menu As UIMenu
    Public itemNew, itemEdit, itemRoute, itemSave As UIMenuItem
    Public itemRName, itemRNum, itemRAuthor, itemRDesc, itemRVersion, itemRPSpawn, itemRBSpawn, itemRBHead, itemRBModel, itemRExtra, itemRFare, itemRStations As UIMenuItem
    Public itemSName, itemSIndex, itemSCoords As UIMenuItem
    Public itemX, itemY, itemZ As UIMenuItem
    Public CurrentRoute As BusRoute
    Public _menuPool As MenuPool
    Public Rectangle = New UIResRectangle()
    Dim StationMenuSelectedItem As UIMenuItem
    Public config As ScriptSettings = ScriptSettings.Load("scripts\BusSimulatorV\creatorconfig.ini")
    Public Modifier, Key As Keys
    Public ShowInfo As Boolean

    Public Sub New()
        LoadSettings()
        CurrentRoute.Stations = New List(Of Station)
        _menuPool = New MenuPool()
        Rectangle.Color = Color.FromArgb(0, 0, 0, 0)

        CreateMainMenu()
        CreateRouteMenu()
        CreateCreatorMenu()
        CreateStationMenu()
        CreateStationInfoMenu()
        CreateVector3Menu()
    End Sub

    Public Sub LoadSettings()
        Try
            Modifier = config.GetValue(Of Keys)("CONTROL", "Modifier", Keys.Shift)
            Key = config.GetValue(Of Keys)("CONTROL", "Key", Keys.Delete)
            ShowInfo = config.GetValue(Of Boolean)("GENERAL", "ShowInfo", True)
        Catch ex As Exception
            Logger.Log(String.Format("(LoadSettings): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateMainMenu()
        Try
            MainMenu = New UIMenu("", "MAIN MENU", New Point(0, -107))
            MainMenu.SetBannerType(Rectangle)
            MainMenu.MouseEdgeEnabled = False
            _menuPool.Add(MainMenu)
            itemNew = New UIMenuItem("New", "Create a new bus route. Note: All unsave changes will be loss.") : MainMenu.AddItem(itemNew)
            itemEdit = New UIMenuItem("Edit", "Edit current bus route.") With {.Enabled = False} : MainMenu.AddItem(itemEdit)
            itemSave = New UIMenuItem("Save", "Save current bus route.") With {.Enabled = False} : MainMenu.AddItem(itemSave)
            itemRoute = New UIMenuItem("Load", "Load and edit an existing bus route.") : MainMenu.AddItem(itemRoute)
            MainMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub UpdateMainMenu()
        Try
            itemEdit.Enabled = False
            itemSave.Enabled = False
        Catch ex As Exception
            Logger.Log(String.Format("(UpdateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateRouteMenu()
        Try
            RouteMenu = New UIMenu("", "LOAD", New Point(0, -107))
            RouteMenu.SetBannerType(Rectangle)
            RouteMenu.MouseEdgeEnabled = False
            _menuPool.Add(RouteMenu)

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    Dim br As BusRoute = New BusRoute(xmlFile)
                    br = br.ReadFromFile
                    Dim item = New UIMenuItem(Path.GetFileName(xmlFile))
                    With item
                        .SubString1 = xmlFile
                        .Description = $"Name: {br.RouteName}~n~Version: {br.Version}~n~Author: {br.Author}~n~Description: {br.Description}                                                                                                                                                                                                                                    "
                    End With
                    RouteMenu.AddItem(item)
                End If
            Next
            RouteMenu.RefreshIndex()
            MainMenu.BindMenuToItem(RouteMenu, itemRoute)
        Catch ex As Exception
            Logger.Log(String.Format("(CreateRouteMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshRouteMenu()
        Try
            RouteMenu.MenuItems.Clear()

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    Dim br As BusRoute = New BusRoute(xmlFile)
                    br = br.ReadFromFile
                    Dim item = New UIMenuItem(Path.GetFileName(xmlFile))
                    With item
                        .SubString1 = xmlFile
                        .Description = $"Name: {br.RouteName}~n~Version: {br.Version}~n~Author: {br.Author}~n~Description: {br.Description}                                                                                                                                                                                                                                    "
                    End With
                    RouteMenu.AddItem(item)
                End If
            Next
            RouteMenu.RefreshIndex()
            MainMenu.BindMenuToItem(RouteMenu, itemRoute)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshRouteMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateCreatorMenu()
        Try
            CreatorMenu = New UIMenu("", "ROUTE EDITOR", New Point(0, -107))
            CreatorMenu.SetBannerType(Rectangle)
            CreatorMenu.MouseEdgeEnabled = False
            _menuPool.Add(CreatorMenu)
            itemRName = New UIMenuItem("Name", "Name of this Bus Route.") : CreatorMenu.AddItem(itemRName)
            itemRNum = New UIMenuItem("Number", "Number of this Bus Route.") : CreatorMenu.AddItem(itemRNum)
            itemRAuthor = New UIMenuItem("Author", "Name of the Author.") : CreatorMenu.AddItem(itemRAuthor)
            itemRDesc = New UIMenuItem("Description", "Description of this Bus Route.") : CreatorMenu.AddItem(itemRDesc)
            itemRVersion = New UIMenuItem("Version", "Version of this Bus Route.") : CreatorMenu.AddItem(itemRVersion)
            itemRPSpawn = New UIMenuColoredItem("Player Position", "Position for Player Spawn Point.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRPSpawn)
            itemRBSpawn = New UIMenuColoredItem("Bus Position", "Position for Bus Spawn Point.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRBSpawn)
            itemRBHead = New UIMenuItem("Bus Heading", "Heading of the Bus when spawn.") : CreatorMenu.AddItem(itemRBHead)
            itemRBModel = New UIMenuItem("Bus Model", "Model name of the vehicle use in this Route.") : CreatorMenu.AddItem(itemRBModel)
            itemRExtra = New UIMenuItem("Turn On Extra", "Which Extra Component to turn on in this Route.") : CreatorMenu.AddItem(itemRExtra)
            itemRFare = New UIMenuItem("Fare", "How much money to earn per passenger.") : CreatorMenu.AddItem(itemRFare)
            itemRStations = New UIMenuColoredItem("Stations", "List of the Station for this Route.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRStations)
            CreatorMenu.RefreshIndex()
            MainMenu.BindMenuToItem(CreatorMenu, itemEdit)
        Catch ex As Exception
            Logger.Log(String.Format("(CreateCreatorMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshCreatorMenu()
        Try
            CreatorMenu.MenuItems.Clear()
            itemRName = New UIMenuItem("Name", "Name of this Bus Route.") : CreatorMenu.AddItem(itemRName)
            itemRNum = New UIMenuItem("Number", "Number of this Bus Route.") : CreatorMenu.AddItem(itemRNum)
            itemRAuthor = New UIMenuItem("Author", "Name of the Author.") : CreatorMenu.AddItem(itemRAuthor)
            itemRDesc = New UIMenuItem("Description", "Description of this Bus Route.") : CreatorMenu.AddItem(itemRDesc)
            itemRVersion = New UIMenuItem("Version", "Version of this Bus Route.") : CreatorMenu.AddItem(itemRVersion)
            itemRPSpawn = New UIMenuColoredItem("Player Position", "Position for Player Spawn Point.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRPSpawn)
            itemRBSpawn = New UIMenuColoredItem("Bus Position", "Position for Bus Spawn Point.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRBSpawn)
            itemRBHead = New UIMenuItem("Bus Heading", "Heading of the Bus when spawn.") : CreatorMenu.AddItem(itemRBHead)
            itemRBModel = New UIMenuItem("Bus Model", "Model name of the vehicle use in this Route.") : CreatorMenu.AddItem(itemRBModel)
            itemRExtra = New UIMenuItem("Sign Livery", "Which Sign Livery to use in this Route.") : CreatorMenu.AddItem(itemRExtra)
            itemRFare = New UIMenuItem("Fare", "How much money to earn per passenger.") : CreatorMenu.AddItem(itemRFare)
            itemRStations = New UIMenuColoredItem("Stations", "List of the Station for this Route.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRStations)
            CreatorMenu.RefreshIndex()
            MainMenu.BindMenuToItem(CreatorMenu, itemEdit)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshCreatorMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub UpdateCreatorMenu()
        Try
            itemRStations.SetRightLabel(CurrentRoute.TotalStation)
        Catch ex As Exception
            Logger.Log(String.Format("(UpdateCreatorMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateStationMenu()
        Try
            StationMenu = New UIMenu("", "STATIONS", New Point(0, -107))
            StationMenu.SetBannerType(Rectangle)
            StationMenu.MouseEdgeEnabled = False
            StationMenu.AddInstructionalButton(New InstructionalButton(GTA.Control.CreatorDelete, "Delete Station"))
            StationMenu.AddInstructionalButton(New InstructionalButton(GTA.Control.Jump, "Add Station"))
            _menuPool.Add(StationMenu)
            StationMenu.RefreshIndex()
            CreatorMenu.BindMenuToItem(StationMenu, itemRStations)
        Catch ex As Exception
            Logger.Log(String.Format("(CreateStationMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshStationMenu()
        Try
            StationMenu.MenuItems.Clear()

            For Each station As Station In CurrentRoute.Stations
                Dim item = New UIMenuItem(station.StationName)
                With item
                    .SubInteger1 = station.StationIndex
                    .SetRightLabel(station.StationIndex)
                    .SubString1 = station.StationCoords.X
                    .SubString2 = station.StationCoords.Y
                    .SubString3 = station.StationCoords.Z
                    .SubString4 = station.StationName
                End With
                StationMenu.AddItem(item)
            Next
            StationMenu.RefreshIndex()
            CreatorMenu.BindMenuToItem(StationMenu, itemRStations)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshStationMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateStationInfoMenu()
        Try
            StationInfoMenu = New UIMenu("", "STATION INFO", New Point(0, -107))
            StationInfoMenu.SetBannerType(Rectangle)
            StationInfoMenu.MouseEdgeEnabled = False
            _menuPool.Add(StationInfoMenu)

            itemSIndex = New UIMenuItem("Index", "Index of the Station. Note: Number Must be unique.") : StationInfoMenu.AddItem(itemSIndex)
            itemSName = New UIMenuItem("Name", "Name of the Station.") : StationInfoMenu.AddItem(itemSName)
            itemSCoords = New UIMenuColoredItem("Position", "Position of the Station.", Color.CadetBlue, Color.DodgerBlue) : StationInfoMenu.AddItem(itemSCoords)

            StationInfoMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateStationInfoMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshStationInfoMenu(itemToBind As UIMenuItem)
        Try
            StationInfoMenu.MenuItems.Clear()

            StationInfoMenu.Stat1 = itemToBind.SubInteger1
            itemSIndex = New UIMenuItem("Index", "Index of the Station. Note: Number Must be unique.") With {.SubInteger1 = itemToBind.SubInteger1, .SubInteger2 = itemToBind.SubInteger1} : itemSIndex.SetRightLabel(itemToBind.SubInteger1) : StationInfoMenu.AddItem(itemSIndex)
            itemSName = New UIMenuItem("Name", "Name of the Station.") With {.SubString1 = itemToBind.SubString4} : itemSName.SetRightLabel(itemToBind.SubString4) : StationInfoMenu.AddItem(itemSName)
            itemSCoords = New UIMenuColoredItem("Position", "Position of the Station.", Color.CadetBlue, Color.DodgerBlue) With {.SubString1 = itemToBind.SubString1, .SubString2 = itemToBind.SubString2, .SubString3 = itemToBind.SubString3} : itemSCoords.SetRightLabel(">") : StationInfoMenu.AddItem(itemSCoords)

            StationInfoMenu.RefreshIndex()
            StationMenu.BindMenuToItem(StationInfoMenu, itemToBind)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshStationInfoMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateVector3Menu()
        Try
            Vector3Menu = New UIMenu("", "POSITION", New Point(0, -107))
            Vector3Menu.SetBannerType(Rectangle)
            Vector3Menu.MouseEdgeEnabled = False
            Vector3Menu.AddInstructionalButton(New InstructionalButton(GTA.Control.Jump, "Capture Coordinates"))
            _menuPool.Add(Vector3Menu)

            itemX = New UIMenuItem("X", "X Coordinate.") : Vector3Menu.AddItem(itemX)
            itemY = New UIMenuItem("Y", "Y Coordinate.") : Vector3Menu.AddItem(itemY)
            itemZ = New UIMenuItem("Z", "Z Coordinate.") : Vector3Menu.AddItem(itemZ)

            Vector3Menu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateVector3Menu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshVector3Menu(menuToBind As UIMenu, itemToBind As UIMenuItem)
        Try
            Vector3Menu.MenuItems.Clear()

            itemX = New UIMenuItem("X", "X Coordinate.") With {.SubString1 = itemToBind.SubString1} : itemX.SetRightLabel(itemToBind.SubString1) : Vector3Menu.AddItem(itemX)
            itemY = New UIMenuItem("Y", "Y Coordinate.") With {.SubString1 = itemToBind.SubString2} : itemY.SetRightLabel(itemToBind.SubString2) : Vector3Menu.AddItem(itemY)
            itemZ = New UIMenuItem("Z", "Z Coordinate.") With {.SubString1 = itemToBind.SubString3} : itemZ.SetRightLabel(itemToBind.SubString3) : Vector3Menu.AddItem(itemZ)

            Vector3Menu.RefreshIndex()
            menuToBind.BindMenuToItem(Vector3Menu, itemToBind)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshVector3Menu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub RouteMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles RouteMenu.OnItemSelect
        Try
            Dim br As BusRoute = New BusRoute(selectedItem.SubString1)
            br = br.ReadFromFile
            br.RouteFileName = selectedItem.SubString1
            CurrentRoute = br

            itemEdit.Enabled = True
            itemSave.Enabled = True
            itemRoute.SetRightLabel(selectedItem.Text)
            For Each item As UIMenuItem In sender.MenuItems
                item.SetRightBadge(UIMenuItem.BadgeStyle.None)
            Next
            selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick)

            itemRAuthor.SetRightLabel(br.Author) : itemRAuthor.SubString1 = br.Author
            itemRBHead.SetRightLabel(br.BusHeading) : itemRBHead.SubString1 = br.BusHeading
            itemRBModel.SetRightLabel(br.BusModel) : itemRBModel.SubString1 = br.BusModel
            itemRBSpawn.SetRightLabel(">") : itemRBSpawn.SubString1 = br.BusSpawnPoint.X : itemRBSpawn.SubString2 = br.BusSpawnPoint.Y : itemRBSpawn.SubString3 = br.BusSpawnPoint.Z
            itemRDesc.SetRightLabel(">") : itemRDesc.SubString1 = br.Description
            itemRExtra.SetRightLabel(br.TurnOnExtra) : itemRExtra.SubInteger1 = br.TurnOnExtra
            itemRFare.SetRightLabel($"{br.RouteFare}") : itemRFare.SubInteger1 = br.RouteFare
            itemRName.SetRightLabel(br.RouteName) : itemRName.SubString1 = br.RouteName
            itemRNum.SetRightLabel(br.RouteNumber) : itemRNum.SubInteger1 = br.RouteNumber
            itemRPSpawn.SetRightLabel(">") : itemRPSpawn.SubString1 = br.PlayerSpawnPoint.X : itemRPSpawn.SubString2 = br.PlayerSpawnPoint.Y : itemRPSpawn.SubString3 = br.PlayerSpawnPoint.Z
            itemRStations.SetRightLabel(br.TotalStation)
            itemRVersion.SetRightLabel(br.Version) : itemRVersion.SubString1 = br.Version
            sender.Visible = False
            CreatorMenu.Visible = True
        Catch ex As Exception
            Logger.Log(String.Format("(RouteMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub MainMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles MainMenu.OnItemSelect
        If selectedItem Is itemSave Then
            Dim fileName As String = Nothing
            If CurrentRoute.RouteFileName = Nothing Then
                fileName = $"scripts\BusSimulatorV\Route\{Game.GetUserInput($"{CurrentRoute.RouteNumber}.xml", 65535)}"
            Else
                fileName = $"scripts\BusSimulatorV\Route\{Game.GetUserInput(IO.Path.GetFileName(CurrentRoute.RouteFileName), 65535)}"
            End If
            If fileName = "" Then fileName = $"scripts\BusSimulatorV\Route\{Game.GetUserInput($"{CurrentRoute.RouteNumber}.xml", 65535)}"
            Dim playerPos As New Vector3(CSng(itemRPSpawn.SubString1), CSng(itemRPSpawn.SubString2), CSng(itemRPSpawn.SubString3))
                Dim busPos As New Vector3(CSng(itemRBSpawn.SubString1), CSng(itemRBSpawn.SubString2), CSng(itemRBSpawn.SubString3))
                Dim stations As List(Of Station) = CurrentRoute.Stations
                'stations = stations.OrderBy(Function(x) x.StationIndex).ToList()
                CurrentRoute = New BusRoute(fileName, itemRName.SubString1, itemRNum.SubInteger1, playerPos, busPos, itemRBHead.SubString1, itemRBModel.SubString1, stations, itemRFare.SubInteger1)
                With CurrentRoute
                    .Author = itemRAuthor.SubString1
                    .Version = itemRVersion.SubString1
                    .Description = itemRDesc.SubString1
                    .TurnOnExtra = itemRExtra.SubInteger1
                    .BusHeading = CSng(itemRBHead.SubString1)
                End With
                CurrentRoute.Save()
                UI.Notify($"{CurrentRoute.RouteName} saved successfully.")
                CurrentRoute = Nothing
                UpdateMainMenu()
                RefreshRouteMenu()
                RefreshCreatorMenu()
                RefreshStationMenu()
            ElseIf selectedItem Is itemNew Then
                CurrentRoute.Stations = New List(Of Station)
            RefreshCreatorMenu()
            RefreshStationMenu()
            itemEdit.Enabled = True
            itemSave.Enabled = True
            itemRoute.SetRightLabel("")
            itemRoute.SubString1 = ""
            RefreshRouteMenu()
        End If
    End Sub

    Private Sub CreatorMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles CreatorMenu.OnItemSelect
        Dim cr As BusRoute = CurrentRoute
        Select Case selectedItem.Text
            Case itemRAuthor.Text
                Dim author As String = Native.Function.Call(Of String)(Hash.GET_PLAYER_NAME, Game.Player)
                If itemRAuthor.SubString1 = "" Then
                    author = author
                Else
                    author = itemRAuthor.SubString1
                End If
                cr.Author = Game.GetUserInput(author, 65535)
                itemRAuthor.SetRightLabel(cr.Author) : itemRAuthor.SubString1 = cr.Author
            Case itemRBHead.Text
                If Game.Player.Character.IsInVehicle Then
                    cr.BusHeading = Game.Player.Character.LastVehicle.Heading
                Else
                    cr.BusHeading = Game.Player.Character.Heading
                End If
                itemRBHead.SetRightLabel(cr.BusHeading) : itemRBHead.SubString1 = cr.BusHeading.ToString
            Case itemRBModel.Text
                cr.BusModel = Game.GetUserInput(itemRBModel.SubString1, 65535)
                itemRBModel.SetRightLabel(cr.BusModel) : itemRBModel.SubString1 = cr.BusModel
            Case itemRBSpawn.Text
                CreatorMenu.BindMenuToItem(Vector3Menu, itemRBSpawn)
                RefreshVector3Menu(CreatorMenu, itemRBSpawn)
            Case itemRDesc.Text
                cr.Description = Game.GetUserInput(itemRDesc.SubString1, 65535)
                itemRDesc.SetRightLabel(">") : itemRDesc.SubString1 = cr.Description
            Case itemRExtra.Text
                Dim temp = Game.GetUserInput(itemRExtra.SubInteger1.ToString, 65535)
                If IsNumeric(temp) Then
                    cr.TurnOnExtra = temp
                    itemRExtra.SetRightLabel(cr.TurnOnExtra) : itemRExtra.SubInteger1 = cr.TurnOnExtra
                Else
                    UI.Notify("Please Enter Valid Number.")
                    Exit Sub
                End If
                'cr.TurnOnExtra = Game.GetUserInput(itemRExtra.SubInteger1.ToString, 65535)
            Case itemRFare.Text
                Dim temp = Game.GetUserInput(itemRFare.SubInteger1.ToString, 65535)
                If IsNumeric(temp) Then
                    cr.RouteFare = temp
                    itemRFare.SetRightLabel(cr.RouteFare) : itemRFare.SubInteger1 = cr.RouteFare
                Else

                End If
            Case itemRName.Text
                cr.RouteName = Game.GetUserInput(itemRName.SubString1, 65535)
                itemRName.SetRightLabel(cr.RouteName) : itemRName.SubString1 = cr.RouteName
            Case itemRNum.Text
                Dim temp = Game.GetUserInput(itemRNum.SubInteger1.ToString, 65535)
                If IsNumeric(temp) Then
                    cr.RouteNumber = temp
                    itemRNum.SetRightLabel(cr.RouteNumber) : itemRNum.SubInteger1 = cr.RouteNumber
                Else
                    UI.Notify("Please Enter Valid Number.")
                    Exit Sub
                End If
            Case itemRPSpawn.Text
                CreatorMenu.BindMenuToItem(Vector3Menu, itemRPSpawn)
                RefreshVector3Menu(CreatorMenu, itemRPSpawn)
            Case itemRVersion.Text
                cr.Version = Game.GetUserInput(itemRVersion.SubString1, 65535)
                itemRVersion.SetRightLabel(cr.Version) : itemRVersion.SubString1 = cr.Version
            Case itemRStations.Text
                RefreshStationMenu()
                CreatorMenu.BindMenuToItem(StationMenu, itemRStations)
        End Select
    End Sub

    Private Sub StationMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles StationMenu.OnItemSelect
        StationMenu.BindMenuToItem(StationInfoMenu, selectedItem)
        RefreshStationInfoMenu(selectedItem)
    End Sub

    Private Sub StationInfoMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles StationInfoMenu.OnItemSelect
        Dim cs As Station = CurrentRoute.Stations(itemSIndex.SubInteger2)

        Select Case selectedItem.Text
            Case itemSIndex.Text
                Dim temp = Game.GetUserInput(itemSIndex.SubInteger1.ToString, 65535)
                If IsNumeric(temp) Then
                    If temp <= CurrentRoute.TotalStation - 1 Then
                        If temp >= 0 Then
                            Dim si = temp
                            itemSIndex.SetRightLabel(si) : itemSIndex.SubInteger1 = CInt(si)
                        Else
                            UI.Notify("Index cannot less than 0.")
                            Exit Sub
                        End If
                    Else
                        UI.Notify($"Index cannot greater than {CurrentRoute.TotalStation - 1}.")
                        Exit Sub
                    End If
                Else
                    UI.Notify("Please Enter Valid Number.")
                    Exit Sub
                End If
            Case itemSName.Text
                Dim sn = Game.GetUserInput(itemSName.SubString1, 65535)
                itemSName.SetRightLabel(sn) : itemSName.SubString1 = sn
            Case itemSCoords.Text
                StationInfoMenu.BindMenuToItem(Vector3Menu, itemSCoords)
                RefreshVector3Menu(StationInfoMenu, itemSCoords)
        End Select
    End Sub

    Private Sub StationInfoMenu_OnMenuClose(sender As UIMenu) Handles StationInfoMenu.OnMenuClose
        Dim oldStation As Station = CurrentRoute.Stations(itemSIndex.SubInteger2)
        Dim newStation As New Station(itemSIndex.SubInteger1, itemSName.SubString1, New Vector3(CSng(itemSCoords.SubString1), CSng(itemSCoords.SubString2), CSng(itemSCoords.SubString3)))
        CurrentRoute.Stations.Remove(oldStation)
        CurrentRoute.Stations.Add(newStation)

        CurrentRoute.Stations = CurrentRoute.Stations.OrderBy(Function(x) x.StationIndex).ToList()
        RefreshStationMenu()
    End Sub

    Private Sub Vector3Menu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles Vector3Menu.OnItemSelect
        Select Case selectedItem.Text
            Case itemX.Text
                Dim temp = Game.GetUserInput(itemX.SubString1, 65535)
                Dim sng As Single
                If Single.TryParse(temp, sng) Then
                    itemX.SubString1 = temp
                    itemX.SetRightLabel(itemX.SubString1)
                Else
                    UI.Notify("Please Enter Valid Float/Single.")
                    Exit Sub
                End If
            Case itemY.Text
                Dim temp = Game.GetUserInput(itemY.SubString1, 65535)
                Dim sng As Single
                If Single.TryParse(temp, sng) Then
                    itemY.SubString1 = temp
                    itemY.SetRightLabel(itemY.SubString1)
                Else
                    UI.Notify("Please Enter Valid Float/Single.")
                    Exit Sub
                End If
            Case itemZ.Text
                Dim temp = Game.GetUserInput(itemZ.SubString1, 65535)
                Dim sng As Single
                If Single.TryParse(temp, sng) Then
                    itemZ.SubString1 = temp
                    itemZ.SetRightLabel(itemZ.SubString1)
                Else
                    UI.Notify("Please Enter Valid Float/Single.")
                    Exit Sub
                End If
        End Select
    End Sub

    Private Sub Vector3Menu_OnMenuClose(sender As UIMenu) Handles Vector3Menu.OnMenuClose
        sender.ParentItem.SubString1 = itemX.SubString1
        sender.ParentItem.SubString2 = itemY.SubString1
        sender.ParentItem.SubString3 = itemZ.SubString1
        sender.ParentItem.SetRightLabel(">")
    End Sub

    Private Sub Creator_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        _menuPool.ProcessMenus()

        If _menuPool.IsAnyMenuOpen Then If ShowInfo Then DrawCoords()

        If StationMenu.Visible Then
            If Game.IsControlJustReleased(0, GTA.Control.Jump) Then
                Dim cr As BusRoute = CurrentRoute
                Dim stationName As String = Game.GetUserInput(World.GetStreetName(Game.Player.Character.Position), 65535)
                Dim stationCoord As Vector3 = If(Game.Player.Character.IsInVehicle, Game.Player.Character.LastVehicle.Position, Game.Player.Character.Position)
                Dim station As New Station()
                With station
                    .StationIndex = cr.TotalStation
                    .StationName = stationName
                    .StationCoords = stationCoord
                End With
                cr.Stations.Add(station)
                RefreshStationMenu()
                UpdateCreatorMenu()
            ElseIf Game.IsControlJustReleased(0, GTA.Control.CreatorDelete) Then
                If StationMenu.MenuItems.Count <> 0 Then
                    Dim cr As BusRoute = CurrentRoute
                    Dim currentStation As Station = cr.Stations(StationMenu.MenuItems(StationMenu.CurrentSelection).SubInteger1)
                    cr.Stations.Remove(currentStation)
                    RefreshStationMenu()
                    UpdateCreatorMenu()
                End If
            End If
        End If

        If Vector3Menu.Visible Then
            If Game.IsControlJustReleased(0, GTA.Control.Jump) Then
                If Game.Player.Character.IsInVehicle Then
                    itemX.SubString1 = Game.Player.Character.LastVehicle.Position.X : itemX.SetRightLabel(Game.Player.Character.LastVehicle.Position.X.ToString)
                    itemY.SubString1 = Game.Player.Character.LastVehicle.Position.Y : itemY.SetRightLabel(Game.Player.Character.LastVehicle.Position.Y.ToString)
                    itemZ.SubString1 = Game.Player.Character.LastVehicle.Position.Z - 1.0F : itemZ.SetRightLabel((Game.Player.Character.LastVehicle.Position.Z - 1.0F).ToString)
                Else
                    itemX.SubString1 = Game.Player.Character.Position.X : itemX.SetRightLabel(Game.Player.Character.Position.X.ToString)
                    itemY.SubString1 = Game.Player.Character.Position.Y : itemY.SetRightLabel(Game.Player.Character.Position.Y.ToString)
                    itemZ.SubString1 = Game.Player.Character.Position.Z - 1.0F : itemZ.SetRightLabel((Game.Player.Character.Position.Z - 1.0F).ToString)
                End If
            End If
        End If
    End Sub

    Private Sub Creator_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted

    End Sub

    Private Sub Creator_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Modifiers = Modifier AndAlso e.KeyCode = Key Then
            MainMenu.Visible = Not MainMenu.Visible
        End If
    End Sub

    Public Sub DrawCoords()
        Try
            Dim SRMR As SizeF = UIMenu.GetScreenResolutionMaintainRatio
            Dim SZB As Point = UIMenu.GetSafezoneBounds
            Dim pv3 As Vector3 = Game.Player.Character.Position
            Dim cv3 As Vector3 = GameplayCamera.Position
            Dim av3 As Vector3 = World.GetCrosshairCoordinates.HitCoords
            Dim pr3 As Vector3 = Game.Player.Character.Rotation
            Dim cr3 As Vector3 = GameplayCamera.Rotation

            Dim X As Integer = 500
            Dim Y As Integer = 190
            Dim BoxLayout1 As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(400, 190), Color.FromArgb(200, 0, 0, 0)) : BoxLayout1.Draw()
            Dim BoxTitle1 As New UIResRectangle(New Point(BoxLayout1.Position.X + 1, BoxLayout1.Position.Y + 1), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle1.Draw()
            Dim TitleText1 As New UIResText("Route Info", New Point(BoxTitle1.Position.X + (BoxTitle1.Size.Width / 2), BoxTitle1.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText1.Outline = True : TitleText1.Draw()
            Dim Line1 As New UIResText($"Route Name: {CurrentRoute.RouteName}", New Point(BoxTitle1.Position.X + 10, BoxTitle1.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line1.Outline = True : Line1.Draw()
            Dim Line2 As New UIResText($"File Name: {IO.Path.GetFileName(CurrentRoute.RouteFileName)}", New Point(BoxTitle1.Position.X + 10, BoxTitle1.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line2.Outline = True : Line2.Draw()
            Dim BoxTitle2 As New UIResRectangle(New Point(BoxLayout1.Position.X + 1, BoxLayout1.Position.Y + 81), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle2.Draw()
            Dim TitleText2 As New UIResText("World Info", New Point(BoxTitle2.Position.X + (BoxTitle2.Size.Width / 2), BoxTitle2.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText2.Outline = True : TitleText2.Draw()
            Dim Line7 As New UIResText($"Street Name: {World.GetStreetName(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line7.Outline = True : Line7.Draw()
            Dim Line8 As New UIResText($"Zone Name: {World.GetZoneName(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line8.Outline = True : Line8.Draw()
            Dim Line9 As New UIResText($"Zone Label: {World.GetZoneNameLabel(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 70), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line9.Outline = True : Line9.Draw()

            X = 902
            Dim BoxLayout2 As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(400, 190), Color.FromArgb(200, 0, 0, 0)) : BoxLayout2.Draw()
            Dim BoxTitle3 As New UIResRectangle(New Point(BoxLayout2.Position.X + 1, BoxLayout2.Position.Y + 1), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle3.Draw()
            Dim TitleText3 As New UIResText("Position Coordinates", New Point(BoxTitle3.Position.X + (BoxTitle3.Size.Width / 2), BoxTitle3.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText3.Outline = True : TitleText3.Draw()
            Dim Line3 As New UIResText($"Player X: {pv3.X.ToString("0.0000")} Y: {pv3.Y.ToString("0.0000")} Z: {pv3.Z.ToString("0.0000")} H: {Game.Player.Character.Heading.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line3.Outline = True : Line3.Draw()
            Dim Line5 As New UIResText($"Camera X: {cv3.X.ToString("0.0000")} Y: {cv3.Y.ToString("0.0000")} Z: {cv3.Z.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line5.Outline = True : Line5.Draw()
            Dim Line6 As New UIResText($"Crosshair X: {av3.X.ToString("0.0000")} Y: {av3.Y.ToString("0.0000")} Z: {av3.Z.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 70), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line6.Outline = True : Line6.Draw()
            Dim BoxTitle4 As New UIResRectangle(New Point(BoxLayout2.Position.X + 1, BoxLayout2.Position.Y + 101), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle4.Draw()
            Dim TitleText4 As New UIResText("Rotation Coordinates", New Point(BoxTitle4.Position.X + (BoxTitle4.Size.Width / 2), BoxTitle4.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText4.Outline = True : TitleText4.Draw()
            Dim Line10 As New UIResText($"Player X: {pr3.X.ToString("0.0000")} Y: {pr3.Y.ToString("0.0000")} Z: {pr3.Z.ToString("0.0000")}", New Point(BoxTitle4.Position.X + 10, BoxTitle4.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line10.Outline = True : Line10.Draw()
            Dim Line12 As New UIResText($"Camera X: {cr3.X.ToString("0.0000")} Y: {cr3.Y.ToString("0.0000")} Z: {cr3.Z.ToString("0.0000")}", New Point(BoxTitle4.Position.X + 10, BoxTitle4.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line12.Outline = True : Line12.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawCoords): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub
End Class
