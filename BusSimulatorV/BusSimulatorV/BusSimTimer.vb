Imports System.Drawing
Imports System.IO
Imports GTA
Imports GTA.Math
Imports GTA.Native
Imports INMNativeUI
Imports System.Windows.Forms
Imports BusSimulatorV.BusSim

Public Class BusSimTimer
    Inherits Script

    Private Sub FixPedEnterBus()
        On Error Resume Next
        If Not PassengerPedGroup.Count = 0 Then
            For Each ped As Ped In PassengerPedGroup
                If Not ped.IsInVehicle(Bus) Then
                    ped.Task.ClearAll()
                    ped.Task.EnterVehicle(Bus, Bus.GetEmptySeat, 10000, 2.0, EnterBusFlag.Normal)
                End If
            Next
        End If
    End Sub

    Private Sub FixPedLeaveBus()
        On Error Resume Next
        If Not LeavedPassengerPedGroup.Count = 0 Then
            For Each ped As Ped In LeavedPassengerPedGroup
                ped.CurrentBlip.Remove()
                Select Case ped.SeatIndex
                    Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                        ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                    Case VehicleSeat.Passenger
                        ped.Task.LeaveVehicle(Bus, LeaveVehicleFlags.LeaveDoorOpen)
                End Select
                ped.RelationshipGroup = 0
            Next
        End If
    End Sub

    Private Sub AllPedLeaveBus()
        On Error Resume Next
        If Not LastStationPassengerPedGroup.Count = 0 Then
            For Each ped As Ped In LastStationPassengerPedGroup
                ped.CurrentBlip.Remove()
                Select Case ped.SeatIndex
                    Case VehicleSeat.ExtraSeat1, VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                        ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                    Case VehicleSeat.Passenger
                        ped.Task.LeaveVehicle(Bus, LeaveVehicleFlags.LeaveDoorOpen)
                End Select
                ped.RelationshipGroup = 0
            Next
        End If
    End Sub

    Private Sub PedAutoShufferingSeat()
        On Error Resume Next
        For Each ped As Ped In Bus.Passengers
            If ped.IsInVehicle(Bus) Then
                Select Case ped.SeatIndex
                    Case VehicleSeat.LeftRear, VehicleSeat.RightRear
                        ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptyExtraSeat(ped.SeatIndex))
                End Select
            End If
        Next
    End Sub

    Private Sub BusSimTimer_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedEnterBus()
        PedAutoShufferingSeat()
        FixPedLeaveBus()
        AllPedLeaveBus()
    End Sub
End Class
