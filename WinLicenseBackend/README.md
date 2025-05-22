# LicensePortal Backend

This is the backend for the **LicensePortal** Web UI, built with **ASP.NET Core (.NET 8)**.  
It provides REST APIs to generate and download license keys using the [WinLicense](https://www.oreans.com/help/wl/) SDK.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Visual Studio 2022 (v17.8+) or Visual Studio Code
- WinLicense SDK (DLLs must be accessible to the application)

---

## Development Server

To run the backend locally:

```bash
dotnet run
```

By default, the API will be available at:

```
https://localhost:7128
```

You can test the API using tools like Postman, SwaggerUI or through the Angular frontend.

---

## Build

To build the project:

```bash
dotnet build
```

The compiled output will be in the `bin/` directory.

## 🔐 Notes

- Make sure required DLLs from the WinLicense SDK are in the correct location.

---

## Author

Made with ❤️ using .NET 8 and Angular 16.
