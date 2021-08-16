@echo off
echo µ±Ç°Â·¾¶ %cd%

echo f |xcopy ".\Debug\appsettings.json" ".\patch\appsettings.json" /y /e /i 
echo f |xcopy ".\Debug\feeling.exe" ".\patch/feeling.exe" /y /e /i 
echo f |xcopy ".\Debug\feeling.exe.config" ".\patch\feeling.exe.config" /s /i 
echo f |xcopy ".\Debug\feeling.pdb" ".\patch\feeling.pdb" /y /e /i 



pause