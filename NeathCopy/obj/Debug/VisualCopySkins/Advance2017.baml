   M S B A M L   `   `   `  ����D  @NeathCopy, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:NeathCopy.VisualCopySkinsNeathCopy.VisualCopySkins  5      1clr-namespace:NeathCopy.ToolsNeathCopy.Tools  5      ;"clr-namespace:NeathCopy.ViewModelsNeathCopy.ViewModels  5          Advance2017W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source   	moreBrush �� ImageSource j� Stretch  pauseResumeBrush  	skipBrush  cancelBrush �� 
TargetType  InfoLabelStyle �� Property �� Value  	TextBrush �� 
TargetType ��Name 	 vc_VM	 � DataContext
 �� Triggers P� Property P� Value �� 
TargetName  ButtonMouseHoverBrush  ButtonPressedBrush T� Y 	 VisualCopyStyle 
 BorderBrush  ClientAreaBackgroundBrush � RowDefinitions  ControlBackgroundBrush
 �� Top �� Orientation �� Left  t2 �� StringFormat  t3  t4  t10 � Maximum � Value  ProgressBarIndicatorBrush  ProgressBarTrackBrush �� TextAlignment �� EndPoint �� 
StartPoint � Color � Offset
 � Tag  t99 � ToolTip    NeathCopy.Tools.MyCommands   MoreCommand  t98   PauseCommand  t101   SkipCommand  t100    CancelCommand  t14  t109  t28  t1  t13  t12   5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      1local'clr-namespace:NeathCopy.VisualCopySkins  5      'toolsclr-namespace:NeathCopy.Tools  5      1
viewmodels"clr-namespace:NeathCopy.ViewModels  5        5	      �� 5
   
   $5 //NeathCopy;component/Resources/ProgressBar.xaml?�6   �� 5   
   $. (/NeathCopy;component/Resources/Menu.xaml?�6   �� 5   
   $> 8/NeathCopy;component/Resources/ViewModelsDictionary.xaml?�6   %�   &	        &	 B     &	 �     &	 �     &	     '�� }    7  &		 �    �� 5      $ Images/Light/more.png��6#   $ Uniform=�6G   �� 5      $ Images/Light/resume.png��6*   $ Uniform=�6P   �� 5      $ Images/Light/skip.png��6#   $ Uniform=�6G   �� 5      $" Images/Light/cancel_zoom.png��6%   $ Uniform=�6P   �� 5      " ��6   .�5   
   ��  � ��$
 Bold"�6'   �� 5   
    � ��# �  6'   5      �� 5      " ��6   .�5   
   ��  �@J�SnapsToDevicePixels . 60   �� 5   
    � �� 5      �� 5      " ��6"   .R�5      �� $ 
mainBorder��6   8	   $��1q�5      #��z��5       ._�5"      � $��grid1��6   .+�5#      � $��
storeColor��6#   $��3q�65   #��z��6@   .+�5$   "   �� $��content��5%   @   $��Center=�5$   3   $��Center=�5%   %   6@   5&      5(      5)      
 5*      P� 5+       �@J�IsMouseOver . . �5,      �� $ grid1��6%   # �  6N    � ��5-      P� 5.       � �� . . �5/      �� # �  6;    � ���� 51      $ content��6%    � z� 52   "   T� 53   &   $	 1.0Q�69   55      56      P� 57       �@��IsDefaulted . . �58      �� $ 
mainBorder��6%    � �� �	#FF2828286T   59      P� 5:       �@J�	IsFocused . . �5;      �� $ 
mainBorder��6%    � �� �	#FF2828286T   5<      P� 5=       � {� .  . �5>      �� $ 
mainBorder��6%    �@J�Opacity$	 0.7Q�6P   �� 5?       � ��
 �����6;   5@      5B      5D      5E      �� 5H      " ��6   .�5I   
   ��  � �� 5J      �� 5K      .R�5L      �� $ 
mainBorder��6   #��� 
 62   $��1q�6^   ._�5M      � $	��410��6   #���  6+    5N      �� 5O   "   $
��Auto �60   �� 5P   "   $
��Auto �60   .+�5R      �� $��0��6%   $��0,1,0,1q�62   #���  6D   ._�5S   "   �� $	��133��6)   $��5,0,5,0q�66   .+�5T   &   �� $��11��6-   $F��@/NeathCopy;component/VisualCopySkins/Images/Advance2017/file.png��68   $ 8��6�   �� 5U   &   $��9��6-   $H��B/NeathCopy;component/VisualCopySkins/Images/Advance2017/folder.png��67   $ 25��6�   �� 5V   &   $��9��6-   $H��B/NeathCopy;component/VisualCopySkins/Images/Advance2017/folder.png��67   $ 46��6�   �� 5Y   &   $ 
Horizontal=�61   $ 15��6J   $ 6��6[   $	��260��6j   .+�5Z   *   �� #���  64   #���  6P   �� 5[   *   $��file1_tb��64   $��3,0,0,0q�6D   #���  6V   ��5\   0   �� *CurrentFile.Name+$ 0��5]   '   �� 5`   &   $ 
Horizontal=�61   $ 15��6J   $ 24��6[   $	��260��6k   .+�5a   *   �� #���  64   #���  6P   �� 5b   *   $��from1_tb��64   $��3,0,0,0q�6D   #���  6V   ��5c   0   �� *From+$ 0��5d   '   �� 5g   &   $ 
Horizontal=�61   $ 15��6J   $ 45��6[   $	��260��6k   .+�5h   *   �� #���  64   #���  6Q   �� 5i   *   $��to1_tb��64   $��3,0,0,0q�6B   #���  6T   ��5j   0   �� *To+$ 0��5k   '   �� 5n   &   $ 
Horizontal=�61   $	 280��6K   $ 2��6]   .+�5o   *   �� $��16��61   $K��E/NeathCopy;component/VisualCopySkins/Images/Advance2017/lightning.png��6<   �� 5p   *   .+�5q   .   �� $ 
Horizontal=�69   $��Center=�6R   $ -4��6o   .+�5r   2   �� #���  6<   #���  6e   $
��10.3(�6�   �� 5s   2   $��speed_tb��6<   #���  6L   $
��Bold"�6u   $��2,0,0,0q�6�   $
��10.3(�6�   ��5t   8   �� *	SpeedInfo+$ 0��5u   /   � 5v   .   $��speed_pb��6:   $	��100��6K   $��3��6W   $ 14��6c   $ 0��6s   ��5w   6   �� *
SpeedBrush+ 5x   2   �� *
MaxSpeed+ 5y   2   �� *Speed+5z   +   5{   '   �� 5~   &   $ 
Horizontal=�61   $��2��6J   $��4��6W   $	 285��6g   $ 22��6y   .+�5   *   �� $��30��60   $��20��6;   $��Uniform=�6G   $ ��Images/Advance2017/hdd.png��6Y   �� 5�   *   $ Vertical=�65   $��3,0,0,0q�6L   .+�5�   .   �� $��targetDevice_tb��68   $��73��6O   #���  6Z   ��5�   4   �� *TargetDevice+$ 0��� 5�   .   $��targetDevice_pb��6:   $��70��6Q   $��3��6\   ��5�   6   �� *DriverSizeBrush+ 5�   7   �� *DriverSizePorcent+5�   +   5�   '   � 5�   &   $��single_progBar��62   $��15��6H   $	��399��6T   $��15��6`   $ 72��6p   #���  5�   .   #���  6g    5�   .   �� *SinglePorcent+�� 5�   &   $��singleSize_tb��60   #���  6E   $
��Left=�5�   ,   $	��135��6G   $
 Left=�6S   $��10(�6i   $ 73��6w   $ 3��6�   ��5�   ,   �� *CurrentFileSizeTransferred+$ 0���� 5�   &   $��singlePorcent_tb��60   #���  6H   $ Center=�6q   $��50��5�   ,   $��10(�67   $��Center=�6E   $	 179��6b   $ 72��6t   ��5�   ,   �� *SinglePorcent+$ {0:f2}%���� 5�   &   $��countFiles_tb��60   #���  6E   $��Right=�5�   ,   $	��142��6H   $ Right=�6T   $��10(�6k   $	 254��6y   $ 73��6�   ��5�   ,   �� *
FilesCount+$ 0��� 5�   &   $��overall_progBar��62   $��15��6I   $	��399��6U   $ 92��6a   #���  5�   .   #���  6g    5�   .   �� *OverallPorcent+�� 5�   &   $��overallSize_tb��60   #���  6F   $
��Left=�5�   ,   $	��135��6G   $
 Left=�6S   $��10(�6i   $ 92��6w   $ 3��6�   ��5�   ,   �� *OverallSizeTransferred+$ 0���� 5�   &   $��overallPorcent_tb��60   #���  6I   $ Center=�5�   ,   $��50��6D   $��10(�6P   $	 179��6`   $ 92��6r   ��5�   ,   �� *OverallPorcent+$ {0:f2}%���� 5�   &   $��remainingTime1_tb��60   #���  6I   $ Right=�5�   ,   $	��142��6B   $��10(�6O   $ 92��6]   $	 254��6m   ��5�   ,   �� *RemainingTime+$ Remaining: {0}���� 5�   &   $��2��61   $	��399��6<   $	 114��6H   $��0q�6Y   ��5�   *   �� 5�   .   $	 0,0&�6B   $	 1,0&�6Q   .��5�   2   � $
 Blue��6?   $ 0Q�6L   � 5�   2   $ Black��6?   $	 0.5Q�6M   � 5�   2   $
 Blue��6?   $ 1Q�6L   5�   /   5�   '   �� 5�   &   $ 
Horizontal=�61   $	 313��6J   $ 48��6\   .+�5�   *   �� $��list_bt��6w   $ Windows8��61   $��20��6@   $��15��6K   # �  6W   #���   6�   #��Z 5�   *   �� 5�   *   $��pauseResume_bt��6Y   $��20��61   $��15��6<   $��2,0,2,0q�6H   # �  6o   #��Z 5�   )   ��5�   )   �� *PauseButtonBrush+�� 5�   *   $��skip_bt��6j   $��20��61   $��15��6<   # �  6I   #���  6z   #��Z 5�   *   �� 5�   *   $��	cancel_bt��6Y   $��20��61   $��15��6<   $��2,0,2,0q�6H   # �  6j   #���  6�   #��Z  5�   *   5�   '   5�   #   5�      9� 5�      $��1��6(   $��	5,-18,6,0q�65   $��Bottom=�6H   #���  6c   $
��Left=�6�   .��5�   "   �� #���  6-   $ Vertical=�6d   $��5,0,0,0q�6|   $	��392��6�   .+�5�   &   �� $	��1.5��60   $��0,0,0,4q�6=   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6R   �� 5�   *   $��inqueueNumber_tb��64   $��2,0,0,0q�6L   ��5�   8   �� *InqueueNumber+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6Q   �� 5�   *   $��operation2_tb��64   $��2,0,0,0q�6I   ��5�   4   �� *	Operation+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6P   �� 5�   *   $��copyOption_tb��64   $��2,0,0,0q�6I   ��5�   4   �� *
CopyOption+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6P   �� 5�   *   $��file2_tb��64   $��2,0,0,0q�6E   ��5�   0   �� *CurrentFile.Name+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6P   �� 5�   *   $��from2_tb��64   $��2,0,0,0q�6E   ��5�   0   �� *From+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6P   �� 5�   *   $��to2_tb��64   $��2,0,0,0q�6C   ��5�   0   �� *To+$ 0��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6Q   �� 5�   *   $��elapsedTime_tb��65   $��2,0,0,0q�6K   ��5�   5   �� *ElapsedTime+$ Elapsed: {0}��5�   '   �� 5�   &   $ 
Horizontal=�61   .+�5�   *   �� #���  64   #���  6Q   �� 5   *   $��remainingTime2_tb��64   $��2,0,0,0q�6N   ��5  4   �� *RemainingTime+$ Remaining: {0}��5  '   �� 5  &   $	��1.5��60   $��0,4,0,4q�6=   5  #   5     5     5	     5
     5     5     5     