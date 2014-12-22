@echo off
set tool="..\src\WinTzToMomentJsTzTool\bin\Release\WinTzToMomentJsTzTool.exe"
if exist %tool% (
echo "Build test data time zones for period : 1990 - 2030"
%tool% gentest 1990 2030 > test_data.json
echo Generated 'test_data.json'
node test.js
) else (
echo "File %tool% not found"
echo "Please, compile ..\src\WinTzToMomentJsTzTool\WinTzToMomentJsTzTool.csproj"
)
