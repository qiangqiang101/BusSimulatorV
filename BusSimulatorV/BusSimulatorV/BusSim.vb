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
    Public FrontDoorKey, RearDoorKey, LeftBlinkerKey, RightBlinkerKey As GTA.Control
    Public FrontDoorKey2, RearDoorKey2, LeftBlinkerKey2, RightBlinkerKey2 As Keys
    Dim Speedometer As SpeedMeasurement
    Public LeftBlinker, RightBlinker As Boolean
    Public PedRelationshipGroup As Integer
    Public Shared PassengerPedGroup As New List(Of Ped)
    Public Shared LeavedPassengerPedGroup As New List(Of Ped)
    Public Shared LastStationPassengerPedGroup As New List(Of Ped)

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

        If IsInGame Then
            If Game.Player.Character.IsInVehicle(Bus) Then
                DrawMarker(CurrentRoute.Stations(CurrentStationIndex).StationCoords)
                If IsGameUIVisible() Then UpdateTimerBars() : DrawSeat() ': UI.ShowSubtitle(Bus.GetEmptySeatString)
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
                    'If Not LeavedPassengerPedGroup.Count = 0 Then
                    '    For Each ped As Ped In LeavedPassengerPedGroup
                    '        ped.CurrentBlip.Remove()
                    '        Select Case ped.SeatIndex
                    '            Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                    '                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                    '        End Select
                    '        ped.Task.LeaveVehicle(Bus, LeaveVehicleFlags.LeaveDoorOpen)
                    '        ped.RelationshipGroup = 0
                    '    Next
                    'End If
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()

                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = "Start Mission"
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    BigMessageThread.MessageInstance.ShowMissionPassedMessage("Mission Failed")
                    UI.ShowSubtitle("You abandoned your bus.")
                Catch ex As Exception
                    Logger.Log(String.Format("(Abandoned Bus): {0} {1}", ex.Message, ex.StackTrace))
                End Try
            End If

            If Bus.Position.DistanceTo(CurrentRoute.Stations(CurrentStationIndex).StationCoords) <= 7.5F Then
                Dim b As Blip = BlipDict.Item(CurrentStationIndex)
                BlipDict.Remove(CurrentStationIndex)
                b.Remove()
                If Not CurrentStationIndex = CurrentRoute.TotalStation Then CurrentStationIndex += 1

                If CurrentStationIndex = CurrentRoute.TotalStation Then
                    For Each ped As Ped In Bus.Passengers
                        If Not ped = Game.Player.Character Then
                            'ped.CurrentBlip.Remove()
                            'ped.RelationshipGroup = 0
                            'ped.Task.ClearAll()
                            'Select Case ped.SeatIndex
                            '    Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                            '        ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                            'End Select
                            'ped.Task.LeaveVehicle(Bus, LeaveVehicleFlags.LeaveDoorOpen)
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
                    BigMessageThread.MessageInstance.ShowMissionPassedMessage("Mission Passed")
                Else
                    b = BlipDict.Item(CurrentStationIndex)
                    b.ShowRoute = True
                    If Not PassengerPedGroup.Count = 0 Then
                        If Not Bus.Position.DistanceTo(CurrentRoute.Stations(0).StationCoords) <= 5.0F Then
                            Dim ped As Ped = PassengerPedGroup(New Random().Next(0, PassengerPedGroup.Count))
                            LeavedPassengerPedGroup.Add(ped)
                            ped.Task.ClearAll()
                            PassengerPedGroup.Remove(ped)
                        End If
                        'If Not LeavedPassengerPedGroup.Count = 0 Then
                        '    For Each ped As Ped In LeavedPassengerPedGroup
                        '        ped.CurrentBlip.Remove()
                        '        Select Case ped.SeatIndex
                        '            Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                        '                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                        '        End Select
                        '        ped.Task.LeaveVehicle(Bus, LeaveVehicleFlags.LeaveDoorOpen)
                        '        ped.RelationshipGroup = 0
                        '    Next
                        'End If
                    End If
                    If Not PassengerPedGroup.Count >= 15 Then
                        Dim pedCount As Integer = 0, maxPed As Integer = 3
                        For Each ped As Ped In World.GetNearbyPeds(Game.Player.Character, 15.0F)
                            If pedCount < maxPed AndAlso Not ped = Game.Player.Character AndAlso Not ped.IsInVehicle() Then
                                If Not PassengerPedGroup.Contains(ped) Then
                                    ped.StopPedFlee
                                    ped.RelationshipGroup = PedRelationshipGroup
                                    ped.Task.ClearAll()

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
        Else
            DrawMarker(MenuActivator, New Vector3(1.0F, 1.0F, 1.0F), Color.CadetBlue)
        End If

        If Game.Player.Character.Position.DistanceTo(MenuActivator) <= 2.0 AndAlso Not Game.Player.Character.IsInVehicle Then
            If Game.IsControlJustReleased(0, GTA.Control.Context) Then
                MainMenu.Show()
                MenuCamera = World.CreateCamera(CameraPos, CameraRot, GameplayCamera.FieldOfView)
                World.RenderingCamera = MenuCamera
            Else
                DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to work as Bus Driver.")
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

    Public Sub DrawSeat()
        Try
            Dim SRMR As SizeF = UIMenu.GetScreenResolutionMaintainRatio
            Dim SZB As Point = UIMenu.GetSafezoneBounds
            Dim X As Integer = 500
            Dim Y As Integer = 200
            Dim BusLayout As New UIResRectangle(New Point(((X - SZB.X) - 1), (((Convert.ToInt32(SRMR.Height) - SZB.Y) - Y) - 4)), New Size(42, 190), Color.FromArgb(200, 0, 0, 0)) : BusLayout.Draw()
            Dim BusNum As New UIResRectangle(New Point(BusLayout.Position.X + 1, BusLayout.Position.Y + 1), New Size(40, 25), Color.Orange) : BusNum.Draw()
            Dim busText As New UIResText(CurrentRoute.RouteNumber, New Point(BusNum.Position.X + (BusNum.Size.Width / 2), BusNum.Position.Y), 0.3F, Color.White, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : busText.Draw()

            Dim SD As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.Driver), Color.LightGreen, Color.White)) : SD.Draw()
            Dim SP As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.Passenger), Color.LightGreen, Color.White)) : SP.Draw()
            Dim SLR As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.LeftRear), Color.LightGreen, Color.White)) : SLR.Draw()
            Dim SRR As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.RightRear), Color.LightGreen, Color.White)) : SRR.Draw()
            Dim SE1 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat1), Color.LightGreen, Color.White)) : SE1.Draw()
            Dim SE2 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat2), Color.LightGreen, Color.White)) : SE2.Draw()
            Dim SE3 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat3), Color.LightGreen, Color.White)) : SE3.Draw()
            Dim SE4 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat4), Color.LightGreen, Color.White)) : SE4.Draw()
            Dim SE5 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat5), Color.LightGreen, Color.White)) : SE5.Draw()
            Dim SE6 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat6), Color.LightGreen, Color.White)) : SE6.Draw()
            Dim SE7 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat7), Color.LightGreen, Color.White)) : SE7.Draw()
            Dim SE8 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat8), Color.LightGreen, Color.White)) : SE8.Draw()
            Dim SE9 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat9), Color.LightGreen, Color.White)) : SE9.Draw()
            Dim SE10 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat10), Color.LightGreen, Color.White)) : SE10.Draw()
            Dim SE11 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat11), Color.LightGreen, Color.White)) : SE11.Draw()
            Dim SE12 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat12), Color.LightGreen, Color.White)) : SE12.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(DrawSeat): {0} {1}", ex.Message, ex.StackTrace))
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
