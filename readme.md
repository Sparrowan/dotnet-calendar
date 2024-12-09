# Google Calendar Integration - Setup and Run Instructions

## Overview

This project integrates with Google Calendar to fetch events for an authenticated user. The app allows users to sign in using Google authentication and displays their calendar events.

## Prerequisites

- **.NET Core SDK** (Version 9 or later)
- **Visual Studio** or **Visual Studio Code** with C# extension
- **Google Developer Console Account** for creating a project and obtaining credentials
- **Google Calendar API** enabled in Google Developer Console

## Steps

### 1. Clone the Repository

First, clone the repository to your local machine using the following command:

```bash
git clone git@github.com:Sparrowan/dotnet-calendar.git

```

### 2. Google Developer Console Setup

Follow the steps below to set up the Google Developer Console and obtain the required credentials:

#### a. Create a Google Developer Project

1. Go to the [Google Developer Console](https://console.developers.google.com/).
2. Click on **Create Project**.
3. Enter a project name and click **Create**.

#### b. Enable the Google Calendar API

1. In the Developer Console, navigate to **API & Services** → **Library**.
2. Search for **Google Calendar API** and click **Enable** to enable it for your project.

#### c. Create OAuth 2.0 Credentials

1. Navigate to **API & Services** → **Credentials**.
2. Click **Create Credentials** → **OAuth client ID**.
3. If prompted, configure the **OAuth consent screen** by filling in the required fields.
4. Under **Application Type**, select **Web application**.
5. Set the **Authorized JavaScript origins** (e.g., `http://localhost:5207` for local development).
6. Set the **Authorized redirect URIs** (e.g., `http://localhost:5207/signin-google` for local development).
7. Click **Create** to generate your OAuth credentials.

#### d. Download the Credentials File

1. After creating the OAuth client ID, click the **Download** button next to your credentials to download the `credentials.json` file.
2. Place the `credentials.json` file in the `Credentials` directory in your project folder.
3. Update:
```bash
 "Google": {
    "ClientId": "",
    "ClientSecret": "",
    "Scope": [ "https://www.googleapis.com/auth/calendar.readonly" ],
    "ApplicationName": "Test App",
    "User": "user",  
    "CalendarId": "test"
  }
```
in the ```bash appSettings.json```


### 3. Install Dependencies

Ensure you have all required NuGet packages installed in your project. You can install them by running the following commands in the project directory:

```bash
dotnet add package Google.Apis.Calendar.v3
dotnet add package Google.Apis.Auth
dotnet add package Microsoft.AspNetCore.Authentication.Cookies
dotnet add package Microsoft.AspNetCore.Authentication.Google
dotnet add package Microsoft.Extensions.Logging
```

### 4. Run the app

```bash
dotnet build
dotnet run
````