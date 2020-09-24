#!/bin/bash

dotnet dev-certs https -ep /app/cindi_cert.pfx -p "P@ssw0rd123!"

# Start the second process
/usr/bin/dotnet /app/Cindi.Presentation.dll -D