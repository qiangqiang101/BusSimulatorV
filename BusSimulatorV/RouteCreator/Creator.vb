Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI

Public Class Creator
    Inherits Script

    Public WithEvents MainMenu, RouteMenu, CreatorMenu As UIMenu
    Public itemMRoute, itemNew, itemRoute, itemSave As UIMenuItem
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
            itemNew = New UIMenuItem("New", "Create a new bus route.")
            MainMenu.AddItem(itemNew)
            itemSave = New UIMenuItem("Save", "Save current bus route.")
            With itemSave
                .Enabled = False
            End With
            MainMenu.AddItem(itemSave)
            itemRoute = New UIMenuItem("Load", "Load and edit an existing bus route.")
            MainMenu.AddItem(itemRoute)
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
                    itemMRoute = New UIMenuItem(Path.GetFileNameWithoutExtension(xmlFile))
                    With itemMRoute
                        .SubString1 = xmlFile
                        .Description = $"Author: {br.Author}~n~Version: {br.Version}~n~Description: {br.Description}"
                    End With
                    RouteMenu.AddItem(itemMRoute)
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
            _menuPool.Add(RouteMenu)

            RouteMenu.RefreshIndex()
            MainMenu.BindMenuToItem(CreatorMenu, itemNew)
            MainMenu.BindMenuToItem(CreatorMenu, itemMRoute)
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
            itemMRoute = selectedItem
            sender.GoBack()
        Catch ex As Exception
            Logger.Log(String.Format("(RouteMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub MainMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles MainMenu.OnItemSelect

    End Sub

    Private Sub Creator_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        _menuPool.ProcessMenus()
    End Sub

    Private Sub Creator_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted

    End Sub
End Class
