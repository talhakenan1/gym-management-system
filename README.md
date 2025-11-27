# FitLife Gym - Spor Salonu Yönetim ve Randevu Sistemi

Bu proje, ASP.NET Core MVC kullanılarak geliştirilmiş kapsamlı bir spor salonu yönetim ve randevu sistemidir.

## 🎯 Proje Özellikleri

### Temel Özellikler
- **Modern Web Arayüzü**: Bootstrap 5 ile responsive tasarım
- **Kimlik Doğrulama**: ASP.NET Core Identity ile güvenli giriş sistemi
- **Rol Bazlı Yetkilendirme**: Admin ve Üye rolleri
- **CRUD İşlemleri**: Tüm varlıklar için tam CRUD desteği
- **Randevu Sistemi**: Çakışma kontrolü ile akıllı randevu yönetimi

### Teknik Özellikler
- **Platform**: ASP.NET Core MVC (.NET 9)
- **Veritabanı**: SQL Server LocalDB
- **ORM**: Entity Framework Core
- **Kimlik Yönetimi**: ASP.NET Core Identity
- **Frontend**: Bootstrap 5, Font Awesome, jQuery
- **Validation**: Client & Server-side validation

## 🏗️ Proje Yapısı

### Modeller
- **ApplicationUser**: Kullanıcı bilgileri (Identity'den türetilmiş)
- **Gym**: Spor salonu bilgileri
- **Trainer**: Antrenör bilgileri
- **Service**: Hizmet tanımları
- **Appointment**: Randevu kayıtları
- **TrainerService**: Antrenör-Hizmet ilişkisi
- **TrainerAvailability**: Antrenör müsaitlik saatleri

### Controller'lar
- **HomeController**: Ana sayfa, antrenörler ve hizmetler listesi
- **AdminController**: Admin dashboard ve yönetim
- **TrainersController**: Antrenör CRUD işlemleri
- **ServicesController**: Hizmet CRUD işlemleri
- **AppointmentsController**: Randevu yönetimi

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- .NET 9 SDK
- SQL Server LocalDB
- Visual Studio 2022 veya VS Code

### Kurulum Adımları

1. **Projeyi klonlayın**
```bash
git clone [repository-url]
cd GymManagementSystem
```

2. **Bağımlılıkları yükleyin**
```bash
dotnet restore
```

3. **Veritabanını oluşturun**
```bash
dotnet ef database update
```

4. **Projeyi çalıştırın**
```bash
dotnet run
```

5. **Tarayıcıda açın**
```
http://localhost:5109
```

## 👤 Varsayılan Kullanıcılar

### Admin Kullanıcısı
- **Email**: ogrencinumarasi@sakarya.edu.tr
- **Şifre**: sau

### Test Verileri
Proje ilk çalıştırıldığında otomatik olarak şu test verileri oluşturulur:
- 3 Antrenör
- 5 Hizmet türü
- Antrenör müsaitlik saatleri
- Antrenör-Hizmet ilişkileri

## 📱 Özellikler

### Kullanıcı Özellikleri
- ✅ Üye kayıt ve giriş sistemi
- ✅ Antrenör ve hizmet listelerini görüntüleme
- ✅ Randevu alma (çakışma kontrolü ile)
- ✅ Kendi randevularını görüntüleme ve iptal etme
- ✅ Responsive tasarım (mobil uyumlu)

### Admin Özellikleri
- ✅ Dashboard ile sistem istatistikleri
- ✅ Antrenör yönetimi (CRUD)
- ✅ Hizmet yönetimi (CRUD)
- ✅ Randevu yönetimi ve onaylama
- ✅ Antrenör müsaitlik saatleri yönetimi

### Teknik Özellikler
- ✅ Entity Framework Core ile veritabanı yönetimi
- ✅ Identity ile güvenli kimlik doğrulama
- ✅ Rol bazlı yetkilendirme
- ✅ Client ve Server-side validation
- ✅ AJAX ile dinamik içerik
- ✅ Responsive Bootstrap 5 tasarım

## 🗄️ Veritabanı Yapısı

### Ana Tablolar
- **AspNetUsers**: Kullanıcı bilgileri
- **AspNetRoles**: Roller (Admin, Member)
- **Gyms**: Spor salonu bilgileri
- **Trainers**: Antrenör bilgileri
- **Services**: Hizmet tanımları
- **Appointments**: Randevu kayıtları
- **TrainerServices**: Antrenör-Hizmet ilişkisi
- **TrainerAvailabilities**: Antrenör müsaitlik saatleri

## 🔧 Geliştirme Notları

### Gelecek Özellikler
- [ ] REST API geliştirme
- [ ] Yapay zeka entegrasyonu
- [ ] Ödeme sistemi entegrasyonu
- [ ] Email bildirimleri
- [ ] Raporlama sistemi
- [ ] Mobil uygulama API'si

### Bilinen Sorunlar
- Şu anda tek spor salonu desteği
- Ödeme sistemi entegre değil
- Email bildirimleri aktif değil

## 📄 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici

**Talha Kenan Yaylacık**
- Öğrenci No: B211210099
- Email: talhakennan1 qgmail.com
- GitHub: github.com/talhakenan1

---

**Not**: Bu proje Sakarya Üniversitesi Web Programlama dersi kapsamında geliştirilmiştir.
