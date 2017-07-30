# AutoClicker

_Ever needed the **auto-fire** feature in your **mouse**?..._

So, here it is. And it's activated with the keyboard ;)

<hr>

_Notice: This program uses C# 7 syntax._

## Usage

Run.
Find the icon in system tray, adjust parameters.
Or just run, don't touch anything, the defaults are sane.

The mouse "auto-fire" is activated by pressing the `Scroll Lock` key.

Your keyboard will give you a hind with its LED when the thing is on.

You can make a Start Menu shortcut in Windows 10 by right clicking on
the `AutoClicker.exe` file and selecting "Pin to start" option. 

_Pin Cursor_ feature is intended to hold the mouse cursor pinned to the point
where the AutoClicker was activated with `Num Lock` key.

> _You really should not turn this option off, because mouse cursor clicking
everything around like crazy could be danerous._

If you want to test this toy, try [Cookie Clicker](http://orteil.dashnet.org/cookieclicker/).

## Operation

This program works as a special kind of tray icon application.
It has no other UI than the tray icon and its menu.
To create a windowless application properly I overrode the `ApplicationContext` class.

You may ask why, and it's the right question. Because I want to make things happen
when the Windows message pump is started, and dispose things properly
when it's stopped.

If you touch the UI before `Application.Run` call, it won't work. It makes no sense
to touch it after either. So we need to pass all the UI code to `Application.Run`.

Typically Windows Forms application pass... form to it. We don't need a form,
what we need is an `ApplicationContext`.

This neat little trick allows us to show a nice popup balloon on startup.

The next tricky part is accessing the global input events.

This is achieved by using Win32 API, see GlobalInput class documentation in code.

## License

```
DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
Version 2, December 2004 

Copyright (C) 2004 Sam Hocevar <sam@hocevar.net> 

Everyone is permitted to copy and distribute verbatim or modified 
copies of this license document, and changing it is allowed as long 
as the name is changed. 

DO WHAT THE FUCK YOU WANT TO PUBLIC LICENSE 
TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION 

0. You just DO WHAT THE FUCK YOU WANT TO.
```

## Disclaimer

_I'm not responsible for any damage caused by using this program or it's portions._

`GlobalInput` class uses some unmanaged code to make system voodoo, but I consider
it safe if used properly, and by properly I mean to dispose the instance before
exiting the application. You also should not block the global event handler.
Well, it's called every time a key is pressed in your whole damn system, so if you
break it, you'll probably have to restart the system ;)

The `GlobalInput` class is a part of the `Woof Toolkit` that is not yet publicly
released. If you're curious like the killed cat, it's a .NET Framework extension
that allows me to code DRY. It contains everything but the kitchen sink not specific
to any business logic, but rather to system and .NET Framework itself.

## But why?...

Because I could. LOL. It's fun to cheat in Cookie Clicker with C# ;)

BTW, If you ever wondered how to make a totally legit Windows UI without a window,
with a tray icon only, here's how, use it as a tutorial.