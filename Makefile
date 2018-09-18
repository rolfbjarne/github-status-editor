DESTDIR ?= /usr/local/bin/
CONFIGURATION ?= Debug

all: github-status-editor.exe

github-status-editor.exe: Program.cs Makefile 
	nuget restore
	msbuild /p:Configuration=$(CONFIGURATION) github-status-editor.sln

github-status-editor: github-status-editor.exe
	mkbundle --simple ./bin/$(CONFIGURATION)/github-status-editor.exe -o $@ -L ./bin/$(CONFIGURATION)

install: github-status-editor
	install -d $(DESTDIR)
	cp github-status-editor $(DESTDIR)

clean:
	msbuild /target:Clean github-status-editor.sln
	@rm -Rf bin
	@rm -Rf obj
