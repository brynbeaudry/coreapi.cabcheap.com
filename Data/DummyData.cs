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

            IServiceScope scope = scopeFactory.CreateScope();
            
                RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                UserSeedAsync(db, roleManager, userManager);


               /*  if (!db.Images.Any())
                {
                    db.Images.AddRange(GetImages(db).ToArray());
                    db.SaveChanges();
                }
                if (!db.Posts.Any())
                {
                    db.Posts.AddRange(GetPosts(db).ToArray());
                    db.SaveChanges();
                }
                if (!db.Comments.Any())
                {
                    db.Comments.AddRange(GetComments(db).ToArray());
                    db.SaveChanges();
                } */
   
        }

        public static async void UserSeedAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
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

        //Make Posts first
        /* public static List<Post> GetPosts(ApplicationDbContext context)
        {
            List<Post> posts = new List<Post>()
            {
                new Post()
                {   
                    Id = 1,
                    Title = "Things are looking up for BC's economy because of CN Rail",
                    Text = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum",
                    Image = context.Images.FirstOrDefault(i => i.Id == 1),
                    CreatedBy = context.ApplicationUsers.FirstOrDefault(u => u.Email.Equals("richardbeaudry@shaw.ca"))
                },
            };
            //context.SaveChanges();
            return posts;
        }
 */




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


