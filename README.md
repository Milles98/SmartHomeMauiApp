# SmartHomeMauiApp

SmartHomeMauiApp is a cross-platform smart home management application built using .NET MAUI. 
The app integrates with Azure IoT Hub to manage various smart home devices such as AC, Fans, and Lamps. 
It supports Android, iOS, MacCatalyst, and Windows platforms, and it utilizes modern MVVM architecture and asynchronous programming patterns.

## Table of Contents
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Features](#features)
- [Project Dependencies](#project-dependencies)
- [Testing](#testing)
- [Contributing](#contributing)
- [Contact](#contact)

## Project Structure

This solution consists of five main projects:

1. SmartHomeMauiApp:
    - The main project for the smart home app using .NET MAUI.
    - Implements the UI, device management, and settings for controlling IoT devices.
    - Organized using the MVVM pattern for clean separation of concerns.

2. Shared.Library:
    - A shared library that provides core services and models used by the main app.
    - Contains reusable services like Azure IoT Hub interactions, communication handling, and database management.

3. AzureFunctions:
    - Handles serverless logic, such as device registration, and communication between the app and Azure IoT services.

4. SmartHomeMauiApp.Tests:
    - Contains unit tests and integration tests for validating the core functionality of the SmartHomeMauiApp.
    - Includes xUnit tests for testing viewmodels, services, and Azure Functions.

5. Platforms:

- Platform-specific implementations for handling features across Android, iOS, Windows, and MacCatalyst.

## Folders in SmartHomeMauiApp

* MVVM/ViewModels: Contains the viewmodels following the MVVM pattern.
    - AddDeviceViewModel.cs: Manages device addition logic.
    - SettingsViewModel.cs: Handles user preferences and settings storage.
    - MainViewModel.cs: Handles the main page and device interactions.
    - DeviceDetailViewModel.cs: Manages device-specific details for viewing and control.
    - HistoryViewModel.cs: Retrieves and displays device usage or state history.

* MVVM/Views: Contains the XAML pages that represent the UI of the app.
    - AddDevicePage.xaml
    - SettingsPage.xaml
    - DeviceDetailPage.xaml
    - MainPage.xaml
    - HistoryPage.xaml

* Services: Provides common services, including navigation and preferences management.
    - INavigationService.cs
    - IPreferencesService.cs
    - PreferencesService.cs
    - NavigationService.cs

* Database: Handles SQLite database operations.
    - DbContext.cs
    - IDbContext.cs

## Prerequisites

Before running the project, ensure you have the following:

* .NET 8.0 SDK
* Visual Studio 2022 (or later) with the following workloads:
    - .NET MAUI
    - Azure Development (for Azure Functions)
* Azure IoT Hub: Ensure you have an active Azure IoT Hub for device registration and management.

## Configuration

### Azure IoT Hub Setup

To connect with Azure IoT Hub, configure the following settings:

1. Connection String: Provide your IoT Hub connection string in the app settings.
2. Device Registration: Ensure that devices are properly registered via the Azure Functions project, which interacts with the IoT Hub.

### Database Setup

The app uses SQLite for local storage of settings and device configurations. The database is initialized automatically when the app runs.

## Features

* Cross-Platform: Runs on Android, iOS, MacCatalyst, and Windows platforms with a shared codebase using .NET MAUI.
* Azure IoT Hub Integration: Seamlessly connect and manage IoT devices via Azure IoT Hub.
* Device Management: Add, remove, and control IoT devices (AC, Fan, Lamp) from within the app.
* User Settings Management: Store and update user preferences, such as email and connection strings.
* Device State History: View historical data related to device states and interactions.
* Testable Architecture: Built with MVVM and dependency injection, making it easy to test components individually.

## Project Dependencies

The following key libraries and packages are used in the solution:

* Azure Communication Email (v1.0.1): For sending communication through Azure's email services.
* CommunityToolkit.MVVM (v8.2.2): MVVM Toolkit for simplifying viewmodel implementation.
* Microsoft.Azure.Devices.Shared (v1.30.4): For working with IoT device twin and IoT Hub communication.
* Microsoft.Maui.Controls: Core UI framework for building cross-platform apps.
* sqlite-net-pcl (v1.9.172): SQLite wrapper for database interactions.

## Testing

Unit tests and integration tests are written using xUnit in the SmartHomeMauiApp.Tests project. The tests cover:

* ViewModels (e.g., SettingsViewModelTests, AddDeviceViewModelTests)
* Azure Functions interactions
* Services such as DeviceManager and PreferencesService

## Contributing

This project is currently a school assignment, so external contributions are not yet accepted. However, feel free to fork the repository and experiment on your own!

## Contact

For any questions or feedback, please contact [mille.elfver98@gmail.com](mailto:mille.elfver98@gmail.com).
    
