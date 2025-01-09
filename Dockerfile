FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
# RUN dotnet publish -c Release -o out




# RUN dotnet ef migrations add InitialCreate
# RUN dotnet ef database update

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Install the .NET SDK 8.0 on the runtime container
RUN apt-get update && apt-get install -y \
    wget \
    curl \
    ca-certificates \
    && curl -sSL https://packages.microsoft.com/keys/microsoft.asc | tee /etc/apt/trusted.gpg.d/microsoft.asc \
    && curl -sSL https://packages.microsoft.com/config/ubuntu/20.04/prod.list | tee /etc/apt/sources.list.d/dotnet.list \
    && apt-get update \
    && apt-get install -y dotnet-sdk-8.0

RUN dotnet tool install --global dotnet-ef

# Ensure the PATH includes .NET global tools
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /App
COPY --from=build-env /App .

# Copy the migration script
COPY run-migrations.sh .

# Make the script executable
RUN chmod +x run-migrations.sh
RUN apt-get update && apt-get install -y default-mysql-client jq

# ENTRYPOINT ["dotnet", "XR5_0TrainingRepo.dll"]
ENTRYPOINT ["./run-migrations.sh"]