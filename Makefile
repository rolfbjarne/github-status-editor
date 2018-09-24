all:
	nuget restore
	msbuild *.csproj
