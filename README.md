# Arcsom Asset Management
This project consists of a MAUI client and a Web API. The MAUI client is used to manage assets, manufacturers, and products, while the Web API provides the backend functionality.

## MAUI Client

### Navigation
There is a flyout menu on the left side of the screen that allows you to navigate between different pages:
- **Dashboard**: Main page of the application.
- **Assets**: View and manage assets.
- **Manufacturers**: View and manage manufacturers.
- **Products**: View and manage products.
- **Manage**: Simulate online and offline mode, manual synchronization, and resetting the database.

### Notifications
There are 3 types of notifications. 
- Message box: this is triggered by the ModalErrorHandler service. These show error messages.
- Snackbar messages: these show messages like a reminder that you need to log in.
- Toast messages: these do not work on windows. The test button will trigger this when running the app on android.

### Services
#### SeedDataService
This service is responsible for seeding data into the database. It uses the AppFaker service to generate random data for assets, manufacturers, and products.
#### AppFaker
Specifically designed to generate random data for the application. It uses the Faker.net package to create realistic data for assets, manufacturers, and products.
#### ModalErroHandler
This service handles errors that occur in the application. It displays error messages in a modal dialog.
#### ConnectivityService
This service checks the connectivity status of the device. It provides methods to determine if the device is online or offline.
There is a possibility to simulate online and offline mode in the 'Manage' page.
#### SyncManager
This service manages the synchronization of data between the local database and the Web API. It provides methods to manually synchronize data and reset the database.
When there is no internet connection, the application will use the local database to perform operations. Every operation is then logged in the SyncQueue table.
When the device is online again, the application will empty the SyncQueue table and send the operations to the Web API.
After the whole SyncQueue table is processed, the local database will be updated with the latest data from the Web API.
Each of the database tables has a specific EntitySyncService that handles the synchronization of that specific table.
#### AuthService
This service checks wether the user is authenticated. It checks the JWT token in the local database and verifies its validity.
It removes the token if the user logs out.
#### AuthHeaderHandler
Whenever a request is made to the Web API, this handler adds the JWT token to the Authorization header of the request if needed.




### Seeding data for the database
Running the application will automatically seed data into the database. (if there is none)

Assets, Manufacturers and Products are generated using the 'Faker.net' package.

## API
There are api's available. Check the swagger documentation for more information.
url/swagger
### Creating the database
Create a database on the localhost called 'ArcsomAssets' and change the connection string.

Running the 'dotnet ef database update' command will do the following:
- Create the default identity tables with the exception of the User table, which has a ulong as identifier instead of a uuid.
- Create 3 tables for the business logic, Assets, Manufacturers and Products

### Assets, Manufacturers and Products
Assets, Manufacturers and Products are the main entities in this application. They can be managed through the MAUI client and the Web API.
These contain basic CRUD operations, as well as some additional functionality like searching and filtering.
Authorization is required for these operations.
### Authentication
The Web API uses JWT authentication.
When a user signs in, a JWT token is generated and returned. This token must be included in the Authorization header of subsequent requests to access protected resources.
This token is valid for 2 hours.


This project was created with the help of the following resources:
- MAUI example app
- ChatGPT
- Github Copilot