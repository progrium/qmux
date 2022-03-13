build_qmux: 
	dotnet build ./qmux/qmux.csproj
build_gostdlib:
	dotnet build ./gostdlib/gostdlib.csproj
build: build_qmux
test: 
	dotnet test ./qmux.tests/qmux.tests.csproj
test_gostdlib:
	dotnet test ./gostdlib.tests/gostdlib.tests.csproj
clean_src: 
	dotnet clean ./qmux/qmux.csproj
clean_test: 
	dotnet clean ./qmux.tests/qmux.tests.csproj
clean: clean_src clean_test