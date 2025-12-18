-- Update existing Services to have Category
UPDATE Services SET Category = 'Fitness' WHERE Name LIKE '%Training%' OR Name LIKE '%Fitness%';
UPDATE Services SET Category = 'Yoga' WHERE Name LIKE '%Yoga%';
UPDATE Services SET Category = 'Cardio' WHERE Name LIKE '%Cardio%';
UPDATE Services SET Category = 'Strength' WHERE Name LIKE '%Weight%';
UPDATE Services SET Category = 'Genel' WHERE Category IS NULL OR Category = '';

-- Update existing TrainerAvailabilities to have IsAvailable = 1
UPDATE TrainerAvailabilities SET IsAvailable = 1 WHERE IsAvailable IS NULL OR IsAvailable = 0;

-- Verify updates
SELECT 'Services with Category' as TableInfo, COUNT(*) as Count FROM Services WHERE Category IS NOT NULL AND Category != '';
SELECT 'TrainerAvailabilities with IsAvailable' as TableInfo, COUNT(*) as Count FROM TrainerAvailabilities WHERE IsAvailable = 1;