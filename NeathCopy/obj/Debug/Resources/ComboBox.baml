   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:NeathCopy.ResourcesNeathCopy.Resources  5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source   ComboBoxToggleButton �� 
TargetType � ColumnDefinitions ��Name  ComboBoxRadius �� CornerRadius  NormalBrush  NormalBorderBrush  WindowBackgroundBrush F�Name  
GlyphBrush �� Triggers P� Property	 P� Value
 �� 
TargetName  	DarkBrush �� Value �� Property  PressedBrush  DisabledBackgroundBrush 	 DisabledBorderBrush 
 DisabledForegroundBrush  ComboBoxTextBox �� 
TargetType �� 	ClickMode d� 	IsChecked �� Path �� Mode �� RelativeSource J� IsHitTestVisible �� SelectionBoxItem �� SelectionBoxItemTemplate ��Name �� 
IsReadOnly �� 
IsReadOnly �� IsDropDownOpen � AllowsTransparency J� SnapsToDevicePixels � ActualWidth �� MaxDropDownHeight  SolidBorderBrush K� IsItemsHost-   'System.Windows.Input.KeyboardNavigation    DirectionalNavigation  P� 
SourceName! �� Triggers�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      +local!clr-namespace:NeathCopy.Resources  5        5      �� 5   
   $ Shared.xaml?�6   %�  &	        &	 �    '�� ,    �� 5	      " d�63   .R�5
   
   �  5      �� 5      �� 5      $��20 �6#   .+�5      �� $ Border��5      $��2��5      # �  5      #���  5      #���  5      $��1q�5      �� 5      $��0��5      $ 0��5      $��1q�5      #���  5      #���  5      $��0,0,1,0q�5      F� 5      $ Arrow��5      $��1��5      #���  5       $��Center=�5!      $��Center=�5"      "����        � 	=  	= � z     	5#      5$       5%   
   P� 5&       �@J�IsMouseOver	 . . �5'      �� $
 Border��6   # �  6C    � ��5(      P� 5)       �@d�	IsChecked$
	 trueY�68   . �5*      �� $
 Border��6   # �  6C    � ��5+      P� 5,       � {�	 .  . �5-      �� $
 Border��6   # �  6C    � ���� 5.      $
 Border��6   # � 	 6D    � ���� 5/      # � 
 6/    � ���� 50      $
 Arrow��6   # � 
 6<    � ��51      53      �� 55      " ��6.   .R�56   
   �� $ PART_ContentHost��6   |�.  #��z��6=   57      �� 59      " ��6&   .�5:   
   ��  �@J�SnapsToDevicePixels . 60   �� 5;   
    �@�OverridesDefaultStyle . 62   �� 5<   
    � ��$
 Auto=�6G   �� 5=   
    � ��$
 Auto=�6E   �� 5>   
    � �� . 6:   �� 5?   
    � ��$	 120��6%   �� 5@   
    � ��$ 20��6&   �� 5A   
    � �� 5B      �� 5C      " ��6"   .R�5D      � .+�5E      d� $��ToggleButton��5F      #���   5G      $��2��5H      |�.  $ Press=�5K       5J      �� $ IsDropDownOpen�$ TwoWay=� �� *TemplatedParent+5L      �� 5M      $��ContentSite��5N       .  #��z 5P      #��z 5Q      #��z��5R      $��3,3,23,3q�5S      $��Center=�5T      $
��Left=�5U      �� 5V      $ PART_EditableTextBox��6"   #���  5X      $
��Left=�5Y      $��Center=�5Z      $��3,3,23,3q�5[      |�. 
������ $y�Hidden=�5^      # z 5_      ��5W      Z� 5_      � 5`      $��Popup��5a      $��Bottom=�5b      #��z 5c       . |�.  $��Slide=�5f      .��5g      � $��DropDown��5h       . #��z 5j      #��z 5k      .+�5l   "   �� $ DropDownBorder��5m      #���  5n      $��1q�5o      #���  5p      �� 5q   "   $��4,6,4,6q�6/    . .��5r   &   ��  . $ 	Contained=�6D   5s   #   5t      5u      5v       5w      P� 5x       �@��HasItems	 .  . �5y      �� $
 DropDownBorder��6%    � ��$ 95��6V   5z      P� 5{       � {�	 .  . �5|      �� # � 
 6;    � ��5}      P� 5~       �@��
IsGrouping	 . . �5      ��  � �� .  6N   5�      P� 5�      $  Popup��6"    �@�AllowsTransparency	 . . �5�      �� $
 DropDownBorder��6%    �@��CornerRadius$ 0��6Y   �� 5�      $
 DropDownBorder��6%    � ��$ 0,2,0,0q�6S   5�      P� 5�       �@��
IsEditable	 . . �5�      ��  � �� .  6:   �� 5�      $
 PART_EditableTextBox��6%    � y�$ Visible=�6`   �� 5�      $
 ContentSite��6%    � y�$ Hidden=�6T   5�      5�      5�      ! 5�   
   5�      5�      