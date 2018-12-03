Imports GTA
Imports System.Runtime.CompilerServices
Imports GTA.Native
Imports INMNativeUI
Imports GTA.Math
Imports System.Drawing
Imports System.Media

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
        If vehicle.IsSeatFree(VehicleSeat.LeftRear) Then Return VehicleSeat.LeftRear
        If vehicle.IsSeatFree(VehicleSeat.RightRear) Then Return VehicleSeat.RightRear
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat1) Then Return VehicleSeat.ExtraSeat1
        Return VehicleSeat.Passenger
    End Function

    <Extension()>
    Public Function GetEmptyExtraSeat(ByVal vehicle As Vehicle, originSeat As VehicleSeat) As VehicleSeat
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat12) Then Return VehicleSeat.ExtraSeat12
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat11) Then Return VehicleSeat.ExtraSeat11
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat10) Then Return VehicleSeat.ExtraSeat10
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat9) Then Return VehicleSeat.ExtraSeat9
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat8) Then Return VehicleSeat.ExtraSeat8
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat7) Then Return VehicleSeat.ExtraSeat7
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat6) Then Return VehicleSeat.ExtraSeat6
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat5) Then Return VehicleSeat.ExtraSeat5
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat4) Then Return VehicleSeat.ExtraSeat4
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat3) Then Return VehicleSeat.ExtraSeat3
        If vehicle.IsSeatFree(VehicleSeat.ExtraSeat2) Then Return VehicleSeat.ExtraSeat2

        'If vehicle.IsSeatFree(VehicleSeat.LeftRear) Then Return VehicleSeat.LeftRear
        'If vehicle.IsSeatFree(VehicleSeat.RightRear) Then Return VehicleSeat.RightRear
        Return originSeat
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
    Public Function MaxSpeedMPH(ByVal vehicle As Vehicle) As Single
        Return (Native.Function.Call(Of Single)(Hash._0xF417C2502FFFED43, vehicle.Model.Hash) * 3600) / 1609.344
    End Function

    <Extension()>
    Public Function MaxSpeedKPH(ByVal vehicle As Vehicle) As Single
        Return vehicle.MaxSpeedMPH * 1.609344
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

    Public Enum Directions
        YouHaveArrive
        RecalculatingRoute
        ProceedTheHighlightingRoute
        KeepLeft
        TurnLeft
        TurnRight
        KeepRight
        GoStraight
        JoinTheFreeway
        ExitFreeway
    End Enum

    Public Function GenerateDirectionsToCoord(pos As Vector3) As Directions
        Dim f4, f5, f6 As New OutputArgument()
        Native.Function.Call(Hash.GENERATE_DIRECTIONS_TO_COORD, pos.X, pos.Y, pos.Z, True, f4, f5, f6)
        Dim direction As String = f4.GetResult(Of Single)()
        Return CInt(direction.Substring(0, 1))
    End Function

    <Extension()>
    Public Function IsVehicleFull(vehicle As Vehicle) As Boolean
        Dim maxSeat As Integer = vehicle.PassengerSeats
        Dim passengers As Integer = vehicle.Passengers.Count
        If passengers = maxSeat Then Return True Else Return False
    End Function

    Public Enum MissionCompleteScaleformMedal
        Gold = 1
        Silver
        Bronze
        Skull
        Unk
    End Enum

    <Extension()>
    Public Sub MutePed(ped As Ped, mute As Boolean)
        Native.Function.Call(Hash.STOP_PED_SPEAKING, ped, mute)
    End Sub

    Public Sub SoundPlayer(waveFile As String, volume As Integer)
        Using stream As New WaveStream(IO.File.OpenRead(waveFile))
            stream.Volume = volume
            Using player As New SoundPlayer(stream)
                player.Play()
            End Using
        End Using
    End Sub

    <Extension()>
    Public Sub SetTattoo(ped As Ped, collection As String, overlay As String)
        Native.Function.Call(Hash._SET_PED_DECORATION, collection.GetHashKey, overlay.GetHashKey)
    End Sub

    <Extension()>
    Public Function GetHashKey(str As String) As Integer
        Return Native.Function.Call(Of Integer)(Hash.GET_HASH_KEY, str)
    End Function

    <Extension()>
    Public Sub TurnBusInteriorLightsOn(bus As Vehicle)
        Dim random As New Random
        Dim b1, b2, b3, b4 As Integer
        b1 = random.Next(0, 500)
        b2 = random.Next(0, 500)
        b3 = random.Next(0, 500)
        b4 = random.Next(0, 500)
        If bus.EngineRunning AndAlso Not bus.IsDead Then
            If bus.LightsOn Then
                If bus.HasBone("misc_w") AndAlso bus.HasBone("misc_x") AndAlso bus.HasBone("misc_y") AndAlso bus.HasBone("misc_z") Then
                    If bus.HasBone("misc_w") AndAlso Not b1 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_w"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_x") AndAlso Not b2 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_x"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_y") AndAlso Not b3 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_y"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_z") AndAlso Not b4 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_z"), Color.White, 2.3, 5.0)
                ElseIf bus.HasBone("misc_g") AndAlso bus.HasBone("misc_h") AndAlso bus.HasBone("misc_i") AndAlso bus.HasBone("misc_j") Then
                    If bus.HasBone("misc_g") AndAlso Not b1 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_g"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_h") AndAlso Not b2 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_h"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_i") AndAlso Not b3 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_i"), Color.White, 2.3, 5.0)
                    If bus.HasBone("misc_j") AndAlso Not b4 = 1 Then World.DrawLightWithRange(bus.GetBoneCoord2("misc_j"), Color.White, 2.3, 5.0)
                End If
                bus.InteriorLightOn = True
            End If
        End If
    End Sub

    <Extension()>
    Public Function GetBoneCoord2(entity As Entity, bone As String) As Vector3
        Dim bc As Vector3 = entity.GetBoneCoord(bone)
        Return New Vector3(bc.X, bc.Y, bc.Z - 0.5F)
    End Function

    <Extension()>
    Public Function IsPedTooFarAwayFrom(ped As Ped, bus As Vehicle) As Boolean
        If World.GetDistance(bus.Position, ped.Position) > 25.0F Then Return True Else Return False
    End Function

    <Extension()>
    Public Function GetSeatPosition(bus As Vehicle, seat As VehicleSeat) As Vector3
        Select Case seat
            Case VehicleSeat.Driver, VehicleSeat.LeftFront
                Return bus.GetBoneCoord("seat_dside_f")
            Case VehicleSeat.Passenger, VehicleSeat.RightFront
                Return bus.GetBoneCoord("seat_pside_f")
            Case VehicleSeat.LeftRear
                Return bus.GetBoneCoord("seat_dside_r")
            Case VehicleSeat.RightRear
                Return bus.GetBoneCoord("seat_pside_r")
            Case VehicleSeat.ExtraSeat1
                Return bus.GetBoneCoord("seat_dside_r1")
            Case VehicleSeat.ExtraSeat2
                Return bus.GetBoneCoord("seat_pside_r1")
            Case VehicleSeat.ExtraSeat3
                Return bus.GetBoneCoord("seat_dside_r2")
            Case VehicleSeat.ExtraSeat4
                Return bus.GetBoneCoord("seat_pside_r2")
            Case VehicleSeat.ExtraSeat5
                Return bus.GetBoneCoord("seat_dside_r3")
            Case VehicleSeat.ExtraSeat6
                Return bus.GetBoneCoord("seat_pside_r3")
            Case VehicleSeat.ExtraSeat7
                Return bus.GetBoneCoord("seat_dside_r4")
            Case VehicleSeat.ExtraSeat8
                Return bus.GetBoneCoord("seat_pside_r4")
            Case VehicleSeat.ExtraSeat9
                Return bus.GetBoneCoord("seat_dside_r5")
            Case VehicleSeat.ExtraSeat10
                Return bus.GetBoneCoord("seat_pside_r5")
            Case VehicleSeat.ExtraSeat11
                Return bus.GetBoneCoord("seat_dside_r6")
            Case VehicleSeat.ExtraSeat12
                Return bus.GetBoneCoord("seat_pside_r6")
            Case Else
                Return bus.GetBoneCoord(0)
        End Select
    End Function

    Public Sub DrawingSeat(uiRect As UIResRectangle, uiText As UIResText, bus As Vehicle, pos As Point, textString As String, seat As VehicleSeat)
        If bus.PassengerSeats >= (seat + 1) Then
            Dim Rect As New UIResRectangle(pos, New Size(18, 18), If(bus.IsSeatFree(seat), Color.FromArgb(200, Color.LightGreen), Color.FromArgb(200, Color.White))) : Rect.Draw()
            Dim Text As New UIResText(textString, New Point(Rect.Position.X + (Rect.Size.Width / 2), Rect.Position.Y), 0.2F, Color.Black, GTA.Font.ChaletLondon, UIResText.Alignment.Centered) : Text.Outline = True : Text.Draw()
        End If
    End Sub

    Public Enum Difficulty
        Normal
        Hard
        VeryHard
        ExtremelyHard
    End Enum

    <Extension()>
    Public Sub StartScenarioInPlace(ped As Ped, scenario As String, playEnterAnim As Boolean)
        Native.Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, ped.Handle, scenario, -1, playEnterAnim)
    End Sub

    Dim hooker As New List(Of Model) From {PedHash.Hooker01SFY, PedHash.Hooker02SFY, PedHash.Hooker03SFY, PedHash.DeadHooker}
    <Extension()>
    Public Function IsHooker(ped As Ped) As Boolean
        Return hooker.Contains(ped.Model)
    End Function

    <Extension()>
    Public Function FrontBumper(bus As Vehicle) As Vector3
        If bus.HasBone("bumper_f") Then Return bus.GetBoneCoord("bumper_f") Else Return bus.Position
    End Function

    <Extension()>
    Public Function IsBus(bus As Vehicle) As Boolean
        If bus.PassengerSeats > 9 AndAlso bus.ClassType = VehicleClass.Service Then
            Return True
        Else
            Return False
        End If
    End Function

    <Extension()>
    Public Sub SetPropDoor(prop As Prop, close As Boolean)
        Native.Function.Call(Hash._DOOR_CONTROL, prop.Model, prop.Position.X, prop.Position.Y, prop.Position.Z, close, 0F, 50.0F, 0F)
    End Sub

    Public Function IsNamedRenderTargetRegistered(prop_ex_bus_text As String) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.IS_NAMED_RENDERTARGET_REGISTERED, prop_ex_bus_text)
    End Function

    Public Sub RegisterNamedRenderTarget(prop_ex_bus_text As String)
        Native.Function.Call(Hash.REGISTER_NAMED_RENDERTARGET, prop_ex_bus_text, False)
    End Sub

    Public Sub ReleaseNamedRenderTarget(prop_ex_bus_text As String)
        Native.Function.Call(Hash.RELEASE_NAMED_RENDERTARGET, prop_ex_bus_text)
    End Sub

    Public Function GetNamedRenderTargetRenderId(prop_ex_bus_text As String) As Integer
        Return Native.Function.Call(Of Integer)(Hash.GET_NAMED_RENDERTARGET_RENDER_ID, prop_ex_bus_text)
    End Function

    Public Sub SetTextRenderId(id As Integer)
        Native.Function.Call(Hash.SET_TEXT_RENDER_ID, id)
    End Sub

    <Extension()>
    Public Function IsNamedRenderTargetLinked(ex_prop_ex_bus_text As Prop) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.IS_NAMED_RENDERTARGET_LINKED, ex_prop_ex_bus_text.Model)
    End Function

    <Extension()>
    Public Sub LinkNamedRenderTarget(ex_prop_ex_bus_text As Prop)
        Native.Function.Call(Hash.LINK_NAMED_RENDERTARGET, ex_prop_ex_bus_text.Model)
    End Sub

    <Extension()>
    Public Sub SetText(scaleform As Scaleform, prop As Prop, text As String)
        scaleform.CallFunction("SET_ORGANISATION_NAME", text, -1, 0, 7)
        If Not IsNamedRenderTargetRegistered("prop_ex_office_text") Then
            RegisterNamedRenderTarget("prop_ex_office_text")
            prop.LinkNamedRenderTarget
            If Not prop.IsNamedRenderTargetLinked Then
                ReleaseNamedRenderTarget("prop_ex_office_text")
            End If
        End If
    End Sub

    <Extension()>
    Public Sub DrawText(scaleform As Scaleform, prop As Prop)
        If scaleform.IsValid Then
            Dim bus As Vehicle = prop.GetEntityAttachedTo
            Dim id As Integer = GetNamedRenderTargetRenderId("prop_ex_office_text")
            SetTextRenderId(id)
            If World.GetDistance(Bus.Position, Game.Player.Character.Position) < 25.0F Then
                scaleform.Render2DScreenSpace(New PointF(0.196 * 1.75, 0.345 * 1.5), New SizeF(0.46 * 2.5, 0.66 * 2.5))
            End If
        End If
    End Sub

    <Extension()>
    Public Sub DrawText(scaleform As Scaleform, bus As Vehicle)
        If scaleform.IsValid Then
            Dim id As Integer = GetNamedRenderTargetRenderId("prop_ex_office_text")
            SetTextRenderId(id)
            If World.GetDistance(bus.Position, Game.Player.Character.Position) < 25.0F Then
                'scaleform.Render2DScreenSpace(New PointF(0.196 * 1.75, 0.345 * 1.5), New SizeF(0.46 * 2.5, 0.66 * 2.5))
                scaleform.Render2DScreenSpace(New PointF(0F, 0F), New SizeF(0F, 0F))
            End If
        End If
    End Sub

    Public Function IsDLCInstalled() As Boolean
        Return Native.Function.Call(Of Boolean)(Hash.IS_DLC_PRESENT, "bussim".GetHashKey)
    End Function
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