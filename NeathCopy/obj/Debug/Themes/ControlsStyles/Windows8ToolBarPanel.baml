   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullQ-clr-namespace:NeathCopy.Themes.ControlsStylesNeathCopy.Themes.ControlsStyles  5      1clr-namespace:NeathCopy.ToolsNeathCopy.Tools  5          Windows8ToolBarPanelW SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089   ToolBarBackground  �� 
StartPoint �� EndPoint � Color � Offset  ButtonPressedBrush  ButtonMouseHoverBrush �� 
TargetType  myButtonStyle �� Property �� Value �� 
TargetType �� Triggers	 P� Property
 P� Value �� 
TargetName T� Y  ToolBarPanelStyle �� Orientation � 	Resources �� BasedOn  moveFirstBrush J� SnapsToDevicePixels    NeathCopy.Tools.MyCommands   MoveFirstCommand  t23 � ToolTip  t24  moveUpBrush   MoveUpCommand 	 moveDownBrush 
 t25   MoveDownCommand  moveLastBrush  t26   MoveLastCommand  removeFromListBrush  t27   RemoveCommand  saveListBrush  t20   SaveCommand  loadListBrush  t19   LoadCommand   5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      7local-clr-namespace:NeathCopy.Themes.ControlsStyles  5      'toolsclr-namespace:NeathCopy.Tools  5      %�
  &	        &	 �     &	 @    &	 �    &	 �    7  �� 5      $  0,0.5&�64   $ 1,0.5&�6G   ��5
      � � 5      $ #00ccff��6   $	 0.0Q�6/   � 5      $
 Blue��6   $	 1.0Q�6,   5      5      �� 5      $	  0,0&�65   $	 0,1&�6F   .��5   
   � $ White��6   $ 0Q�6%   � 5   
   $
 Blue��6   $ 1Q�6$   5      �� 5      $	  0,0&�68   $	 0,1&�6I   .��5   
   � $ 	AliceBlue��6   $ 0Q�6)   � 5   
   $ DarkBlue��6   $ 1Q�6(   5      �� 5      " ��6   .�5   
   ��  �@J�SnapsToDevicePixels . 60   �� 5   
    � ��$ 25��6#   �� 5   
    � ��$ 25��6"   �� 5    
    � ��$ 0,2,0,2q�6#   �� 5!   
    � �� 5"      �� 5#      " ��6"   .R�5$      �� $��border��6   $��1q�5%      #��z��5&      ._�5(      � $��grid1��6   .+�5)      � $��
storeColor��6#   $��3q�65   #��z��6@   .+�5*   "   �� $��content��5+   @   $��Center=�5*   3   $��Center=�5+   %   6@   5,      5.      5/       50      P� 51      	 �@J�IsMouseOver
 . . �52      �� $ grid1��6%   # �  6N    � ��53      P� 54      	 � ��
 . . �55      �� # �  6;    � ���� 57      $ content��6%    � z� 58   "   T� 59   &   $	 1.0Q�69   5;      5<      P� 5=      	 �@��IsDefaulted
 . . �5>      �� $ border��6%    � �� �	#FF2828286P   5?      P� 5@      	 �@J�	IsFocused
 . . �5A      �� $ border��6%    � �� �	#FF2828286P   5B      P� 5C      	 � {�
 .  . �5D      �� $ border��6%    �@J�Opacity$	 0.7Q�6L   �� 5E       � ��
 �����6;   5F      5H      5J      5K      �� 5M      " ��6   .�5N   
   ��  � �� 5O      �� 5P      .R�5Q      a� $��Center=�6#   $ Vertical=�6>    5S      ���� 5T      '��       " ��6$   8   .�5U   "   ��  � ��$ 25��6;   �� 5V   "    � ��$ 25��6:   �� 5W   "    � ��$ 0,2,0,2q�6;   5X      5Y      .+�5\      �� $��moveFirst_button��6"   #���  6:    . 
���  ��#��Z 5]   3   # �  6j   5^      �� 5a      $��moveUp_button��6!   # �  66   #���  6V   #��Z 5b   !   5c      �� 5f      $��moveDown_button��6!   #��� 	 68   # � 
 6e   #��Z 5g   !   5h      �� 5k      $��moveLast_button��6!   #���  68   # �  6e   #��Z 5l       5m      �� 5p      $��remove_button��6!   #���  66   # �  6i   #��Z 5q   !   5r      �� 5u      $��saveList_button��6!   #���  68   # �  6e   #��Z 5v       5w      �� 5z      $��loadList_button��6!   #���  68   # �  6f   #��Z 5{   !   5|      5~      5      5�      5�      5�      