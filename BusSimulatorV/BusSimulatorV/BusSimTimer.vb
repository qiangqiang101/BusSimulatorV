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
        For Each ped As Ped In PassengerPedGroup
            If Not ped.IsInVehicle(Bus) Then
                ped.Task.ClearAll()
                ped.Task.EnterVehicle(Bus, Bus.GetEmptySeat, 10000, 2.0, EnterBusFlag.Normal)
                Script.Wait(1000)
            End If
        Next
    End Sub

    Private Sub BusSimTimer_Tick(sender As Object, e As EventArgs) Handles Me.Tick
        FixPedEnterBus()
    End Sub
End Class
