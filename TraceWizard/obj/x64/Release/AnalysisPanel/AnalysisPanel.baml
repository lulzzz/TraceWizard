   M S B A M L   `   `   `  ���� F  BTraceWizard, Version=5.3.0.0, Culture=neutral, PublicKeyToken=null5clr-namespace:TraceWizard.TwAppTraceWizard.TwApp  %    TraceWizard.TwApp.AnalysisPanel   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     1localclr-namespace:TraceWizard.TwApp     ����    Settings  �� Source#  [   �� Path$* $Default.AnalysisPanelBackgroundColor� �� Mode$ OneWay=� J� InputBindings ��  �� 	Modifiers$
 Ctrlf�
 �� Key$ A��%   TraceWizard.TwApp.AnalysisPanel   SelectAllCommand �� Command# Z �� $
 Ctrlf�$ Z��   UndoCommand# Z �� $ 	Alt+Shiftf�$ 	Backspace��	   RedoCommand# Z	 �� $ F7�� 
   IncreaseZoomCenterCommand# Z
 �� $ F8��    DecreaseZoomCenterCommand# Z �� $ F9��(   !IncreaseVerticalZoomCenterCommand# Z �� $	 F10��(   !DecreaseVerticalZoomCenterCommand# Z .��]� -   $��	dockPanel��$��Stretch=�$��4q���. .+�#   TraceWizard.TwApp.TwStatusBar  -     Name$ 	StatusBar��$��Bottom=�$
��30,0q�]� -   $��dockCommandPanel��$��Bottom=�$��Stretch=���. $
��30,0q�.+�$   TraceWizard.TwApp.CommandPanel  -     Name$ CommandPanel��$
��Left=�]� -   $��dockSummaryPanel��$��Bottom=�$
��Left=���.  $
��30,4q�.+�-   'TraceWizard.TwApp.FixtureProfilesEditor  -     Name$ FixtureProfilesEditor��$
��Left=��� -   $$��SeparatorFixtureProfilesEditor��$
��Left=�#��["�]� -   $��dockEventPanelsHorizontal��$
��Left=���.  .+�"   TraceWizard.TwApp.EventPanel  -	     Name$ SelectedEventPanel��$
��Left=��� -
   $!��SeparatorSelectedEventPanel��$
��Left=�#��["�  -   $ CurrentEventPanel��$
��Left=��� -   $ ��SeparatorCurrentEventPanel��$
��Left=�#��["�%   TraceWizard.TwApp.FixturesPanel  -     Name$ FixturesPanel��$
��Left=�$
��Left=��� -   $��SeparatorLogPanel��$
��Left=�#��["�*   $TraceWizard.TwApp.LogPropertiesPanel  -     Name$ LogPropertiesPanel��$
��Left=��� -   $��SeparatorPieChartsPanel��$
��Left=�#��["�&    TraceWizard.TwApp.PieChartsPanel  -     Name$ PieChartsPanel��$
��Left=��� -   $��SeparatorReportsPanel��$
��Left=�#��["�)	   #TraceWizard.TwApp.QuickReportsPanel	  -    	 Name$ ReportsPanel��$
��Left=��� $��Bottom=�#��["�$
��30,0q�]� -   $��dockGraphToolBar��$��Bottom=�$
��Left=���.  $
��30,4q�.+�$
   TraceWizard.TwApp.GraphToolBar
  -    
 Name$ GraphToolBar��$
��Left=�$
��Left=���   GroupBoxBorderStyle#��[ ._�]� .+�]� -   $��dockEventPanelsVertical����.  $
��Left=�$	��150��.+�  -   $  SelectedEventPanelVertical��$	��Top=��� -   $)��#SeparatorSelectedEventPanelVertical��$	��Top=�#��["�  -   $ CurrentEventPanelVertical��$	��Top=�*   $TraceWizard.TwApp.StyledEventsViewer  -     Name$ StyledEventsViewer��