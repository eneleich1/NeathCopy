   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:NeathCopy.ResourcesNeathCopy.Resources  5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� 
TargetType �� Property �� Value �� 
TargetType � 	Resources'   !System.Windows.VisualStateManager    VisualStateGroups%  System.Windows.VisualStateGroup   States   System.Windows.VisualState   
Storyboard g� Duration	 �� 
TargetName
 �� TargetProperty R� KeyTime R� Value ��Name �� CornerRadius   ProgressBarTrackBorderBrush  Key �� EndPoint �� 
StartPoint � Color � Offset �� Color �Name ��Name  animate_Brush  ControlLightColor  ControlMediumColor  ControlDarkColor�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      +local!clr-namespace:NeathCopy.Resources  5      %7  '�       7  �� 5      "  �5   
   .�5   
   ��  � �� 5	      �� 5
      " �6"   .R�5      � #��z��6    5      ���� 5      '��       "  ��6$   .�5   "   ��  � ��$ 9(�6=   �� 5   "    � ��
 �   �6?   �� 5   "    � ��$ Center=�6F   5      5       5        5      CommonStates��. 5   "     Determinate��6.     5   "   Indeterminate��. 5   &   �� .��5   *   T� $ 00:00:00D�6H   $	 PART_Indicator��5   4   $
 
Background�5   4   ..�5   .   i� $ 00:00:00��6E    5   2   ��5   6   Transparent��6S   5    /   5!   +   5#   '   5$   #   5%      .+�5(      �� $ 
PART_Track��6!   $ 0��65   $	��0.5q�6F   #���   5)   !   5*      �� 5,      $ PART_Indicator��6!   $��1q�69   $ 0��5-      $��0q�5.      $
��Left=�5/      #��z��50       52      ���� 53   "   animate_Brush $ 0,0.5&�6L   $ 1,0.5&�6]   .��54   &   � $ Transparent��63   $ 0Q�6G   � 55   &   $ White��63   $	 0.5Q�6A   � 56   &   $ Transparent��63   $ 1Q�6G   58   #   �� 59   "   animate_Brush1 $	 0,0&�5:      $	 1,0&�5;      ��5=   *   � � 5>   .   $ 	#000000FF��6;   $ 0Q�5?      � 5@   .   $ 	#600000FF��6;   $	 0.4Q�5A      � 5B   .   $ 	#600000FF��6;   $	 0.6Q�5C      � 5D   .   $ 	#000000FF��6;   $ 1Q�5E      5F   +   5H   #   5I      ��5J      �� 5K   "   $ Transparent��62   ._�5Y      � $ 	Animation��67   }�. .+�5Z   "   �� $ PART_GlowRect��6,   $	��100��5[      $
��Left=�5\      8��  $��
-100,0,0,0q�5_      5`      5a      5c      5d      5f      �� 5g   
    � �� 5h      �� 5i      $	 0,1&�6&   $	 0,0&�5j      .��5k      � # �  6#   $ 0Q�5l      � 5m      # �  6#   $ 1Q�5n      5o      5q      �� 5r   
    � �� 5s      �� 5t      $ 0.5,1&�6&   $ 0.5,0&�5u      .��5v      � # �  6#   $ 0Q�5w      � 5x      # �  6#   $ 1Q�5y      5z      5|      5}      5      