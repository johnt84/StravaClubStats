var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.StravaClubStatsBlazorServerApp>("stravaclubstatsblazorserverapp");

builder.AddProject<Projects.StravaClubStatsAdminApp>("stravaclubstatsadminapp");

builder.Build().Run();
