@echo off
echo µ±Ç°Â·¾¶ %cd%

del ".\patch\appsettings.json" /q;
del ".\patch\feeling.exe" /q;
del ".\patch\feeling.exe.config" /q;
del ".\patch\feeling.pdb" /q;
del ".\patch\OgameService.dll" /q;
del ".\patch\OgameService.dll.config" /q;
del ".\patch\OgameService.pdb" /q;
del ".\patch\OgameServiceLog.config" /q;


echo f |xcopy ".\Release\appsettings.json" ".\patch\appsettings.json" /y /e /i 
echo f |xcopy ".\Release\feeling.exe" ".\patch\feeling.exe" /y /e /i 
echo f |xcopy ".\Release\feeling.exe.config" ".\patch\feeling.exe.config" /s /i 
echo f |xcopy ".\Release\feeling.pdb" ".\patch\feeling.pdb" /y /e /i 

echo f |xcopy ".\Release\OgameService.dll" ".\patch\OgameService.dll" /y /e /i 
echo f |xcopy ".\Release\OgameService.dll.config" ".\patch\OgameService.dll.config" /y /e /i 
echo f |xcopy ".\Release\OgameService.pdb" ".\patch\OgameService.pdb" /s /i 
echo f |xcopy ".\Release\OgameServiceLog.config" ".\patch\OgameServiceLog.config" /y /e /i 



pause