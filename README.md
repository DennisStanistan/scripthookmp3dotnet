# Max Payne 3 Scripthook .NET

This is an ASI plugin for Max Payne 3 based on [UnknownModder's scripthook](https://unknownmodder.github.io/maxpayne3), this plugin borrows some of [Crosire's SHVDN](https://github.com/crosire/scripthookvdotnet) implementations.
Works the same way as Crosire's V .NET scripthook (Insert to reload, scripts in /scripts, logs)

# Requirements
* [Max Payne 3 Scripthook by UnknownModder](https://unknownmodder.github.io/maxpayne3)
* [Visual C++ Redistributable 2017 (x86)](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)
* [.NET Framework 4.5.2](https://www.microsoft.com/net/download/dotnet-framework-runtime/net452)

# Downloads
The project is in it's early stage of development, so it has issues that must be fixed and code that needs to be rewritten and as a result a pre-built binary isn't available for now. However you can download the repo and compile it.

# Known Issues
* Game will sometimes freeze if the game's window is not focused
* Calling natives may crash the game sometimes
* Calling natives may cause other .asi plguins that use UnknownModder's scripthook to crash which then will crash the game.
* String arguements in `Function.Call` will crash the game, has something to do with my implementation.

# Contribute
You will need Visual Studio 2017, Windows SDK 8.1 (which you can get from the Visual Studio 2017 installer) with C++ cli support.
Ignore the "X" is ambigious errors & "cannot overload functions distinguished by return type alone", as far as I can tell it's a visual studio bug and it's a by-product of the scripthook's SDK and the code will compile fine.
