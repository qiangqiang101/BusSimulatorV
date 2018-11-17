﻿Imports GTA
Imports System.Runtime.CompilerServices
Imports GTA.Native
Imports INMNativeUI
Imports GTA.Math
Imports System.Drawing

Module Helper

    <Extension()>
    Public Sub RemoveAllExtras(ByVal vehicle As Vehicle)
        If vehicle.ExtraExists(0) Then vehicle.ToggleExtra(0, False)
        If vehicle.ExtraExists(1) Then vehicle.ToggleExtra(1, False)
        If vehicle.ExtraExists(2) Then vehicle.ToggleExtra(2, False)
        If vehicle.ExtraExists(3) Then vehicle.ToggleExtra(3, False)
        If vehicle.ExtraExists(4) Then vehicle.ToggleExtra(4, False)
        If vehicle.ExtraExists(5) Then vehicle.ToggleExtra(5, False)
        If vehicle.ExtraExists(6) Then vehicle.ToggleExtra(6, False)
        If vehicle.ExtraExists(7) Then vehicle.ToggleExtra(7, False)
        If vehicle.ExtraExists(8) Then vehicle.ToggleExtra(8, False)
        If vehicle.ExtraExists(9) Then vehicle.ToggleExtra(9, False)
        If vehicle.ExtraExists(10) Then vehicle.ToggleExtra(10, False)
    End Sub

    <Extension()>
    Public Function GetEmptySeat(ByVal vehicle As Vehicle) As VehicleSeat
        If vehicle.IsSeatFree(VehicleSeat.LeftRear) Then
            Return VehicleSeat.LeftRear
        Else
            If vehicle.IsSeatFree(VehicleSeat.RightRear) Then
                Return VehicleSeat.RightRear
            Else
                Return VehicleSeat.Passenger
            End If
        End If
    End Function

    <Extension()>
    Public Function GetEmptyExtraSeat(ByVal vehicle As Vehicle, originSeat As VehicleSeat) As VehicleSeat
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat12) Then
            Return VehicleSeat.ExtraSeat12
        Else
            If vehicle.IsSeatFree(VehicleSeat.ExtraSeat11) Then
                Return VehicleSeat.ExtraSeat11
            Else
                If vehicle.IsSeatFree(VehicleSeat.ExtraSeat10) Then
                    Return VehicleSeat.ExtraSeat10
                Else
                    If vehicle.IsSeatFree(VehicleSeat.ExtraSeat9) Then
                        Return VehicleSeat.ExtraSeat9
                    Else
                        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat8) Then
                            Return VehicleSeat.ExtraSeat8
                        Else
                            If vehicle.IsSeatFree(VehicleSeat.ExtraSeat7) Then
                                Return VehicleSeat.ExtraSeat7
                            Else
                                If vehicle.IsSeatFree(VehicleSeat.ExtraSeat6) Then
                                    Return VehicleSeat.ExtraSeat6
                                Else
                                    If vehicle.IsSeatFree(VehicleSeat.ExtraSeat5) Then
                                        Return VehicleSeat.ExtraSeat5
                                    Else
                                        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat4) Then
                                            Return VehicleSeat.ExtraSeat4
                                        Else
                                            If vehicle.IsSeatFree(VehicleSeat.ExtraSeat3) Then
                                                Return VehicleSeat.ExtraSeat3
                                            Else
                                                If vehicle.IsSeatFree(VehicleSeat.ExtraSeat2) Then
                                                    Return VehicleSeat.ExtraSeat2
                                                Else
                                                    If vehicle.IsSeatFree(VehicleSeat.ExtraSeat1) Then
                                                        Return VehicleSeat.ExtraSeat1
                                                    Else
                                                        Return originSeat
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
        End If
    End Function

    <Extension()>
    Public Function SpeedMPH(ByVal vehicle As Vehicle) As Double
        Return ((vehicle.Speed * 3600) / 1609.344)
    End Function

    <Extension()>
    Public Function SpeedKPH(ByVal vehicle As Vehicle) As Double
        Return vehicle.SpeedMPH * 1.609344
    End Function

    <Extension()>
    Public Sub Show(ByVal uimenu As UIMenu)
        uimenu.Visible = True
    End Sub

    <Extension()>
    Public Sub Hide(ByVal uimenu As UIMenu)
        uimenu.Visible = False
    End Sub

    <Extension()>
    Public Function IsAnyDoorOpen(ByVal vehicle As Vehicle) As Boolean
        If vehicle.IsDoorOpen(VehicleDoor.BackLeftDoor) Then Return True
        If vehicle.IsDoorOpen(VehicleDoor.BackRightDoor) Then Return True
        If vehicle.IsDoorOpen(VehicleDoor.FrontLeftDoor) Then Return True
        If vehicle.IsDoorOpen(VehicleDoor.FrontRightDoor) Then Return True
        If vehicle.IsDoorOpen(VehicleDoor.Hood) Then Return True
        If vehicle.IsDoorOpen(VehicleDoor.Trunk) Then Return True
        Return False
    End Function

    <Extension()>
    Public Sub HazardLights(ByVal vehicle As Vehicle, onoff As Boolean)
        vehicle.LeftIndicatorLightOn = onoff
        vehicle.RightIndicatorLightOn = onoff
    End Sub

    Public Function IsCheating(Cheat As String) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash._0x557E43C447E700A8, GTA.Game.GenerateHash(Cheat))
    End Function

    Public Sub DisplayHelpTextThisFrame(ByVal [text] As String)
        Dim arguments As InputArgument() = New InputArgument() {"STRING"}
        Native.Function.Call(Hash._0x8509B634FBE7DA11, arguments)
        Dim argumentArray2 As InputArgument() = New InputArgument() {[text]}
        Native.Function.Call(Hash._0x6C188BE134E074AA, argumentArray2)
        Dim argumentArray3 As InputArgument() = New InputArgument() {0, 0, 1, -1}
        Native.Function.Call(Hash._0x238FFE5C7B0498A6, argumentArray3)
    End Sub

    Public Enum SpeedMeasurement
        MPH
        KPH
    End Enum

    Public Enum EnterBusFlag
        None
        Normal
        WarpTo
        Teleport
        AllowJacking = 8
        TeleportDirectly = 16
        EnterFromOppositeSide = 262144
        DoNotEnter = 524288
    End Enum

    Public Enum LeaveBusFlag
        Normal
        Normal2nd
        Teleport = 16
        SlowerNormal = 64
        LeaveDoorsOpen = 256
        Unk1 = 320
        Unk2 = 512
        BailOut = 4096
        ThrowingOut = 4160
        Unk3 = 131072
        MovesToPassengerSeatFirst = 262144
    End Enum

    <Extension()>
    Public Sub SetAsLeader(ByVal player As Ped)
        Native.Function.Call(Hash.SET_PED_AS_GROUP_LEADER, player.Handle, player.CurrentPedGroup)
    End Sub

    <Extension()>
    Public Sub EnterGroup(ByVal ped As Ped, group As PedGroup, Optional neverLeavesGroup As Boolean = True)
        Game.Player.Character.SetAsLeader()
        Native.Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, ped.Handle, group)
        ped.NeverLeavesGroup = neverLeavesGroup
        Game.Player.Character.SetAsLeader()
    End Sub

    <Extension()>
    Public Sub StopPedFlee(ByVal ped As Ped)
        Native.Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, True)
        Native.Function.Call(Hash.SET_PED_FLEE_ATTRIBUTES, ped, 0, 0)
        Native.Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 17, 1)
    End Sub

    <Extension()>
    Public Function GetNearestNonPlayerPed(ByVal coords As Vector3, radius As Single) As Ped
        Dim peds As Ped() = World.GetNearbyPeds(Game.Player.Character.Position, radius)
        For Each ped As Ped In peds
            If Not ped.IsInVehicle AndAlso Not ped = Game.Player.Character Then
                Return ped
                Exit Function
            End If
        Next
        Return Nothing
    End Function

    Public Function IsGameUIVisible() As Boolean
        Return Native.Function.Call(Of Boolean)(Hash._0xAF754F20EB5CD51A)
    End Function

    Private PedSeat As New Dictionary(Of Ped, VehicleSeat)
    <Extension()>
    Public Function Seat(ped As Ped) As VehicleSeat
        Return PedSeat.Item(ped)
    End Function

    <Extension()>
    Public Sub Seat(ped As Ped, seat As VehicleSeat)
        PedSeat.Add(ped, seat)
    End Sub

    <Extension()>
    Public Sub ClearSeat(ped As Ped)
        If ped.DoesPedHasSeatInMemory Then PedSeat.Remove(ped)
    End Sub

    <Extension()>
    Private Function DoesPedHasSeatInMemory(ped As Ped) As Boolean
        Return PedSeat.ContainsKey(ped)
    End Function

    Public Sub PlayMissionCompleteAudio(flags As MissionCompleteAudioFlags)
        Select Case flags
            Case MissionCompleteAudioFlags.Dead
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "DEAD")
            Case MissionCompleteAudioFlags.MichaelSmall01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "MICHAEL_SMALL_01")
            Case MissionCompleteAudioFlags.MichaelBig01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "MICHAEL_BIG_01")
            Case MissionCompleteAudioFlags.FranklinSmall01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "FRANKLIN_SMALL_01")
            Case MissionCompleteAudioFlags.FranklinBig01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "FRANKLIN_BIG_01")
            Case MissionCompleteAudioFlags.GenericFailed
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "GENERIC_FAILED")
            Case MissionCompleteAudioFlags.TrevorSmall01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "TREVOR_SMALL_01")
            Case MissionCompleteAudioFlags.TrevorBig01
                Native.Function.Call(Hash.PLAY_MISSION_COMPLETE_AUDIO, "TREVOR_BIG_01")
        End Select
        While (Not Native.Function.Call(Of Boolean)(Hash.IS_MISSION_COMPLETE_PLAYING))
            Script.Yield()
        End While
    End Sub

    Public Enum MissionCompleteAudioFlags
        Dead
        MichaelSmall01
        MichaelBig01
        FranklinSmall01
        FranklinBig01
        GenericFailed
        TrevorSmall01
        TrevorBig01
    End Enum

    <Extension()>
    Public Sub PlayMissionCompleteScaleform(scaleform As Scaleform, title As String, subtitle As String, medal As MissionCompleteScaleformMedal, percent As Integer, time As Integer, ParamArray objectives As ObjectiveItem())
        scaleform = New Scaleform("MISSION_COMPLETE")
        Dim timeout As Integer = time
        Dim start = DateTime.Now
        'While (Not scaleform.IsLoaded) AndAlso DateTime.Now.Subtract(start).TotalMilliseconds < timeout
        '    Script.Yield()
        'End While
        scaleform.CallFunction("SET_MISSION_TITLE", "", title)
        Dim objIndex As Integer = 0
        For Each obj As ObjectiveItem In objectives
            Select Case obj.Type
                Case ObjectiveItem.ObjectiveItemType.Time
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, True, True, 2, CInt(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.Number
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, 2, CSng(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.Task
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, obj.RightLabel.ToString.Split("/")(0), obj.RightLabel.ToString.Split("/")(1), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.Percentage
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, 2, CInt(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.MoneyMonetized
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, 8, CInt(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.MoneyDemonetized
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, 8, CInt(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.RP
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, 8, CInt(obj.RightLabel), obj.Title)
                Case ObjectiveItem.ObjectiveItemType.Objective
                    scaleform.CallFunction("SET_DATA_SLOT", objIndex, obj.CheckboxChecked, obj.Title)
            End Select
            objIndex += 1
        Next
        scaleform.CallFunction("SET_TOTAL", CInt(medal), percent, subtitle)
        scaleform.CallFunction("DRAW_MENU_LIST")
    End Sub

    <Extension()>
    Public Sub DrawMissionCompletedScaleform(scaleform As Scaleform, position As PointF, size As SizeF, color As Color)
        Native.Function.Call(Hash.DRAW_SCALEFORM_MOVIE, scaleform.Handle, position.X, position.Y, size.Width, size.Height, CInt(color.R), CInt(color.G), CInt(color.B), CInt(color.A), 0)
    End Sub

    Public Enum MissionCompleteScaleformMedal
        Gold = 1
        Silver
        Bronze
        Skull
        Unk
    End Enum
End Module

Public Class ObjectiveItem

    Public Property Type As ObjectiveItemType
    Public Property Title As String
    Public Property RightLabel As Object
    Public CheckboxChecked As Integer

    Public Enum ObjectiveItemType
        Time
        Number
        Task
        Percentage
        MoneyMonetized
        MoneyDemonetized
        RP
        Objective
    End Enum

    Public Sub New(_type As ObjectiveItemType, _title As String, _rightLabel As Object, _checked As Boolean)
        Type = _type
        Title = _title
        RightLabel = _rightLabel
        CheckboxChecked = If(_checked, 1, 0)
    End Sub

End Class