#!/bin/sh

rm -rf ./publish/
dotnet clean
dotnet publish -o publish -r linux-x64 -c Release -p:PublishTrimmed=true -p:PublishSingleFile=true -p:PublishReadyToRun=true
