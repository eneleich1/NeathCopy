   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null=#clr-namespace:NeathCopy.UsedWindowsNeathCopy.UsedWindows  5   	   =#clr-namespace:NeathCopy.UsedWindowsNeathCopy.UsedWindows  5   	   1clr-namespace:NeathCopy.ToolsNeathCopy.Tools  5   	   *    $NeathCopy.UsedWindows.CopyListWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   5local#clr-namespace:NeathCopy.UsedWindows     5   	   ;usedwindows#clr-namespace:NeathCopy.UsedWindows     5   	   /toolsclr-namespace:NeathCopy.Tools     5   	   -   5        $��copyWnd��5
   q     J� 	AllowDrop  .    UsedWindowStyle#���   6	    � WindowStartupLocation$ CenterOwner=�64    � Title$ CopyListWindow��5   	   $	��300��6    $��545.51��6-    � SizeToContent$ Width=�6<    � 
ResizeMode$ 	CanResize=�6R    � Icon$C =/NeathCopy;component/Themes/Images/Windows8Images/listwnd.png��6i    � DataContext 5      *   $NeathCopy.UsedWindows.CopyListWindow  5   
    J� CommandBindings 5      )  #System.Windows.Input.CommandBinding  5   
   -   5            NeathCopy.Tools.MyCommands   MoveFirstCommand	   Command#	 Z 5      6P     5   
   -   5        
   MoveUpCommand#	 Z
 5      6M     5   
   -   5           MoveDownCommand#	 Z 5      6O     5   
   -   5           MoveLastCommand#	 Z 5      6O     5   
   -   5           RemoveCommand#	 Z 5      6M     5   
   -   5           SaveCommand#	 Z 5      6K     5   
   -   5           LoadCommand#	 Z 5      6K    � 	Resources 5        filesVS��%   &	       �� 5   
   6   5      .��5      ]� $��3q�6     ControlBackgroundBrush#���  6   .+�5    
   o�   MenuBackground#���  6   $	��Top=�6>   .@�5!      m�   t15#���  6   .@�5"      m� -	   5        $��loadOd_menuItem��5"        t113#���  62   6{   m� 5#      -
   5        $��saveOd_menuItem��5#        t114#���  62   6z   m� 5$      -   5        $��load_menuItem��5$        t19#���  60   6n   m� 5%      -   5        $��save_menuItem��5%        t20#���  60   6m   m� 5&      -   5        $��loadCompressed_menuItem��5&       	 t21#��� 	 6:   6�   m� 5'      -   5        $��saveCompressed_menuItem��5'       
 t22#��� 
 6:   6�   5(      5)      � 5*   
     ToolBarBackground#���  6   $��Center=�6A   $��Right=�6\   .+�5+      �� -   5        $��MyToolBarPanel��5+        ToolBarPanelStyle#���  6,   5,      5-      �� 5.   
    �� Orientation$ 
Horizontal=�6   $��Bottom=�6.     ButtonMouseHoverBrush#���  6F   .+�5/      ��   t108#���  6   �� 50      $	��0.5q�6   ._�51      �� -   5        $��filesCount_tb��51       �� TextAlignment$ Right=�62   $	��100��6H   $��3q�6T   #���  6_   ��52      ��  �� ElementName$ copyWnd�� �� Path$ 	Tag.Count�53      54      �� 55   
   -   5        $��MainListView��55   +   $	��Top=�6   ��56      ��  �� Source# [  �� View 57      �� 58       �� AllowsColumnReorder . .P�59      ��   t16#���  6V    �� Width$ 70��6u    �� DisplayMemberBinding 6&   �� $ 	CopyState�6u   �� 5:        t17#���  6P   $	 240��6o    6%   �� $
 Name�6o   �� 5;        t115#���  6T   $	 240��6t    6%   �� $ FullName�6t   �� 5<        t116#���  6W   $	 240��6w    6%   �� $ DestinyPath�6w   �� 5=        t18#���  6X   $ 80��6w    6%   �� $ FormatedSize�6w   5>      5@      5A      5B      