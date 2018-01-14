using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Text;
using api.cabcheap.com.Data;
using api.cabcheap.com.Models;

namespace api.cabcheap.com.Data
{
    public class DummyData
    {
        public static void Initialize(ApplicationDbContext db, IServiceProvider services)
        {
             IServiceScopeFactory scopeFactory = services.GetRequiredService<IServiceScopeFactory>();

            using (IServiceScope scope = scopeFactory.CreateScope())
            {

                RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                UserSeedAsync(db, roleManager, userManager).Wait();
                //this ensures that you can run the rest of the function only after the users have been created. 
                //This was done in another project to make sure that users could be queried
 
                if (!(db.Waypoints.Any()))
                {
                    db.Waypoints.AddRange(GetWaypoints(db).ToArray());
                    db.SaveChanges();
                }
                if (!(db.Routes.Any()))
                {
                    db.Routes.AddRange(GetRoutes(db).ToArray());
                    db.SaveChanges();
                }
                if (!(db.Trips.Any()))
                {
                    db.Trips.AddRange(GetTrips(db).ToArray());
                    db.SaveChanges();
                    UpdateRoutesWithTrips(db);
                    db.SaveChanges();
                }
            }
   
        }

        public static async Task UserSeedAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            var isAdminRoleExist = await roleManager.RoleExistsAsync("Admin");
            var isMemberRoleExist = await roleManager.RoleExistsAsync("Member");
            if (!isAdminRoleExist)
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!isMemberRoleExist)
            {
                await roleManager.CreateAsync(new IdentityRole("Member"));
            }
            if (await userManager.FindByNameAsync("The_Editor") == null)
            {
                var user = new ApplicationUser
                {
                    Email = "babeaudry@gmail.com",
                    UserName = "brynbeaudry",
                    FirstName = "Bryn",
                    LastName = "Beaudry",
                    ProviderName = "EMAIL"
                };
                var result = await userManager.CreateAsync(user, "password");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
            if (await userManager.FindByNameAsync("a") == null)
            {
                var user = new ApplicationUser
                {
                    Email = "a@a.a",
                    UserName = "a",
                    ProviderName = "EMAIL"
                };
                var result = await userManager.CreateAsync(user, "password");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                }
            }
            if (await userManager.FindByNameAsync("b") == null)
            {
                var user = new ApplicationUser
                {
                    Email = "b@b.b",
                    UserName = "b",
                    ProviderName = "EMAIL"
                };
                var result = await userManager.CreateAsync(user, "password");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                }
            }
            if (await userManager.FindByNameAsync("c") == null)
            {
                var user = new ApplicationUser
                {
                    Email = "c@c.c",
                    UserName = "c",
                    ProviderName = "EMAIL"
                };
                var result = await userManager.CreateAsync(user, "password");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                }
            }
            if (await userManager.FindByNameAsync("d") == null)
            {
                var user = new ApplicationUser
                {
                    Email = "d@d.d",
                    UserName = "d",
                    ProviderName = "EMAIL"
                };
                var result = await userManager.CreateAsync(user, "password");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Member");
                }
            }
        }

        //Make Routes, leave soe isolated and connect two
        public static List<Waypoint> GetWaypoints(ApplicationDbContext context)
        {
            List<Waypoint> posts = new List<Waypoint>()
            {
                //airport
                new Waypoint()
                {
                    Id = 1,
                    Location = new Location()
                    {
                        Latitude = 49.194269,
                        Longitude = -123.178582
                    },
                },
                //metrotown
                new Waypoint()
                {
                    Id = 2,
                    Location = new Location()
                    {
                        Latitude = 49.226230,
                        Longitude = -122.999153
                    },
                },
                //kitsilano
                new Waypoint()
                {
                    Id = 3,
                    Location = new Location()
                    {
                        Latitude = 49.270591,
                        Longitude = -123.158112
                    },
                },
                //Ubc
                new Waypoint()
                {
                    Id = 4,
                    Location = new Location()
                    {
                        Latitude = 49.260635,
                        Longitude = -123.246002
                    },
                },
                //new west
                new Waypoint()
                {
                    Id = 5,
                    Location = new Location()
                    {
                        Latitude = 49.207866,
                        Longitude =  -122.899418
                    },
                },
            };
            //context.SaveChanges();
            return posts;
        }

        //Make Routes, leave soe isolated and connect two
        public static List<Route> GetRoutes(ApplicationDbContext context)
        {
            List<Route> routes = new List<Route>()
            {
                //metroteon
                new Route()
                {   
                    Id = 1,
                    StartTripPostion = -1,
                    EndTripPostion = -1,
                    StartWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 1),
                    EndWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 2),
                    User = context.ApplicationUsers.FirstOrDefault(x => x.UserName == "a"),
                    Cost = 0,
                    Trip = null,
                },
                //kits
                new Route()
                {
                    Id = 2,
                    StartTripPostion = -1,
                    EndTripPostion = -1,
                    StartWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 1),
                    EndWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 3),
                    User = context.ApplicationUsers.FirstOrDefault(x => x.UserName == "b"),
                    Cost = 0,
                    Trip = null,
                },
                //ubc
                new Route()
                {
                    Id = 3,
                    StartTripPostion = -1,
                    EndTripPostion = -1,
                    StartWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 1),
                    EndWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 4),
                    User = context.ApplicationUsers.FirstOrDefault(x => x.UserName == "c"),
                    Cost = 0,
                    Trip = null,
                },
                //new west
                new Route()
                {
                    Id = 4,
                    StartTripPostion = -1,
                    EndTripPostion = -1,
                    StartWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 1),
                    EndWaypoint = context.Waypoints.FirstOrDefault(x => x.Id == 5),
                    User = context.ApplicationUsers.FirstOrDefault(x => x.UserName == "d"),
                    Cost = 0,
                    Trip = null,
                },
            };
            //context.SaveChanges();
            return routes;
        }

        //Make Routes, leave soe isolated and connect two
        public static List<Trip> GetTrips(ApplicationDbContext context)
        {
            List<Trip> trips = new List<Trip>()
            {
                new Trip()
                {
                    Id = 1,
                    Cost = 0,
                },

            };
            //context.SaveChanges();
            return trips;
        }

        public static void UpdateRoutesWithTrips(ApplicationDbContext context)
        {
            //airport to kitslinao
            var route1OnTrip1 = context.Routes.FirstOrDefault(x => x.Id == 2);
            route1OnTrip1.Trip = context.Trips.FirstOrDefault(x => x.Id == 1);
            route1OnTrip1.StartTripPostion = 0;
            route1OnTrip1.EndTripPostion = 1;
        
            //airport to ubc =
            var route2OnTrip1 = context.Routes.FirstOrDefault(x => x.Id == 3);
            route2OnTrip1.Trip = context.Trips.FirstOrDefault(x => x.Id == 1);
            route2OnTrip1.StartTripPostion = 2;
            route2OnTrip1.EndTripPostion = 3;
            context.SaveChanges();
        }





        /*
        public static List<Event> GetEvents(ApplicationDbContext context)
        {
            List<Event> events = new List<Event>()
            {
                new Event()
                {
                    Activity = new Activity()
                    {
                        ActivityDescription = "Senior’s Golf Tournament"
                    },
                    StartDate = new DateTime(2017, 12, 22, 12, 00, 0),
                    EndDate = new DateTime(2017, 12, 22, 13, 30, 0),
                    IsActive = true,
                    EnteredByUsername = "a",
                },
            //context.SaveChanges();
            return events;
        }
        */
    }
}


