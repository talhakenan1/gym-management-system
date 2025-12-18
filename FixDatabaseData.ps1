# PowerShell script to fix database data
Write-Host "Veritabanı verilerini güncelleme scripti" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Bu script şunları yapacak:" -ForegroundColor Yellow
Write-Host "1. Services tablosuna Category ekleyecek" -ForegroundColor Yellow
Write-Host "2. TrainerAvailabilities tablosuna IsAvailable = 1 ekleyecek" -ForegroundColor Yellow
Write-Host ""
Write-Host "ÖNEMLI: Uygulamayı kapatın ve bu scripti çalıştırın!" -ForegroundColor Red
Write-Host ""
Write-Host "SQL Komutları:" -ForegroundColor Cyan
Write-Host ""

$sqlCommands = @"
-- Update existing Services to have Category
UPDATE Services SET Category = 'Fitness' WHERE (Name LIKE '%Training%' OR Name LIKE '%Fitness%') AND (Category IS NULL OR Category = '');
UPDATE Services SET Category = 'Yoga' WHERE Name LIKE '%Yoga%' AND (Category IS NULL OR Category = '');
UPDATE Services SET Category = 'Cardio' WHERE Name LIKE '%Cardio%' AND (Category IS NULL OR Category = '');
UPDATE Services SET Category = 'Strength' WHERE Name LIKE '%Weight%' AND (Category IS NULL OR Category = '');
UPDATE Services SET Category = 'Genel' WHERE Category IS NULL OR Category = '';

-- Update existing TrainerAvailabilities to have IsAvailable = 1
UPDATE TrainerAvailabilities SET IsAvailable = 1;

-- Verify updates
SELECT 'Services Updated' as Info, COUNT(*) as Count FROM Services WHERE Category IS NOT NULL AND Category != '';
SELECT 'TrainerAvailabilities Updated' as Info, COUNT(*) as Count FROM TrainerAvailabilities WHERE IsAvailable = 1;
"@

Write-Host $sqlCommands -ForegroundColor White
Write-Host ""
Write-Host "Bu SQL komutlarını SQL Server Management Studio veya Visual Studio'da çalıştırın." -ForegroundColor Green
Write-Host ""
Write-Host "Veya şu komutu kullanın:" -ForegroundColor Cyan
Write-Host "sqlcmd -S (localdb)\mssqllocaldb -d GymManagementSystemDb -Q `"<SQL_KOMUTLARI>`"" -ForegroundColor White