# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["BlazeJump.Web/BlazeJump.Web.csproj", "BlazeJump.Web/"]
COPY ["BlazeJump.Common/BlazeJump.Common.csproj", "BlazeJump.Common/"]

# Restore dependencies
RUN dotnet restore "BlazeJump.Web/BlazeJump.Web.csproj"

# Copy all source code
COPY . .

# Build and publish the application
WORKDIR "/src/BlazeJump.Web"
RUN dotnet build "BlazeJump.Web.csproj" -c Release -o /app/build
RUN dotnet publish "BlazeJump.Web.csproj" -c Release -o /app/publish

# Runtime stage - Use nginx to serve the static files
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copy the published files from the build stage
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx configuration
COPY ./nginx.conf /etc/nginx/nginx.conf

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
