   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:NeathCopy.UsedWindowsNeathCopy.UsedWindows  5   	   ,    &NeathCopy.UsedWindows.UserDropUIWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:NeathCopy.UsedWindows     5   	   -   5          � SizeToContent$  WidthAndHeight=�5       � Topmost .  � WindowStartupLocation$ CenterOwner=�6I    � Title$ UserDropUIWindow��5   	      UsedWindowStyle#���   6#    � Icon$@ :/NeathCopy;component/Themes/Images/Windows8Images/info.png��6M   .��5	      �   ControlBackgroundBrush#���  6   .+�5   
   � $��20q�6   $��Center=�6    � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $
��Auto �6    �� 5      $
��Auto �6    .+�5      �� $��1��6    �� Orientation$ 
Horizontal=�6&   .+�5      �� -   5        $��	option_cb��5      $
��Bold"�6,   $��Center=�6>   $��80��6Y    �� SelectedIndex$ 0��6d   .@�5      ��   t55#���  6#   �� 5        t56#���  6#   5      �� 5      -   5        $	��tb1��5      $	��200��6%   $��3,0,3,0q�61     	TextBrush#���  6B   #���  6k   �� 5      -   5        $��	browse_bt��5   D   
������ $
��Bold"�62   $��5,0,0,0q�6n   $��25��6   $	��...��6�   5      �� 5      $��2��6   $ 
Horizontal=�6&   $��Center=�6?   $��0,15,0,0q�6\   $��23��6n   .+�5      �� -   5        $��ok_bt��5   v   
������ 
����  �$
��Bold"�6D     t57#���  6V   $��60��6�    �� 	IsDefault . 6�   �� 5      -   5        $��	cancel_bt��5   d   
������ $
��Bold"�62     t58#���  6D   $��20,0,0,0q�6�   
���  ��$��60��6�   	 �� IsCancel	 . 6�   5      5      5      5      