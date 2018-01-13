# api.cabcheap.com
# This is for ensuring that the correct version of OPenId Dict is used
dotnet add package OpenIddict --version 2.0.0-rc2-0772 --source https://www.myget.org/F/aspnet-contrib/api/v3/index.json

# Make sure that you export your dev environment
can be found in startup.

# The order of totally refreshing the databae is
dotnet ef database drop 
# no dummy data
dotnet ef database update 
# Uncomment your dummy data initializer in Startup, and then just run the program normally.