   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:NeathCopy.ResourcesNeathCopy.Resources  5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source   MenuScrollViewer �� 
TargetType �� Property �� Value �� 
TargetType  ControlBackgroundBrush � ColumnDefinitions � RowDefinitions  NormalBorderBrush �� Orientation	 K� IsItemsHost  SolidBorderBrush
 m� IsSubmenuOpen � AllowsTransparency J� SnapsToDevicePixels  WindowBackgroundBrush-   'System.Windows.Input.KeyboardNavigation    DirectionalNavigation �� Triggers P� Property P� Value �� 
TargetName  NormalBrush P� 
SourceName  DisabledForegroundBrush z� SharedSizeGroup  
GlyphBrush ��Name m� InputGestureText  SelectedBackgroundBrush 	 ButtonMouseHoverBrush � HorizontalOffset �� Triggers  m� TopLevelHeaderTemplateKey m� TopLevelItemTemplateKey m� SubmenuHeaderTemplateKey m� SubmenuItemTemplateKey�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      +local!clr-namespace:NeathCopy.Resources  5        5      �� 5   
   $ Shared.xaml?�6   %  &	        'o� _    7  7  (�� �    54      *MenuItem.SeparatorStyleKey+)7  (�� �    5A      *$"MenuItem.TopLevelHeaderTemplateKey+)7  7  7   7  7  (�� �    5s      *" MenuItem.TopLevelItemTemplateKey+)7  7  (�� v	    5�      *!MenuItem.SubmenuItemTemplateKey+)7  7  7  7  7  (�� �    5�      *#!MenuItem.SubmenuHeaderTemplateKey+)7  7  7  7   7  7  'm� ]    7 7 7 7 �� 5
      " ��6%   .�5   
   ��  �@�OverridesDefaultStyle . 62   �� 5   
    � �� 5      �� 5      " ��6"   .R�5      � #���  6    5      �� 5      $
��Auto �6/   �� 5       5      �� 5      �� 5      $
��Auto �6,   .+�5      �� $��1��61   5      5      5      5      �� 5"      " o�6"   .�5#   
   ��  �@�OverridesDefaultStyle . 62   �� 5$   
    �@J�SnapsToDevicePixels . 60   �� 5%   
    � �� 5&      �� 5'      " o�6"   .R�5(      �� 8��  8�� $��1q�5+      ._�5,      �� #���  6%   }�. $ 
Horizontal=�6o   	 . 6�   5-      5.      50      51      �� 54      " ��6:   .�55   
   ��  � ��$ 1��6#   �� 56   
    � ��$ 0,4,0,4q�6#   �� 57   
    � �� 58      �� 59      " ��6"   .R�5:      �� 8��  $��1q�6M   5;      5=      5>      �� 5A      " m�6L   .R�5B   
   �� $��Border��6   ._�5C      � .+�5D      �� $��6,3,6,3q�5E   	   $��Header��5F   	   ��. 5G   	   � 5H      $��Popup��5I   	   $��Bottom=�5J   	   #��z
 5K   	    . |�.  $
��Fade=�5N   	   .��5O      �� $��SubmenuBorder��5P       . 8��  8�� $��1q�5T      ._�5U      �� ��. 8�� .��5W      �� 	 . $ Cycle=�5Y      5Z      5[      5\      5]      5^       5_   
   P� 5`      " �@m�IsSuspendingPopupAnimation . . �5a      �� $ Popup��6    � ��$
 None=�6F   5b      P� 5c       �@m�IsHighlighted . . �5d      �� $ Border��6   8   � ��6C   �� 5e      $ Border��6    � ��
 ���� 6D   5f      P� 5g      $ Popup��6    �@�AllowsTransparency . . �5h      �� $ SubmenuBorder��6    �@��CornerRadius$ 0,0,4,4��6L   �� 5i      $ SubmenuBorder��6    �@��Padding$ 0,0,0,3q�6G   5j      P� 5k       � {� .  . �5l      �� 8   � ��6/   5m      5o      �� 5r      " m�5t      .R�5u   
   �� $��Border��6   ._�5v      � .+�5w      �� $��6,3,6,3q�5x   	   $��Header��5y   	   ��. 5z   	   5{      5|       5}   
   P� 5~       �@m�IsHighlighted . . �5      �� $ Border��6   8    � ��6C   �� 5�      $ Border��6    � ��
 ���� 6D   5�      P� 5�       � {� .  . �5�      �� 8   � ��6/   5�      5�      �� 5�      " m�5�      .R�5�   
   �� $��Border��6   ._�5�      � $��grid1��6   #���  6     5�      �� 5�      $
��Auto �6'   $
 Icon��64   �� 5�      $��* �6'   �� 5�      $
��Auto �6'   $ Shortcut��64   �� 5�      $��13 �6'   .+�5�      �� $
��Icon��5�   	   $��6,0,6,0q�5�   	   $��Center=�5�   	   $
��Icon��5�   	   �� 5�      $��Check��5�   	   $��13��5�   	   $��13��6   $y�	Collapsed=�5�   	   $��6,0,6,0q�5�   	   8��  $��1q�5�   	   8�� ._�5�      F� $��	CheckMark��5�      $��7��5�      $��7��6   $y�Hidden=�5�       .  8�� $��2��5�      +����        ���j ��j �    ��j ���j     	5�      5�      �� 5�      $��
HeaderHost��5�   	   $��1��5�   	   $��Header��5�   	   ��. 5�   	   �� 5�      $ InputGestureText��6   $��2��5�   	   #��z 5�   	   $��5,2,0,2q�5�   	   $��Right=�5�   	   5�      5�       5�   
   P� 5�       6&   Z�  �@m�Icon. �5�      �� $
 Icon��6    � y�$ Hidden=�6A   5�      P� 5�       �@m�	IsChecked . . �5�      �� $ 	CheckMark��6    � y�$ Visible=�6F   5�      P� 5�       �@m�IsCheckable . . �5�      �� $ Check��6    � y�$ Visible=�6B   �� 5�      $
 Icon��6    � y�$ Hidden=�6A   5�      P� 5�       �@m�IsHighlighted . . �5�      �� $ Border��6   8   � ��6C   5�      P� 5�       � {� .  . �5�      �� 8   � ��6/   5�      P� 5�       �@J�IsMouseOver . . �5�      �� $ grid1��6   # � 	 6B    � ��5�      5�      �� 5�      " m�5�      .R�5�   
   �� $��Border��6   ._�5�      � $��grid1��6   #���  6     5�      �� 5�      $
��Auto �6'   $
 Icon��64   �� 5�      $��* �6'   �� 5�      $
��Auto �6'   $ Shortcut��64   �� 5�      $��13 �6'   .+�5�      �� $
��Icon��5�   	   $��6,0,6,0q�5�   	   $��Center=�5�   	   $
��Icon��5�   	   �� 5�      $��
HeaderHost��5�   	   $��1��5�   	   $��Header��5�   	   ��. 5�   	   �� 5�      $ InputGestureText��6   $��2��5�   	   #��z 5�   	   $��5,2,2,2q�5�   	   $��Right=�5�   	   F� 5�      $��3��5�   	   $��Center=�5�   	   $��Center=�5�   	   "����        �    ��j � 	= �g5 	8��  5�   	   � 5�      $��Popup��5�   	   $��Right=�5�   	   $ -4��5�   	   #��z
 5�   	    . |�.  $
��Fade=�5�   	   .��5�      �� $��SubmenuBorder��5�       . 8�� 8�� $��1q�5�      ._�5�      �� ��. 8�� .��5�      �� 	 . $ Cycle=�5     5     5     5     5     5      5  
   P� 5	      6&   Z�  �@m�Icon. �5
     �� $
 Icon��6    � y�$ 	Collapsed=�6A   5     P� 5      �@m�IsHighlighted . . �5     �� $ Border��6   8   � ��6C   5     P� 5     $ Popup��6    �@�AllowsTransparency . . �5     �� $ SubmenuBorder��6    �@��CornerRadius$ 4��6L   �� 5     $ SubmenuBorder��6    �@��Padding$ 0,3,0,3q�6G   5     P� 5      � {� .  . �5     �� 8   � ��6/   5     P� 5      �@J�IsMouseOver . . �5     �� $ grid1��6   # � 	 6B    � ��5     5     �� 5     " m�6&   .�5  
   ��  �@�OverridesDefaultStyle . 62    5  
   P� 5       �@m�Role$ TopLevelHeader=�6&   . �5!     ��  � ��8   6-   �� 5"      �@�IsSharedSizeScope . 6;   5#     P� 5$      �@m�Role$ TopLevelItem=�6&   . �5%     ��  � ��8  6-   5&     P� 5'      �@m�Role$ SubmenuHeader=�6&   . �5(     ��  � ��8  6-   5)     P� 5*      �@m�Role$ SubmenuItem=�6&   . �5+     ��  � ��8  6-   5,     5.     51     