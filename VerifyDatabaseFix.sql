-- Veritabanı düzeltmelerini doğrulama scripti
USE GymManagementSystemDb;
GO

PRINT '========================================';
PRINT 'VERİTABANI DOĞRULAMA RAPORU';
PRINT '========================================';
PRINT '';

-- Services kontrolü
PRINT '1. SERVICES TABLOSU:';
PRINT '--------------------';
SELECT 
    Id,
    Name,
    Category,
    Price,
    Duration,
    IsActive
FROM Services
ORDER BY Id;

PRINT '';
PRINT 'Services Özet:';
SELECT 
    'Toplam Hizmet' as Bilgi,
    COUNT(*) as Sayi
FROM Services
UNION ALL
SELECT 
    'Category Dolu' as Bilgi,
    COUNT(*) as Sayi
FROM Services
WHERE Category IS NOT NULL AND Category != ''
UNION ALL
SELECT 
    'Category Boş' as Bilgi,
    COUNT(*) as Sayi
FROM Services
WHERE Category IS NULL OR Category = '';

PRINT '';
PRINT '2. TRAINER AVAILABILITIES TABLOSU:';
PRINT '-----------------------------------';
SELECT 
    ta.Id,
    t.FirstName + ' ' + t.LastName as Antrenor,
    CASE ta.DayOfWeek
        WHEN 0 THEN 'Pazar'
        WHEN 1 THEN 'Pazartesi'
        WHEN 2 THEN 'Salı'
        WHEN 3 THEN 'Çarşamba'
        WHEN 4 THEN 'Perşembe'
        WHEN 5 THEN 'Cuma'
        WHEN 6 THEN 'Cumartesi'
    END as Gun,
    CONVERT(VARCHAR(5), ta.StartTime, 108) as BaslangicSaati,
    CONVERT(VARCHAR(5), ta.EndTime, 108) as BitisSaati,
    ta.IsAvailable as Musait,
    ta.IsActive as Aktif
FROM TrainerAvailabilities ta
INNER JOIN Trainers t ON ta.TrainerId = t.Id
ORDER BY t.FirstName, ta.DayOfWeek;

PRINT '';
PRINT 'TrainerAvailabilities Özet:';
SELECT 
    'Toplam Müsaitlik' as Bilgi,
    COUNT(*) as Sayi
FROM TrainerAvailabilities
UNION ALL
SELECT 
    'IsAvailable = 1' as Bilgi,
    COUNT(*) as Sayi
FROM TrainerAvailabilities
WHERE IsAvailable = 1
UNION ALL
SELECT 
    'IsAvailable = 0 veya NULL' as Bilgi,
    COUNT(*) as Sayi
FROM TrainerAvailabilities
WHERE IsAvailable = 0 OR IsAvailable IS NULL;

PRINT '';
PRINT '3. TRAINERS TABLOSU:';
PRINT '--------------------';
SELECT 
    Id,
    FirstName + ' ' + LastName as AdSoyad,
    Email,
    Specialization,
    Experience,
    IsActive
FROM Trainers
ORDER BY Id;

PRINT '';
PRINT '========================================';
PRINT 'DOĞRULAMA TAMAMLANDI';
PRINT '========================================';
GO