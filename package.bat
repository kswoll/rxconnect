cd Nuget
mkdir build
cd build

copy ..\SexyReact.nuspec .
..\..\packages\NugetUtilities.1.0.8\UpdateVersion SexyReact.nuspec -SetVersion %APPVEYOR_BUILD_VERSION%
..\..\packages\NugetUtilities.1.0.8\UpdateReleaseNotes SexyReact.nuspec "%APPVEYOR_REPO_COMMIT_MESSAGE%"

mkdir lib
mkdir lib\net45

copy ..\..\SexyReact.Net45\bin\Debug\SexyReact.* lib\net45

..\..\Nuget\nuget pack SexyReact.nuspec

copy *.nupkg ..
rem nuget push *.nupkg ca9804b4-8f40-4b56-a35b-9ff23423a428 -source https://nuget.org

cd ..
rmdir build /S /Q
cd ..

cd SexyReact.Fody
cd Nuget
mkdir build
cd build

copy ..\SexyReact.Fody.nuspec .
..\..\..\packages\NugetUtilities.1.0.8\UpdateVersion SexyReact.Fody.nuspec -SetVersion %APPVEYOR_BUILD_VERSION%
..\..\..\packages\NugetUtilities.1.0.8\UpdateReleaseNotes SexyReact.Fody.nuspec "%APPVEYOR_REPO_COMMIT_MESSAGE%"

mkdir build
mkdir weaver
mkdir lib
mkdir lib\net45

copy ..\..\bin\Debug\SexyReact.Fody.* weaver
copy ..\SexyReact.Fody.props build
copy ..\Placeholder.txt lib\net45

..\..\Nuget\nuget pack SexyReact.Fody.nuspec

copy *.nupkg ..
rem nuget push *.nupkg ca9804b4-8f40-4b56-a35b-9ff23423a428 -source https://nuget.org

cd ..
rmdir build /S /Q
cd ..
cd ..