﻿Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI
Imports System.Windows.Forms

Public Class BusSim
    Inherits Script

    Public WithEvents MainMenu, RouteMenu As UIMenu
    Public itemMRoute, itemPlay, itemRoute As UIMenuItem
    Public CurrentRoute As BusRoute
    Public _menuPool As MenuPool
    Public _timerPool As TimerBarPool
    Public Rectangle = New UIResRectangle()
    Public config As ScriptSettings = ScriptSettings.Load("scripts\BusSimulatorV\config.ini")
    Public Earned As Integer = 0
    Public Bus As Vehicle
    Public IsInGame As Boolean = False
    Public PlayerOriginalPosition, PlayerOriginalRotation As Vector3
    Public EarnedTimerBar, SpeedTimerBar, NextStationTimerBar As TextTimerBar
    Public CurrentStationIndex As Integer
    Public BlipDict As New Dictionary(Of Integer, Blip)
    Public BlipList As New List(Of Blip)
    Public ModBlip As Blip
    Public CameraPos, CameraRot As Vector3
    Public MenuCamera As Camera
    Public MenuActivator As New Vector3(436.4384, -625.9146, 28.70776)
    Public FrontDoorKey, RearDoorKey, LeftBlinkerKey, RightBlinkerKey As GTA.Control
    Public FrontDoorKey2, RearDoorKey2, LeftBlinkerKey2, RightBlinkerKey2 As Keys
    Dim Speedometer As SpeedMeasurement
    Public LeftBlinker, RightBlinker As Boolean

    Public Sub LoadSettings()
        Try
            FrontDoorKey = config.GetValue(Of GTA.Control)("JOYCONTROL", "FrontDoor", GTA.Control.ScriptRUp)
            RearDoorKey = config.GetValue(Of GTA.Control)("JOYCONTROL", "RearDoor", GTA.Control.ScriptRDown)
            LeftBlinkerKey = config.GetValue(Of GTA.Control)("JOYCONTROL", "LeftBlinker", GTA.Control.ScriptRLeft)
            RightBlinkerKey = config.GetValue(Of GTA.Control)("JOYCONTROL", "RightBlinker", GTA.Control.ScriptRRight)
            FrontDoorKey2 = config.GetValue(Of Keys)("KBCONTROL", "FrontDoor", Keys.Up)
            RearDoorKey2 = config.GetValue(Of Keys)("KBCONTROL", "RearDoor", Keys.Down)
            LeftBlinkerKey2 = config.GetValue(Of Keys)("KBCONTROL", "LeftBlinker", Keys.Left)
            RightBlinkerKey2 = config.GetValue(Of Keys)("KBCONTROL", "RightBlinker", Keys.Right)
            Speedometer = config.GetValue(Of SpeedMeasurement)("GENERAL", "Speedometer", SpeedMeasurement.KPH)
        Catch ex As Exception
            Logger.Log(String.Format("(LoadSettings): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub SaveSettings()
        Try
            If Not File.Exists("scripts\BusSimulatorV\config.ini") Then
                config.SetValue(Of GTA.Control)("JOYCONTROL", "FrontDoor", GTA.Control.ScriptRUp)
                config.SetValue(Of GTA.Control)("JOYCONTROL", "RearDoor", GTA.Control.ScriptRDown)
                config.SetValue(Of GTA.Control)("JOYCONTROL", "LeftBlinker", GTA.Control.ScriptRLeft)
                config.SetValue(Of GTA.Control)("JOYCONTROL", "RightBlinker", GTA.Control.ScriptRRight)
                config.SetValue(Of Keys)("KBCONTROL", "FrontDoor", Keys.Up)
                config.SetValue(Of Keys)("KBCONTROL", "RearDoor", Keys.Down)
                config.SetValue(Of Keys)("KBCONTROL", "LeftBlinker", Keys.Left)
                config.SetValue(Of Keys)("KBCONTROL", "RightBlinker", Keys.Right)
                config.SetValue(Of SpeedMeasurement)("GENERAL", "Speedometer", SpeedMeasurement.KPH)
                config.Save()
            End If
        Catch ex As Exception
            Logger.Log(String.Format("(SaveSettings): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub New()
        SaveSettings()
        LoadSettings()

        _menuPool = New MenuPool()
        _timerPool = New TimerBarPool()
        Rectangle.Color = Color.FromArgb(0, 0, 0, 0)

        CreateMainMenu()
        CreateRouteMenu()

        CameraPos = New Vector3(427.3593, -670.04, 29.90757)
        CameraRot = New Vector3(2.562092, 0, -16.7178)

        ModBlip = World.CreateBlip(MenuActivator)
        With ModBlip
            .IsShortRange = True
            .Sprite = BlipSprite.Bus
            .Name = "Bus Depot"
        End With
    End Sub

    Public Sub CreateMainMenu()
        Try
            MainMenu = New UIMenu("", "MAIN MENU", New Point(0, -107))
            MainMenu.SetBannerType(Rectangle)
            MainMenu.MouseEdgeEnabled = False
            _menuPool.Add(MainMenu)
            itemRoute = New UIMenuItem("Select Route", "Select from 24 playable routes.")
            MainMenu.AddItem(itemRoute)
            itemPlay = New UIMenuItem("Start Mission", "Play selected route.")
            With itemPlay
                .Enabled = False
            End With
            MainMenu.AddItem(itemPlay)
            MainMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateRouteMenu()
        Try
            RouteMenu = New UIMenu("", "ROUTE SELECTION", New Point(0, -107))
            RouteMenu.SetBannerType(Rectangle)
            RouteMenu.MouseEdgeEnabled = False
            _menuPool.Add(RouteMenu)

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    itemMRoute = New UIMenuItem(Path.GetFileNameWithoutExtension(xmlFile))
                    With itemMRoute
                        .SubString1 = xmlFile
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

    Private Sub RouteMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles RouteMenu.OnItemSelect
        Try
            Dim br As BusRoute = New BusRoute(selectedItem.SubString1)
            br = br.ReadFromFile
            CurrentRoute = br
            itemPlay.Enabled = True
            itemRoute.SetRightLabel(selectedItem.Text)
            sender.GoBack()
        Catch ex As Exception
            Logger.Log(String.Format("(RouteMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub BusSim_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        Try
            _menuPool.ProcessMenus()

            If _menuPool.IsAnyMenuOpen Then
                Game.DisableControlThisFrame(0, GTA.Control.MoveUpDown)
                Game.DisableControlThisFrame(0, GTA.Control.MoveLeftRight)
                Game.DisableControlThisFrame(0, GTA.Control.MoveDown)
                Game.DisableControlThisFrame(0, GTA.Control.MoveDownOnly)
                Game.DisableControlThisFrame(0, GTA.Control.MoveLeft)
                Game.DisableControlThisFrame(0, GTA.Control.MoveLeftOnly)
                Game.DisableControlThisFrame(0, GTA.Control.MoveRight)
                Game.DisableControlThisFrame(0, GTA.Control.MoveRightOnly)
                Game.DisableControlThisFrame(0, GTA.Control.MoveUp)
                Game.DisableControlThisFrame(0, GTA.Control.MoveUpOnly)
                Game.DisableControlThisFrame(0, GTA.Control.Jump)
                Game.DisableControlThisFrame(0, GTA.Control.Cover)
                Game.DisableControlThisFrame(0, GTA.Control.Context)
            End If

            If IsInGame Then
                UpdateTimerBars()

                If Bus.Position.DistanceTo(Game.Player.Character.Position) >= 50.0F Then
                    For Each blip As Blip In BlipList
                        blip.Remove()
                    Next
                    CurrentStationIndex = 0
                    For Each ped As Ped In Game.Player.Character.CurrentPedGroup
                        If Not ped = Game.Player.Character Then
                            ped.LeaveGroup()
                            Game.Player.Character.CurrentPedGroup.Remove(ped)
                            ped.Task.LeaveVehicle(Bus, False)
                        End If
                    Next
                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = "Start Mission"
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    BigMessageThread.MessageInstance.ShowMissionPassedMessage("Mission Failed")
                    UI.ShowSubtitle("You abandoned your bus.")
                End If

                If Bus.Position.DistanceTo(CurrentRoute.Stations(CurrentStationIndex).StationCoords) <= 5.0F Then
                    Dim b As Blip = BlipDict.Item(CurrentStationIndex)
                    BlipDict.Remove(CurrentStationIndex)
                    b.Remove()
                    CurrentStationIndex += 1
                    If CurrentStationIndex = (CurrentRoute.Stations.Count) Then
                        For Each ped As Ped In Game.Player.Character.CurrentPedGroup
                            If Not ped = Game.Player.Character Then
                                ped.LeaveGroup()
                                Game.Player.Character.CurrentPedGroup.Remove(ped)
                                ped.Task.LeaveVehicle(Bus, False)
                            End If
                        Next
                        IsInGame = False
                        BlipDict.Clear()
                        BlipList.Clear()
                        CurrentStationIndex = 0
                        Game.Player.Character.Money += Earned
                        Earned = 0
                        itemPlay.Text = "Start Mission"
                        itemPlay.Enabled = True
                        RemoveTimerBars()
                        BigMessageThread.MessageInstance.ShowMissionPassedMessage("Mission Passed")
                    Else
                        b = BlipDict.Item(CurrentStationIndex)
                        b.ShowRoute = True
                        If Not Game.Player.Character.CurrentPedGroup.Count = 0 Then
                            Dim ped As Ped = Game.Player.Character.CurrentPedGroup.GetMember(New Random().Next(0, Game.Player.Character.CurrentPedGroup.Count))
                            If Not ped = Game.Player.Character Then
                                ped.CurrentBlip.Remove()
                                ped.Task.LeaveVehicle(Bus, False)
                                ped.LeaveGroup()
                                Game.Player.Character.CurrentPedGroup.Remove(ped)
                            End If
                        End If
                        If Not Game.Player.Character.CurrentPedGroup.Count = 0 Then
                            Dim ped As Ped = Game.Player.Character.CurrentPedGroup.GetMember(New Random().Next(0, Game.Player.Character.CurrentPedGroup.Count))
                            If Not ped = Game.Player.Character Then
                                ped.CurrentBlip.Remove()
                                ped.Task.LeaveVehicle(Bus, False)
                                ped.LeaveGroup()
                                Game.Player.Character.CurrentPedGroup.Remove(ped)
                            End If
                        End If
                        If Not Game.Player.Character.CurrentPedGroup.Count = 7 Then
                            Dim Peds As Ped() = World.GetNearbyPeds(Game.Player.Character, 10.0)
                            Dim MaxRider As Integer = New Random().Next(0, Peds.Count + 1), Rider As Integer = 0
                            If Not Rider = MaxRider Then
                                For Each ped As Ped In Peds
                                    If Not ped.IsInVehicle Then
                                        Game.Player.Character.CurrentPedGroup.Add(ped, False)
                                        ped.Task.FightAgainstHatedTargets(5000, 0)
                                        ped.AlwaysKeepTask = True
                                        ped.AddBlip()
                                        Earned += CurrentRoute.RouteFare
                                        Rider += 1
                                    End If
                                Next
                            End If
                        End If
                    End If
                    End If

                    If Game.IsControlJustReleased(0, FrontDoorKey) AndAlso Game.Player.Character.IsInVehicle(Bus) Then
                        If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                        If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                    End If
                    If Game.IsControlJustReleased(0, RearDoorKey) AndAlso Game.Player.Character.IsInVehicle(Bus) Then
                        If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                        If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False) Else Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                    End If
                    Bus.LeftIndicatorLightOn = LeftBlinker
                    Bus.RightIndicatorLightOn = RightBlinker
                    If Game.IsControlJustReleased(0, LeftBlinkerKey) AndAlso Game.Player.Character.IsInVehicle(Bus) Then LeftBlinker = Not LeftBlinker
                    If Game.IsControlJustReleased(0, RightBlinkerKey) AndAlso Game.Player.Character.IsInVehicle(Bus) Then RightBlinker = Not RightBlinker
                End If

                If Game.Player.Character.Position.DistanceTo2D(MenuActivator) <= 3.0 AndAlso Not Game.Player.Character.IsInVehicle Then
                If Game.IsControlJustReleased(0, GTA.Control.Context) Then
                    MainMenu.Show()
                    MenuCamera = World.CreateCamera(CameraPos, CameraRot, GameplayCamera.FieldOfView)
                    World.RenderingCamera = MenuCamera
                Else
                    DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to work.")
                End If
            End If
        Catch ex As Exception
            Logger.Log(String.Format("(BusSim_Tick): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub BusSim_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        Try
            If Not Bus = Nothing Then Bus.Delete()
            For Each blip As Blip In BlipList
                blip.Remove()
            Next
            ModBlip.Remove()
            For Each ped As Ped In Game.Player.Character.CurrentPedGroup
                ped.CurrentBlip.Remove()
                Game.Player.Character.CurrentPedGroup.Remove(ped)
            Next
        Catch ex As Exception
            Logger.Log(String.Format("(BusSim_Aborted): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub MainMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles MainMenu.OnItemSelect
        Try
            If selectedItem.Text = "Start Mission" Then
                sender.Hide()
                World.RenderingCamera = Nothing
                World.DestroyAllCameras()
                PlayerOriginalPosition = Game.Player.Character.Position
                PlayerOriginalRotation = Game.Player.Character.Rotation
                Game.Player.Character.Position = CurrentRoute.PlayerSpawnPoint
                If Not Bus = Nothing Then Bus.Delete()
                Bus = World.CreateVehicle(CurrentRoute.BusModel, CurrentRoute.BusSpawnPoint, CurrentRoute.BusHeading)
                With Bus
                    .IsPersistent = True
                    .RemoveAllExtras()
                    If .ExtraExists(CurrentRoute.TurnOnExtra) Then .ToggleExtra(CurrentRoute.TurnOnExtra, True)
                    .PlaceOnGround()
                End With
                For Each station As Station In CurrentRoute.Stations
                    Dim b As Blip = World.CreateBlip(station.StationCoords)
                    With b
                        .Color = BlipColor.Yellow
                        .ShowNumber(station.StationIndex)
                        .Name = station.StationName
                    End With
                    If Not BlipDict.ContainsKey(station.StationIndex) Then BlipDict.Add(station.StationIndex, b)
                    If Not BlipList.Contains(b) Then BlipList.Add(b)
                Next
                CurrentStationIndex = 0
                DrawtimerBars()
                selectedItem.Text = "Stop Mission"
                selectedItem.Enabled = False
                Game.Player.Character.SetAsLeader()
                IsInGame = True
            End If
        Catch ex As Exception
            Logger.Log(String.Format("(MainMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub DrawtimerBars()
        Try
            EarnedTimerBar = New TextTimerBar("EARNED", "$" & Earned.ToString("0"))
            Select Case Speedometer
                Case SpeedMeasurement.KPH
                    SpeedTimerBar = New TextTimerBar("SPEED KPH", Bus.SpeedKPH.ToString("0"))
                Case Else
                    SpeedTimerBar = New TextTimerBar("SPEED MPH", Bus.SpeedMPH.ToString("0"))
            End Select
            NextStationTimerBar = New TextTimerBar("NEXT STATION", CurrentRoute.Stations(CurrentStationIndex).StationName)
            _timerPool.Add(EarnedTimerBar)
            _timerPool.Add(SpeedTimerBar)
            _timerPool.Add(NextStationTimerBar)
            _timerPool.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawtimerBars): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub UpdateTimerBars()
        Try
            _timerPool.Remove(EarnedTimerBar)
            _timerPool.Remove(SpeedTimerBar)
            _timerPool.Remove(NextStationTimerBar)
            EarnedTimerBar = New TextTimerBar("EARNED", "$" & Earned.ToString("0"))
            Select Case Speedometer
                Case SpeedMeasurement.KPH
                    SpeedTimerBar = New TextTimerBar("SPEED KPH", Bus.SpeedKPH.ToString("0"))
                Case Else
                    SpeedTimerBar = New TextTimerBar("SPEED MPH", Bus.SpeedMPH.ToString("0"))
            End Select
            NextStationTimerBar = New TextTimerBar("NEXT STATION", CurrentRoute.Stations(CurrentStationIndex).StationName)
            _timerPool.Add(EarnedTimerBar)
            _timerPool.Add(SpeedTimerBar)
            _timerPool.Add(NextStationTimerBar)
            _timerPool.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(UpdateTimerBars): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RemoveTimerBars()
        _timerPool.Remove(EarnedTimerBar)
        _timerPool.Remove(SpeedTimerBar)
        _timerPool.Remove(NextStationTimerBar)
    End Sub

    Private Sub MainMenu_OnMenuClose(sender As UIMenu) Handles MainMenu.OnMenuClose
        World.RenderingCamera = Nothing
        World.DestroyAllCameras()
    End Sub

    Private Sub BusSim_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If IsInGame Then
            If e.KeyCode = FrontDoorKey2 AndAlso Game.Player.Character.IsInVehicle(Bus) Then
                If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
            End If
            If e.KeyCode = RearDoorKey2 AndAlso Game.Player.Character.IsInVehicle(Bus) Then
                If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False) Else Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
            End If
            If e.KeyCode = LeftBlinkerKey2 Then LeftBlinker = Not LeftBlinker
            If e.KeyCode = RightBlinkerKey2 Then RightBlinker = Not RightBlinker
        End If
    End Sub
End Class
