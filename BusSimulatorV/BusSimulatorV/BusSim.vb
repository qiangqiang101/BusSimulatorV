Imports System.Drawing
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
    Public Shared Bus As Vehicle
    Public IsInGame As Boolean = False
    Public PlayerOriginalPosition, PlayerOriginalRotation As Vector3
    Public EarnedTimerBar, SpeedTimerBar, NextStationTimerBar, StationTimerBar As TextTimerBar
    Public CurrentStationIndex As Integer = 0
    Public BlipDict As New Dictionary(Of Integer, Blip)
    Public BlipList As New List(Of Blip)
    Public ModBlip As Blip
    Public CameraPos, CameraRot As Vector3
    Public MenuCamera As Camera
    Public MenuActivator As New Vector3(436.4384, -625.9146, 28.70776)
    Dim Speedometer As SpeedMeasurement
    Dim DebugMode As Boolean
    Public LeftBlinker, RightBlinker As Boolean
    Public AutoDoors, AutoBlinkers, Brakelights As Boolean
    Public PedRelationshipGroup As Integer
    Public Shared PassengerPedGroup As New List(Of Ped)
    Public Shared LeavedPassengerPedGroup As New List(Of Ped)
    Public Shared LastStationPassengerPedGroup As New List(Of Ped)
    Dim door, door1, door2, door3 As Boolean
    Public TTS As Boolean
    Dim SAPI As Object

    Public Sub LoadSettings()
        Try
            Speedometer = config.GetValue(Of SpeedMeasurement)("GENERAL", "Speedometer", SpeedMeasurement.KPH)
            DebugMode = config.GetValue(Of Boolean)("GENERAL", "DebugMode", False)
            AutoDoors = config.GetValue(Of Boolean)("GENERAL", "AutoDoors", True)
            AutoBlinkers = config.GetValue(Of Boolean)("GENERAL", "AutoBlinkers", True)
            Brakelights = config.GetValue(Of Boolean)("GENERAL", "Brakelights", True)
            TTS = config.GetValue(Of Boolean)("GENERAL", "TTS", False)
        Catch ex As Exception
            Logger.Log(String.Format("(LoadSettings): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub SaveSettings()
        Try
            If Not File.Exists("scripts\BusSimulatorV\config.ini") Then
                config.SetValue(Of SpeedMeasurement)("GENERAL", "Speedometer", SpeedMeasurement.KPH)
                config.SetValue(Of Boolean)("GENERAL", "DebugMode", False)
                config.SetValue(Of Boolean)("GENERAL", "AutoDoors", True)
                config.SetValue(Of Boolean)("GENERAL", "AutoBlinkers", True)
                config.SetValue(Of Boolean)("GENERAL", "Brakelights", True)
                config.SetValue(Of Boolean)("GENERAL", "TTS", False)
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

        If TTS Then
            SAPI = CreateObject("SAPI.spvoice")
            SAPI.Volume = 50
        End If
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
            RouteMenu.AddInstructionalButton(New InstructionalButton(GTA.Control.Reload, "Refresh"))
            _menuPool.Add(RouteMenu)

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    Dim br As BusRoute = New BusRoute(xmlFile)
                    br = br.ReadFromFile
                    itemMRoute = New UIMenuItem(Path.GetFileNameWithoutExtension(xmlFile))
                    With itemMRoute
                        .SubString1 = xmlFile
                        .Description = $"Author: {br.Author}~n~Version: {br.Version}~n~Description: {br.Description}                                                                                                                                                                                                                                    "
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

    Public Sub RefreshRouteMenu()
        Try
            RouteMenu.MenuItems.Clear()

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    Dim br As BusRoute = New BusRoute(xmlFile)
                    br = br.ReadFromFile
                    itemMRoute = New UIMenuItem(Path.GetFileNameWithoutExtension(xmlFile))
                    With itemMRoute
                        .SubString1 = xmlFile
                        .Description = $"Author: {br.Author}~n~Version: {br.Version}~n~Description: {br.Description}                                                                                                                                                                                                                                    "
                    End With
                    RouteMenu.AddItem(itemMRoute)
                End If
            Next
            RouteMenu.RefreshIndex()
            MainMenu.BindMenuToItem(RouteMenu, itemRoute)
        Catch ex As Exception
            Logger.Log(String.Format("(RefreshRouteMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub RouteMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles RouteMenu.OnItemSelect
        Try
            Dim br As BusRoute = New BusRoute(selectedItem.SubString1)
            br = br.ReadFromFile
            CurrentRoute = br

            itemPlay.Enabled = True
            itemRoute.SetRightLabel(selectedItem.Text)
            For Each item As UIMenuItem In sender.MenuItems
                item.SetRightBadge(UIMenuItem.BadgeStyle.None)
            Next
            selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick)
            sender.GoBack()
        Catch ex As Exception
            Logger.Log(String.Format("(RouteMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub BusSim_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        'Try
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

        If RouteMenu.Visible Then
            If Game.IsControlJustReleased(0, GTA.Control.Reload) Then
                RefreshRouteMenu()
            End If
        End If

        If IsInGame Then
            If Game.Player.Character.IsInVehicle(Bus) Then
                DrawMarker(CurrentRoute.Stations(CurrentStationIndex).StationCoords)
                If IsGameUIVisible() Then UpdateTimerBars() : DrawDebugObjects()
            End If

            If AutoDoors Then
                For Each passenger As Ped In LeavedPassengerPedGroup
                    door1 = passenger.IsInVehicle(Bus)
                Next
                For Each passenger As Ped In LastStationPassengerPedGroup
                    door3 = passenger.IsInVehicle(Bus)
                Next
                For Each passenger As Ped In PassengerPedGroup
                    door2 = Not passenger.IsInVehicle(Bus)
                Next

                If Not door1 AndAlso Not door2 AndAlso Not door3 Then               'no no no
                    door = False
                ElseIf Not door1 AndAlso Not door2 AndAlso door3 Then               'no no yes
                    door = True
                ElseIf Not door1 AndAlso door2 AndAlso door3 Then                   'no yes yes
                    door = True
                ElseIf door1 AndAlso door2 AndAlso door3 Then                       'yes yes yes
                    door = True
                ElseIf Not door1 AndAlso door2 AndAlso Not door3 Then               'no yes no
                    door = True
                ElseIf door1 AndAlso door2 AndAlso Not door3 Then                   'yes yes no
                    door = True
                ElseIf door1 AndAlso Not door2 AndAlso door3 Then                   'yes no yes
                    door = True
                ElseIf door1 AndAlso Not door2 AndAlso Not door3 Then               'yes no no
                    door = True
                End If

                If door Then
                    If Not Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                    If Not Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                    If Not Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                    If Not Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                Else
                    If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False)
                End If
            End If

            If Brakelights Then
                If Bus.SpeedMPH = 0 Then Bus.BrakeLightsOn = True Else Bus.BrakeLightsOn = False
            End If

            If AutoBlinkers Then
                If Bus.IsAnyDoorOpen Then
                    Bus.LeftIndicatorLightOn = True
                    Bus.RightIndicatorLightOn = True
                Else
                    Select Case GenerateDirectionsToCoord(BlipDict.Item(CurrentStationIndex).Position)
                        Case Directions.GoStraight, Directions.JoinTheFreeway, Directions.KeepLeft, Directions.KeepRight, Directions.ProceedTheHighlightingRoute, Directions.YouHaveArrive
                            Bus.LeftIndicatorLightOn = False
                            Bus.RightIndicatorLightOn = False
                        Case Directions.TurnLeft, Directions.RecalculatingRoute
                            Bus.LeftIndicatorLightOn = True
                        Case Directions.TurnRight, Directions.ExitFreeway
                            Bus.RightIndicatorLightOn = True
                    End Select
                End If
            End If

            Try
                For Each ped As Ped In Bus.Passengers
                    If Not LeavedPassengerPedGroup.Contains(ped) Then
                        ped.StopPedFlee
                        ped.Task.LookAt(Game.Player.Character)
                        ped.RelationshipGroup = PedRelationshipGroup
                        ped.CurrentBlip.Alpha = 0
                    End If
                Next
            Catch ex As Exception
                Logger.Log(String.Format("(Stop Ped Flee): {0} {1}", ex.Message, ex.StackTrace))
            End Try

            If Bus.Position.DistanceTo(Game.Player.Character.Position) >= 50.0F Then 'Abandoned mission
                Try
                    For Each blip As Blip In BlipList
                        blip.Remove()
                    Next
                    CurrentStationIndex = 0
                    For Each ped As Ped In PassengerPedGroup
                        LeavedPassengerPedGroup.Add(ped)
                        ped.Task.ClearAll()
                        PassengerPedGroup.Remove(ped)
                    Next
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()

                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = "Start Mission"
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.GenericFailed)
                    Effects.Start(ScreenEffect.DeathFailMpDark, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard("Mission Failed", $"You abandoned your {Bus.FriendlyName}.", HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_RED, 5000)
                Catch ex As Exception
                    Logger.Log(String.Format("(Abandoned Bus): {0} {1}", ex.Message, ex.StackTrace))
                End Try
            End If

            If Bus.IsDead Then 'Destroyed bus
                Try
                    For Each blip As Blip In BlipList
                        blip.Remove()
                    Next
                    CurrentStationIndex = 0
                    For Each ped As Ped In PassengerPedGroup
                        LeavedPassengerPedGroup.Add(ped)
                        ped.Task.ClearAll()
                        PassengerPedGroup.Remove(ped)
                    Next
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()

                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = "Start Mission"
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.GenericFailed)
                    Effects.Start(ScreenEffect.DeathFailMpDark, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard("Mission Failed", $"You blowed up your {Bus.FriendlyName}.", HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_RED, 5000)
                Catch ex As Exception
                    Logger.Log(String.Format("(Destroyed Bus): {0} {1}", ex.Message, ex.StackTrace))
                End Try
            End If

            If Bus.Position.DistanceTo(CurrentRoute.Stations(CurrentStationIndex).StationCoords) <= 20.0F Then
                Dim b As Blip = BlipDict.Item(CurrentStationIndex)
                BlipDict.Remove(CurrentStationIndex)
                b.Remove()

                If Not CurrentStationIndex = CurrentRoute.TotalStation Then CurrentStationIndex += 1

                If (CurrentStationIndex = CurrentRoute.TotalStation) Then
                    If TTS Then SAPI.speak($"Station {CurrentRoute.Stations(CurrentStationIndex).StationName}", 1)

                    For Each ped As Ped In Bus.Passengers
                        If Not ped = Game.Player.Character Then
                            LastStationPassengerPedGroup.Add(ped)
                        End If
                    Next
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()

                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    CurrentStationIndex = 0
                    Game.Player.Character.Money += Earned
                    Earned = 0
                    itemPlay.Text = "Start Mission"
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.FranklinBig01)
                    Effects.Start(ScreenEffect.SuccessNeutral, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard("Mission Passed", $"You completed {CurrentRoute.RouteName}.", HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_GOLD, 5000)

                    Bus.RemoveAllExtras
                    Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                Else
                    Dim random As New Random()
                    Dim leaveCount As Integer = random.Next(0, 3)

                    If PassengerPedGroup.Count <> 0 AndAlso leaveCount <> 0 Then
                        SoundPlayer("scripts\BusSimulatorV\Sound\bell.wav", 70)
                    End If

                    b = BlipDict.Item(CurrentStationIndex)
                    b.ShowRoute = True

                    If Not Bus.IsSeatFree(VehicleSeat.Passenger) Then
                        Dim ped As Ped = Bus.GetPedOnSeat(VehicleSeat.Passenger)
                        LeavedPassengerPedGroup.Add(ped)
                        ped.Task.ClearAll()
                        PassengerPedGroup.Remove(ped)
                    End If
                    If Not PassengerPedGroup.Count = 0 AndAlso leaveCount >= 1 Then
                        If Not Bus.Position.DistanceTo(CurrentRoute.Stations(0).StationCoords) <= 5.0F Then
                            Dim ped As Ped = PassengerPedGroup(random.Next(0, PassengerPedGroup.Count))
                            LeavedPassengerPedGroup.Add(ped)
                            ped.Task.ClearAll()
                            PassengerPedGroup.Remove(ped)
                        End If
                    End If
                    If Not PassengerPedGroup.Count = 0 AndAlso leaveCount >= 2 Then
                        If Not Bus.Position.DistanceTo(CurrentRoute.Stations(0).StationCoords) <= 5.0F Then
                            Dim ped As Ped = PassengerPedGroup(random.Next(0, PassengerPedGroup.Count))
                            LeavedPassengerPedGroup.Add(ped)
                            ped.Task.ClearAll()
                            PassengerPedGroup.Remove(ped)
                        End If
                    End If
                    If Not PassengerPedGroup.Count >= 15 Then
                        Dim pedCount As Integer = 0, maxPed As Integer = 3
                        For Each ped As Ped In World.GetNearbyPeds(CurrentRoute.Stations(CurrentStationIndex - 1).StationCoords, 20.0F)
                            If pedCount < maxPed AndAlso Not ped = Game.Player.Character AndAlso Not ped.IsInVehicle() Then
                                If Not PassengerPedGroup.Contains(ped) Then
                                    ped.StopPedFlee
                                    ped.RelationshipGroup = PedRelationshipGroup
                                    ped.Task.ClearAll()
                                    ped.AlwaysKeepTask = True
                                    ped.Seat((Bus.GetEmptySeat - 1) + pedCount)

                                    Dim pedblip As Blip = ped.AddBlip
                                    With pedblip
                                        .Sprite = BlipSprite.Friend
                                        .Color = BlipColor.Blue
                                        .IsFriendly = True
                                        .Name = "Passenger"
                                    End With
                                    PassengerPedGroup.Add(ped)
                                    pedCount += 1
                                    Earned += CurrentRoute.RouteFare
                                End If
                            End If
                        Next
                    End If
                    If TTS Then SAPI.speak($"Station {CurrentRoute.Stations(CurrentStationIndex - 1).StationName}, Next Station, {CurrentRoute.Stations(CurrentStationIndex).StationName}", 1)
                End If
            End If
        Else
                DrawMarker(MenuActivator, New Vector3(1.0F, 1.0F, 1.0F), Color.CadetBlue)
        End If

        If Game.Player.Character.Position.DistanceTo(MenuActivator) <= 2.0 AndAlso Not Game.Player.Character.IsInVehicle Then
            If Not _menuPool.IsAnyMenuOpen Then
                DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to work as Bus Driver.")
            End If
            If Game.IsControlJustReleased(0, GTA.Control.Context) Then
                MainMenu.Show()
                MenuCamera = World.CreateCamera(CameraPos, CameraRot, GameplayCamera.FieldOfView)
                World.RenderingCamera = MenuCamera
            End If
        End If
        'Catch ex As Exception
        '    Logger.Log(String.Format("(BusSim_Tick): {0} {1}", ex.Message, ex.StackTrace))
        'End Try
    End Sub

    Private Sub BusSim_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        On Error Resume Next
        If Not Bus = Nothing Then Bus.Delete()
        For Each blip As Blip In BlipList
            blip.Remove()
        Next
        ModBlip.Remove()
        If Not PassengerPedGroup.Count = 0 Then
            For Each ped As Ped In PassengerPedGroup
                ped.CurrentBlip.Remove()
                ped.RelationshipGroup = 0
                PassengerPedGroup.Remove(ped)
            Next
        End If
        If Not LeavedPassengerPedGroup.Count = 0 Then
            For Each ped As Ped In LeavedPassengerPedGroup
                ped.CurrentBlip.Remove()
                ped.RelationshipGroup = 0
                LeavedPassengerPedGroup.Remove(ped)
            Next
        End If
        PassengerPedGroup.Clear()
        LeavedPassengerPedGroup.Clear()
    End Sub

    Private Sub MainMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles MainMenu.OnItemSelect
        Try
            If selectedItem.Text = "Start Mission" Then
                sender.Hide()
                World.RenderingCamera = Nothing
                World.DestroyAllCameras()

                If Not Bus = Nothing Then Bus.Delete()
                Bus = World.CreateVehicle(CurrentRoute.BusModel, CurrentRoute.BusSpawnPoint, CurrentRoute.BusHeading)
                With Bus
                    .IsPersistent = True
                    .RemoveAllExtras()
                    If .ExtraExists(CurrentRoute.TurnOnExtra) Then .ToggleExtra(CurrentRoute.TurnOnExtra, True)
                    .PlaceOnGround()
                    .Repair()
                End With

                PlayerOriginalPosition = Game.Player.Character.Position
                PlayerOriginalRotation = Game.Player.Character.Rotation
                Game.Player.Character.Position = CurrentRoute.PlayerSpawnPoint
                Game.Player.Character.Task.WarpIntoVehicle(Bus, VehicleSeat.Driver)

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
                PedRelationshipGroup = World.AddRelationshipGroup("BusPassengerGroup")
                World.SetRelationshipBetweenGroups(Relationship.Companion, PedRelationshipGroup, Game.Player.Character.RelationshipGroup)
                World.SetRelationshipBetweenGroups(Relationship.Companion, Game.Player.Character.RelationshipGroup, PedRelationshipGroup)
                'Game.Player.Character.SetAsLeader()
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
            StationTimerBar = New TextTimerBar("STATION", $"{CurrentStationIndex}/{CurrentRoute.TotalStation}")
            _timerPool.Add(EarnedTimerBar)
            _timerPool.Add(SpeedTimerBar)
            _timerPool.Add(StationTimerBar)
            _timerPool.Add(NextStationTimerBar)
            _timerPool.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawtimerBars): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub DrawMarker(position As Vector3, Optional size As Vector3 = Nothing, Optional color As Color = Nothing)
        Dim p As New Vector3(position.X, position.Y, position.Z - 1.0F)
        If size = Nothing Then size = New Vector3(2.0F, 2.0F, 3.0F)
        If color = Nothing Then color = Color.Yellow
        World.DrawMarker(MarkerType.VerticalCylinder, p, p, Vector3.Zero, size, color)
    End Sub

    Public Sub UpdateTimerBars()
        Try
            _timerPool.Remove(EarnedTimerBar)
            _timerPool.Remove(SpeedTimerBar)
            _timerPool.Remove(NextStationTimerBar)
            _timerPool.Remove(StationTimerBar)
            EarnedTimerBar = New TextTimerBar("EARNED", "$" & Earned.ToString("0"))
            Select Case Speedometer
                Case SpeedMeasurement.KPH
                    SpeedTimerBar = New TextTimerBar("SPEED KPH", Bus.SpeedKPH.ToString("0"))
                Case Else
                    SpeedTimerBar = New TextTimerBar("SPEED MPH", Bus.SpeedMPH.ToString("0"))
            End Select
            NextStationTimerBar = New TextTimerBar("NEXT STATION", CurrentRoute.Stations(CurrentStationIndex).StationName)
            StationTimerBar = New TextTimerBar("STATION", $"{CurrentStationIndex}/{CurrentRoute.TotalStation}")
            _timerPool.Add(EarnedTimerBar)
            _timerPool.Add(SpeedTimerBar)
            _timerPool.Add(StationTimerBar)
            _timerPool.Add(NextStationTimerBar)
            _timerPool.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(UpdateTimerBars): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub DrawDebugObjects()
        If DebugMode Then
            DrawSeat()
            DrawDoor()
            DrawCoords()
        End If
    End Sub

    Public Sub DrawSeat()
        Try
            Dim SRMR As SizeF = UIMenu.GetScreenResolutionMaintainRatio
            Dim SZB As Point = UIMenu.GetSafezoneBounds
            Dim X As Integer = 500
            Dim Y As Integer = 190
            Dim BusLayout As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(42, 190), Color.FromArgb(200, 0, 0, 0)) : BusLayout.Draw()
            Dim BusNum As New UIResRectangle(New Point(BusLayout.Position.X + 1, BusLayout.Position.Y + 1), New Size(40, 25), Color.FromArgb(200, Color.Orange)) : BusNum.Draw()
            Dim busText As New UIResText(CurrentRoute.RouteNumber, New Point(BusNum.Position.X + (BusNum.Size.Width / 2), BusNum.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : busText.Draw()

            Dim SD As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.Driver), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SD.Draw()
            Dim SP As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.Passenger), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SP.Draw()
            Dim SLR As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.LeftRear), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SLR.Draw()
            Dim SRR As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.RightRear), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SRR.Draw()
            Dim SE1 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat1), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE1.Draw()
            Dim SE2 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat2), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE2.Draw()
            Dim SE3 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat3), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE3.Draw()
            Dim SE4 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat4), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE4.Draw()
            Dim SE5 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat5), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE5.Draw()
            Dim SE6 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat6), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE6.Draw()
            Dim SE7 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat7), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE7.Draw()
            Dim SE8 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat8), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE8.Draw()
            Dim SE9 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat9), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE9.Draw()
            Dim SE10 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat10), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE10.Draw()
            Dim SE11 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat11), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE11.Draw()
            Dim SE12 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat12), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE12.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawSeat): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub DrawDoor()
        Try
            Dim SRMR As SizeF = UIMenu.GetScreenResolutionMaintainRatio
            Dim SZB As Point = UIMenu.GetSafezoneBounds
            Dim X As Integer = 544
            Dim Y As Integer = 190
            Dim BusLayout As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(42, 190), Color.FromArgb(200, 0, 0, 0)) : BusLayout.Draw()
            Dim BusNum As New UIResRectangle(New Point(BusLayout.Position.X + 1, BusLayout.Position.Y + 1), New Size(40, 25), Color.FromArgb(200, Color.Orange)) : BusNum.Draw()
            Dim busText As New UIResText("Door", New Point(BusNum.Position.X + (BusNum.Size.Width / 2), BusNum.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : busText.Draw()

            Dim SD As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(door1, Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SD.Draw()
            Dim LeaveGroup As New UIResText("LV", New Point(SD.Position.X + (SD.Size.Width / 2), SD.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : LeaveGroup.Draw()
            Dim SP As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(door2, Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SP.Draw()
            Dim EnterGroup As New UIResText("EV", New Point(SP.Position.X + (SP.Size.Width / 2), SP.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : EnterGroup.Draw()
            Dim SLR As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(door3, Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SLR.Draw()
            Dim AllLeaveGroup As New UIResText("AL", New Point(SLR.Position.X + (SLR.Size.Width / 2), SLR.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : AllLeaveGroup.Draw()
            Dim SRR As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.RightRear), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SRR.Draw()
            Dim DoorGroup As New UIResText("DR", New Point(SRR.Position.X + (SRR.Size.Width / 2), SRR.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : DoorGroup.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawSeat): {0} {1}", ex.Message, ex.StackTrace))
        End Try
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

            Dim X As Integer = 588
            Dim Y As Integer = 190
            Dim BoxLayout1 As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(400, 190), Color.FromArgb(200, 0, 0, 0)) : BoxLayout1.Draw()
            Dim BoxTitle1 As New UIResRectangle(New Point(BoxLayout1.Position.X + 1, BoxLayout1.Position.Y + 1), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle1.Draw()
            Dim TitleText1 As New UIResText("Route Info", New Point(BoxTitle1.Position.X + (BoxTitle1.Size.Width / 2), BoxTitle1.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText1.Outline = True : TitleText1.Draw()
            Dim Line1 As New UIResText($"Route Name: {CurrentRoute.RouteName}", New Point(BoxTitle1.Position.X + 10, BoxTitle1.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line1.Outline = True : Line1.Draw()
            Dim Line2 As New UIResText($"Bus Model: {CurrentRoute.BusModel}", New Point(BoxTitle1.Position.X + 10, BoxTitle1.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line2.Outline = True : Line2.Draw()
            Dim BoxTitle2 As New UIResRectangle(New Point(BoxLayout1.Position.X + 1, BoxLayout1.Position.Y + 81), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle2.Draw()
            Dim TitleText2 As New UIResText("World Info", New Point(BoxTitle2.Position.X + (BoxTitle2.Size.Width / 2), BoxTitle2.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText2.Outline = True : TitleText2.Draw()
            Dim Line7 As New UIResText($"Street Name: {World.GetStreetName(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line7.Outline = True : Line7.Draw()
            Dim Line8 As New UIResText($"Zone Name: {World.GetZoneName(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line8.Outline = True : Line8.Draw()
            Dim Line9 As New UIResText($"Zone Label: {World.GetZoneNameLabel(pv3)}", New Point(BoxTitle2.Position.X + 10, BoxTitle2.Position.Y + 70), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line9.Outline = True : Line9.Draw()

            X = 990
            Dim BoxLayout2 As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(400, 190), Color.FromArgb(200, 0, 0, 0)) : BoxLayout2.Draw()
            Dim BoxTitle3 As New UIResRectangle(New Point(BoxLayout2.Position.X + 1, BoxLayout2.Position.Y + 1), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle3.Draw()
            Dim TitleText3 As New UIResText("Position Coordinates", New Point(BoxTitle3.Position.X + (BoxTitle3.Size.Width / 2), BoxTitle3.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText3.Outline = True : TitleText3.Draw()
            Dim Line3 As New UIResText($"Bus X: {pv3.X.ToString("0.0000")} Y: {pv3.Y.ToString("0.0000")} Z: {pv3.Z.ToString("0.0000")} H: {Game.Player.Character.Heading.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line3.Outline = True : Line3.Draw()
            Dim Line5 As New UIResText($"Camera X: {cv3.X.ToString("0.0000")} Y: {cv3.Y.ToString("0.0000")} Z: {cv3.Z.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line5.Outline = True : Line5.Draw()
            Dim Line6 As New UIResText($"Crosshair X: {av3.X.ToString("0.0000")} Y: {av3.Y.ToString("0.0000")} Z: {av3.Z.ToString("0.0000")}", New Point(BoxTitle3.Position.X + 10, BoxTitle3.Position.Y + 70), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line6.Outline = True : Line6.Draw()
            Dim BoxTitle4 As New UIResRectangle(New Point(BoxLayout2.Position.X + 1, BoxLayout2.Position.Y + 101), New Size(398, 25), Color.FromArgb(200, Color.Orange)) : BoxTitle4.Draw()
            Dim TitleText4 As New UIResText("Rotation Coordinates", New Point(BoxTitle4.Position.X + (BoxTitle4.Size.Width / 2), BoxTitle4.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : TitleText4.Outline = True : TitleText4.Draw()
            Dim Line10 As New UIResText($"Bus X: {pr3.X.ToString("0.0000")} Y: {pr3.Y.ToString("0.0000")} Z: {pr3.Z.ToString("0.0000")}", New Point(BoxTitle4.Position.X + 10, BoxTitle4.Position.Y + 30), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line10.Outline = True : Line10.Draw()
            Dim Line12 As New UIResText($"Camera X: {cr3.X.ToString("0.0000")} Y: {cr3.Y.ToString("0.0000")} Z: {cr3.Z.ToString("0.0000")}", New Point(BoxTitle4.Position.X + 10, BoxTitle4.Position.Y + 50), 0.25F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Left) : Line12.Outline = True : Line12.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawCoords): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub RemoveTimerBars()
        _timerPool.Remove(EarnedTimerBar)
        _timerPool.Remove(SpeedTimerBar)
        _timerPool.Remove(StationTimerBar)
        _timerPool.Remove(NextStationTimerBar)
    End Sub

    Private Sub MainMenu_OnMenuClose(sender As UIMenu) Handles MainMenu.OnMenuClose
        World.RenderingCamera = Nothing
        World.DestroyAllCameras()
    End Sub

    Private Sub BusSim_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp

    End Sub
End Class
