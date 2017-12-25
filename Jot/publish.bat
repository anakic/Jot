REM delete
del *.nupkg
nuget pack jot.csproj
nuget push Jot.*.nupkg -Source "nuget.org"