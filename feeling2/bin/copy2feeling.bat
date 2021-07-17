@echo off
echo µ±Ç°Â·¾¶ %cd%

xcopy "./Release" "./feeling" /y /e

if exist ".\feeling\GPUCache\" rd /s/q ".\feeling\GPUCache\";
if exist ".\feeling\Out\" rd /s/q ".\feeling\Out\";
if exist ".\feeling\OgameServiceLog\" rd /s/q ".\feeling\OgameServiceLog\";

del ".\feeling\UserData.dat" /q;
del ".\feeling\ex_mission.cfg" /q;
del ".\feeling\pirate_mission.cfg" /q;
del ".\feeling\pirate_mission1.cfg" /q;
del ".\feeling\OgClient.cfg" /q;
del ".\feeling\debug.log" /q;

pause