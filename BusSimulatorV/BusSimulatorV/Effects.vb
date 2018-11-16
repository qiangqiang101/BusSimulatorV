Imports GTA
Imports GTA.Native

Public Enum ScreenEffect
    SwitchHudIn
    SwitchHudOut
    FocusIn
    FocusOut
    MinigameEndNeutral
    MinigameEndTrevor
    MinigameEndFranklin
    MinigameEndMichael
    MinigameTransitionOut
    MinigameTransitionIn
    SwitchShortNeutralIn
    SwitchShortFranklinIn
    SwitchShortTrevorIn
    SwitchShortMichaelIn
    SwitchOpenMichaelIn
    SwitchOpenFranklinIn
    SwitchOpenTrevorIn
    SwitchHudMichaelOut
    SwitchHudFranklinOut
    SwitchHudTrevorOut
    SwitchShortFranklinMid
    SwitchShortMichaelMid
    SwitchShortTrevorMid
    DeathFailOut
    CamPushInNeutral
    CamPushInFranklin
    CamPushInMichael
    CamPushInTrevor
    SwitchSceneFranklin
    SwitchSceneTrevor
    SwitchSceneMichael
    SwitchSceneNeutral
    MpCelebWin
    MpCelebWinOut
    MpCelebLose
    MpCelebLoseOut
    DeathFailNeutralIn
    DeathFailMpDark
    DeathFailMpIn
    MpCelebPreloadFade
    PeyoteEndOut
    PeyoteEndIn
    PeyoteIn
    PeyoteOut
    MpRaceCrash
    SuccessFranklin
    SuccessTrevor
    SuccessMichael
    DrugsMichaelAliensFightIn
    DrugsMichaelAliensFight
    DrugsMichaelAliensFightOut
    DrugsTrevorClownsFightIn
    DrugsTrevorClownsFight
    DrugsTrevorClownsFightOut
    HeistCelebPass
    HeistCelebPassBw
    HeistCelebEnd
    HeistCelebToast
    MenuMgHeistIn
    MenuMgTournamentIn
    MenuMgSelectionIn
    ChopVision
    DmtFlightIntro
    DmtFlight
    DrugsDrivingIn
    DrugsDrivingOut
    SwitchOpenNeutralFib5
    HeistLocate
    MpJobLoad
    RaceTurbo
    MpIntroLogo
    HeistTripSkipFade
    MenuMgHeistOut
    MpCoronaSwitch
    MenuMgSelectionTint
    SuccessNeutral
    ExplosionJosh3
    SniperOverlay
    RampageOut
    Rampage
    DontTazemeBro
End Enum

Module Effects

    Private ReadOnly _effects As String() = {"SwitchHUDIn", "SwitchHUDOut", "FocusIn", "FocusOut", "MinigameEndNeutral", "MinigameEndTrevor", "MinigameEndFranklin", "MinigameEndMichael", "MinigameTransitionOut", "MinigameTransitionIn", "SwitchShortNeutralIn", "SwitchShortFranklinIn", "SwitchShortTrevorIn", "SwitchShortMichaelIn", "SwitchOpenMichaelIn", "SwitchOpenFranklinIn", "SwitchOpenTrevorIn", "SwitchHUDMichaelOut", "SwitchHUDFranklinOut", "SwitchHUDTrevorOut", "SwitchShortFranklinMid", "SwitchShortMichaelMid", "SwitchShortTrevorMid", "DeathFailOut", "CamPushInNeutral", "CamPushInFranklin", "CamPushInMichael", "CamPushInTrevor", "SwitchSceneFranklin", "SwitchSceneTrevor", "SwitchSceneMichael", "SwitchSceneNeutral", "MP_Celeb_Win", "MP_Celeb_Win_Out", "MP_Celeb_Lose", "MP_Celeb_Lose_Out", "DeathFailNeutralIn", "DeathFailMPDark", "DeathFailMPIn", "MP_Celeb_Preload_Fade", "PeyoteEndOut", "PeyoteEndIn", "PeyoteIn", "PeyoteOut", "MP_race_crash", "SuccessFranklin", "SuccessTrevor", "SuccessMichael", "DrugsMichaelAliensFightIn", "DrugsMichaelAliensFight", "DrugsMichaelAliensFightOut", "DrugsTrevorClownsFightIn", "DrugsTrevorClownsFight", "DrugsTrevorClownsFightOut", "HeistCelebPass", "HeistCelebPassBW", "HeistCelebEnd", "HeistCelebToast", "MenuMGHeistIn", "MenuMGTournamentIn", "MenuMGSelectionIn", "ChopVision", "DMT_flight_intro", "DMT_flight", "DrugsDrivingIn", "DrugsDrivingOut", "SwitchOpenNeutralFIB5", "HeistLocate", "MP_job_load", "RaceTurbo", "MP_intro_logo", "HeistTripSkipFade", "MenuMGHeistOut", "MP_corona_switch", "MenuMGSelectionTint", "SuccessNeutral", "ExplosionJosh3", "SniperOverlay", "RampageOut", "Rampage", "Dont_tazeme_bro"}

    Private Function EffectToString(screenEffect As ScreenEffect) As String
        If screenEffect >= 0 AndAlso CInt(screenEffect) <= _effects.Length Then Return _effects(CInt(screenEffect))
        Return "INVALID"
    End Function

    Sub Start(effectName As ScreenEffect, Optional duration As Integer = 0, Optional looped As Boolean = False)
        Native.Function.Call(Hash._START_SCREEN_EFFECT, EffectToString(effectName), duration, looped)
    End Sub

    Sub [Stop]()
        Native.Function.Call(Hash._STOP_ALL_SCREEN_EFFECTS)
    End Sub

    Sub [Stop](screenEffect As ScreenEffect)
        Native.Function.Call(Hash._STOP_SCREEN_EFFECT, EffectToString(screenEffect))
    End Sub

    Function IsActive(screenEffect As ScreenEffect) As Boolean
        Return Native.Function.Call(Of Boolean)(Hash._GET_SCREEN_EFFECT_IS_ACTIVE, EffectToString(screenEffect))
    End Function

End Module