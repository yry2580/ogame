@echo off
echo µ±Ç°Â·¾¶ %cd%

xcopy "./Release" "./feeling" /y /e

if exist ".\feeling\GPUCache\" rd /s/q ".\feeling\GPUCache\";
if exist ".\feeling\Out\" rd /s/q ".\feeling\Out\";

del ".\feeling\UserData.dat" /q;
del ".\feeling\debug.log" /q;

pause