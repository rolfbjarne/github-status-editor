XAMMAC_PATH=/Library/Frameworks/Mono.framework/Versions/5.14.0/lib/mono/4.6.1-api
DESTDIR ?= /usr/local/bin/
CONFIGURATION ?= Debug

all: github-status-editor.exe

github-status-editor.exe: Program.cs Makefile 
	nuget restore
	msbuild /p:Configuration=$(CONFIGURATION) github-status-editor.sln

bundle: github-status-editor.exe
	mkbundle --simple ./bin/$(CONFIGURATION)/github-status-editor.exe -o github-status-editor -L ./bin/$(CONFIGURATION)

install: bundle github-status-editor
	install -d $(DESTDIR)
	cp github-status-editor $(DESTDIR)

clean:
	msbuild /target:Clean github-status-editor.sln
	@rm -Rf bin
	@rm -Rf obj