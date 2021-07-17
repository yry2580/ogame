@echo off
echo µ±Ç°Â·¾¶ %cd%

xcopy ".\feeling\Common" ".\feeling2\Common" /y /e
xcopy ".\feeling\Controls" ".\feeling2\Controls" /y /e
xcopy ".\feeling\Handler" ".\feeling2\Handler" /y /e
xcopy ".\feeling\Html" ".\feeling2\Html" /y /e
xcopy ".\feeling\Module" ".\feeling2\Module" /y /e
xcopy ".\feeling\Native" ".\feeling2\Native" /y /e
xcopy ".\feeling\Properties" ".\feeling2\Properties" /y /e
xcopy ".\feeling\User" ".\feeling2\User" /y /e
xcopy ".\feeling\Util" ".\feeling2\Util" /y /e
xcopy ".\feeling\Parser" ".\feeling2\Parser" /y /e
xcopy ".\feeling\MainForm.cs" ".\feeling2" /y
xcopy ".\feeling\MainForm.Designer.cs" ".\feeling2" /y
pause