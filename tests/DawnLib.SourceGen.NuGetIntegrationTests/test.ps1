# Navigate to repository/solution root directory
$MyDir = [System.IO.Path]::GetDirectoryName($MyInvocation.MyCommand.Definition)
Set-Location "$MyDir/../.."

# Build NuGet packages
dotnet pack -c Release -o ./nuget-artifacts

# Restore the project using the custom config file, restoring packages to a local folder
dotnet restore ./tests/DawnLib.SourceGen.NuGetIntegrationTests --packages ./nuget-packages --configfile NuGet.integration-tests.config 

# Build the project (no restore), using the packages restored to the local folder
dotnet build ./tests/DawnLib.SourceGen.NuGetIntegrationTests -c Release --packages ./nuget-packages --no-restore

# Test the project (no build or restore)
dotnet test ./tests/DawnLib.SourceGen.NuGetIntegrationTests -c Release --no-build --no-restore
