   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:NeathCopy.Themes.ControlsStylesNeathCopy.Themes.ControlsStyles  5      1clr-namespace:NeathCopy.ToolsNeathCopy.Tools  5          Theme1ToolBarPanelW SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ToolBarBackground  �� 
StartPoint �� EndPoint � Color � Offset �� 
TargetType  myButtonStyle �� Property �� Value �� 
TargetType �� Triggers	 P� Property
 P� Value �� 
TargetName  ButtonMouseHoverBrush  ButtonPressedBrush T� Y  ToolBarPanelStyle �� Orientation � 	Resources �� BasedOn  moveFirstBrush J� SnapsToDevicePixels    NeathCopy.Tools.MyCommands   MoveFirstCommand  t23 � ToolTip  t24  moveUpBrush   MoveUpCommand 	 moveDownBrush 
 t25   MoveDownCommand  moveLastBrush  t26   MoveLastCommand  removeFromListBrush  t27   RemoveCommand  saveListBrush  t20   SaveCommand  loadListBrush  t19   LoadCommand   5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      7local-clr-namespace:NeathCopy.Themes.ControlsStyles  5      'toolsclr-namespace:NeathCopy.Tools  5      %�	  &	        &	 �     &	 p    7  �� 5      $  0,0.5&�64   $ 1,0.5&�6G   ��5
      � � 5      $ White��6   $	 0.0Q�6-   � 5      $ White��6   $	 1.0Q�6-   5      5      �� 5      " ��6   .�5   
   ��  �@J�SnapsToDevicePixels . 60   �� 5   
    � ��$ 25��6#   �� 5   
    � ��$ 25��6"   �� 5   
    � ��$ 0,2,0,2q�6#   �� 5   
    � �� 5      �� 5      " ��6"   .R�5      �� $��border��6   $��1q�5      #��z��5      ._�5      � $��grid1��6   .+�5      � $��
storeColor��6#   $��3q�65   #��z��6@   .+�5   "   �� $��content��5    @   $��Center=�5   3   $��Center=�5    %   6@   5!      5#      5$       5%      P� 5&      	 �@J�IsMouseOver
 . . �5'      �� $ grid1��6%   # �  6N    � ��5(      P� 5)      	 � ��
 . . �5*      �� # �  6;    � ���� 5,      $ content��6%    � z� 5-   "   T� 5.   &   $	 1.0Q�69   50      51      P� 52      	 �@��IsDefaulted
 . . �53      �� $ border��6%    � �� �	#FF2828286P   54      P� 55      	 �@J�	IsFocused
 . . �56      �� $ border��6%    � �� �	#FF2828286P   57      P� 58      	 � {�
 .  . �59      �� $ border��6%    �@J�Opacity$	 0.7Q�6L   �� 5:       � ��
 �����6;   5;      5=      5?      5@      �� 5B      " ��6   .�5C   
   ��  � �� 5D      �� 5E      .R�5F      a� $��Center=�6#   $ Vertical=�6>    5H      ���� 5I      '��       " ��6$   8   .�5J   "   ��  � ��$ 25��6;   �� 5K   "    � ��$ 25��6:   �� 5L   "    � ��$ 0,2,0,2q�6;   5M      5N      .+�5Q      �� $��moveFirst_button��6"   #���  6:    . 
���  ��#��Z 5R   3   # �  6j   5S      �� 5V      $��moveUp_button��6!   # �  66   #���  6V   #��Z 5W   !   5X      �� 5[      $��moveDown_button��6!   #��� 	 68   # � 
 6e   #��Z 5\   !   5]      �� 5`      $��moveLast_button��6!   #���  68   # �  6e   #��Z 5a       5b      �� 5e      $��remove_button��6!   #���  66   # �  6i   #��Z 5f   !   5g      �� 5j      $��saveList_button��6!   #���  68   # �  6e   #��Z 5k       5l      �� 5o      $��loadList_button��6!   #���  68   # �  6f   #��Z 5p   !   5q      5t      5u      5w      5x      5{      