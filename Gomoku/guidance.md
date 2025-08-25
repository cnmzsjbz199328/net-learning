The project demonstrated in the "Learn Live - Build your first microservice with .NET" YouTube video focuses on building and deploying microservices using .NET, Docker, and Docker Compose. This process illustrates how to break down an application into smaller, independent services, containerize them, and manage their deployment and communication.
Here is a detailed, step-by-step walkthrough of the project, from initial setup to running the complete application:
Project Overview: Microservices with .NET and Docker
Microservices are small, independent, and loosely coupled applications, each designed to perform a specific function well. This architectural style allows for individual scaling, independent deployment, and development by small, specialized teams. .NET is a suitable platform for building microservices due to its cloud-native first design, high performance, cross-platform capabilities (Windows, Mac, Linux), and robust tooling.
The project involves:
1. Developing a backend Web API that provides pizza information.
2. Developing a frontend web application that displays this information.
3. Containerizing both services using Docker.
4. Orchestrating their deployment and communication using Docker Compose.
Full Project Workflow
Step 1: Initial Setup and Prerequisites
Before you begin, ensure you have the necessary tools installed:
• Visual Studio Code (VS Code) or Visual Studio 2022 as your Integrated Development Environment (IDE).
• Git for cloning repositories.
• .NET 6 SDK (or later compatible versions) for developing .NET applications.
• Docker Desktop installed, which provides the Docker engine for building and running containers locally.
Step 2: Building the Backend Microservice (Pizza Info API)
The backend is an ASP.NET Core Web API designed to return a list of pizzas.
1. Clone the project repository: Use Git to clone the project containing both backend and frontend code.
    ◦ Command example: git clone [repository_url].
2. Navigate to the backend directory: Change your current directory to the Backend folder within the cloned repository.
    ◦ Command example: cd Backend.
3. Build the .NET application: Compile the backend project using the .NET CLI. This step builds the application without involving Docker.
    ◦ Command example: dotnet build.
4. Run the .NET application locally (optional verification): Start the backend API to ensure it functions correctly outside of a container environment. It will typically run on a default port like 5901.
    ◦ Command example: dotnet run.
5. Verify the API endpoint: Open a web browser or use a tool like Swagger (often built into new ASP.NET Core Web API projects) to access the API endpoint. You should see a JSON response containing pizza details.
    ◦ Example URL: http://localhost:5901/pizza-info.
Step 3: Containerizing the Backend Microservice
This step packages the backend application and its dependencies into a Docker image, which can then be run as a container.
1. Create a Dockerfile: In the root of your backend project directory (Backend folder), create a new file named Dockerfile (with no file extension).
2. Add Dockerfile instructions: These instructions tell Docker how to build the image.
    ◦ FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build: Specifies the base image for the build stage, using the .NET SDK 6.0.
    ◦ WORKDIR /src: Sets the working directory inside the Docker image.
    ◦ COPY Backend.csproj ./: Copies the backend project file to the working directory.
    ◦ RUN dotnet restore: Restores NuGet package dependencies.
    ◦ COPY . .: Copies all remaining source code files into the image.
    ◦ RUN dotnet publish -c Release -o /app: Publishes the application in Release configuration to an /app directory within the image.
    ◦ FROM mcr.microsoft.com/dotnet/aspnet:6.0: Defines the base image for the final runtime stage, using the smaller ASP.NET runtime image.
    ◦ WORKDIR /app: Sets the working directory for the runtime environment.
    ◦ EXPOSE 80: Informs Docker that the container listens on port 80 at runtime.
    ◦ COPY --from=build /app .: Copies the published application from the build stage to the final image's working directory.
    ◦ ENTRYPOINT ["dotnet", "Backend.dll"]: Specifies the command to run when the container starts.
3. Build the Docker image: From the Backend directory, execute the Docker build command. This command uses the Dockerfile to create a Docker image and tags it with a name.
    ◦ Command example: docker build -t pizza-backend ..
4. Verify image creation: Open Docker Desktop to confirm that the pizza-backend image has been created and is available.
5. Run the Docker container (optional verification): Start a container from your newly built image and map an external port to the container's internal port.
    ◦ Command example: docker run -p 5200:80 --name pizza-backend-container pizza-backend (maps host port 5200 to container port 80, names the container).
6. Verify container operation: Access the API using the mapped port from your host.
    ◦ Example URL: http://localhost:5200/pizza-info.
Step 4: Building the Frontend Application
The frontend is a simple ASP.NET Razor Page application that fetches and displays pizza information from the backend API.
1. Frontend project structure: Similar to the backend, the frontend project will have its own Frontend.csproj and a Dockerfile.
2. Frontend Dockerfile: The Dockerfile for the frontend will be similar to the backend's, but it will reference the Frontend.csproj and Frontend.dll. It also exposes port 80 internally.
3. Backend URL configuration: Crucially, the frontend application has a configuration setting (BackendUrl) to specify where it can reach the backend API. Initially, for local development outside Docker Compose, this might be localhost:5901. When using Docker Compose, this will change.
Step 5: Orchestrating Microservices with Docker Compose
To manage both the frontend and backend microservices together, Docker Compose is used. Docker Compose allows you to define and run multi-container Docker applications using a YAML file.
1. Stop any running containers: If the backend container is still running independently, stop and remove it using Docker Desktop or CLI commands.
    ◦ Command example: docker stop pizza-backend-container then docker rm pizza-backend-container.
2. Create docker-compose.yml: In the root directory of your project (one level up from both Backend and Frontend folders), create a file named docker-compose.yml.
3. Define services in docker-compose.yml:
    ◦ version: '3.4': Specifies the Docker Compose file format version.
    ◦ services:: Defines the individual services.
        ▪ backend::
            • image: pizza-backend-image: Assigns an image name.
            • build: ./Backend: Instructs Docker Compose to build the image using the Dockerfile located in the ./Backend directory.
            • ports: - "5900:80": Maps host port 5900 to the container's internal port 80 for the backend service.
        ▪ frontend::
            • image: pizza-frontend-image: Assigns an image name.
            • build: ./Frontend: Instructs Docker Compose to build the image using the Dockerfile in the ./Frontend directory.
            • depends_on: - backend: Specifies that the frontend service depends on the backend service, ensuring backend starts first.
            • environment: - BackendUrl=http://backend: This is a critical configuration. Within the Docker Compose network, backend is the service name, which acts as its DNS name. The frontend uses http://backend to communicate with the backend microservice.
            • ports: - "5902:80": Maps host port 5902 to the container's internal port 80 for the frontend service.
4. Build Docker Compose services: Navigate to the directory containing docker-compose.yml and execute the build command. Docker Compose will build both images, reusing existing ones if no changes were detected.
    ◦ Command example: docker compose build.
5. Run Docker Compose services: Start both services defined in the docker-compose.yml file.
    ◦ Command example: docker compose up.
Step 6: Running the Composed Application
Once Docker Compose is up, both your backend and frontend services will be running as interconnected containers.
1. Access the frontend: Open your web browser and navigate to the port you mapped for the frontend service.
    ◦ Example URL: http://localhost:5902.
2. The frontend application will now fetch pizza data from the backend microservice, demonstrating successful inter-service communication within the Docker network.
Further Exploration and Advanced Concepts
The video and related sources also highlight advanced topics for building resilient and scalable microservice applications:
• Orchestration Beyond Docker Compose: For production-level deployments, more powerful orchestrators like Kubernetes are used. Microsoft offers related services like Azure Container Apps which support serverless, event-driven microservices that can scale to zero.
• Security: Implementing security with Bearer Tokens, particularly JSON Web Tokens (JWT), for authentication and authorization in stateless APIs. Central authorities like IdentityServer can streamline token issuance.
• Data Management: Each microservice typically has its own database, promoting autonomy and allowing selection of the best database technology for its specific task (Database-per-Service pattern).
• Communication Patterns: Beyond simple APIs, microservices can communicate asynchronously using message queues and publish-subscribe models (e.g., RabbitMQ, Azure Service Bus) for long-running processes, enhancing scalability and stability.
• Resiliency and Monitoring: Implementing patterns for fault tolerance like Caching and Message Brokers. Health checks (liveness and readiness probes) are essential for monitoring service status and ensuring maximum uptime.
• API Gateways: A centralized entry point (API Gateway pattern) simplifies client interactions with multiple microservices, handling concerns like logging, security, and rate limiting. The Backend-for-Frontend (BFF) pattern allows customized gateways for different client types (e.g., mobile, web).
• Centralized Logging: Crucial for troubleshooting and monitoring distributed microservices, providing real-time insights across the system.
• Learning Resources: Microsoft Learn offers extensive modules on .NET and microservices, including a full series on modern web development with .NET.