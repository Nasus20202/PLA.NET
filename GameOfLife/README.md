# Conway's Game of Life - WPF Application

## Opis Projektu

Aplikacja Conway's Game of Life zaimplementowana w WPF zgodnie z wymaganiami laboratorium. Aplikacja demonstruje zaawansowane koncepcje WPF i zachowuje wysokie standardy kodowania.

## Zaimplementowane Wymagania

### ✅ Funkcjonalność Podstawowa
- **Konfigurowalny rozmiar planszy**: Domyślnie 100x100, możliwość zmiany rozmiaru (10-500)
- **Edycja stanu**: Kliknięcie/przeciąganie myszą by przełączać komórki (tylko w trybie edycji)
- **Czyszczenie planszy**: Przycisk "Clear"
- **Losowanie stanu**: Przycisk "Randomize" (30% prawdopodobieństwo żywej komórki)
- **Wykonywanie kroków**: Przycisk "Step" do pojedynczego kroku
- **Ciągła animacja**: Przycisk "Start/Stop" z regulacją prędkości (suwak 1-200)
- **Powrót do edycji**: Po zatrzymaniu automatyczny powrót do trybu edycji

### ✅ Zapis/Odczyt do Pliku
- Format tekstowy (.gol)
- Zawiera: rozmiar planszy, reguły, generację, stan komórek
- Komórki zapisywane jako 'O' (żywa) i '.' (martwa)
- Pełna kompatybilność zapisu i odczytu

### ✅ Statystyki
- **Liczba pokoleń**: Bieżące pokolenie
- **Liczba żywych komórek**: Aktualna liczba
- **Urodzone komórki**: Suma wszystkich narodzonych komórek
- **Martwe komórki**: Suma wszystkich zmarłych komórek

### ✅ Konfigurowalne Reguły
- Format: **B[liczby]/S[liczby]**
- Przykład: **B3/S23** (standardowe reguły Conwaya)
- B (Birth): liczby sąsiadów dające narodziny
- S (Survival): liczby sąsiadów dające przeżycie
- Przycisk "Apply Rules" do zastosowania

### ✅ Dwupoziomowy Zoom
- Poziomy: 50%, 75%, 100%, 125%, 150%, 175%, 200%
- Przyciski +/- do zmiany
- ScrollViewer do przewijania powiększonego widoku
- Cała plansza "żyje" niezależnie od widoku

### ✅ Zmiana Prezentacji Komórek
- **Kolor**: 8 predefiniowanych kolorów (Lime Green, Cyan, Magenta, Yellow, Orange, Red, Blue, White)
- **Kształt**: 3 opcje (Rectangle, Ellipse, RoundedRectangle)
- Natychmiastowa aktualizacja wizualna

## Zastosowane Koncepcje WPF

### 🎨 Styles (Style)
- **ModernButtonStyle**: Podstawowy styl przycisków z zaokrąglonymi rogami
- **PrimaryButtonStyle**: Zielony styl dla akcji głównych (Start)
- **DangerButtonStyle**: Czerwony styl dla akcji destruktywnych (Stop, Clear)
- **ModernTextBoxStyle**: Ciemny motyw dla pól tekstowych
- **ModernLabelStyle**: Styl dla etykiet tekstowych
- **StatsLabelStyle**: Wyróżniony styl dla statystyk
- **PanelStyle**: Styl dla paneli kontrolnych z obramowaniem

### 📋 Templates (Szablony)
- **ControlTemplate dla Button**: Custom template z Border i zaokrąglonymi rogami
- **Dynamiczne kształty komórek**: Rectangle z konfigurowalnymi RadiusX/RadiusY

### ⚡ Triggers (Wyzwalacze)
- **IsMouseOver**: Zmiana koloru przycisku przy najechaniu
- **IsPressed**: Zmiana koloru przy kliknięciu
- **IsEnabled**: Przezroczystość dla wyłączonych kontrolek
- **DataTrigger**: Zmiana koloru statusu (Running=zielony, Editing=pomarańczowy)

### 🎬 Animations (Animacje)
- **DispatcherTimer**: Płynna animacja generacji
- **Smooth transitions**: Wizualne przejścia między stanami
- **Real-time updates**: Natychmiastowa aktualizacja UI

### 🏗️ Inne Koncepcje WPF
- **MVVM Pattern**: Pełna separacja Model-View-ViewModel
- **Data Binding**: Dwukierunkowe bindowanie danych
- **ICommand**: RelayCommand dla wszystkich akcji
- **INotifyPropertyChanged**: Automatyczne odświeżanie UI
- **DependencyProperty**: Właściwości zależne w kontrolce GameGrid
- **Custom Control**: GameGrid jako niestandardowa kontrolka Canvas
- **Value Converters**: ColorStringToBrushConverter
- **Resource Dictionaries**: Scentralizowane zasoby w XAML

## Architektura Kodu

### 📁 Struktura Projektu
```
GameOfLife/
├── Models/
│   ├── GameOfLifeEngine.cs      - Silnik gry (logika automatu)
│   ├── GameRules.cs              - Reguły B/S
│   └── GameState.cs              - Stan gry (save/load)
├── ViewModels/
│   ├── ViewModelBase.cs          - Bazowa klasa ViewModel
│   ├── RelayCommand.cs           - Implementacja ICommand
│   └── MainViewModel.cs          - Główny ViewModel
├── Controls/
│   └── GameGrid.cs               - Custom kontrolka Canvas
├── Converters/
│   └── ColorStringToBrushConverter.cs
├── MainWindow.xaml               - Główny widok
└── MainWindow.xaml.cs            - Code-behind
```

### 🎯 Zastosowane Wzorce i Zasady

#### MVVM (Model-View-ViewModel)
- **Model**: GameOfLifeEngine, GameRules, GameState
- **View**: MainWindow.xaml, GameGrid
- **ViewModel**: MainViewModel z pełnym data binding

#### SOLID Principles
- **Single Responsibility**: Każda klasa ma jedną odpowiedzialność
- **Open/Closed**: Łatwe rozszerzanie (np. nowe reguły, kształty)
- **Dependency Inversion**: Zależności przez abstrakcje (ICommand)

#### Clean Code
- Czytelne nazwy zmiennych i metod
- Dokumentacja XML dla publicznych klas/metod
- Separacja logiki biznesowej od UI
- Obsługa błędów z komunikatami użytkownika

## Instrukcja Użycia

### Podstawowe Operacje
1. **Edycja planszy**: Kliknij na komórki by je przełączać (żywa/martwa)
2. **Start**: Rozpocznij animację automatu
3. **Stop**: Zatrzymaj animację (powrót do edycji)
4. **Step**: Wykonaj pojedynczy krok
5. **Randomize**: Wypełnij planszę losowymi komórkami
6. **Clear**: Wyczyść całą planszę

### Zaawansowane Funkcje
- **Zmiana rozmiaru**: Ustaw Width/Height i kliknij "Resize Grid"
- **Zmiana reguł**: Wpisz reguły (np. B36/S23) i kliknij "Apply Rules"
- **Zoom**: Użyj przycisków +/- do przybliżenia/oddalenia
- **Zapisz**: Zapisz stan gry do pliku .gol
- **Wczytaj**: Wczytaj zapisany stan

### Popularne Reguły
- **B3/S23** - Conway's Game of Life (klasyczne)
- **B36/S23** - HighLife
- **B3678/S34678** - Day & Night
- **B368/S245** - Morley
- **B1357/S1357** - Replicator

## Wymagania Techniczne

- **.NET 8.0** (net8.0-windows)
- **WPF** (Windows Presentation Foundation)
- **C# 12** z nullable reference types

## Kompilacja i Uruchomienie

```bash
# Kompilacja
dotnet build

# Uruchomienie
dotnet run
```

## Cechy Wyróżniające

✨ **Profesjonalny Design**: Ciemny motyw inspirowany Visual Studio  
✨ **Responsywność**: Płynna animacja nawet dla dużych plansz  
✨ **Intuicyjny UI**: Logiczne grupowanie funkcji w panelach  
✨ **Solidna Architektura**: MVVM, SOLID, Clean Code  
✨ **Pełna Funkcjonalność**: Wszystkie wymagania + dodatkowe features  
✨ **Bez Ostrzeżeń**: Clean build (0 warnings, 0 errors)  

## Autor

Projekt zrealizowany zgodnie z wymaganiami laboratorium WPF 2025.

