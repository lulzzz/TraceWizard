   M S B A M L   `   `   `  ����   TraceWizard&     TraceWizard.TwApp.ProgressWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QWPFToolkit, Version=3.5.40128.1, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -   $��Window��  � Title$  Progress��$	��142��$	��300�� � WindowStyle$
 None=� � 
ResizeMode$ NoResize=� � WindowStartupLocation$ CenterScreen=� � Topmost . .���� $��2q�
�������._��  � RowDefinitions �� $��32 ��� $��32 ��� $��* ��� $��42 �.+��� $��0�� �� Orientation$ 
Horizontal=�$��Center=�$��Center=�.+��� $��14(�Processing:�� $��14(�$��5,0,0,0q�����  �� Path$ Current� �� ElementName$ Window��	 �� Mode$	 OneWay=��� $��14(�$��5,0,5,0q�of�� $��14(����� $ Total�$ Window��$	 OneWay=��� $��1��$ 
Horizontal=�$��Center=�$	��Top=�.+��� $��12(�
KeyCode:�� $��12(�$��5,0,0,0q����� $ KeyCode�$ Window��$	 OneWay=�� $��2��$	��200��$��20��
 � Minimum$
 0Q� � Value �� $ Current�$ Window��$	 OneWay=� � Maximum �� $ Total�$ Window��$	 OneWay=��� $��3��$ 
Horizontal=�$��Center=�.+��� -   $��6,6,6,6q�$��80��$��30��.��Cancel