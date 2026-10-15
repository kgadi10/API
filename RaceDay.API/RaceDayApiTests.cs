using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Data;
using RaceDay.Models;
using Xunit;

public class RaceDayApiTests : IClassFixture<RaceDayFactory>
{
    private readonly HttpClient _client;

    public RaceDayApiTests(RaceDayFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = true
        });
    }

    private async Task Register(string email, string role)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "RaceDay123!",
            phoneNumber = "0712345678",
            role
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task Login(string email)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email,
            password = "RaceDay123!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RegistrationAndLoginWork()
    {
        await Register("participant1@example.com", "Participant");
        await Login("participant1@example.com");
    }

    [Fact]
    public async Task OrganiserCanCreateEvent()
    {
        await Register("organiser1@example.com", "Organiser");
        await Login("organiser1@example.com");

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Test Community Run",
            eventType = "Running",
            eventDate = "2027-06-20",
            startTime = "07:00:00",
            location = "Polokwane",
            description = "A test running event",
            registrationDeadline = "2027-06-10"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantCannotCreateEvent()
    {
        await Register("participant2@example.com", "Participant");
        await Login("participant2@example.com");

        var response = await _client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Unauthorised Event",
            eventType = "Running",
            eventDate = "2027-06-20",
            startTime = "07:00:00",
            location = "Polokwane",
            description = "Should be rejected",
            registrationDeadline = "2027-06-10"
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ParticipantCanEnrolInAnAvailableCategory()
    {
        await Register("organiser2@example.com", "Organiser");
        await Login("organiser2@example.com");

        var eventResponse = await _client.PostAsJsonAsync("/api/events", new
        {
            eventName = "Enrolment Test Run",
            eventType = "Running",
            eventDate = "2027-06-20",
            startTime = "07:00:00",
            location = "Polokwane",
            description = "Testing enrolment",
            registrationDeadline = "2027-06-10"
        });

        Assert.Equal(HttpStatusCode.Created, eventResponse.StatusCode);

        var eventData = await eventResponse.Content.ReadFromJsonAsync<EventResponse>();
        Assert.NotNull(eventData);

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventData.EventID}/categories", new
            {
                categoryName = "10 KM",
                distanceKM = 10,
                entryFee = 100,
                maximumParticipants = 100
            });

        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);

        var categoryData = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(categoryData);

        await Register("participant3@example.com", "Participant");
        await Login("participant3@example.com");

        var enrolmentResponse = await _client.PostAsJsonAsync("/api/enrolments", new
        {
            eventID = eventData.EventID,
            categoryID = categoryData.CategoryID
        });

        Assert.Equal(HttpStatusCode.Created, enrolmentResponse.StatusCode);
    }

    private sealed class EventResponse
    {
        public int EventID { get; set; }
    }

    private sealed class CategoryResponse
    {
        public int CategoryID { get; set; }
    }
}

public class RaceDayFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<RaceDayDbContext>();
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();

            services.AddDbContext<RaceDayDbContext>(options =>
                options.UseInMemoryDatabase("RaceDayTests_" + Guid.NewGuid()));
        });
    }
}