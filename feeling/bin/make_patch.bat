@echo off
echo µ±Ç°Â·¾¶ %cd%

del ".\patch\appsettings.json" /q;
del ".\patch\feeling.exe" /q;
del ".\patch\feeling.exe.config" /q;
del ".\patch\feeling.pdb" /q;

echo f |xcopy ".\Release\appsettings.json" ".\patch\appsettings.json" /y /e /i 
echo f |xcopy ".\Release\feeling.exe" ".\patch\feeling.exe" /y /e /i 
echo f |xcopy ".\Release\feeling.exe.config" ".\patch\feeling.exe.config" /s /i 
echo f |xcopy ".\Release\feeling.pdb" ".\patch\feeling.pdb" /y /e /i 



pause