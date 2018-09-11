Imports GTA
Imports System.Runtime.CompilerServices
Imports GTA.Native
Imports INMNativeUI
Imports GTA.Math

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
        If vehicle.IsSeatFree(VehicleSeat.Passenger) Then Return VehicleSeat.Passenger
        If vehicle.IsSeatFree(VehicleSeat.LeftRear) Then Return VehicleSeat.LeftRear
        If vehicle.IsSeatFree(VehicleSeat.RightRear) Then Return VehicleSeat.RightRear
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat1) Then Return VehicleSeat.ExtraSeat1
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat2) Then Return VehicleSeat.ExtraSeat2
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat3) Then Return VehicleSeat.ExtraSeat3
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat4) Then Return VehicleSeat.ExtraSeat4
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat5) Then Return VehicleSeat.ExtraSeat5
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat6) Then Return VehicleSeat.ExtraSeat6
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat7) Then Return VehicleSeat.ExtraSeat7
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat8) Then Return VehicleSeat.ExtraSeat8
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat9) Then Return VehicleSeat.ExtraSeat9
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat10) Then Return VehicleSeat.ExtraSeat10
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat11) Then Return VehicleSeat.ExtraSeat11
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat12) Then Return VehicleSeat.ExtraSeat12
        Return VehicleSeat.Any
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
        Normal = 1
        Teleport = 3
        TeleportDirectly = 16
    End Enum

    Public Enum LeaveBusFlag
        Normal
        Normal2nd
        Teleport = 16
        SlowerNormal = 64
        LeaveDoorsOpen = 256
        Unk1 = 320
        Unk2 = 512
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
End Module