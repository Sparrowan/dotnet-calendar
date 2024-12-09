using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Google.Apis.Util.Store;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Google.Apis.Services;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // This action is secured by [Authorize] and requires the user to be authenticated
    [Authorize]
    public async Task<IActionResult> Index()
    {
        try
        {
            // Log info about the authentication process
            _logger.LogInformation("Authenticating to Google API...");

            // Get the calendar service (with valid credentials)
            var calendarService = await GetCalendarServiceAsync();
            _logger.LogInformation("Successfully authenticated to Google API.");

            // Retrieve the calendar events
            var events = await GetCalendarEventsAsync(calendarService);
            _logger.LogInformation("Successfully retrieved calendar events. Event count: {EventCount}", events?.Count ?? 0);

            // Return the events to the view
            return View(events);
        }
        catch (Exception ex)
        {
            // Log any errors
            _logger.LogError(ex, "Error retrieving calendar events.");

            // Return a simple error message as a string directly
            return View("Error", ex.Message);  // Pass the error message directly
        }
    }

    public async Task<IActionResult> Logout()
    {
        // Sign out from the authentication cookie
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Optionally, sign out from Google (if you're using Google authentication)
        await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);

        // Redirect to the home page or any other page
        return RedirectToAction("Index", "Home"); // Redirect to home page after logout
    }

    // This method handles the OAuth process and provides an authenticated Google Calendar service
    private async Task<CalendarService> GetCalendarServiceAsync()
    {
        try
        {
            // Define the path to your credentials file
            var credentialsPath = "Credentials/credentials.json";

            // Log the credentials file path to ensure it's correct
            _logger.LogInformation("Loading credentials from file at: {CredentialsPath}", credentialsPath);

            // Ensure the current directory is logged to verify the correct file location
            _logger.LogInformation("Current directory: {CurrentDirectory}", Directory.GetCurrentDirectory());

            // Check if the credentials file exists before attempting to load it
            if (!System.IO.File.Exists(credentialsPath))
            {
                _logger.LogError("Credentials file not found at: {CredentialsPath}", credentialsPath);
                throw new InvalidOperationException("Credentials file not found.");
            }

            // Load the credentials from the credentials file
            UserCredential credential;

            using (var stream = new FileStream("Credentials/credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { CalendarService.Scope.Calendar },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Test App",
            });
        }
        catch (FileNotFoundException fnfEx)
        {
            // Log and handle case where credentials file is not found
            _logger.LogError(fnfEx, "Credentials file not found.");
            throw new InvalidOperationException("Credentials file not found.", fnfEx);
        }
        catch (Exception ex)
        {
            // Log other exceptions during authentication
            _logger.LogError(ex, "Error during authentication with Google API.");
            throw new InvalidOperationException("Error during authentication with Google API.", ex);
        }
    }

    // This method retrieves events from the authenticated user's Google Calendar
    private async Task<IList<Event>> GetCalendarEventsAsync(CalendarService service)
    {
        try
        {
            // Log info about fetching events
            _logger.LogInformation("Fetching calendar events...");

            // Set up the request to get events from the primary calendar
            var request = service.Events.List("primary");
            request.TimeMin = DateTime.Now;  // Fetch only future events
            request.ShowDeleted = false;     // Do not include deleted events
            request.SingleEvents = true;     // Get recurring events as single occurrences
            request.MaxResults = 10;         // Limit the number of events to fetch
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime; // Order events by start time

            // Timeout for the API request to avoid infinite waiting
            service.HttpClient.Timeout = TimeSpan.FromSeconds(30);  // Set a timeout for the API call

            // Execute the request and fetch the events
            var events = await request.ExecuteAsync();

            // Log the result and return the events
            _logger.LogInformation("Successfully fetched calendar events. Event count: {EventCount}", events.Items?.Count ?? 0);
            return events.Items;
        }
        catch (Exception ex)
        {
            // Log any errors while fetching events
            _logger.LogError(ex, "Error retrieving events from Google Calendar.");
            throw new InvalidOperationException("Error retrieving events from Google Calendar.", ex);
        }
    }
}
