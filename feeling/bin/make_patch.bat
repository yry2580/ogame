@echo off
echo ��ǰ·�� %cd%

del ".\patch\appsettings.json" /q;
del ".\patch\feeling.exe" /q;
del ".\patch\feeling.exe.config" /q;
del ".\patch\feeling.pdb" /q;
del ".\patch\OgameService.dll" /q;
del ".\patch\OgameService.dll.config" /q;
del ".\patch\OgameService.pdb" /q;
del ".\patch\OgameServiceLog.config" /q;


echo f |xcopy ".\Release\appsettings.json" ".\patch" /y
echo f |xcopy ".\Release\feeling.exe" ".\patch" /y
echo f |xcopy ".\Release\feeling.exe.config" ".\patch"
echo f |xcopy ".\Release\feeling.pdb" ".\patch" /y
echo f |xcopy ".\Release\OgameService.dll" ".\patch" /y
echo f |xcopy ".\Release\OgameService.dll.config" ".\patch" /y 
echo f |xcopy ".\Release\OgameService.pdb" ".\patch" /y
echo f |xcopy ".\Release\OgameServiceLog.config" ".\patch" /y 



pause