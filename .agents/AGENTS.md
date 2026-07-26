# LiteNex Launcher — Proje Kuralları

## KRITIK: Güncelleme Sureci (ASLA IHLAL ETME)

Her kod degisikligi yapildiktan sonra GitHub'a push etmeden once asagidaki 3 dosya **her zaman birlikte ve ayni anda** guncellenmek zorundadir:

### 1. version.json (GitHub'daki surum bildirimi)
- "version" ve "versionCode" degerlerini artir

### 2. Launcher.cs (Client icindeki sabit surum kodu)
- CURRENT_VERSION_CODE sabiti → version.json ile birebir eslesmelidir
- CURRENT_VERSION_NAME sabiti → version.json ile birebir eslesmelidir

### 3. Binary'ler (derleme)
- LiteNex.exe yeniden derlenmeli
- LiteNexSetup.exe yeniden derlenmeli

---

## Dogru Push Akisi (Her guncelleme icin)

1. Kodu degistir
2. version.json → version ve versionCode degerlerini artir
3. Launcher.cs → CURRENT_VERSION_CODE ve CURRENT_VERSION_NAME sabitlerini ayni degere guncelle
4. Derle: LiteNex.exe + LiteNexSetup.exe
5. GitPush.bat ile GitHub'a push et (derleme + push otomatik)

---

## Neden Kritik?

version.json'daki versionCode > Launcher.cs'deki CURRENT_VERSION_CODE oldugunda:
- Steam-style acilis ekrani guncelleme bulduğunu sanir
- LiteNex_new.exe indirir, update.bat yazar
- Launcher'i kapatip yeniden baslatir
- Yeni baslatilan client yine ayni eski kodu okur => SONSUZ DONGU, ACILIP KAPANMA

---

## Versiyon Artirma Kurali

Degisiklik turune gore:
- Buyuk ozellik / yapisal degisiklik → X.Y.Z → X.(Y+1).0
- Kucuk ozellik / iyilestirme → X.Y.Z → X.Y.(Z+1)
- versionCode = major * 100 + minor * 10 + patch (ornek: 6.5.1 → 651)

---

## Batch Dosyalari

- Build.bat → Sadece LiteNex.exe derler, pause YOK
- BuildSetup.bat → LiteNex.exe + LiteNexSetup.exe derler, pause YOK
- GitPush.bat → Derle + commit + push yapar, pause YOK, etkilesim beklemez
