# RaceDay

## System Description

RaceDay is a full-stack web-based event management system designed for the South African road running, walking and cycling community. The system allows event organisers to create and manage sporting events, categories, routes, participant enrolments and race results. Participants can browse upcoming events, enrol in events and view their personal performance history.

The system is designed to reduce the reliance on paper-based registration, spreadsheets and disconnected communication channels by providing a centralised platform for managing road running, walking and cycling events.
Intial raceday repository setup
Add REadme PROJECT
Create initial ERD
Updat ERD user entity
Add events entity toERD
Add Enrolments entity to ERD
Add event API endpoints
Add SQL database schema 
Add SQL sample data
Add github actions validation workflow
Add API authentication endpoint
Add routes entity to ERD
Update ERD relationships
Add raceday system description
add enrolments entity to ERD
ADD CATEGORY API ENDPOINTS
ADD YOUTUBE LINK
ADD sql sample data

## User Roles

### Event Organiser

The Event Organiser is responsible for managing events on the RaceDay platform. Organisers can create, update and delete events, manage event categories and routes, view participant enrolments and record or update participant results.

### Participant

The Participant can create an account, log in, manage their profile, browse upcoming events, view event categories and routes, enrol in events, cancel enrolments and view their personal race performance history.

## Project Documentation

The planning documents for RaceDay are stored in the `/docs` folder.

The documentation includes:

- RaceDay Entity Relationship Diagram (ERD)
- API Endpoint Plan
- SQL Database Script

## Database

The RaceDay database is designed using SQL Server and contains the following entities:

- Users
- Events
- Routes
- Categories
- Enrolments
- Results

The database script creates the database structure, relationships, constraints and sample data required for the RaceDay system.

## CI/CD

GitHub Actions is used to validate the RaceDay repository structure and confirm that the required documentation files are present.

### Successful CI/CD Build



## YouTube Planning Walkthrough

The following unlisted YouTube video demonstrates the RaceDay Part 1 planning process, including the planning documents, ERD decisions, API endpoint plan and execution of the SQL database script in SQL Server Management Studio.

YouTube Video:https://youtu.be/5jrlBPCu9Qg

PASTE YOUR UNLISTED YOUTUBE LINK HERE# API
PROG PART1
# RaceDay API

## 1. Project Overview Part 2

RaceDay is an ASP.NET Core Web API for managing running, walking
and cycling events in South Africa.

The API allows event organisers to manage events, categories, routes,
participant enrolments and results. Participants can view events,
manage their profiles, enrol in events and view their results.

## 2. Technologies

- C#
- ASP.NET Core Web API
- Entity Framework Core Code First
- Microsoft SQL Server
- Cookie-based session authentication
- Password hashing
- Swagger / OpenAPI
- xUnit automated tests
- GitHub Actions

## 3. User Roles

### Organiser
- Create, view, update and delete their events, subject to enrolment rules.
- Manage event categories and routes.
- View enrolments for their events.
- Record, update and delete results for their events.

### Participant
- Register and log in.
- View and update their profile.
- View available events and categories.
- Enrol in categories and cancel their enrolments.
- View their own results.

## 4. Database

The database uses Entity Framework Core Code First and SQL Server.

Entities:
- Users
- Events
- Categories
- Routes
- Enrolments
- Results

The database uses primary keys, foreign keys, unique constraints and
relationships to maintain data integrity.

## 5. Authentication and Security

Passwords are hashed using ASP.NET Core's password hashing service.
The API uses an HTTP-only session cookie and role-based authorisation.
Organiser operations are restricted to the organiser who owns the event.

## 6. Running the Project

1. Install the .NET 8 SDK and SQL Server or LocalDB.
2. Configure the RaceDayConnection connection string.
3. Restore NuGet packages.
4. Run the EF Core database migration commands.
5. Run the API using HTTPS.
6. Open /swagger to explore and test the endpoints.

## 7. Testing

The RaceDay.Tests project contains automated API tests for:
- Registration and login.
- Organiser event creation.
- Participant restrictions on organiser operations.
- Participant enrolment.
Add the Enrolment model.
Add database relationships and constraints.



Add request validation models.


Add the Result model.



Configure the database context.
Create the Web API project.
Add the User model.
Add the Category model.



Add the Route model.

Add the Event model.
