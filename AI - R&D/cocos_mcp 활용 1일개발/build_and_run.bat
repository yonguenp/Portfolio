@echo off
echo Starting Cocos Creator build...
"C:\ProgramData\cocos\editors\Creator\3.8.8\CocosCreator.exe" --project "%~dp0" --build "platform=web-desktop"
if %errorlevel% == 0 (
    echo Build completed!
    echo Output: %~dp0build\web-desktop\
) else (
    echo Build failed with code %errorlevel%
)
pause
