@echo off
set PATH=%PATH%;..\build\nant
nant -buildfile:build.xml %1
