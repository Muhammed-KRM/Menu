@echo off
chcp 65001 > nul
title QR Menu Sistemi Başlatıcı

echo ========================================================
echo       🍽️ LEZZET DURAĞI QR MENÜ SİSTEMİ BAŞLATILIYOR
echo ========================================================
echo.

echo [1/2] .NET Backend API Başlatılıyor (Port: 7000 / 5000)...
start "QR Menu - Backend API" cmd /k "cd /d D:\menü\QrMenu.Backend && dotnet run"

timeout /t 3 /nobreak > nul

echo [2/2] Angular Frontend Başlatılıyor (Port: 4200)...
start "QR Menu - Angular Frontend" cmd /k "cd /d D:\menü\QrMenu.Frontend && npm start"

echo.
echo ========================================================
echo  ✅ Sistem Başlatıldı!
echo.
echo  📱 Müşteri QR Menüsü : http://localhost:4200/
echo  💻 Restoran Admin    : http://localhost:4200/admin
echo  🌐 Backend Swagger   : https://localhost:7000/openapi/v1.json
echo ========================================================
echo.
pause
