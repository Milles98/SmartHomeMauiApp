SmartHome Maui App
SmartHome Maui App is a cross-platform mobile application designed to manage and control smart home IoT devices using Azure IoT services. Built with .NET MAUI, this app provides a seamless experience across iOS, Android, Windows, and macOS platforms.

Table of Contents
Features
Project Structure
Dependencies
Getting Started
Usage
Contributing
License
Features
Device Management: Manage and control various IoT devices connected to Azure IoT Hub.
MVVM Architecture: Implements the Model-View-ViewModel pattern for maintainable and testable code.
Azure IoT Integration: Provides secure and reliable communication with Azure IoT Hub for device management.
SQLite Database: Uses SQLite for local data storage, including user settings, IoT Hub configurations, and device data.
Cross-Platform: Built with .NET MAUI to support iOS, Android, Windows, and macOS from a single codebase.
User Notifications: Sends and receives notifications for device status changes and errors.
Project Structure
The project is organized into several folders:

MVVM: Contains the Models, Views, and ViewModels for the application.
Models: Defines the data models such as UserSettings, IoTHubSettings, and DeviceSettings.
Views: XAML files defining the user interface of the application (e.g., MainPage, DeviceDetailPage, SettingsPage, HistoryPage).
ViewModels: Classes that handle the binding of data models to views (e.g., MainViewModel, DeviceDetailViewModel, SettingsViewModel, HistoryViewModel).
Database: Contains the SQLite database context and data access methods.
DbContext.cs: Manages the SQLite connection and provides methods for CRUD operations.
Services: Contains service classes for managing business logic and communication with Azure IoT Hub.
DeviceManager.cs: Handles device-related operations such as connecting, disconnecting, and invoking methods on IoT devices.
Resources: Contains application resources such as fonts, images, and styles.
Dependencies
The project relies on the following NuGet packages:

CommunityToolkit.Mvvm - Provides MVVM utilities for .NET applications.
Microsoft.Maui.Essentials - Provides cross-platform APIs for native device features.
Microsoft.Data.Sqlite - A lightweight SQLite database engine for local data storage.
Microsoft.Azure.Devices.Client - Azure IoT Hub client library for secure device communication.
Getting Started
Prerequisites
.NET 8.0 SDK or later
Visual Studio 2022 with the .NET MAUI workload installed
An Azure IoT Hub instance
Installation
Clone the repository:
git clone https://github.com/your-username/SmartHomeMauiApp.git
cd SmartHomeMauiApp
Open the solution: Open SmartHomeMauiApp.sln in Visual Studio.

Restore NuGet packages: Visual Studio will automatically restore all NuGet packages on build.

Configure Azure IoT Hub: Update the IoT Hub connection string in MauiProgram.cs or set it dynamically through the app's settings page.

Usage
To run the application, start the project from Visual Studio. The application will initialize, connect to the IoT Hub, and display the main page.

Main Components
MainPage.xaml: Displays the list of devices and allows navigation to the device detail, settings, or history pages.
DeviceDetailPage.xaml: Shows detailed information about a selected device and allows for controlling its state.
SettingsPage.xaml: Allows the user to update and save IoT Hub connection settings and user information.
HistoryPage.xaml: Displays the history of device interactions, settings changes, and IoT Hub activity.
Contributing
Contributions are welcome! Please follow these steps:

Fork the repository.
Create a new feature branch (git checkout -b feature/your-feature).
Commit your changes (git commit -m 'Add some feature').
Push to the branch (git push origin feature/your-feature).
Open a pull request.
License
Distributed under the MIT License. See LICENSE for more information.

