# Weather Application

## App structure
- `WpfApp1/` — Main application project containing XAML views, code-behind, shared view-model logic, and service integrations.
  - `App.xaml` / `App.xaml.cs` — Application entry, resource dictionaries, and startup initialization.
  - `MainWindow.xaml` / `MainWindow.xaml.cs` — Primary window layout with bindings for current conditions, hourly preview, and daily forecast cards.
  - `ViewModelBase.cs` — Central view model coordinating location lookup, forecast retrieval, and property change notifications for the UI.
  - `services.cs` — HTTP client helpers that call OpenWeather endpoints (current, daily, hourly, and air quality) and map responses to view-model structures.
  - `formats.cs` — Utility helpers for formatting units (temperature, wind, timestamps) prior to display.
  - `Images/` — Static assets (e.g., weather icons) referenced by `MainWindow.xaml` for the forecast cards.

## Data flow and responsibilities
- **Startup:** `App.xaml.cs` initializes the app and opens `MainWindow`.
- **Location resolution:** `ViewModelBase` attempts IP-based geolocation (via `services.cs`) and falls back to manual city searches triggered from the UI.
- **Data retrieval:** `services.cs` issues HTTP requests for current weather, hourly snapshots, daily forecasts, and air-quality indices, returning lightweight DTOs.
- **Binding and formatting:** `ViewModelBase` normalizes service responses (with help from `formats.cs`), exposes bindable properties, and raises change notifications consumed by `MainWindow.xaml` bindings.
- **Presentation:** `MainWindow.xaml` arranges current conditions, hourly list, and seven-day cards, using assets from `Images/` to render status icons.
