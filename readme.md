# 🌟 **SprintMaster**

**SprintMaster** is a project management tool developed as part of a senior project at the **University of Florida**, created by the **AgileMinds** development team.

👨‍💻 **AgileMinds Team Members**:
- **Carlos Martinez**
- **Thomas Martin**
- **Kevin Estrella**
- **Matthew Strenges**
- **Ricardo Carvalheira**

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
**🛠️ Unit Testing**
- 🧪 Use Unit Test Boilerplate Generator to generate boilerplates.
- 🛠️ Frameworks: NUnit and Moq.
  
**🎭 End-to-End Testing**
- 🔍 Playwright is configured in the AgileMindsTest directory.

---

## 🚀 Running the Application
**Using Docker Compose**
1. Set docker-compose as the "Start Up" configuration in Visual Studio.
2. Run the following command:
  ```sh
  docker compose up -d
  ```
3. Access the application via the frontend and backend URLs.

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
- Carlos Martinez - GitHub Profile
- Thomas Martin - GitHub Profile
- Kevin Estrella - GitHub Profile
- Matthew Strenges - GitHub Profile
- Ricardo Carvalheira - GitHub Profile

---

## 📝 License
This project is developed as part of a university program. Licensing details can be added here if required.
```sh
  ### How to Use:
  1. Copy and paste the above text into your `README.md` file in your GitHub repository.
  2. Replace placeholder links for team member GitHub profiles (e.g., `[GitHub Profile](#)`) with actual URLs.
  3. Modify the "License" section if your project has a specific license type.
```
