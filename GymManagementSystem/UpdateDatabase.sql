-- Mevcut veritabanını güncellemek için SQL script
-- Bu script mevcut verileri korur ve sadece eksik alanları günceller

USE GymManagementSystemDb;
GO

-- 1. Services tablosundaki Category alanını güncelle
PRINT 'Services tablosu güncelleniyor...';

UPDATE Services 
SET Category = CASE 
    WHEN Name LIKE '%Personal Training%' THEN 'Fitness'
    WHEN Name LIKE '%Group Fitness%' THEN 'Fitness'
    WHEN Name LIKE '%Yoga%' THEN 'Yoga'
    WHEN Name LIKE '%Cardio%' THEN 'Cardio'
    WHEN Name LIKE '%Weight%' THEN 'Strength'
    WHEN Name LIKE '%Training%' THEN 'Fitness'
    ELSE 'Genel'
END
WHERE Category IS NULL OR Category = '';

PRINT 'Services güncellendi: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' kayıt';

-- 2. TrainerAvailabilities tablosundaki IsAvailable alanını güncelle
PRINT 'TrainerAvailabilities tablosu güncelleniyor...';

UPDATE TrainerAvailabilities 
SET IsAvailable = 1
WHERE IsAvailable IS NULL OR IsAvailable = 0;

PRINT 'TrainerAvailabilities güncellendi: ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' kayıt';

-- 3. Kontrol sorguları
PRINT '';
PRINT '=== GÜNCELLEMELER TAMAMLANDI ===';
PRINT '';

PRINT 'Services Durumu:';
SELECT 
    Category,
    COUNT(*) as Adet
FROM Services
GROUP BY Category
ORDER BY Category;

PRINT '';
PRINT 'TrainerAvailabilities Durumu:';
SELECT 
    COUNT(*) as ToplamKayit,
    SUM(CASE WHEN IsAvailable = 1 THEN 1 ELSE 0 END) as MusaitKayit
FROM TrainerAvailabilities;

PRINT '';
PRINT 'Güncelleme başarıyla tamamlandı!';
GO