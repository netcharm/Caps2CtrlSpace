# Caps2CtrlSpace

CapsLock 切换输入法中英文

> __Modified by netcharm__
1. Add CapsLock Indicator Light option, but can not auto detect IME 
State with current input focus, so need you start with eng-input state, 
then the light maybe turn-on when press capslock to chinese input. 
1. Change application's icon.
1. Add Always on Top option, note: this option is not saved, so next 
startup, is re-enabled.
1. Support sync capslock incicator light state to all keyboards with 
indicator lights
1. Add contextmenu to notify tray icon for show window, exit app.
1. Add Timer to auto switch CapsLock Indicator light with keyboard 
layout switch, but has buggy, when you change
input state after switch to other keyboard layout and switch back, it 
maybe display negtived indicator light.
1. Add contextmenu to Ime Mode & Indicator Image Control, for save 
the Keyboard Layout Mode & Layout Indicator Image.
1. Supported automatic switch to english input mode when `KeePass` 
hot key pressed.
1. Using snapshot the ImeModeButton window on system notification 
area to check & compare Ime Mode in English/locale, 
note: you must take this area image(png format) by yourself in your 
system, and with right size, maybe `22x34`, 
file name format: `{KeyborardLayoutCode}_{Mode}.png`, 
1. KeyborardLayoutCode displayed in application bottom like `2052`, 
1. Mode: 
>   1. `0` = Layout (maybe 36x34)
>   1. `1` = English
>   1. `2` = Ime Locale
>   1. `3` = Disabled (maybe unnecessary) 
>   1. `4` = Close (maybe unnecessary)

> Note: 
1. not supported the IME "Floating Language Toolbar mode", 
only supported embeded in System Notification Tray Area
1. Require `Run As Administrator` for change keyboard indicator light.

# 原理

监听按键，如果是Capslock，则向系统发送Ctrl+Space切换输入法状态

# 开发环境

> __Modified by netcharm__
1. VS2015 Express Desktop
1. .net framework 4.6.2
 
# Binary下载

__Modified by netcharm__

## Mine Binary

1. [Bitbucket 7z package](https://bitbucket.org/netcharm/caps2ctrlspace/downloads) (__Preferred__)
1. [~~Bitbucket exe only~~](https://bitbucket.org/netcharm/caps2ctrlspace/src/master/Caps2CtrlSpace.exe)
1. [~~Github exe only~~](https://github.com/netcharm/Caps2CtrlSpace/blob/master/Caps2CtrlSpace.exe)

## Original Binary

1. [Github](https://github.com/cuiliang/Caps2CtrlSpace/blob/master/Caps2CtrlSpace.exe)
  
# 使用

启动后，会自动隐藏窗口显示在系统托盘内一个桔黄色的小点的图标。 这时候可以按capslock来切换中英文了。
 
> Modified by netcharm
> Sys-Tray icon changed to a composited character icon

*自动启动*
 
双击系统托盘图标，在打开的窗口中选中自动启动的选项即可。
 
