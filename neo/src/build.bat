@echo off
set PATH=%PATH%;..\tools\nant
nant -buildfile:build.xml %1
