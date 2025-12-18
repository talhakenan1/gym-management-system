# FitLife Gym API Test Examples

Bu dosya, oluşturulan REST API endpoint'lerini test etmek için örnek istekler içerir.

## Base URL
```
http://localhost:5109
```

## API Endpoints

### 1. Trainers API

#### Tüm Antrenörleri Listele
```http
GET /api/trainers
```

#### Belirli Tarihteki Uygun Antrenörleri Getir
```http
GET /api/trainers/available/2024-12-15
```

#### Belirli Antrenörün Detaylarını Getir
```http
GET /api/trainers/1
```

### 2. Services API

#### Tüm Hizmetleri Listele
```http
GET /api/services
```

#### Kategoriye Göre Hizmetleri Getir
```http
GET /api/services/category/fitness
```

#### Fiyat Aralığına Göre Hizmetleri Getir
```http
GET /api/services/price-range?min=50&max=200
```

#### Belirli Hizmetin Detaylarını Getir
```http
GET /api/services/1
```

#### Hizmet Kategorilerini Listele
```http
GET /api/services/categories
```

### 3. Appointments API (Authentication Required)

#### Kullanıcının Randevularını Getir
```http
GET /api/appointments/user/{userId}
Authorization: Bearer {token}
```

#### Antrenörün Belirli Tarihteki Randevularını Getir
```http
GET /api/appointments/trainer/1/date/2024-12-15
Authorization: Bearer {token}
```

#### Bekleyen Randevuları Getir (Admin Only)
```http
GET /api/appointments/pending
Authorization: Bearer {token}
```

#### Randevu İstatistiklerini Getir (Admin Only)
```http
GET /api/appointments/statistics
Authorization: Bearer {token}
```

## Test Komutları (PowerShell)

### Antrenörleri Test Et
```powershell
Invoke-RestMethod -Uri "http://localhost:5109/api/trainers" -Method GET
```

### Hizmetleri Test Et
```powershell
Invoke-RestMethod -Uri "http://localhost:5109/api/services" -Method GET
```

### Belirli Tarihteki Uygun Antrenörleri Test Et
```powershell
Invoke-RestMethod -Uri "http://localhost:5109/api/trainers/available/2024-12-15" -Method GET
```

### Kategoriye Göre Hizmetleri Test Et
```powershell
Invoke-RestMethod -Uri "http://localhost:5109/api/services/category/fitness" -Method GET
```

## Curl Komutları

### Antrenörleri Test Et
```bash
curl -X GET "http://localhost:5109/api/trainers"
```

### Hizmetleri Test Et
```bash
curl -X GET "http://localhost:5109/api/services"
```

### Belirli Tarihteki Uygun Antrenörleri Test Et
```bash
curl -X GET "http://localhost:5109/api/trainers/available/2024-12-15"
```

## Özellikler

### LINQ Filtreleme Örnekleri
1. **Tarihe Göre Uygun Antrenörler**: `TrainersApiController.GetAvailableTrainers()` metodu LINQ kullanarak belirli tarihteki uygun antrenörleri filtreler.
2. **Kategoriye Göre Hizmetler**: `ServicesApiController.GetServicesByCategory()` metodu LINQ ile kategori filtrelemesi yapar.
3. **Fiyat Aralığına Göre Hizmetler**: `ServicesApiController.GetServicesByPriceRange()` metodu LINQ ile fiyat filtrelemesi yapar.
4. **Kullanıcı Randevuları**: `AppointmentsApiController.GetUserAppointments()` metodu LINQ ile kullanıcıya özel randevuları filtreler.

### Güvenlik Özellikleri
- Authentication gerekli endpoint'ler için `[Authorize]` attribute'u kullanılmıştır
- Admin-only endpoint'ler için `[Authorize(Roles = "Admin")]` kullanılmıştır
- Kullanıcılar sadece kendi randevularını görebilir (admin hariç)

### API Response Formatı
Tüm API endpoint'leri JSON formatında response döner ve sadece gerekli alanları içerir (projection kullanılarak).