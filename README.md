# SmartHomeMauiApp

SmartHomeMauiApp is a cross-platform application built using .NET MAUI. It supports Android, iOS, MacCatalyst, and Windows platforms. The app is designed to manage smart home devices and integrates with Azure IoT Hub.

## Project Structure

- **SmartHomeMauiApp**: The main project containing the application code.
- **Shared.Library**: A shared library project containing common services and utilities used by the main application.

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 (or later) with .NET MAUI workload installed

## Getting Started

1. **Clone the repository**:

- git clone https://github.com/your-repo/SmartHomeMauiApp.git
- cd SmartHomeMauiApp

2. **Restore NuGet packages**:

- dotnet restore

3. **Build the solution**:

- dotnet build

4. **Run the application**:
    - For Android:

- dotnet build -t:Run -f net8.0-android

    - For Windows:
 
- dotnet build -t:Run -f net8.0-windows10.0.19041.0

## Project Configuration

### SmartHomeMauiApp.csproj

The main project file for the SmartHomeMauiApp. It includes configurations for different target frameworks, package references, and other project settings.

### Shared.Library.csproj

The project file for the shared library. It includes common services and utilities used by the main application.

## Dependencies

- `azure.communication.email` (v1.0.1)
- `CommunityToolkit.Mvvm` (v8.2.2)
- `Microsoft.Azure.Devices.Shared` (v1.30.4)
- `Microsoft.Maui.Controls`
- `Microsoft.Maui.Controls.Compatibility`
- `Microsoft.Extensions.Logging.Debug` (v8.0.0)
- `sqlite-net-pcl` (v1.9.172)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.

## Contributing

Contributions are welcome! Please open an issue or submit a pull request for any changes.

## Contact

For any questions or feedback, please contact [your-email@example.com](mailto:your-email@example.com).
    
