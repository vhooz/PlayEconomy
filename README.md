# PLAY ECONOMY

## Prerequisites

- [Visual Studio Code](https://code.visualstudio.com)
- .Net Framework
- Docker
- [Latest Node.JS LTS version (64 bit)](https://nodejs.org/en/download)

# Start MongoDB / RabbitMQ with Docker Compose

Note: You need to edit the docker-compose with the correct image for your operating system. The images used are for ARM64 processors

- Data Base: MongoDB
- Message Broker: RabbitMQ (http://localhost:15672/ > guest guest)

```
Play.Infra » docker-compose up
```

# Connect to the MongoDB

    mongodb://localhost:27017/?readPreference=primary&directConnection=true&ssl=false

# Setup Admin

The first time you start the mongoDB, you need to create the admin password for the application.

```
Play.Identity/src/Play.Identity.Service   » dotnet user-secrets init
Play.Identity/src/Play.Identity.Service   » dotnet user-secrets set "IdentitySettings: AdminUserPassword" "Pass@word1"
```

# Start Services

.Net based microservices.

```
Play.Catalog/src/Play.Catalog.Service     » dotnet run
Play.Inventory/src/Play.Inventory.Service » dotnet run
Play.Identity/src/Play.Identity.Service   » dotnet run
```

Register a new account:

    https://localhost:5003/Identity/Account/Login

    player1@play.com / admin@play.com > Pass@word1

# Start Frontend

React based frontend for the Play Economy system.

## To build the frontend

```
Play.Frontend » npm install
```

## To run the frontend locally

```
Play.Frontend » npm start
```

Then navigate to http://localhost:3000 in your browser.
