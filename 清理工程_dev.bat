@echo off
title 清理VS2010调试文件
cls
color 0A
del /s /f /a *.aps
del /s /f /a *.ncb
del /s /f /a *.htm
del /s /f /a *.obj
del /s /f /a *.manifest
del /s /f /a *.pch
del /s /f /a *.pdb
del /s /f /a *.idb
del /s /f /a *.ilk
del /s /f /a *.exp
del /s /f /a *.dep
del /s /f /a *.bsc
del /s /f /a *.sbr
del /s /f /a *.dsw
del /s /f /a *.dev
del /s /f /a *.sdf
del /s /f /a *.cod
del /s /f /a *.ipch
del /s /f /a *.tlog
del /s /f /a *.log
del /s /f /a *.plg
del /s /f /a *.opt
del /s /f /a *.positions
del /s /f /a *.suo
del /s /f /a *.scc
del /s /f /a *.unsuccessfulbuild
del /s /f /a *.lastbuildstate

@for /r . %%a in (.) do @if exist "%%a\_tmp" rd /s /q "%%a\_tmp"

@for /r . %%a in (.) do @if exist "%%a\_tmp_files" rd /s /q "%%a\_tmp_files"
@for /r . %%a in (.) do @if exist "%%a\Debug" rd /s /q "%%a\Debug"
@for /r . %%a in (.) do @if exist "%%a\Release" rd /s /q "%%a\Release"

@for /r . %%a in (.) do @if exist "%%a\ipch" rd /s /q "%%a\ipch"

@for /r . %%a in (.) do @if exist "%%a\bin" rd /s /q "%%a\bin"

@for /r . %%a in (.) do @if exist "%%a\obj" rd /s /q "%%a\obj"


ECHO 清理完成

pause