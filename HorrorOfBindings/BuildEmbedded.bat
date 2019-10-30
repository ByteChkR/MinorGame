@echo off

cd packager

echo Running Packager:
dotnet ResourcePackager.dll ..\HorrorOfBindings.csproj @filelist.txt

cd ..

echo Building Demo Project:
dotnet build

cd packager
echo Restoring Initial csproj file:

dotnet ResourcePackager.dll ..\HorrorOfBindings.csproj.backup

cd ..

echo Finished

pause