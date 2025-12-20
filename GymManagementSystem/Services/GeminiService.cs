using System.Text;
using System.Text.Json;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private readonly string _modelId;

        public GeminiService(HttpClient httpClient, IConfiguration configuration, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Gemini:ApiKey"] ?? "";
            _baseUrl = _configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta/models/";
            _modelId = _configuration["Gemini:ModelId"] ?? "gemini-pro";
            
            // Configure HttpClient timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<string> GenerateExerciseRecommendations(int age, int height, decimal weight, 
            string gender, FitnessGoal goal, ActivityLevel activityLevel)
        {
            var bmi = CalculateBMI(height, weight);
            var bmiCategory = GetBMICategory(bmi);
            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            var prompt = $@"Tarih: {timestamp}

🏋️ KİŞİYE ÖZEL EGZERSİZ PROGRAMI TALEBİ

📊 KULLANICI PROFİLİ:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
• Yaş: {age} yaşında
• Boy: {height} cm
• Kilo: {weight} kg
• BMI: {bmi:F1} ({bmiCategory})
• Cinsiyet: {gender}
• Fitness Hedefi: {GetFitnessGoalText(goal)}
• Mevcut Aktivite Seviyesi: {GetActivityLevelText(activityLevel)}

📝 LÜTFEN AŞAĞIDAKİ FORMATTA DETAYLI BİR EGZERSİZ PROGRAMI HAZIRLA:

1️⃣ HAFTALIK ANTRENMAN PLANI
   - Her gün için spesifik egzersizler
   - Antrenman süresi ve yoğunluğu
   - Dinlenme günleri

2️⃣ DETAYLI EGZERSİZ LİSTESİ
   - Her egzersiz için: Set sayısı, tekrar sayısı, dinlenme süresi
   - Doğru form açıklaması
   - Alternatif hareketler

3️⃣ ILERLEME PLANI
   - Haftalık hedefler
   - 4 haftalık gelişim beklentisi
   - Zorluk artırma önerileri

4️⃣ ÖNEMLİ UYARILAR
   - Sakatlık önleme ipuçları
   - Isınma ve soğuma rutini
   - Beslenme ve hidrasyon hatırlatmaları

Bu kullanıcının {GetFitnessGoalText(goal)} hedefine ulaşması için optimize edilmiş, bilimsel temelli ve uygulanabilir bir program oluştur. Türkçe olarak yaz.";

            return await CallGemini(prompt);
        }

        public async Task<string> GenerateDietRecommendations(int age, int height, decimal weight, 
            string gender, FitnessGoal goal, ActivityLevel activityLevel)
        {
            var bmi = CalculateBMI(height, weight);
            var bmiCategory = GetBMICategory(bmi);
            var bmr = CalculateBMR(age, height, weight, gender);
            var tdee = CalculateTDEE(bmr, activityLevel);
            var targetCalories = GetTargetCalories(tdee, goal);
            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            var prompt = $@"Tarih: {timestamp}

🥗 KİŞİYE ÖZEL BESLENME PROGRAMI TALEBİ

📊 KULLANICI PROFİLİ:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━
• Yaş: {age} yaşında
• Boy: {height} cm
• Kilo: {weight} kg
• BMI: {bmi:F1} ({bmiCategory})
• Cinsiyet: {gender}
• Fitness Hedefi: {GetFitnessGoalText(goal)}
• Aktivite Seviyesi: {GetActivityLevelText(activityLevel)}

📈 HESAPLANAN DEĞERLER:
• Bazal Metabolizma Hızı (BMR): ~{bmr:F0} kcal/gün
• Toplam Günlük Enerji Harcaması (TDEE): ~{tdee:F0} kcal/gün
• Hedef Kalori Alımı: ~{targetCalories:F0} kcal/gün

📝 LÜTFEN AŞAĞIDAKİ FORMATTA DETAYLI BİR BESLENME PROGRAMI HAZIRLA:

1️⃣ GÜNLÜK MAKRO HEDEFLERİ
   - Protein: ... gram/gün
   - Karbonhidrat: ... gram/gün
   - Yağ: ... gram/gün

2️⃣ ÖRNEK GÜNLÜK MENÜ
   🌅 Kahvaltı (saat ve kalori)
   🍎 Ara Öğün 1
   🥗 Öğle Yemeği
   🍌 Ara Öğün 2
   🍽️ Akşam Yemeği
   🥛 Gece Atıştırması (isteğe bağlı)

3️⃣ ÖNERİLEN BESİNLER
   - Protein kaynakları
   - Karbonhidrat kaynakları
   - Sağlıklı yağlar
   - Sebze ve meyveler

4️⃣ KAÇINILMASI GEREKENLER
   - İşlenmiş gıdalar
   - Şekerli içecekler
   - Zararlı yağlar

5️⃣ HİDRASYON VE TAKVİYE ÖNERİLERİ
   - Günlük su tüketimi
   - Vitamin/mineral önerileri

Bu kullanıcının {GetFitnessGoalText(goal)} hedefine ulaşması için optimize edilmiş, bilimsel temelli ve uygulanabilir bir beslenme programı oluştur. Türkçe olarak yaz.";

            return await CallGemini(prompt);
        }

        public async Task<string> AnalyzePhoto(string photoDescription)
        {
            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            var prompt = $@"Tarih: {timestamp}

📸 FOTOĞRAF ANALİZİ TALEBİ

Kullanıcı fitness için fotoğraf yükledi.
Açıklama: {photoDescription}

Bu bilgiye dayanarak pozitif ve motive edici bir analiz yap:
1. Genel değerlendirme
2. Güçlü yönler
3. Gelişim alanları
4. Öneriler
5. Motivasyon mesajı

Türkçe olarak yaz.";

            return await CallGemini(prompt);
        }

        private async Task<string> CallGemini(string prompt)
        {
            // Check if API key is configured
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "your-gemini-api-key-here")
            {
                _logger.LogWarning("Gemini API key is not configured. Using fallback recommendations.");
                return GetFallbackRecommendation();
            }
            
            try
            {
                _logger.LogInformation("Calling Gemini API...");
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.9,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 4096
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Construct URL using configuration
                var url = $"{_baseUrl}{_modelId}:generateContent?key={_apiKey}";
                
                _logger.LogInformation("Sending request to Gemini API...");
                var response = await _httpClient.PostAsync(url, content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Gemini API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (responseObj.TryGetProperty("candidates", out var candidates) && 
                        candidates.GetArrayLength() > 0)
                    {
                        var text = candidates[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();
                        
                        _logger.LogInformation("Successfully received Gemini API response.");
                        return text ?? "Öneri oluşturulamadı.";
                    }
                    else
                    {
                        _logger.LogWarning("Gemini API returned empty candidates.");
                        return GetFallbackRecommendation();
                    }
                }
                else
                {
                    _logger.LogError($"Gemini API Error: {response.StatusCode} - {responseContent}");
                    
                    // Try to parse error message
                    try
                    {
                        var errorObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                        if (errorObj.TryGetProperty("error", out var error))
                        {
                            var message = error.GetProperty("message").GetString();
                            _logger.LogError($"Gemini API Error Message: {message}");
                        }
                    }
                    catch { }
                    
                    return GetFallbackRecommendation();
                }
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("Gemini API request timed out.");
                return GetFallbackRecommendation();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Gemini API Exception: {ex.Message}");
                return GetFallbackRecommendation();
            }
        }

        private double CalculateBMI(int height, decimal weight)
        {
            var heightInMeters = height / 100.0;
            return (double)weight / (heightInMeters * heightInMeters);
        }

        private string GetBMICategory(double bmi)
        {
            if (bmi < 18.5) return "Zayıf";
            if (bmi < 25) return "Normal";
            if (bmi < 30) return "Fazla Kilolu";
            return "Obez";
        }

        private double CalculateBMR(int age, int height, decimal weight, string gender)
        {
            // Mifflin-St Jeor Equation
            var bmr = 10 * (double)weight + 6.25 * height - 5 * age;
            return gender?.ToLower() == "erkek" ? bmr + 5 : bmr - 161;
        }

        private double CalculateTDEE(double bmr, ActivityLevel level)
        {
            var multiplier = level switch
            {
                ActivityLevel.Sedentary => 1.2,
                ActivityLevel.LightlyActive => 1.375,
                ActivityLevel.ModeratelyActive => 1.55,
                ActivityLevel.VeryActive => 1.725,
                ActivityLevel.ExtremelyActive => 1.9,
                _ => 1.55
            };
            return bmr * multiplier;
        }

        private double GetTargetCalories(double tdee, FitnessGoal goal)
        {
            return goal switch
            {
                FitnessGoal.WeightLoss => tdee - 500,
                FitnessGoal.MuscleGain => tdee + 300,
                _ => tdee
            };
        }

        private string GetFallbackRecommendation()
        {
            var random = new Random();
            var tips = new[]
            {
                "🏃 Kardiyovasküler sağlığınız için haftada en az 150 dakika orta yoğunluklu egzersiz yapın.",
                "💪 Kas kütlenizi korumak için haftada 2-3 gün kuvvet antrenmanı ekleyin.",
                "🥗 Protein alımınızı vücut ağırlığınızın kg başına 1.6-2.2 gram olarak hedefleyin.",
                "💧 Günde en az 2-3 litre su için.",
                "😴 Kas onarımı için günde 7-9 saat uyku alın."
            };
            
            var randomTip = tips[random.Next(tips.Length)];
            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            
            return $@"
⚠️ **AI Servisi Şu An Kullanılamıyor**

Oluşturulma Tarihi: {timestamp}

**Genel Fitness Önerileri:**

**🏋️ Egzersiz Programı:**
- Haftada 3-4 gün düzenli egzersiz yapın
- Kardiyo ve kuvvet antrenmanını birleştirin
- Her antrenman öncesi 10 dakika ısınma yapın
- Egzersiz sonrası esnetme hareketlerini ihmal etmeyin

**🥗 Beslenme Önerileri:**
- Günde 3 ana öğün ve 2 ara öğün tüketin
- Bol su için (günde en az 2-3 litre)
- Protein kaynaklarını her öğüne dahil edin
- İşlenmiş gıdalardan kaçının, doğal besinleri tercih edin

**💡 Günün İpucu:**
{randomTip}

**⚙️ Genel Tavsiyeler:**
- Düzenli uyku alın (7-8 saat)
- Stres yönetimi yapın
- İlerlemelerinizi takip edin
- Sabırlı olun ve tutarlı kalın

*Not: Kişiselleştirilmiş öneriler için AI servisi düzeltildiğinde tekrar deneyin veya bir fitness uzmanına danışın.*";
        }

        private string GetFitnessGoalText(FitnessGoal goal)
        {
            return goal switch
            {
                FitnessGoal.WeightLoss => "Kilo Verme",
                FitnessGoal.MuscleGain => "Kas Kazanımı",
                FitnessGoal.Endurance => "Dayanıklılık",
                FitnessGoal.Strength => "Güç Artırımı",
                FitnessGoal.GeneralFitness => "Genel Fitness",
                FitnessGoal.Flexibility => "Esneklik",
                _ => "Genel Fitness"
            };
        }

        private string GetActivityLevelText(ActivityLevel level)
        {
            return level switch
            {
                ActivityLevel.Sedentary => "Hareketsiz (Masa başı iş)",
                ActivityLevel.LightlyActive => "Az Aktif (Haftada 1-3 gün egzersiz)",
                ActivityLevel.ModeratelyActive => "Orta Aktif (Haftada 3-5 gün egzersiz)",
                ActivityLevel.VeryActive => "Çok Aktif (Haftada 6-7 gün egzersiz)",
                ActivityLevel.ExtremelyActive => "Aşırı Aktif (Günde 2 kez egzersiz)",
                _ => "Orta Aktif"
            };
        }
    }
}
