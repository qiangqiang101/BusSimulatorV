Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI
Imports System.Windows.Forms

Public Class Creator
    Inherits Script

    Public WithEvents MainMenu, RouteMenu, CreatorMenu As UIMenu
    Public itemNew, itemRoute, itemSave As UIMenuItem
    Public itemRName, itemRNum, itemRAuthor, itemRDesc, itemRVersion, itemRPSpawn, itemRBSpawn, itemRBHead, itemRBModel, itemRExtra, itemRFare, itemRStations As UIMenuItem
    Public CurrentRoute As BusRoute
    Public _menuPool As MenuPool
    Public Rectangle = New UIResRectangle()

    Public Sub New()
        _menuPool = New MenuPool()
        Rectangle.Color = Color.FromArgb(0, 0, 0, 0)

        CreateMainMenu()
        CreateRouteMenu()
        CreateCreatorMenu()
    End Sub

    Public Sub CreateMainMenu()
        Try
            MainMenu = New UIMenu("", "MAIN MENU", New Point(0, -107))
            MainMenu.SetBannerType(Rectangle)
            MainMenu.MouseEdgeEnabled = False
            _menuPool.Add(MainMenu)
            itemNew = New UIMenuItem("New", "Create a new bus route.") : MainMenu.AddItem(itemNew)
            itemSave = New UIMenuItem("Save", "Save current bus route.") With {.Enabled = False} : MainMenu.AddItem(itemSave)
            itemRoute = New UIMenuItem("Load", "Load and edit an existing bus route.") : MainMenu.AddItem(itemRoute)
            MainMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RefreshMainMenu()
        Try
            MainMenu.MenuItems.Clear()
            itemNew = New UIMenuItem("New", "Create a new bus route.") : MainMenu.AddItem(itemNew)
            itemSave = New UIMenuItem("Save", "Save current bus route.") With {.Enabled = False} : MainMenu.AddItem(itemSave)
            itemRoute = New UIMenuItem("Load", "Load and edit an existing bus route.") : MainMenu.AddItem(itemRoute)
            MainMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
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
            Logger.Log(String.Format("(CreateRouteMenu): {0} {1}", ex.Message, ex.StackTrace))
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
            itemRPSpawn = New UIMenuItem("Player Position", "Position for Player Spawn Point.") : CreatorMenu.AddItem(itemRPSpawn)
            itemRBSpawn = New UIMenuItem("Bus Position", "Position for Bus Spawn Point.") : CreatorMenu.AddItem(itemRBSpawn)
            itemRBHead = New UIMenuItem("Bus Heading", "Heading of the Bus when spawn.") : CreatorMenu.AddItem(itemRBHead)
            itemRBModel = New UIMenuItem("Bus Model", "Model name of the vehicle use in this Route.") : CreatorMenu.AddItem(itemRBModel)
            itemRExtra = New UIMenuItem("Turn On Extra", "Which Extra Component to turn on in this Route.") : CreatorMenu.AddItem(itemRExtra)
            itemRFare = New UIMenuItem("Fare", "How much money to earn per passenger.") : CreatorMenu.AddItem(itemRFare)
            itemRStations = New UIMenuColoredItem("Stations", "List of the Station for this Route.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRStations)
            CreatorMenu.RefreshIndex()
            MainMenu.BindMenuToItem(CreatorMenu, itemNew)
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
            itemRPSpawn = New UIMenuItem("Player Position", "Position for Player Spawn Point.") : CreatorMenu.AddItem(itemRPSpawn)
            itemRBSpawn = New UIMenuItem("Bus Position", "Position for Bus Spawn Point.") : CreatorMenu.AddItem(itemRBSpawn)
            itemRBHead = New UIMenuItem("Bus Heading", "Heading of the Bus when spawn.") : CreatorMenu.AddItem(itemRBHead)
            itemRBModel = New UIMenuItem("Bus Model", "Model name of the vehicle use in this Route.") : CreatorMenu.AddItem(itemRBModel)
            itemRExtra = New UIMenuItem("Turn On Extra", "Which Extra Component to turn on in this Route.") : CreatorMenu.AddItem(itemRExtra)
            itemRFare = New UIMenuItem("Fare", "How much money to earn per passenger.") : CreatorMenu.AddItem(itemRFare)
            itemRStations = New UIMenuColoredItem("Stations", "List of the Station for this Route.", Color.CadetBlue, Color.DodgerBlue) : CreatorMenu.AddItem(itemRStations)
            CreatorMenu.RefreshIndex()
            MainMenu.BindMenuToItem(CreatorMenu, itemNew)
        Catch ex As Exception
            Logger.Log(String.Format("(CreateCreatorMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub RouteMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles RouteMenu.OnItemSelect
        Try
            Dim br As BusRoute = New BusRoute(selectedItem.SubString1)
            br = br.ReadFromFile
            CurrentRoute = br

            itemSave.Enabled = True
            itemRoute.SetRightLabel(selectedItem.Text)
            For Each item As UIMenuItem In sender.MenuItems
                item.SetRightBadge(UIMenuItem.BadgeStyle.None)
            Next
            selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick)

            itemRAuthor.SetRightLabel(br.Author) : itemRAuthor.SubString1 = br.Author
            itemRBHead.SetRightLabel(br.BusHeading) : itemRBHead.SubString1 = br.BusHeading
            itemRBModel.SetRightLabel(br.BusModel) : itemRBModel.SubString1 = br.BusModel
            itemRBSpawn.SetRightLabel("...") : itemRBSpawn.SubString1 = br.BusSpawnPoint.X : itemRBSpawn.SubString2 = br.BusSpawnPoint.Y : itemRBSpawn.SubString3 = br.BusSpawnPoint.Z
            itemRDesc.SetRightLabel("...") : itemRDesc.SubString1 = br.Description
            itemRExtra.SetRightLabel(br.TurnOnExtra) : itemRExtra.SubInteger1 = br.TurnOnExtra
            itemRFare.SetRightLabel($"{br.RouteFare}") : itemRFare.SubInteger1 = br.RouteFare
            itemRName.SetRightLabel(br.RouteName) : itemRName.SubString1 = br.RouteName
            itemRNum.SetRightLabel(br.RouteNumber) : itemRNum.SubInteger1 = br.RouteNumber
            itemRPSpawn.SetRightLabel("...") : itemRPSpawn.SubString1 = br.PlayerSpawnPoint.X : itemRPSpawn.SubString2 = br.PlayerSpawnPoint.Y : itemRPSpawn.SubString3 = br.PlayerSpawnPoint.Z
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
            If CurrentRoute.RouteFileName = Nothing Then
                CurrentRoute.RouteFileName = $"scripts\BusSimulatorV\Route\{Game.GetUserInput($"New.xml", 65535)}"
            End If
            CurrentRoute.Save()
            UI.Notify($"{CurrentRoute.RouteName} saved successfully.")
            CurrentRoute = Nothing
            RefreshMainMenu()
            RefreshRouteMenu()
            RefreshCreatorMenu()
        ElseIf selectedItem Is itemNew Then
            itemSave.Enabled = True
        End If
    End Sub

    Private Sub CreatorMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles CreatorMenu.OnItemSelect
        Select Case selectedItem.Text
            Case itemRAuthor.Text
                CurrentRoute.Author = Game.GetUserInput(itemRAuthor.SubString1, 65535)
                itemRAuthor.SetRightLabel(CurrentRoute.Author) : itemRAuthor.SubString1 = CurrentRoute.Author
        End Select
    End Sub

    Private Sub Creator_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        _menuPool.ProcessMenus()
    End Sub

    Private Sub Creator_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted

    End Sub

    Private Sub Creator_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If e.Modifiers = Keys.Shift AndAlso e.KeyCode = Keys.Delete Then
            MainMenu.Visible = Not MainMenu.Visible
        End If
    End Sub
End Class
