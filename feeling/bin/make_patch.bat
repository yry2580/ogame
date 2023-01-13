@echo off
echo µ±Ç°Â·¾¶ %cd%

// del ".\patch\*.*" /q /s

del ".\patch\appsettings.json" /q;
del ".\patch\feeling.exe" /q;
del ".\patch\feeling.exe.config" /q;
del ".\patch\feeling.pdb" /q;
del ".\patch\OgameService.dll" /q;
del ".\patch\OgameService.dll.config" /q;
del ".\patch\OgameService.pdb" /q;
del ".\patch\OgameServiceLog.config" /q;
del ".\patch\auto_updater.exe" /q;
del ".\patch\auto_updater.pdb" /q;

echo f |xcopy ".\Release\appsettings.json" ".\patch" /y
echo f |xcopy ".\Release\feeling.exe" ".\patch" /y
echo f |xcopy ".\Release\feeling.exe.config" ".\patch"
echo f |xcopy ".\Release\feeling.pdb" ".\patch" /y
echo f |xcopy ".\Release\OgameService.dll" ".\patch" /y
echo f |xcopy ".\Release\OgameService.dll.config" ".\patch" /y 
echo f |xcopy ".\Release\OgameService.pdb" ".\patch" /y
echo f |xcopy ".\Release\OgameServiceLog.config" ".\patch" /y 
echo f |xcopy ".\Release\auto_updater.pdb" ".\patch" /y
echo f |xcopy ".\Release\auto_updater.exe" ".\patch" /y 


pause