# AgileMinds

This is the Repository as the senior project at University of Florida by the following:

- Carlos Martinez
- Thomas Martin
- Kevin Estrella
- Matthew Strenges
- Ricardo Carvalheira

## Frontend
The frontend structure is composed of AgileMindsUI and AgileMindsUI.Client.
This is somewhat typical in Blazor WebAssembly (WASM) applications, where the solution might include multiple projects with distinct roles.

### AgileMindsUI
Server-side or the shared part of the Blazor WebAssembly project
It might include components, shared logic, and backend integration that can be used across different parts of the application.

### AgileMindsUI.Client 
Blazor WebAssembly (WASM) client-side with includes UI components, pages, and routing for the client-side application that runs in the browser.

The frontend uses the [Mudblazor](https://mudblazor.com/getting-started/installation#online-playground) framework.
- https://localhost:50716
Note: The port above is correct if we run it using 'https' or 'Docker Compose'.

## Backend
WebAPI in the AgileMindsWebAPI to be developed.
API works and must be accessed at one of the following to open it in the browser.
- https://localhost:50714/swagger/index.html

Note: The port above is correct if we run it using 'https' or 'Docker Compose'.

## Testing
### Unit testing
Kindly, install `Unit Test Boilerplate Generator` Extension from [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=RandomEngy.UnitTestBoilerplateGenerator) to generate boiler plates for the test cases.
Using NUnit is probably the way to go along with Moq, but other frameworks can also be used if preferred.

### End-to-End Testing
[Playwright](https://playwright.dev/dotnet/docs/intro) is configured in the AgileMindsTest directory.

### Spinning containers

- Use Visual Studio with `docker-compose` as 'Start Up' configuration
- Use the command line with ```docker compose up -d```.

## Database
MySQL container configured and accessible by WebAPI.

To verify connection via webapi, run:

```sh
docker exec -u root -it AgileMindsWebAPI /bin/bash
apt-get update
sudo apt-get install mariadb-client
mysql -h agilemindmysql -u root -p
```
And enter the password.

Alternatively, you can use your host machine to connect to the db.

We can use [MySQL Workbench](https://www.mysql.com/products/workbench/) to access it if needed.