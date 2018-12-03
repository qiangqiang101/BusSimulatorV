Imports GTA
Imports BusSimulatorV.BusSim
Imports System.Drawing
Imports GTA.Math

Public Class BusSimTimer
    Inherits Script

    Public Sub New()
    End Sub

    Private Sub FixPedEnterBus()
        On Error Resume Next
        If Not PassengerPedGroup.Count = 0 AndAlso Not Bus = Nothing Then
            If Bus.IsAnyDoorOpen Then
                For Each ped As Ped In PassengerPedGroup
                    If Not ped.IsSittingInVehicle(Bus) AndAlso Not ped.IsRunning Then
                        Select Case True
                            Case Bus.IsVehicleFull, ped.IsPedTooFarAwayFrom(Bus)
                                ped.CurrentBlip.Remove()
                                PassengerPedGroup.Remove(ped)
                                Earned -= CurrentRoute.RouteFare
                                Return
                        End Select
                        If Not Bus.IsSeatFree(ped.Seat) Then ped.Task.EnterVehicle(Bus, Bus.GetEmptySeat, 15000, 2.0, EnterBusFlag.None) Else ped.Task.EnterVehicle(Bus, ped.Seat, 15000, 2.0, EnterBusFlag.None)
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
                        Case VehicleSeat.LeftRear, VehicleSeat.RightRear, VehicleSeat.ExtraSeat1
                            ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptyExtraSeat(ped.SeatIndex))
                            Script.Wait(200)
                        Case VehicleSeat.Passenger
                            If Not Bus.GetEmptyExtraSeat(ped.SeatIndex) = ped.SeatIndex Then
                                If PassengerPedGroup.Contains(ped) Then ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptyExtraSeat(ped.SeatIndex))
                                Script.Wait(200)
                            Else
                                If PassengerPedGroup.Contains(ped) Then ped.Task.WarpIntoVehicle(Bus, Bus.GetEmptySeat())
                            End If
                    End Select
                    ped.ClearSeat
                End If
            Next
        End If
    End Sub

    Private Sub BusSimTimer_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedEnterBus()
        PedAutoShufferingSeat()
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
                            Case VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4, VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7, VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10, VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12, VehicleSeat.ExtraSeat1
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                                Script.Wait(500)
                            Case VehicleSeat.Passenger, VehicleSeat.LeftRear, VehicleSeat.RightRear
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
            For Each ped As Ped In Bus.Passengers
                If Not ped = Game.Player.Character Then
                    ped.CurrentBlip.Remove()
                    If Not ped.IsInVehicle(Bus) Then
                        LastStationPassengerPedGroup.Remove(ped)
                    Else
                        Select Case ped.SeatIndex
                            Case VehicleSeat.ExtraSeat2, VehicleSeat.ExtraSeat3, VehicleSeat.ExtraSeat4
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.Passenger)
                                Script.Wait(500)
                            Case VehicleSeat.ExtraSeat5, VehicleSeat.ExtraSeat6, VehicleSeat.ExtraSeat7
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.ExtraSeat1)
                                Script.Wait(500)
                            Case VehicleSeat.ExtraSeat8, VehicleSeat.ExtraSeat9, VehicleSeat.ExtraSeat10
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.LeftRear)
                                Script.Wait(500)
                            Case VehicleSeat.ExtraSeat11, VehicleSeat.ExtraSeat12
                                ped.Task.WarpIntoVehicle(Bus, VehicleSeat.RightRear)
                                Script.Wait(500)
                            Case VehicleSeat.Passenger, VehicleSeat.ExtraSeat1, VehicleSeat.LeftRear, VehicleSeat.RightRear
                                ped.Task.LeaveVehicle(Bus, False)
                                Script.Wait(500)
                        End Select
                    End If
                    ped.RelationshipGroup = 0
                End If
            Next
        End If
    End Sub

    Private Sub BusSimTimer2_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedLeaveBus()
        AllPedLeaveBus()
    End Sub
End Class