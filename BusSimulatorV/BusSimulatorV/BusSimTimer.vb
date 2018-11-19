Imports GTA
Imports BusSimulatorV.BusSim
Imports System.Drawing

Public Class BusSimTimer
    Inherits Script

    Dim rd As New Random()
    Public Shared missionCompleteSC As Integer = 0, missionCompleteSF As Scaleform

    Public Sub New()
    End Sub

    Private Sub FixPedEnterBus()
        On Error Resume Next
        If Not PassengerPedGroup.Count = 0 AndAlso Not Bus = Nothing Then
            If Bus.IsAnyDoorOpen AndAlso Bus.SpeedMPH = 0 Then
                For Each ped As Ped In PassengerPedGroup
                    If Not ped.IsInVehicle(Bus) AndAlso Not ped.IsRunning Then
                        If Bus.IsVehicleFull Or World.GetDistance(Bus.Position, ped.Position) >= 20.0F Then
                            ped.CurrentBlip.Remove()
                            PassengerPedGroup.Remove(ped)
                            Earned -= 1
                            Return
                        End If
                        ped.Task.ClearAll()
                        If Not Bus.IsSeatFree(ped.Seat) Then ped.Task.EnterVehicle(Bus, Bus.GetEmptySeat, 10000, 2.0, EnterBusFlag.None) Else ped.Task.EnterVehicle(Bus, ped.Seat, 10000, 2.0, EnterBusFlag.None)
                        ped.AlwaysKeepTask = True
                        Script.Wait(500)
                    End If
                Next
            End If
        End If
    End Sub

    Private Sub PedAutoShufferingSeat()
        On Error Resume Next
        If Not Bus = Nothing Then
            For Each ped As Ped In Bus.Passengers
                If ped.IsInVehicle(Bus) Then
                    Select Case ped.SeatIndex
                        Case VehicleSeat.LeftRear, VehicleSeat.RightRear
                            ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptyExtraSeat(ped.SeatIndex))
                        Case VehicleSeat.Passenger
                            If PassengerPedGroup.Contains(ped) Then ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptyExtraSeat(ped.SeatIndex))
                    End Select
                    ped.ClearSeat
                End If
            Next
        End If
    End Sub

    Private Sub BusSimTimer_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedEnterBus()
        PedAutoShufferingSeat()

        'For Each ped In LeavedPassengerPedGroup
        '    ped.MutePed(True)
        'Next
    End Sub
End Class

Public Class BusSimTimer2
    Inherits Script

    Public Sub New()

    End Sub

    Private Sub FixPedLeaveBus()
        On Error Resume Next
        If Not LeavedPassengerPedGroup.Count = 0 AndAlso Not Bus = Nothing Then
            If Bus.IsAnyDoorOpen Then
                For Each ped As Ped In LeavedPassengerPedGroup
                    ped.CurrentBlip.Remove()
                    If Not ped.IsInVehicle(Bus) Then
                        LeavedPassengerPedGroup.Remove(ped)
                    Else
                        Select Case ped.SeatIndex
                            Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                                Script.Wait(500)
                            Case VehicleSeat.Passenger
                                ped.Task.LeaveVehicle(Bus, False)
                                Script.Wait(500)
                        End Select
                    End If
                    ped.RelationshipGroup = 0
                Next
            End If
        End If
    End Sub

    Private Sub AllPedLeaveBus()
        On Error Resume Next
        If Not LastStationPassengerPedGroup.Count = 0 AndAlso Not Bus = Nothing Then
            If Bus.IsAnyDoorOpen Then
                For Each ped As Ped In LastStationPassengerPedGroup
                    ped.CurrentBlip.Remove()
                    If Not ped.IsInVehicle(Bus) Then
                        LastStationPassengerPedGroup.Remove(ped)
                    Else
                        Select Case ped.SeatIndex
                            Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                                If Bus.IsSeatFree(VehicleSeat.LeftRear) Then
                                    ped.Task.WarpIntoVehicle(Bus, VehicleSeat.LeftRear)
                                Else
                                    If Bus.IsSeatFree(VehicleSeat.RightRear) Then
                                        ped.Task.WarpIntoVehicle(Bus, VehicleSeat.RightRear)
                                    Else
                                        If Bus.IsSeatFree(VehicleSeat.Passenger) Then
                                            ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                                        Else
                                            Exit For
                                        End If
                                    End If
                                End If
                                Script.Wait(500)
                            Case VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear
                                ped.Task.ClearAll()
                                ped.Task.LeaveVehicle(Bus, False)
                                Script.Wait(500)
                        End Select
                    End If
                    ped.RelationshipGroup = 0
                Next
            End If
        End If
    End Sub

    Private Sub BusSimTimer2_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedLeaveBus()
        AllPedLeaveBus()
    End Sub
End Class