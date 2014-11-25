@echo off
set converter="..\WinTzToMomentJsTzTool\bin\Release\WinTzToMomentJsTzTool.exe"
if exist %converter% (
echo "Build time zones for period : 1990 - 2030"
%converter% 1990 2030 > wintz.json
node test.js
) else (
echo "File %converter% not found"
echo "Please, compile ..\WinTzToMomentJsTzTool\WinTzToMomentJsTzTool.csproj"
)
