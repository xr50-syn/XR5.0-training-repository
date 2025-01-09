#!/bin/bash

dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet XR5_0TrainingRepo.dll