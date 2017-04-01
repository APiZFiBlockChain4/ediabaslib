@echo off
SETLOCAL EnableDelayedExpansion

set BATPATH=%~dp0

set DATESTR=%date:~6,4%%date:~3,2%%date:~0,2%
echo !DATESTR!
set PACKAGEPATH="!BATPATH!Package\"
set EDIABASTESTPATH="!PACKAGEPATH!EdiabasTest\"
set TOOLPATH="!PACKAGEPATH!EdiabasLibConfigTool\"
set APINET32PATH="!PACKAGEPATH!ApiNet32\"
set CANADAPTERPATH="!PACKAGEPATH!CanAdapter\"
set CANADAPTERELMPATH="!PACKAGEPATH!CanAdapterElm\"
set ENETADAPTERPATH="!PACKAGEPATH!EnetAdapter\"
if exist "!PACKAGEPATH!" rmdir /s /q "!PACKAGEPATH!"
timeout /T 1 /NOBREAK > nul
mkdir "!PACKAGEPATH!"

mkdir "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\EdiabasTest.exe" "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\*.dll" "!EDIABASTESTPATH!"
copy "!BATPATH!EdiabasTest\bin\Release\EdiabasLib.config" "!EDIABASTESTPATH!"

mkdir "!TOOLPATH!"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\*.dll" "!TOOLPATH!"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\EdiabasLibConfigTool.exe" "!TOOLPATH!"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\EdiabasLibConfigTool.exe.config" "!TOOLPATH!"

mkdir "!TOOLPATH!de"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\de\*.dll" "!TOOLPATH!de"

mkdir "!TOOLPATH!ru"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\ru\*.dll" "!TOOLPATH!ru"

mkdir "!TOOLPATH!Api32"
copy "!BATPATH!EdiabasLibConfigTool\bin\x86\Release\Api32\*.*" "!TOOLPATH!Api32"

mkdir "!APINET32PATH!"
copy "!BATPATH!apiNET32\bin\Release\*.dll" "!APINET32PATH!"
copy "!BATPATH!apiNET32\bin\Release\*.config" "!APINET32PATH!"

mkdir "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\CanAdapter\Release\*.hex" "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\Pld\*.jed" "!CANADAPTERPATH!"
copy "!BATPATH!CanAdapter\UpdateLoader\bin\*.exe" "!CANADAPTERPATH!"

mkdir "!CANADAPTERELMPATH!"
mkdir "!CANADAPTERELMPATH!default"
copy "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\default\production\*.hex" "!CANADAPTERELMPATH!default"
mkdir "!CANADAPTERELMPATH!bc04"
copy "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\bc04\production\*.hex" "!CANADAPTERELMPATH!bc04"
mkdir "!CANADAPTERELMPATH!hc04"
copy "!BATPATH!CanAdapterElm\CanAdapterElm.X\dist\hc04\production\*.hex" "!CANADAPTERELMPATH!hc04"

mkdir "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\mini.bin" "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\openwrt*.bin" "!ENETADAPTERPATH!"
copy "!BATPATH!EnetAdapter\Release\*.img" "!ENETADAPTERPATH!"

set PACKAGEZIP="!BATPATH!Binaries-!DATESTR!.zip"
if exist "!PACKAGEZIP!" del /f /q "!PACKAGEZIP!"
"!PATH_7ZIP!\7z.exe" a -tzip -aoa "!PACKAGEZIP!" "!PACKAGEPATH!*"
