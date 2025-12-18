using System.Text;
using System.Text.Json;
using GymManagementSystem.Models;

namespace GymManagementSystem.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;

        public OpenAIService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration["OpenAI:ApiKey"] ?? "";
            
            if (!string.IsNullOrEmpty(_apiKey) && _apiKey != "your-openai-api-key-here")
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }
        }

        public async Task<string> GenerateExerciseRecommendations(int age, int height, decimal weight, 
            string gender, FitnessGoal goal, ActivityLevel activityLevel)
        {
            var prompt = $@"
            Kullanıcı Profili:
            - Yaş: {age}
            - Boy: {height} cm
            - Kilo: {weight} kg
            - Cinsiyet: {gender}
            - Fitness Hedefi: {GetFitnessGoalText(goal)}
            - Aktivite Seviyesi: {GetActivityLevelText(activityLevel)}

            Bu kullanıcı için detaylı bir egzersiz programı öner. Program şunları içermeli:
            1. Haftalık egzersiz planı (hangi günler hangi egzersizler)
            2. Her egzersiz için set ve tekrar sayıları
            3. Başlangıç, orta ve ileri seviye alternatifleri
            4. Güvenlik önerileri ve dikkat edilmesi gerekenler
            5. İlerleme takibi önerileri

            Türkçe olarak detaylı ve uygulanabilir bir program hazırla.";

            return await CallOpenAI(prompt);
        }

        public async Task<string> GenerateDietRecommendations(int age, int height, decimal weight, 
            string gender, FitnessGoal goal, ActivityLevel activityLevel)
        {
            var bmi = (float)(weight / ((decimal)height / 100 * (decimal)height / 100));
            
            var prompt = $@"
            Kullanıcı Profili:
            - Yaş: {age}
            - Boy: {height} cm
            - Kilo: {weight} kg
            - BMI: {bmi:F1}
            - Cinsiyet: {gender}
            - Fitness Hedefi: {GetFitnessGoalText(goal)}
            - Aktivite Seviyesi: {GetActivityLevelText(activityLevel)}

            Bu kullanıcı için kişiselleştirilmiş bir beslenme planı hazırla. Plan şunları içermeli:
            1. Günlük kalori ihtiyacı hesaplaması
            2. Makro besin dağılımı (protein, karbonhidrat, yağ)
            3. Örnek günlük beslenme programı (kahvaltı, öğle, akşam, ara öğünler)
            4. Önerilen besinler ve kaçınılması gerekenler
            5. Su tüketimi ve takviye önerileri
            6. Egzersiz öncesi ve sonrası beslenme önerileri

            Türkçe olarak detaylı ve uygulanabilir bir beslenme planı hazırla.";

            return await CallOpenAI(prompt);
        }

        public async Task<string> AnalyzePhoto(string photoDescription)
        {
            var prompt = $@"
            Fotoğraf Analizi:
            {photoDescription}

            Bu fotoğraf açıklamasına dayanarak:
            1. Genel vücut kompozisyonu değerlendirmesi
            2. Güçlü ve geliştirilmesi gereken bölgeler
            3. Önerilen egzersiz odak alanları
            4. Motivasyonel tavsiyeler
            5. İlerleme takibi için öneriler

            Pozitif ve motive edici bir dille Türkçe analiz yap.";

            return await CallOpenAI(prompt);
        }

        private async Task<string> CallOpenAI(string prompt)
        {
            // Check if API key is configured
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "your-openai-api-key-here")
            {
                return GetFallbackRecommendation();
            }
            
            try
            {
                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen profesyonel bir fitness antrenörü ve beslenme uzmanısın. Kullanıcılara güvenli, etkili ve kişiselleştirilmiş öneriler veriyorsun." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 1500,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    return responseObj
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "Öneri oluşturulamadı.";
                }
                else
                {
                    return GetFallbackRecommendation();
                }
            }
            catch (Exception)
            {
                return GetFallbackRecommendation();
            }
        }

        private string GetFallbackRecommendation()
        {
            return @"
            **Genel Fitness Önerileri:**

            **Egzersiz Programı:**
            - Haftada 3-4 gün düzenli egzersiz yapın
            - Kardiyo ve kuvvet antrenmanını birleştirin
            - Her antrenman öncesi 10 dakika ısınma yapın
            - Egzersiz sonrası esnetme hareketlerini ihmal etmeyin

            **Beslenme Önerileri:**
            - Günde 3 ana öğün ve 2 ara öğün tüketin
            - Bol su için (günde en az 2-3 litre)
            - Protein kaynaklarını her öğüne dahil edin
            - İşlenmiş gıdalardan kaçının, doğal besinleri tercih edin

            **Genel Tavsiyeler:**
            - Düzenli uyku alın (7-8 saat)
            - Stres yönetimi yapın
            - İlerlemelerinizi takip edin
            - Sabırlı olun ve tutarlı kalın

            *Not: Daha kişiselleştirilmiş öneriler için bir fitness uzmanı veya diyetisyene danışmanızı öneririz.*";
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
        public async Task<string> GenerateImage(string prompt)
        {
            // Check if API key is configured
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "your-openai-api-key-here")
            {
                return "/images/placeholder_future_self.jpg"; // Return a placeholder if no key
            }

            try
            {
                var requestBody = new
                {
                    model = "dall-e-3",
                    prompt = prompt,
                    n = 1,
                    size = "1024x1024"
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/images/generations", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    var imageUrl = responseObj
                        .GetProperty("data")[0]
                        .GetProperty("url")
                        .GetString();

                    // Download image and save locally
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
                        var fileName = $"generated_{Guid.NewGuid()}.png";
                        var relativePath = $"/uploads/generated/{fileName}";
                        var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "generated");
                        
                        Directory.CreateDirectory(absolutePath);
                        await File.WriteAllBytesAsync(Path.Combine(absolutePath, fileName), imageBytes);
                        
                        return relativePath;
                    }
                }
            }
            catch (Exception)
            {
                // Log error
            }

            return "";
        }
    }
}