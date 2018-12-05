Option Explicit On

Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI
Imports System.Windows.Forms
Imports System.Media

Public Class BusSim
    Inherits Script

    Public WithEvents MainMenu, RouteMenu, DifficultyMenu As UIMenu
    Public itemMRoute, itemPlay, itemRoute, itemDifficulty As UIMenuItem
    Public itemNormal, itemHard, itemVeryHard, itemExtremelyHard As UIMenuItem
    Public Shared CurrentRoute As BusRoute
    Public _menuPool As MenuPool
    Public _timerPool As TimerBarPool
    Public Rectangle = New UIResRectangle()
    Public config As ScriptSettings = ScriptSettings.Load("scripts\BusSimulatorV\config.ini")
    Public language As ScriptSettings = ScriptSettings.Load($"scripts\BusSimulatorV\Language\{Game.Language.ToString("f")}.ini")
    Public Shared Earned As Integer = 0
    Public Shared Bus, previewBus As Vehicle ', TextProp As Prop
    Public IsInGame As Boolean = False
    Public PlayerOriginalPosition, PlayerOriginalRotation As Vector3
    Public EarnedTimerBar, NextStationTimerBar As TextTimerBar, StationTimerBar, SpeedTimerBar As BarTimerBar
    Public Shared CurrentStationIndex As Integer = 0
    Public BlipDict As New Dictionary(Of Integer, Blip)
    Public BlipList As New List(Of Blip)
    Public ModBlip As Blip
    Public CameraPos, CameraRot, RouteCameraPos, RouteCameraRot As Vector3
    Public MenuCamera, StartCamera, RouteCamera As Camera
    Public MenuActivator As New Vector3(436.4384, -625.9146, 28.70776)
    Dim Speedometer As SpeedMeasurement
    Public Shared DebugMode As Boolean
    Public LeftBlinker, RightBlinker As Boolean
    Public AutoDoors, AutoBlinkers, Brakelights As Boolean
    Public PedRelationshipGroup As Integer
    Public Shared PassengerPedGroup As New List(Of Ped)
    Public Shared LeavedPassengerPedGroup As New List(Of Ped)
    Public Shared LastStationPassengerPedGroup As New List(Of Ped)
    Dim door, door1, door2, door3 As Boolean
    Public TTS As Boolean, TTSVoice, TTSVolume, BellVolume As Integer
    Dim SAPI As Object
    Public FrontLeftDoorJ, FrontRightDoorJ, RearLeftDoorJ, RearRightDoorJ, ModifierJ As GTA.Control
    Public FrontLeftDoorK, FrontRightDoorK, RearLeftDoorK, RearRightDoork As Keys
    Dim Difficult As Difficulty = Difficulty.Normal
    Dim random As New Random()
    Dim leaveCount As Integer = 0
    Dim bellSounded As Boolean = False
    Dim stopRequested As Boolean = False
    'Dim busScaleform As New Scaleform("ORGANISATION_NAME")

    'Translate Text
    Dim bus_depot, author, version, description, final_station, passenger, next_station, help_text
    'Translate Menu
    Dim MAIN_MENU, ROUTE_SELECTION, DIFFICULTY_LEVEL_SELECTION
    'Translate MenuItem
    Dim select_route, select_route_desc, difficulty_level, difficulty_level_desc, start_mission, start_mission_desc, normal, hard, very_hard, extremely_hard, normal_desc, hard_desc, very_hard_desc, extremely_hard_desc, stop_mission
    'Translate Button
    Dim refresh
    'Translate Big Message
    Dim mission_failed, bus_abandoned, bus_blew, mission_passed, mission_completed
    'Translate Timer Bar
    Dim EARN, SPEED_MPH, SPEED_KPH, NEXT_STATION_TB, STATION

    Public Sub LoadLanguage()
        Try
            Dim s As String = "TEXT"
            bus_depot = language.GetValue(Of String)(s, "BUS_DEPOT", "Bus Depot")
            author = language.GetValue(Of String)(s, "AUTHOR", "Author")
            version = language.GetValue(Of String)(s, "VERSION", "Version")
            description = language.GetValue(Of String)(s, "DESCRIPTION", "Description")
            final_station = language.GetValue(Of String)(s, "FINAL_STATION", "Final station {0}, Thank you for using Los Santos Transit Route {1}.")
            next_station = language.GetValue(Of String)(s, "NEXT_STATION", "Station {0}, Next Station, {1}.")
            passenger = language.GetValue(Of String)(s, "PASSENGER", "Passenger")
            help_text = language.GetValue(Of String)(s, "HELP_TEXT", "Press ~INPUT_CONTEXT~ to work as Bus Driver.")
            Dim m As String = "MENU"
            MAIN_MENU = language.GetValue(Of String)(m, "MAIN_MENU", "MAIN MENU")
            ROUTE_SELECTION = language.GetValue(Of String)(m, "ROUTE_SELECTION", "ROUTE SELECTION")
            DIFFICULTY_LEVEL_SELECTION = language.GetValue(Of String)(m, "DIFFICULTY_LEVEL_SELECTION", "DIFFICULTY LEVEL SELECTION")
            Dim mi As String = "MENUITEM"
            select_route = language.GetValue(Of String)(mi, "SELECT_ROUTE", "Select Route")
            select_route_desc = language.GetValue(Of String)(mi, "SELECT_ROUTE_DESC", "Select from {0} playable routes.")
            difficulty_level = language.GetValue(Of String)(mi, "DIFFICULTY_LEVEL", "Difficulty Level")
            difficulty_level_desc = language.GetValue(Of String)(mi, "DIFFICULTY_LEVEL_DESC", "Select how hard do you want to play this mission.")
            start_mission = language.GetValue(Of String)(mi, "START_MISSION", "Start Mission")
            stop_mission = language.GetValue(Of String)(mi, "STOP_MISSION", "Stop Mission")
            start_mission_desc = language.GetValue(Of String)(mi, "START_MISSION_DESC", "Play selected route.")
            normal = language.GetValue(Of String)(mi, "NORMAL", "Normal")
            normal_desc = language.GetValue(Of String)(mi, "NORMAL_DESC", "Normal, Show everything.")
            hard = language.GetValue(Of String)(mi, "HARD", "Hard")
            hard_desc = language.GetValue(Of String)(mi, "HARD_DESC", "Hard, 2x Fares, No Markers.")
            very_hard = language.GetValue(Of String)(mi, "VERY_HARD", "Professional")
            very_hard_desc = language.GetValue(Of String)(mi, "VERY_HARD_DESC", "Professional, 4x Fares, No Markers and Route Line.")
            extremely_hard = language.GetValue(Of String)(mi, "EXTREMELY_HARD", "(*TmT)")
            extremely_hard_desc = language.GetValue(Of String)(mi, "EXTREMELY_HARD_DESC", "Unbeatable, 8x Fare, No Markers, Route Line and Blips.")
            Dim b As String = "BUTTON"
            refresh = language.GetValue(Of String)(b, "REFRESH", "Refresh")
            Dim bm As String = "BIGMESSAGE"
            mission_failed = language.GetValue(Of String)(bm, "MISSION_FAILED", "Mission Failed")
            bus_abandoned = language.GetValue(Of String)(bm, "BUS_ABANDONED", "You abandoned your {0}.")
            bus_blew = language.GetValue(Of String)(bm, "BUS_BLEW", "You blew up your {0}.")
            mission_passed = language.GetValue(Of String)(bm, "MISSION_PASSED", "Mission Passed")
            mission_completed = language.GetValue(Of String)(bm, "MISSION_COMPLETED", "You completed {0}, Earned ${1}.")
            Dim t As String = "TIMERBAR"
            EARN = language.GetValue(Of String)(t, "EARN", "EARNED")
            SPEED_MPH = language.GetValue(Of String)(t, "SPEED_MPH", "SPEED MPH")
            SPEED_KPH = language.GetValue(Of String)(t, "SPEED_KPH", "SPEED KPH")
            NEXT_STATION_TB = language.GetValue(Of String)(t, "NEXT_STATION", "NEXT STATION")
            STATION = language.GetValue(Of String)(t, "STATIONS", "STATIONS")
        Catch ex As Exception
            Logger.Log(String.Format("(LoadLanguage): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub LoadSettings()
        Try
            Speedometer = config.GetValue(Of SpeedMeasurement)("GENERAL", "Speedometer", SpeedMeasurement.KPH)
            DebugMode = config.GetValue(Of Boolean)("GENERAL", "DebugMode", False)
            AutoDoors = config.GetValue(Of Boolean)("GENERAL", "AutoDoors", True)
            AutoBlinkers = config.GetValue(Of Boolean)("GENERAL", "AutoBlinkers", True)
            Brakelights = config.GetValue(Of Boolean)("GENERAL", "Brakelights", True)
            TTS = config.GetValue(Of Boolean)("GENERAL", "TTS", False)
            TTSVoice = config.GetValue(Of Integer)("GENERAL", "TTSVoice", 0)
            TTSVolume = config.GetValue(Of Integer)("GENERAL", "TTSVolume", 50)
            BellVolume = config.GetValue(Of Integer)("GENERAL", "BellVolume", 50)
            FrontLeftDoorJ = config.GetValue(Of GTA.Control)("JOYCONTROLS", "FrontLeftDoor", GTA.Control.ScriptPadLeft)
            FrontRightDoorJ = config.GetValue(Of GTA.Control)("JOYCONTROLS", "FrontRightDoor", GTA.Control.ScriptPadLeft)
            RearLeftDoorJ = config.GetValue(Of GTA.Control)("JOYCONTROLS", "RearLeftDoor", GTA.Control.ScriptPadRight)
            RearRightDoorJ = config.GetValue(Of GTA.Control)("JOYCONTROLS", "RearRightDoor", GTA.Control.ScriptPadRight)
            ModifierJ = config.GetValue(Of GTA.Control)("JOYCONTROLS", "Modifier", GTA.Control.ScriptSelect)
            FrontLeftDoorK = config.GetValue(Of Keys)("KBCONTROLS", "FrontLeftDoor", Keys.NumPad1)
            FrontRightDoorK = config.GetValue(Of Keys)("KBCONTROLS", "FrontRightDoor", Keys.NumPad1)
            RearLeftDoorK = config.GetValue(Of Keys)("KBCONTROLS", "RearLeftDoor", Keys.NumPad3)
            RearRightDoork = config.GetValue(Of Keys)("KBCONTROLS", "RearRightDoor", Keys.NumPad3)
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
                config.SetValue(Of Integer)("GENERAL", "TTSVoice", 0)
                config.SetValue(Of Integer)("GENERAL", "TTSVolume", 50)
                config.SetValue(Of Integer)("GENERAL", "BellVolume", 50)
                config.SetValue(Of GTA.Control)("JOYCONTROLS", "FrontLeftDoor", GTA.Control.ScriptPadLeft)
                config.SetValue(Of GTA.Control)("JOYCONTROLS", "FrontRightDoor", GTA.Control.ScriptPadLeft)
                config.SetValue(Of GTA.Control)("JOYCONTROLS", "RearLeftDoor", GTA.Control.ScriptPadRight)
                config.SetValue(Of GTA.Control)("JOYCONTROLS", "RearRightDoor", GTA.Control.ScriptPadRight)
                config.SetValue(Of GTA.Control)("JOYCONTROLS", "Modifier", GTA.Control.ScriptSelect)
                config.SetValue(Of GTA.Control)("KBCONTROLS", "FrontLeftDoor", Keys.NumPad1)
                config.SetValue(Of GTA.Control)("KBCONTROLS", "FrontRightDoor", Keys.NumPad1)
                config.SetValue(Of GTA.Control)("KBCONTROLS", "RearLeftDoor", Keys.NumPad3)
                config.SetValue(Of GTA.Control)("KBCONTROLS", "RearRightDoor", Keys.NumPad3)
                config.Save()
            End If
        Catch ex As Exception
            Logger.Log(String.Format("(SaveSettings): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub New()
        SaveSettings()
        LoadSettings()
        LoadLanguage()

        _menuPool = New MenuPool()
        _timerPool = New TimerBarPool()
        Rectangle.Color = Color.FromArgb(0, 0, 0, 0)

        CreateMainMenu()
        CreateRouteMenu()
        CreateDifficultyMenu()

        CameraPos = New Vector3(427.3593, -670.04, 29.90757)
        CameraRot = New Vector3(2.562092, 0, -16.7178)
        RouteCameraPos = New Vector3(425.8456, -655.0043, 28.51719)
        RouteCameraRot = New Vector3(10.6807, 0, 16.9965)

        ModBlip = World.CreateBlip(MenuActivator)
        With ModBlip
            .IsShortRange = True
            .Sprite = BlipSprite.Bus
            .Color = 54
            .Name = bus_depot
        End With

        If TTS Then
            SAPI = CreateObject("SAPI.spvoice")
            SAPI.Voice = SAPI.GetVoices.Item(TTSVoice)
            SAPI.Volume = TTSVolume
        End If
    End Sub

    Public Sub CreateMainMenu()
        Try
            MainMenu = New UIMenu("", MAIN_MENU, New Point(0, -107))
            MainMenu.SetBannerType(Rectangle)
            MainMenu.MouseEdgeEnabled = False
            _menuPool.Add(MainMenu)
            itemRoute = New UIMenuItem(select_route, String.Format(select_route_desc, Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml").Count)) : MainMenu.AddItem(itemRoute)
            itemDifficulty = New UIMenuItem(difficulty_level, difficulty_level_desc) : itemDifficulty.SetRightLabel(normal) : MainMenu.AddItem(itemDifficulty)
            itemPlay = New UIMenuItem(start_mission, start_mission_desc) With {.Enabled = False} : MainMenu.AddItem(itemPlay)
            If Not IsDLCInstalled() Then
                Dim itemWarning As New UIMenuColoredItem("Bus Simulator V DLC not detect!", "Bus Simulator V DLC isn't Install, did you forgot add <Item>dlcpacks:/bussim/</Item> in your dlclist.xml? The Bus Stop won't spawn and Bus LED Sign won't work without it.", Color.DarkRed, Color.PaleVioletRed)
                With itemWarning
                    .Enabled = False
                    .SetRightBadge(UIMenuItem.BadgeStyle.Alert)
                End With
                MainMenu.AddItem(itemWarning)
            End If
            MainMenu.RefreshIndex()
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateRouteMenu()
        Try
            RouteMenu = New UIMenu("", ROUTE_SELECTION, New Point(0, -107))
            RouteMenu.SetBannerType(Rectangle)
            RouteMenu.MouseEdgeEnabled = False
            RouteMenu.AddInstructionalButton(New InstructionalButton(GTA.Control.Reload, refresh))
            _menuPool.Add(RouteMenu)

            For Each xmlFile As String In Directory.GetFiles("scripts\BusSimulatorV\Route", "*.xml")
                If File.Exists(xmlFile) Then
                    Dim br As BusRoute = New BusRoute(xmlFile)
                    br = br.ReadFromFile
                    itemMRoute = New UIMenuItem(br.RouteName)
                    With itemMRoute
                        .SubString1 = xmlFile
                        .SubInteger1 = br.RouteNumber
                        .Description = $"{author}: {br.Author}~n~{version}: {br.Version}~n~{description}: {br.Description}                                                                                                                                                                                                                                    "
                    End With
                    RouteMenu.AddItem(itemMRoute)
                End If
            Next
            RouteMenu.RefreshIndex()
            RouteMenu.MenuItems = RouteMenu.MenuItems.OrderBy(Function(x) x.SubInteger1).ToList
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
                    itemMRoute = New UIMenuItem(br.RouteName)
                    With itemMRoute
                        .SubString1 = xmlFile
                        .SubInteger1 = br.RouteNumber
                        .Description = $"{author}: {br.Author}~n~{version}: {br.Version}~n~{description}: {br.Description}                                                                                                                                                                                                                                    "
                    End With
                    RouteMenu.AddItem(itemMRoute)
                End If
            Next
            RouteMenu.RefreshIndex()
            RouteMenu.MenuItems = RouteMenu.MenuItems.OrderBy(Function(x) x.SubInteger1).ToList
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
            itemRoute.SetRightLabel(selectedItem.SubInteger1)
            For Each item As UIMenuItem In sender.MenuItems
                item.SetRightBadge(UIMenuItem.BadgeStyle.None)
            Next
            selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick)
            sender.GoBack()
        Catch ex As Exception
            Logger.Log(String.Format("(RouteMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub CreateDifficultyMenu()
        Try
            DifficultyMenu = New UIMenu("", DIFFICULTY_LEVEL_SELECTION, New Point(0, -107))
            DifficultyMenu.SetBannerType(Rectangle)
            DifficultyMenu.MouseEdgeEnabled = False
            _menuPool.Add(DifficultyMenu)
            itemNormal = New UIMenuItem($"~g~{normal}", normal_desc) With {.SubString1 = normal, .SubInteger1 = Difficulty.Normal} : DifficultyMenu.AddItem(itemNormal)
            itemHard = New UIMenuItem($"~y~{hard}", hard_desc) With {.SubString1 = hard, .SubInteger1 = Difficulty.Hard} : DifficultyMenu.AddItem(itemHard)
            itemVeryHard = New UIMenuItem($"~o~{very_hard}", very_hard_desc) With {.SubString1 = very_hard, .SubInteger1 = Difficulty.VeryHard} : DifficultyMenu.AddItem(itemVeryHard)
            itemExtremelyHard = New UIMenuItem($"~r~{extremely_hard}", extremely_hard_desc) With {.SubString1 = extremely_hard, .SubInteger1 = Difficulty.ExtremelyHard} : DifficultyMenu.AddItem(itemExtremelyHard)
            DifficultyMenu.RefreshIndex()
            MainMenu.BindMenuToItem(DifficultyMenu, itemDifficulty)
        Catch ex As Exception
            Logger.Log(String.Format("(CreateMainMenu): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub DifficultyMenu_OnItemSelect(sender As UIMenu, selectedItem As UIMenuItem, index As Integer) Handles DifficultyMenu.OnItemSelect
        Try
            Difficult = selectedItem.SubInteger1

            itemDifficulty.SetRightLabel(selectedItem.SubString1)
            For Each item As UIMenuItem In sender.MenuItems
                item.SetRightBadge(UIMenuItem.BadgeStyle.None)
            Next
            selectedItem.SetRightBadge(UIMenuItem.BadgeStyle.Tick)
            sender.GoBack()
        Catch ex As Exception
            Logger.Log(String.Format("(DifficultyMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Sub BusSim_Tick(sender As Object, e As EventArgs) Handles Me.Tick
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
                itemRoute.SetRightLabel("")
            End If
        End If

        If IsInGame Then
            'busScaleform.DrawText(Bus)
            If Not Bus.IsSeatFree(VehicleSeat.Driver) Then
                If Not Bus.Driver = Game.Player.Character Then
                    Bus.Driver.Task.LeaveVehicle(Bus, LeaveVehicleFlags.BailOut)
                End If
            End If

            If Game.Player.Character.IsInVehicle(Bus) Then
                Select Case Difficult
                    Case Difficulty.Normal
                        DrawMarker(CurrentRoute.Stations(CurrentStationIndex).StationCoords)
                End Select

                If IsGameUIVisible() Then UpdateTimerBars() : DrawDebugObjects()

                If Not AutoDoors Then
                    If Game.IsControlJustReleased(0, ModifierJ) AndAlso Game.IsControlJustReleased(0, FrontLeftDoorJ) Then If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                    If Game.IsControlJustReleased(0, ModifierJ) AndAlso Game.IsControlJustReleased(0, FrontRightDoorJ) Then If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                    If Game.IsControlJustReleased(0, ModifierJ) AndAlso Game.IsControlJustReleased(0, RearLeftDoorJ) Then If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                    If Game.IsControlJustReleased(0, ModifierJ) AndAlso Game.IsControlJustReleased(0, RearRightDoorJ) Then If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False) Else Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                End If
            End If

            If AutoDoors AndAlso Bus.EngineRunning Then
                If LeavedPassengerPedGroup.Count <> 0 Then
                    For Each passenger As Ped In LeavedPassengerPedGroup
                        door1 = passenger.IsInVehicle(Bus)
                    Next
                Else
                    door1 = False
                End If
                If LastStationPassengerPedGroup.Count <> 0 Then
                    For Each passenger As Ped In LastStationPassengerPedGroup
                        door3 = passenger.IsInVehicle(Bus)
                    Next
                Else door3 = False
                End If
                If PassengerPedGroup.Count <> 0 Then
                    For Each passenger As Ped In PassengerPedGroup
                        door2 = Not passenger.IsInVehicle(Bus)
                    Next
                Else
                    door2 = False
                End If

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
                    If Bus.SpeedMPH = 0 Then
                        If Not Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                        If Not Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                        If Not Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                        If Not Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                    End If
                Else
                    If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False)
                    If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False)
                End If
            End If

            If Brakelights Then
                If Bus.SpeedMPH = 0 AndAlso Not Game.IsControlPressed(0, GTA.Control.VehicleBrake) Then
                    Bus.BrakeLightsOn = True
                ElseIf Bus.SpeedMPH < 0 AndAlso Game.IsControlPressed(0, GTA.Control.VehicleBrake) Then
                    Bus.BrakeLightsOn = True
                ElseIf Bus.SpeedMPH < 0 AndAlso Not Game.IsControlPressed(0, GTA.Control.VehicleBrake) Then
                    Bus.BrakeLightsOn = False
                End If
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
                            Bus.RightIndicatorLightOn = False
                        Case Directions.TurnRight, Directions.ExitFreeway
                            Bus.RightIndicatorLightOn = True
                            Bus.LeftIndicatorLightOn = False
                    End Select
                End If
            End If

            Try
                For Each ped As Ped In Bus.Passengers
                    If Not LeavedPassengerPedGroup.Contains(ped) Then
                        ped.StopPedFlee
                        'ped.Task.LookAt(Game.Player.Character)
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
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()
                    LastStationPassengerPedGroup.Clear()
                    Earned = 0
                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = start_mission
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.GenericFailed)
                    Effects.Start(ScreenEffect.DeathFailMpDark, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard(mission_failed, String.Format(bus_abandoned, Bus.FriendlyName), HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_RED, 5000)
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
                    PassengerPedGroup.Clear()
                    LeavedPassengerPedGroup.Clear()
                    LastStationPassengerPedGroup.Clear()
                    Earned = 0
                    IsInGame = False
                    BlipDict.Clear()
                    BlipList.Clear()
                    itemPlay.Text = start_mission
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.GenericFailed)
                    Effects.Start(ScreenEffect.DeathFailMpDark, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard(mission_failed, String.Format(bus_blew, Bus.FriendlyName), HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_RED, 5000)
                Catch ex As Exception
                    Logger.Log(String.Format("(Destroyed Bus): {0} {1}", ex.Message, ex.StackTrace))
                End Try
            End If

            If Bus.Position.DistanceTo(CurrentRoute.Stations(CurrentStationIndex).StationCoords) <= 80.0F Then
                If Not bellSounded Then
                    leaveCount = random.Next(0, 3)
                    If Not Bus.IsSeatFree(VehicleSeat.Passenger) Then
                        SoundPlayer("scripts\BusSimulatorV\Sound\bell.wav", BellVolume)
                        bellSounded = True
                        stopRequested = True
                    Else
                        If PassengerPedGroup.Count <> 0 AndAlso leaveCount <> 0 Then
                            SoundPlayer("scripts\BusSimulatorV\Sound\bell.wav", BellVolume)
                            bellSounded = True
                            stopRequested = True
                        End If
                    End If
                End If
                If stopRequested Then Bus.TurnBusStopRequestLightOn
            End If

            If Bus.Position.DistanceTo(CurrentRoute.Stations(CurrentStationIndex).StationCoords) <= 15.0F Then
                Dim b As Blip = BlipDict.Item(CurrentStationIndex)
                BlipDict.Remove(CurrentStationIndex)
                b.Remove()
                bellSounded = False

                If Not CurrentStationIndex = CurrentRoute.TotalStation Then CurrentStationIndex += 1

                If (CurrentStationIndex = CurrentRoute.TotalStation) Then
                    If TTS Then SAPI.speak(String.Format(final_station, CurrentRoute.Stations(CurrentStationIndex - 1).StationName, CurrentRoute.RouteName), 1)

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
                    Select Case Difficult
                        Case Difficulty.Normal
                            Earned = Earned
                        Case Difficulty.Hard
                            Earned *= 2
                        Case Difficulty.VeryHard
                            Earned *= 4
                        Case Difficulty.ExtremelyHard
                            Earned *= 8
                    End Select
                    Game.Player.Character.Money += Earned

                    itemPlay.Text = start_mission
                    itemPlay.Enabled = True
                    RemoveTimerBars()
                    PlayMissionCompleteAudio(MissionCompleteAudioFlags.FranklinBig01)
                    Effects.Start(ScreenEffect.SuccessNeutral, 5000)
                    BigMessageThread.MessageInstance.ShowColoredShard(mission_passed, String.Format(mission_completed, CurrentRoute.RouteName, Earned), HudColor.HUD_COLOUR_BLACK, HudColor.HUD_COLOUR_GOLD, 5000)
                    Earned = 0

                    Bus.RemoveAllExtras
                    Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                    Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
                Else
                    b = BlipDict.Item(CurrentStationIndex)
                    Select Case Difficult
                        Case Difficulty.Normal, Difficulty.Hard
                            b.ShowRoute = True
                    End Select

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
                    stopRequested = False

                    If Not PassengerPedGroup.Count >= Bus.PassengerSeats Then
                        Dim pedCount As Integer = 0, maxPed As Integer = 3
                        For Each ped As Ped In World.GetNearbyPeds(CurrentRoute.Stations(CurrentStationIndex - 1).StationCoords, 20.0F)
                            If pedCount < maxPed AndAlso Not ped = Game.Player.Character AndAlso ped.IsHuman AndAlso Not ped.IsInVehicle() AndAlso Not ped.IsProne AndAlso Not ped.IsHooker AndAlso Not World.CalculateTravelDistance(Bus.Position, ped.Position) >= 20.0F Then
                                If Not PassengerPedGroup.Contains(ped) Then
                                    ped.StopPedFlee
                                    ped.RelationshipGroup = PedRelationshipGroup
                                    ped.Task.ClearAll()
                                    ped.AlwaysKeepTask = True
                                    ped.Seat((Bus.GetEmptySeat + 1) - pedCount)

                                    Dim pedblip As Blip = ped.AddBlip
                                    With pedblip
                                        .Sprite = BlipSprite.Friend
                                        .Color = BlipColor.Blue
                                        .IsFriendly = True
                                        .Name = passenger
                                    End With
                                    PassengerPedGroup.Add(ped)
                                    ped.Task.ClearAllImmediately()
                                    pedCount += 1
                                    Earned += CurrentRoute.RouteFare
                                End If
                            End If
                        Next
                    End If
                    If TTS Then SAPI.speak(String.Format(next_station, CurrentRoute.Stations(CurrentStationIndex - 1).StationName, CurrentRoute.Stations(CurrentStationIndex).StationName), 1)
                End If
            End If
        Else
            DrawMarker(MenuActivator, New Vector3(1.0F, 1.0F, 1.0F), Color.CadetBlue)
        End If

        If Game.Player.Character.Position.DistanceTo(MenuActivator) <= 2.0 AndAlso Not Game.Player.Character.IsInVehicle Then
            If Not _menuPool.IsAnyMenuOpen Then
                DisplayHelpTextThisFrame(help_text)
            End If
            If Game.IsControlJustReleased(0, GTA.Control.Context) Then
                MainMenu.Show()
                StartCamera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView)
                World.RenderingCamera = StartCamera
                MenuCamera = World.CreateCamera(CameraPos, CameraRot, GameplayCamera.FieldOfView)
                StartCamera.InterpTo(MenuCamera, 3000, True, True)
                World.RenderingCamera = MenuCamera
            End If
        End If

        BusInteriorLights()
    End Sub

    Private Sub BusSim_Aborted(sender As Object, e As EventArgs) Handles Me.Aborted
        On Error Resume Next
        Me.Dispose()

        If Not Bus = Nothing Then Bus.Delete()
        If Not previewBus = Nothing Then previewBus.Delete()
        'If Not TextProp = Nothing Then TextProp.Delete()

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
            If selectedItem.Text = start_mission Then
                Game.FadeScreenOut(2000)
                Script.Wait(2000)
                sender.Hide()
                World.RenderingCamera = Nothing
                World.DestroyAllCameras()

                PassengerPedGroup.Clear()
                LeavedPassengerPedGroup.Clear()
                LastStationPassengerPedGroup.Clear()
                If Not Bus = Nothing Then Bus.Delete()
                If Not previewBus = Nothing Then Bus = previewBus Else Bus = World.CreateVehicle(CurrentRoute.BusModel, CurrentRoute.BusSpawnPoint, CurrentRoute.BusHeading)
                With Bus
                    .Position = CurrentRoute.BusSpawnPoint
                    .Heading = CurrentRoute.BusHeading
                    .IsPersistent = True
                    .RemoveAllExtras()
                    If .ExtraExists(CurrentRoute.TurnOnExtra) Then .ToggleExtra(CurrentRoute.TurnOnExtra, True)
                    .PlaceOnGround()
                    .Repair()
                    .Wash()
                End With
                'If Not TextProp = Nothing Then TextProp.Delete()
                'TextProp = World.CreateProp("ex_prop_ex_office_text", Bus.GetBoneCoord("extra_1"), False, False)
                'With TextProp
                '    .IsPersistent = True
                '    .AttachTo(Bus, Bus.GetBoneIndex("extra_1"), Vector3.Zero, Vector3.Zero)
                '    busScaleform.SetText(TextProp, "test")
                'End With

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
                        Select Case Difficult
                            Case Difficulty.ExtremelyHard
                                .Alpha = 0
                        End Select
                    End With
                    If Not BlipDict.ContainsKey(station.StationIndex) Then BlipDict.Add(station.StationIndex, b)
                    If Not BlipList.Contains(b) Then BlipList.Add(b)
                Next

                CurrentStationIndex = 0
                DrawtimerBars()
                selectedItem.Text = stop_mission
                selectedItem.Enabled = False
                PedRelationshipGroup = World.AddRelationshipGroup("BusPassengerGroup")
                World.SetRelationshipBetweenGroups(Relationship.Companion, PedRelationshipGroup, Game.Player.Character.RelationshipGroup)
                World.SetRelationshipBetweenGroups(Relationship.Companion, Game.Player.Character.RelationshipGroup, PedRelationshipGroup)
                Script.Wait(2000)
                Game.FadeScreenIn(2000)
                IsInGame = True
            ElseIf selectedItem.Text = itemRoute.Text Then
                RouteCamera = World.CreateCamera(RouteCameraPos, RouteCameraRot, GameplayCamera.FieldOfView)
                MenuCamera.InterpTo(RouteCamera, 3000, True, True)
                World.RenderingCamera = RouteCamera
                If Not RouteMenu.MenuItems.Count = 0 Then
                    Dim sitem = RouteMenu.MenuItems.Find(Function(x) x.RightBadge = UIMenuItem.BadgeStyle.Tick)
                    Dim br As BusRoute
                    If Not sitem Is Nothing Then
                        br = New BusRoute(sitem.SubString1)
                    Else
                        br = New BusRoute(RouteMenu.MenuItems(0).SubString1)
                    End If
                    br = br.ReadFromFile
                    If Not previewBus = Nothing Then previewBus.Delete()
                    previewBus = World.CreateVehicle(br.BusModel, New Vector3(421.5408, -641.575, 27.4958), 180.1512)
                    With previewBus
                        .RemoveAllExtras()
                        If .ExtraExists(br.TurnOnExtra) Then .ToggleExtra(br.TurnOnExtra, True)
                        .EngineRunning = True
                        .Wash()
                        .Repair()
                    End With
                End If
            End If
        Catch ex As Exception
            Logger.Log(String.Format("(MainMenu_OnItemSelect): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Public Sub DrawtimerBars()
        Try
            EarnedTimerBar = New TextTimerBar(EARN, "$" & Earned.ToString("0"))
            Select Case Speedometer
                Case SpeedMeasurement.KPH
                    SpeedTimerBar = New BarTimerBar(SPEED_KPH) With {.Percentage = 0F, .BackgroundColor = Color.DarkBlue, .ForegroundColor = Color.LightBlue}
                Case Else
                    SpeedTimerBar = New BarTimerBar(SPEED_MPH) With {.Percentage = 0F, .BackgroundColor = Color.DarkSlateBlue, .ForegroundColor = Color.LightBlue}
            End Select
            NextStationTimerBar = New TextTimerBar(NEXT_STATION_TB, CurrentRoute.Stations(CurrentStationIndex).StationName)
            StationTimerBar = New BarTimerBar(STATION) With {.Percentage = 0F, .BackgroundColor = Color.DarkGreen, .ForegroundColor = Color.LightGreen}
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
        World.DrawMarker(MarkerType.VerticalCylinder, p, p, Game.Player.Character.Rotation, size, color)
    End Sub

    Public Sub UpdateTimerBars()
        Try
            Dim res As SizeF = UIMenu.GetScreenResolutionMaintainRatio()
            Dim safe As Point = UIMenu.GetSafezoneBounds()
            Dim SNText As String = CurrentRoute.Stations(CurrentStationIndex).StationName
            Dim SCText As String = $"{CurrentStationIndex}/{CurrentRoute.TotalStation}"
            Dim SOText As String = ""

            EarnedTimerBar.Text = "$" & Earned.ToString("0")
            Select Case Speedometer
                Case SpeedMeasurement.KPH
                    SOText = Bus.SpeedKPH.ToString("0")
                    Dim KmMax As Single = Bus.MaxSpeedKPH * 1.1
                    If Bus.IsInAir Then SpeedTimerBar.Percentage = 1.0F Else SpeedTimerBar.Percentage = Val(Bus.SpeedKPH / KmMax)
                Case Else
                    SOText = Bus.SpeedMPH.ToString("0")
                    Dim MiMax As Single = Bus.MaxSpeedMPH * 1.1
                    If Bus.IsInAir Then SpeedTimerBar.Percentage = 1.0F Else SpeedTimerBar.Percentage = Val(Bus.SpeedMPH / MiMax)
            End Select
            NextStationTimerBar.Text = ""
            UIResText.Draw(SNText, CInt(res.Width) - safe.X - 10, CInt(res.Height) - safe.Y - (42 + (4 * 30 - TextHeight(SNText))), GTA.Font.ChaletLondon, TextSize(SNText), Color.White, UIResText.Alignment.Right, False, False, 0)
            Dim EachStationInterval As Single = 100 / CurrentRoute.TotalStation
            StationTimerBar.Percentage = Val(EachStationInterval * CurrentStationIndex) / 100
            UIResText.Draw(SCText, CInt(res.Width) - safe.X - (160 / 2), CInt(res.Height) - safe.Y - (42 + (4 * 20 - 10)), GTA.Font.ChaletLondon, 0.25F, Color.White, UIResText.Alignment.Centered, False, True, 0)
            UIResText.Draw(SOText, CInt(res.Width) - safe.X - (160 / 2), CInt(res.Height) - safe.Y - (42 + (4 * 10 - 10)), GTA.Font.ChaletLondon, 0.25F, Color.White, UIResText.Alignment.Centered, False, True, 0)
            _timerPool.Draw()
        Catch ex As Exception
            Logger.Log(String.Format("(UpdateTimerBars): {0} {1}", ex.Message, ex.StackTrace))
        End Try
    End Sub

    Private Function TextSize(__text As String) As Single
        Dim result As Single = 0.5F
        Select Case __text.Count
            Case 1 To 10
                result = 0.5F
            Case 11 To 20
                result = 0.35F
            Case 21 To 30
                result = 0.2F
            Case 31 To 999999999
                result = 0.05F
        End Select
        Return result
    End Function

    Private Function TextHeight(__text As String) As Integer
        Dim result As Integer = 0
        Select Case __text.Count
            Case 1 To 10
                result = 0
            Case 11 To 20
                result = 5
            Case 21 To 30
                result = 10
            Case 31 To 999999999
                result = 15
        End Select
        Return result
    End Function

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
            Dim D As New UIResText("D", New Point(SD.Position.X + (SD.Size.Width / 2), SD.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : D.Draw()
            If Bus.PassengerSeats >= 1 Then
                Dim SP As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 2), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.Passenger), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SP.Draw()
                Dim P As New UIResText("P", New Point(SP.Position.X + (SP.Size.Width / 2), SP.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : P.Draw()
            End If
            If Bus.PassengerSeats >= 2 Then
                Dim SLR As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.LeftRear), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SLR.Draw()
                Dim LR As New UIResText("LR", New Point(SLR.Position.X + (SLR.Size.Width / 2), SLR.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : LR.Draw()
            End If
            If Bus.PassengerSeats >= 3 Then
                Dim SRR As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 22), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.RightRear), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SRR.Draw()
                Dim RR As New UIResText("RR", New Point(SRR.Position.X + (SRR.Size.Width / 2), SRR.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : RR.Draw()
            End If
            If Bus.PassengerSeats >= 4 Then
                Dim SE1 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat1), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE1.Draw()
                Dim E1 As New UIResText("E1", New Point(SE1.Position.X + (SE1.Size.Width / 2), SE1.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E1.Draw()
            End If
            If Bus.PassengerSeats >= 5 Then
                Dim SE2 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 42), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat2), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE2.Draw()
                Dim E2 As New UIResText("E2", New Point(SE2.Position.X + (SE2.Size.Width / 2), SE2.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E2.Draw()
            End If
            If Bus.PassengerSeats >= 6 Then
                Dim SE3 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat3), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE3.Draw()
                Dim E3 As New UIResText("E3", New Point(SE3.Position.X + (SE3.Size.Width / 2), SE3.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E3.Draw()
            End If
            If Bus.PassengerSeats >= 7 Then
                Dim SE4 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 62), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat4), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE4.Draw()
                Dim E4 As New UIResText("E4", New Point(SE4.Position.X + (SE4.Size.Width / 2), SE4.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E4.Draw()
            End If
            If Bus.PassengerSeats >= 8 Then
                Dim SE5 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat5), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE5.Draw()
                Dim E5 As New UIResText("E5", New Point(SE5.Position.X + (SE5.Size.Width / 2), SE5.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E5.Draw()
            End If
            If Bus.PassengerSeats >= 9 Then
                Dim SE6 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 82), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat6), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE6.Draw()
                Dim E6 As New UIResText("E6", New Point(SE6.Position.X + (SE6.Size.Width / 2), SE6.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E6.Draw()
            End If
            If Bus.PassengerSeats >= 10 Then
                Dim SE7 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat7), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE7.Draw()
                Dim E7 As New UIResText("E7", New Point(SE7.Position.X + (SE7.Size.Width / 2), SE7.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E7.Draw()
            End If
            If Bus.PassengerSeats >= 11 Then
                Dim SE8 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 102), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat8), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE8.Draw()
                Dim E8 As New UIResText("E8", New Point(SE8.Position.X + (SE8.Size.Width / 2), SE8.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E8.Draw()
            End If
            If Bus.PassengerSeats >= 12 Then
                Dim SE9 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat9), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE9.Draw()
                Dim E9 As New UIResText("E9", New Point(SE9.Position.X + (SE9.Size.Width / 2), SE9.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E9.Draw()
            End If
            If Bus.PassengerSeats >= 13 Then
                Dim SE10 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 122), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat10), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE10.Draw()
                Dim E10 As New UIResText("E10", New Point(SE10.Position.X + (SE10.Size.Width / 2), SE10.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E10.Draw()
            End If
            If Bus.PassengerSeats >= 14 Then
                Dim SE11 As New UIResRectangle(New Point(BusNum.Position.X + 1, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat11), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE11.Draw()
                Dim E11 As New UIResText("E11", New Point(SE11.Position.X + (SE11.Size.Width / 2), SE11.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E11.Draw()
            End If
            If Bus.PassengerSeats >= 15 Then
                Dim SE12 As New UIResRectangle(New Point(BusNum.Position.X + 22, BusNum.Position.Y + BusNum.Size.Height + 142), New Size(18, 18), If(Bus.IsSeatFree(VehicleSeat.ExtraSeat12), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : SE12.Draw()
                Dim E12 As New UIResText("E12", New Point(SE12.Position.X + (SE12.Size.Width / 2), SE12.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : busText.Outline = True : E12.Draw()
            End If
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

    Public Sub DrawBETACopyright()
        Try
            Dim SRMR As SizeF = UIMenu.GetScreenResolutionMaintainRatio
            Dim SZB As Point = UIMenu.GetSafezoneBounds
            Dim copy As New UIResText($"~g~Bus Simulator V ~w~BETA v{My.Application.Info.Version.ToString} ~b~Created by I'm Not MentaL & Yoha~n~~w~{My.Application.Info.Copyright}", New Point((SRMR.Width - SZB.X) / 2, SRMR.Height - SZB.Y - 60), 0.4F, Color.White, GTA.Font.ChaletComprimeCologne, UIResText.Alignment.Centered) : copy.Outline = True : copy.Draw()
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
        If Not previewBus = Nothing Then previewBus.Delete()
        MenuCamera.InterpTo(StartCamera, 3000, True, True)
        World.RenderingCamera = StartCamera
        Script.Wait(3000)
        World.RenderingCamera = Nothing
        World.DestroyAllCameras()
    End Sub

    Private Sub BusSim_KeyUp(sender As Object, e As KeyEventArgs) Handles Me.KeyUp
        If IsInGame Then
            If Not AutoDoors AndAlso Game.Player.Character.IsInVehicle(Bus) Then
                If e.KeyCode = FrontLeftDoorK Then If Bus.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Bus.CloseDoor(VehicleDoor.FrontLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontLeftDoor, False, False)
                If e.KeyCode = FrontRightDoorK Then If Bus.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Bus.CloseDoor(VehicleDoor.FrontRightDoor, False) Else Bus.OpenDoor(VehicleDoor.FrontRightDoor, False, False)
                If e.KeyCode = RearLeftDoorK Then If Bus.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Bus.CloseDoor(VehicleDoor.BackLeftDoor, False) Else Bus.OpenDoor(VehicleDoor.BackLeftDoor, False, False)
                If e.KeyCode = RearRightDoork Then If Bus.IsDoorOpen(VehicleDoor.BackRightDoor) Then Bus.CloseDoor(VehicleDoor.BackRightDoor, False) Else Bus.OpenDoor(VehicleDoor.BackRightDoor, False, False)
            End If
        End If
    End Sub

    Private Sub RouteMenu_OnIndexChange(sender As UIMenu, newIndex As Integer) Handles RouteMenu.OnIndexChange
        Dim br As BusRoute = New BusRoute(sender.MenuItems(newIndex).SubString1)
        br = br.ReadFromFile
        If Not previewBus = Nothing Then previewBus.Delete()
        previewBus = World.CreateVehicle(br.BusModel, New Vector3(421.5408, -641.575, 27.4958), 180.1512)
        With previewBus
            .RemoveAllExtras()
            If .ExtraExists(br.TurnOnExtra) Then .ToggleExtra(br.TurnOnExtra, True)
            .EngineRunning = True
        End With
    End Sub

    Private Sub RouteMenu_OnMenuClose(sender As UIMenu) Handles RouteMenu.OnMenuClose
        RouteCamera.InterpTo(MenuCamera, 3000, True, True)
        World.RenderingCamera = MenuCamera
        'If Not previewBus = Nothing Then previewBus.Delete()
    End Sub

    Dim buses As New List(Of Vehicle)
    Private Sub BusInteriorLights()
        For Each bas In World.GetNearbyVehicles(Game.Player.Character, 50.0F)
            If bas.IsBus Then
                If Not buses.Contains(bas) Then buses.Add(bas)
            End If
        Next

        For Each bas As Vehicle In buses
            bas.TurnBusInteriorLightsOn
        Next

        If Game.Player.Character.LastVehicle.IsBus Then If Not buses.Contains(Game.Player.Character.LastVehicle) Then buses.Add(Game.Player.Character.LastVehicle)
    End Sub
End Class
