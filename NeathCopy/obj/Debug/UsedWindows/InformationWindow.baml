   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:NeathCopy.UsedWindowsNeathCopy.UsedWindows  5   	   -    'NeathCopy.UsedWindows.InformationWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:NeathCopy.UsedWindows     5   	   -   5          � SizeToContent$  WidthAndHeight=�5       � Topmost .  � WindowStartupLocation$ CenterOwner=�6I    � Title$ InformationWindow��5   	      UsedWindowStyle#���   6#    � Icon$@ :/NeathCopy;component/Themes/Images/Windows8Images/info.png��6M   .��5
      �   ControlBackgroundBrush#���  6    � 	Resources 5   
    �� 
TargetType �� Property �� Value��%�   '��       �� 5      " ��6   .�5      ��  � ��
 �����6?   �� 5       � ��
 ��  �6>   �� 5       � ��$ 50��6*   67   �� 5       � ��$ 25��6+   68   �� 5       � ��$
 Bold"�6/   6>   5      5      .+�5   
   �� $��15q�6   .+�5      �� -   5        $��
message_rb��5      $��49��6,   $	��300��68   	 �� 
IsReadOnly	 .   	TextBrush#���  6V   #���  6   �� 5      $��5��6   
������ 6#   �� 5      -   5        $��diskSpace_listview��5   #   $��70��6   
 �� View
 5      �� 5       �� AllowsColumnReorder .  �� ColumnHeaderToolTip$ Employee Information��6:   .P�5      ��   t44#���  6W    �� DisplayMemberBinding 6)   ��  �� Path$ Volumen�6W   �� 5        t45#���  6X    6)   �� $ Capacity�6X   �� 5        t46#���  6Y    6)   �� $ 	UsedSpace�6Y   �� 5        t47#���  6Y    6)   �� $ 	FreeSpace�6Y   �� 5        t48#���  6\    6)   �� $ RequireSpace� �� HeaderTemplate 5      �� 5    "   .R�5!   &   �� 
��� � �$
��Bold"�6C   ��6U   �� 6h   5"   #   5$      �� 5%        t49#���  6X    6)   �� $ NeedMore� 5&      �� 5'   "   .R�5(   &   �� 
����  �$
��Bold"�6B   ��6T   �� 6g   5)   #   5+      5,      5.      �� 5/      $��5��6   
������ 6#   �� 50       �� Orientation$ 
Horizontal=�6   $��Center=�62   .+�51      �� -   5        $��
try_button��51   D   
������ $
��Bold"�62    	 t50#��� 	 6V   $��60��6w   $
��Left=�6�   6�   �� 52      -   5        $��ignore_button��52   U   $��5,0,5,0q�6   
������ $
��Bold"�6C   $��60��6j    
 t51#��� 
 6�   $
��Left=�6�   �� 53      -   5        $��
fit_button��53   D   
������ $
��Bold"�62     t52#���  6V   $	��135��6v   6�   �� 54      -   5        $��cancel_button��54   U   $��5,0,5,0q�6   
������ $
��Bold"�6C   $��60��6k     t53#���  6v   
���  ��$
��Left=�6�   6�   55      56      57      58      