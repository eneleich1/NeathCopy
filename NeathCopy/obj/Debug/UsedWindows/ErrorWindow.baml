   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:NeathCopy.UsedWindowsNeathCopy.UsedWindows  5   	   '    !NeathCopy.UsedWindows.ErrorWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:NeathCopy.UsedWindows     5   	   -   5          � WindowStartupLocation$  CenterOwner=�5   <      UsedWindowStyle#���   5       � Topmost .  � SizeToContent$ WidthAndHeight=�6S    � Title$ ErrorWindow��5   	    � Icon$@ :/NeathCopy;component/Themes/Images/Windows8Images/info.png��6   .��5
      ��   ControlBackgroundBrush#���  6   .+�5      ��  �� Orientation$ Vertical=�6    � 	Resources 5       �� 
TargetType �� Property	 �� Value��%�   '��       �� 5      " ��6   .�5      ��  � ��$	 3��63   �� 5       � ��$	 3��62   �� 5       � ��
	 ���� 67   5      5      .+�5      �� $��7,7,7,3q�6   $ 
Horizontal=�6.   $��Center=�6G   .+�5      �� $��Center=�6    $��30��6<   $��32(�6G   $
��Bold"�6U   
 �� TextAlignment$
 Center=�6g   $
��Wrap=�6~   !6�   �� 5      �� 5      -   5        $��info_tb��5   1    �� 
IsReadOnly . $	��285��6@   $��49��6L     	TextBrush#���  6X   #���  6�   5      �� 5      $��15q�6   $ 
Horizontal=�6&   $��Center=�6?   $��23��6\   .+�5      �� -   5        $��try_bt��5   D   
������ $
��Bold"�62   $��70��6R   
����  �  t41#���  5      �� 5      -   5        $��skipAll_button��5   W   
������ $
��Bold"�62   $��	10,0,10,0q�6D   $��70��6m   
����  �  t32#���  5      �� 5      -   5        $��skip_button��5   D   
������ $
��Bold"�62   $��45��6W   
����  �  t33#���  5      �� 5       -   5        $��cancel_button��5    D   
������ $
��Bold"�62   
���  ��$��55��6j   $��10,0,0,0q�6x     t34#���  5!      5"      5#      5$      5%      