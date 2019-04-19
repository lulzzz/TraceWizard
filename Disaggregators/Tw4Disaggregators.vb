Option Explicit On
Option Strict On

Imports TraceWizard.Disaggregation
Imports TraceWizard.Entities
Imports TraceWizard.Logging

Namespace TraceWizard.Disaggregation.Disaggregators.Tw4

    Public Class NullDisaggregator
        Inherits Tw4Disaggregator

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Null Disaggregator"
            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Disaggregates by starting new event whenever volume drops to zero"
            End Get
        End Property

        Public Overrides Function Disaggregate() As Events
            Events = New Events
            FlowEnumerator = Flows.GetEnumerator()

            Dim evt As [Event] = Nothing
            Dim flow As Flow = Nothing

            Do
                flow = GetNext()

                If IsFlowNothing(flow) Then
                    If Not IsNothing(evt) Then
                        AddToEvents(evt)
                        evt = Nothing
                    End If

                    Exit Do
                ElseIf flow.Volume = 0 Then
                    If Not IsNothing(evt) Then
                        AddToEvents(evt)
                        evt = Nothing
                    End If

                Else
                    If IsNothing(evt) Then
                        evt = New [Event]
                        evt.Channel = Channel.Base
                    End If

                    AddFlow(evt, flow)
                End If

            Loop

            Events.UpdateChannel()
            Events.UpdateSuperPeak(Log)
            Events.UpdateLinkedList()

            Events.UpdateVolume()
            Events.UpdateOriginalVolume()

            Return Events
        End Function

        Public Overrides Function ToString() As String
            Return "Null"
        End Function

    End Class

    Public Class Tw4PostTrickleMergeMidnightSplitDisaggregator
        Inherits Tw4PostTrickleMergeDisaggregator

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Tw4 Post-Trickle Merge Midnight Split Disaggregator"
            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Disaggregates using Tw4 algorithm with post-trickle merge and midnight split"
            End Get
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides Function Disaggregate() As Events
            Dim Events As Events
            Events = MyBase.Disaggregate()
            Events.MidnightSplit()
            Events.UpdateLinkedList()
            Return Events
        End Function

    End Class

    Public Class Tw4PostTrickleMergeDisaggregator
        Inherits Tw4Disaggregator

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Tw4 Post-Trickle Merge Disaggregator"
            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Disaggregates using Tw4 algorithm with post-trickle merge"
            End Get
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Overrides Function Disaggregate() As Events
            Dim Events As Events
            Events = MyBase.Disaggregate()
            Events.MergeAllPostTrickleHorizontally()
            Events.UpdateLinkedList()
            Return Events
        End Function

    End Class

    Public Class Tw4Disaggregator
        Inherits Disaggregator

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Tw4 Disaggregator"
            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Disaggregates using Tw4 algorithm"
            End Get
        End Property

        Public Sub New()
            MyBase.New()
        End Sub

        Public Sub New(ByVal Log As Log)
            MyBase.New(Log)
        End Sub

        Public Sub New(ByVal Log As Log, ByVal Parameters As Parameters)
            MyBase.New(Log, Parameters)
        End Sub

        Protected Property Flows() As List(Of Flow)
            Get
                Flows = Log.Flows
            End Get
            Set(ByVal value As List(Of Flow))
                Throw New NotImplementedException
            End Set
        End Property

        Protected FlowEnumerator As IEnumerator(Of Flow)
        Protected Events As Events

        Public Overrides Function Disaggregate() As Events
            On Error GoTo Procedure_Err

            Const cintBegin As Integer = 0
            Const cintUndeterminedContinue As Integer = 1
            Const cintTrickleContinue As Integer = 2
            Const cintBaseContinue As Integer = 3
            Const cintBaseOrTrickleContinue As Integer = 4
            Const cintBaseEnd As Integer = 5
            Const cintSuperContinue As Integer = 6
            Const cintEnd As Integer = 7

            Dim evt As [Event] = New [Event]()
            Dim eventSuper As [Event] = New [Event]()
            Dim eventWork As [Event] = New [Event]()
            Dim flow As Flow = Nothing
            Dim flowBase As Flow = Nothing
            Dim flowSuper As Flow = Nothing

            Dim intState As Integer
            Dim blnDone As Boolean

            Events = New Events
            FlowEnumerator = Flows.GetEnumerator()

            intState = cintBegin

            Do

                Select Case intState

                    Case cintBegin
                        flow = GetNext()

                        If IsFlowNothing(flow) Then
                            intState = cintEnd
                        ElseIf Not flow.Volume = 0 Then
                            evt = New [Event]
                            intState = cintUndeterminedContinue
                        End If

                    Case cintUndeterminedContinue
                        AddFlow(evt, flow)

                        If Not VolumeBigEnoughForEvent(evt) Then
                            flow = GetNext()

                            If IsFlowNothing(flow) Then
                                evt.Channel = Channel.Runt
                                AddToEvents(evt)

                                intState = cintEnd
                            ElseIf flow.Volume = 0 Then
                                evt.Channel = Channel.Runt
                                AddToEvents(evt)

                                intState = cintBegin
                            End If
                        Else
                            If PeakTooHighForTrickle(evt) Then
                                evt.Channel = Channel.Base
                                flow = GetNext()

                                intState = cintBaseContinue
                            Else
                                evt.Channel = Channel.Trickle
                                flow = GetNext()

                                intState = cintTrickleContinue
                            End If
                        End If

                    Case cintTrickleContinue
                        If IsFlowNothing(flow) Then
                            AddToEvents(evt)

                            intState = cintEnd
                        ElseIf flow.Volume = 0 Then
                            AddToEvents(evt)

                            intState = cintBegin
                        ElseIf Not FlowTooHighForTrickle(flow) Then
                            AddFlow(evt, flow)

                            flow = GetNext()
                        Else
                            AddToEvents(evt)
                            evt = New [Event]

                            intState = cintUndeterminedContinue
                        End If

                    Case cintBaseContinue

                        If IsFlowNothing(flow) Then
                            AddToEvents(evt)

                            intState = cintEnd
                        ElseIf flow.Volume = 0 Then
                            AddToEvents(evt)

                            intState = cintBegin
                        ElseIf FlowTooLowForBaseEvent(flow) Then
                            eventWork = New [Event]

                            intState = cintBaseEnd
                        ElseIf Not FlowTooHighForTrickle(flow) Then
                            eventWork = New [Event]

                            intState = cintBaseOrTrickleContinue
                        ElseIf CanBeginSuperEvent(evt, flow) Then
                            SetBaseline(evt)

                            eventSuper = New [Event]
                            eventSuper.Channel = Channel.Super
                            intState = cintSuperContinue
                        Else
                            AddFlow(evt, flow)
                            flow = GetNext()
                        End If

                    Case cintBaseOrTrickleContinue
                        AddFlow(eventWork, flow)

                        If VolumeBigEnoughForEvent(eventWork) _
                            And DurationLongEnoughForTrickle(eventWork) Then

                            AddToEventsThenConcatenate(evt, eventWork)
                            evt.Channel = Channel.Trickle

                            flow = GetNext()

                            intState = cintTrickleContinue
                        Else
                            flow = GetNext()

                            If IsFlowNothing(flow) Then
                                ConcatenateThenAddToEvents(evt, eventWork)

                                intState = cintEnd
                            ElseIf flow.Volume = 0 Then
                                ConcatenateThenAddToEvents(evt, eventWork)

                                intState = cintBegin
                            ElseIf FlowTooHighForTrickle(flow) Then
                                Concatenate(evt, eventWork)
                                AddFlow(evt, flow)

                                flow = GetNext()

                                intState = cintBaseContinue
                            ElseIf FlowTooLowForBaseEvent(flow) Then
                                Concatenate(evt, eventWork)

                                intState = cintBaseEnd
                            End If
                        End If

                        If intState <> cintBaseOrTrickleContinue Then
                            eventWork = New [Event]
                        End If

                    Case cintBaseEnd
                        AddFlow(eventWork, flow)

                        If VolumeBigEnoughForEvent(eventWork) Then
                            AddToEventsThenConcatenate(evt, eventWork)

                            If PeakTooHighForTrickle(evt) Then
                                evt.Channel = Channel.Base
                                flow = GetNext()

                                intState = cintBaseContinue
                            Else
                                evt.Channel = Channel.Trickle
                                flow = GetNext()

                                intState = cintTrickleContinue
                            End If

                        Else
                            flow = GetNext()

                            If IsFlowNothing(flow) Then
                                ConcatenateThenAddToEvents(evt, eventWork)

                                intState = cintEnd
                            ElseIf flow.Volume = 0 Then
                                ConcatenateThenAddToEvents(evt, eventWork)

                                intState = cintBegin
                            ElseIf FlowTooHighForTrickle(flow) Then
                                AddToEventsThenConcatenate(evt, eventWork)

                                intState = cintUndeterminedContinue
                            End If
                        End If

                        If intState <> cintBaseEnd Then
                            eventWork = New [Event]
                        End If

                    Case cintSuperContinue
                        flowBase = New Flow
                        flowSuper = New Flow

                        ApportionFlow(flow, flowBase, flowSuper, evt.Baseline)

                        AddFlow(evt, flowBase)
                        AddFlow(eventSuper, flowSuper)

                        flow = GetNext()

                        If IsFlowNothing(flow) Then
                            AddToEvents(eventSuper)
                            AddToEvents(evt)

                            intState = cintEnd
                        ElseIf flow.Volume = 0 Then
                            AddToEvents(eventSuper)
                            AddToEvents(evt)

                            intState = cintBegin
                        ElseIf FlowTooLowForSuper(evt, flow) Then
                            AddToEvents(eventSuper)

                            intState = cintBaseContinue
                        End If

                        If intState <> cintSuperContinue Then
                            eventSuper = New [Event]
                        End If

                    Case cintEnd
                        blnDone = True

                End Select

            Loop Until blnDone

            Events.UpdateChannel()
            Events.UpdateSuperPeak(Log)
            Events.UpdateLinkedList()

            Events.UpdateVolume()
            Events.UpdateOriginalVolume()

            Return Events

            Exit Function
Procedure_Err:
            Throw New Exception("Error in DisaggretagorTw4: " + Err.Description)
        End Function

        Protected Function GetLast(ByVal evt As [Event]) As Flow
            Return evt(evt.Count - 1)
        End Function

        Protected Sub AddToEvents(ByVal evt As [Event])
            If evt.Channel = Channel.Runt Then Exit Sub
            'evt.NormalizeVolume()
            Events.Add(evt)
        End Sub

        Protected Sub AddFlow(ByVal [Event] As [Event], ByVal flow As Flow)
            [Event].Add(flow)
        End Sub

        Protected Sub Concatenate(ByVal target As [Event], ByVal source As [Event])
            For Each flow In source
                AddFlow(target, flow)
            Next
        End Sub

        Protected Function GetNext() As Flow
            If FlowEnumerator.MoveNext Then
                Return FlowEnumerator.Current()
            Else
                Return Nothing
            End If
        End Function

        Protected Function IsFlowNothing(ByVal Flow As Flow) As Boolean
            Return IsNothing(Flow)
        End Function

        Protected Sub ApportionFlow(ByVal Flow As Flow, ByRef FlowBase As Flow, ByRef FlowSuper As Flow, ByVal Baseline As Double)
            FlowSuper = New Flow(Flow.TimeFrame, Flow.Rate - Baseline)
            FlowBase = New Flow(Flow.TimeFrame, Baseline)
        End Sub

        Protected Function FlowTooHighForTrickle(ByVal Flow As Flow) As Boolean
            FlowTooHighForTrickle = (Flow.Rate > Parameters.TrickleFlowMax)
        End Function

        Protected Function FlowTooLowForBaseEvent(ByVal Flow As Flow) As Boolean
            FlowTooLowForBaseEvent = (Flow.Rate <= Parameters.BaseFlowMinContinue)
        End Function

        Protected Function CanBeginSuperEvent(ByVal evt As [Event], ByVal Flow As Flow) As Boolean
            CanBeginSuperEvent = (evt.Volume >= Parameters.BaseVolMinBeginSuper _
                And evt.Duration.TotalSeconds >= Parameters.BaseDurMinBeginSuper _
                And PassesDeltaTestForSuper(evt, Flow))
        End Function

        Protected Function PassesDeltaTestForSuper(ByVal evt As [Event], ByVal Flow As Flow) As Boolean
            PassesDeltaTestForSuper = (DeltaFlow(evt, Flow) >= Parameters.BaseDeltaFlowMinBeginSuper _
                And DeltaFlowPercent(evt, Flow) >= Parameters.BaseDeltaFlowPercentMinBeginSuper)
        End Function

        Protected Function DeltaFlow(ByVal evt As [Event], ByVal Flow As Flow) As Double
            Dim FlowLast As Flow
            FlowLast = GetLast(evt)
            DeltaFlow = Flow.Rate - FlowLast.Rate
        End Function

        Protected Function DeltaFlowPercent(ByVal evt As [Event], ByVal Flow As Flow) As Double
            Dim FlowLast As Flow
            FlowLast = GetLast(evt)
            DeltaFlowPercent = Flow.Rate / FlowLast.Rate
        End Function

        Protected Function VolumeBigEnoughForEvent(ByVal evt As [Event]) As Boolean
            VolumeBigEnoughForEvent = (evt.Volume >= Parameters.VolMinBeginEvent)
        End Function

        Protected Function PeakTooHighForTrickle(ByVal evt As [Event]) As Boolean
            PeakTooHighForTrickle = (evt.Peak > Parameters.TrickleFlowMax)
        End Function

        Protected Function DurationLongEnoughForTrickle(ByVal evt As [Event]) As Boolean
            DurationLongEnoughForTrickle = (evt.Duration.TotalSeconds >= Parameters.TrickleDurMinBeginAfterBase)
        End Function

        Protected Function FlowTooLowForSuper(ByVal evt As [Event], ByVal Flow As Flow) As Boolean
            FlowTooLowForSuper = (Flow.Rate <= evt.BaselineWithTolerance)
        End Function

        Protected Sub SetBaseline(ByVal evt As [Event])
            Dim Flow As Flow
            Flow = GetLast(evt)
            evt.Baseline = Flow.Rate
            evt.BaselineWithTolerance = evt.Baseline + Parameters.SuperFlowAboveBaseMinContinue
        End Sub

        Protected Sub ConcatenateThenAddToEvents(ByVal evt As [Event], ByVal EventWork As [Event])
            Concatenate(evt, EventWork)
            AddToEvents(evt)
        End Sub

        Protected Sub AddToEventsThenConcatenate(ByRef evt As [Event], ByVal EventWork As [Event])
            AddToEvents(evt)
            evt = New [Event]
            Concatenate(evt, EventWork)
        End Sub

    End Class
End Namespace