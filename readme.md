# 🌟 **SprintMaster**

**SprintMaster** is a project management tool developed as part of a senior project at the **University of Florida**, created by the **AgileMinds** development team.

---

## 📖 **Table of Contents**

1. [✨ Features](#-features)
2. [⚙️ Prerequisites](#️-prerequisites)
3. [🖼️ Frontend](#-frontend)
4. [🔧 Backend](#-backend)
5. [🗄️ Database](#-database)
6. [🧪 Testing](#-testing)
   - [🛠️ Unit Testing](#️-unit-testing)
   - [🎭 End-to-End Testing](#-end-to-end-testing)
7. [🚀 Running the Application](#-running-the-application)
8. [🛤️ Roadmap](#️-roadmap)
9. [📃 API Documentation](#-api-documentation)
10. [🤝 Contributors](#-contributors)
11. [📝 License](#-license)

---

## ✨ **Features**

- 📝 **Task Management**: Create, assign, and track tasks in sprints.
- 🔔 **Real-Time Notifications**: Stay updated with real-time alerts.
- 👥 **User Roles and Permissions**: Manage access levels for admins, managers, and team members.
- 🌐 **Responsive Design**: Built with MudBlazor for a modern and seamless user experience.
- 🐳 **Dockerized Deployment**: Simplifies setup and deployment.

---

## ⚙️ **Prerequisites**

Before running the project, ensure you have the following installed:

- ✅ [**.NET SDK 6.0+**](https://dotnet.microsoft.com/download/dotnet/6.0)
- 🐳 [**Docker**](https://www.docker.com/)
- 🗄️ [**MySQL or MySQL Workbench**](https://www.mysql.com/products/workbench/)
- Getting the appsettings.json with the appropriate keys for the webapi project.
---

## 🖼️ **Frontend**

The frontend for **SprintMaster** is built with Blazor WebAssembly (WASM) and consists of two components:

### 📂 **AgileMindsUI**
- Shared logic and backend integrations.

### 📂 **AgileMindsUI.Client**
- UI components and client-side routing.

**Frontend URL**:
- 🌐 https://localhost:50716

> ⚠️ *Note*: Ensure to run the application using `https` or `Docker Compose`.

---

## 🔧 **Backend**

The backend is a Web API named `AgileMindsWebAPI`. Use Swagger to explore and test API endpoints.

**Backend URL**:
- 🌐 https://localhost:50714/swagger/index.html

---

## 🗄️ **Database**

The project uses a MySQL database container accessible via the Web API.

### **Connecting to the Database**
1. Run the following commands to connect to the database container:
   ```sh
   docker exec -u root -it AgileMindsWebAPI /bin/bash
   apt-get update
   sudo apt-get install mariadb-client
   mysql -h agilemindmysql -u root -p
   ```
2. When prompted, enter the database password.
> 💡 Tip: Use MySQL Workbench for an easy database access interface.

---

## 🧪 Testing

Our testing pipeline is integrated with GitHub Actions to automate test execution and validate pull requests. This ensures code quality and reliability by catching issues before they are merged into the main branch.

**🛠️ Unit Testing**
- 🧪 Use Unit Test Boilerplate Generator to generate boilerplates.
- 🛠️ Frameworks: NUnit and Moq.

#### **Tests are located at:**
   ```sh
   C:\Users\cemar\Documents\GitHub\AgileMinds\SmartSprint.Tests
   ```
  
**🎭 End-to-End Testing**
- 🔍 Playwright is configured in the AgileMindsTest directory.

#### **Steps for Codegen**
1. **Build the Project**  
   Open the project in Visual Studio and build it.

2. **Navigate to the Build Directory**  
   Open your Command Prompt or terminal and navigate to the following directory:
   ```pwsh
   AgileMinds\AgileMindsTest\bin\Debug\net8.0
   
3. **Install Playwright**  
   Run the following command to install Playwright:  
   ```pwsh
   pwsh playwright.ps1 install

4. **Generate Tests**
   Use Playwright's codegen tool to generate tests by running:
   ```pwsh   
   pwsh .\playwright.ps1 codegen https://localhost:60001

## 🚀 Running the Application

Make sure you have entered the appropriate keys into the `AgileMindsWebAPI\appsettings.json` or you copy the one given to you to that directory. Then, follow the steps below:

**Using Docker Compose**
1. Set docker-compose as the "Start Up" configuration in Visual Studio.
2. Click on the green button to start the application

Alternatively, create a directory named `certificates\` in the root of the project, if one doesn't exist
1. Run the following commands:
```
dotnet dev-certs https -ep "agileminds.pfx" -p "agileminds" 

dotnet dev-certs https --trust

cd ..
```

If you're on mac, and trusting the certificate like in the steps above doesn't work, you can do the following:

```
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes -subj "/CN=localhost"
openssl pkcs12 -export -out agileminds.pfx -inkey key.pem -in cert.pem -password pass:agileminds
sudo security add-trusted-cert -d -r trustRoot -p ssl -k /Library/Keychains/System.keychain cert.pem
```

Open the certificate, select the option to add it to your mac, and click on Add.

2. Then, trun the following command:
  ```sh
docker compose build
docker compose up -d
  ```
3. Access the application via the frontend and backend URLs:
https://localhost:60001/
---

## 🛤️ Roadmap
- Add notifications.
- Implement user authentication.
- Deploy to a cloud service like Azure or AWS.

## 📃 API Documentation
- Use Swagger at https://localhost:50714/swagger/index.html to explore the available endpoints.
**Sample API Endpoint**
Get All Tasks:
  ```sh
  GET /api/tasks
  Response:
  [
    {
        "id": 1,
        "title": "First Task",
        "completed": false
    }
  ]
  ```

---
  
## 🤝 Contributors
**👨‍💻 Meet the AgileMinds Team:**
- Carlos Martinez - [GitHub Profile](https://github.com/CEMartinezp)
- Thomas Martin - [GitHub Profile](https://github.com/thomas-martin-uf)
- Kevin Estrella - [GitHub Profile](https://github.com/Kstrella)
- Matthew Strenges - [GitHub Profile](https://github.com/Matt-Stre)
- Ricardo Carvalheira - [GitHub Profile](https://github.com/ricardocarva)

---

## 📝 License
This project was developed as part of the senior project for the Computer Science bachelor's program at the University of Florida during the Fall semester of 2024.
