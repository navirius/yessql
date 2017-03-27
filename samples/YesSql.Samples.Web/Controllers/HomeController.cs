using System;
using Microsoft.AspNetCore.Mvc;
using YesSql.Core.Services;
using Demo.Models;

namespace Demo.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStore _store;

        public HomeController(IStore store)
        {
            _store = store;
            //_store.InitializeAsync().Wait();
            //using (var session = store.CreateSession())
            //{
            //    //deleting a blog posts tables
            //    session.ExecuteMigration(builder => builder.DropTable("BlogPosts"), false);
            //    //creating a blog posts tables
            //    session.ExecuteMigration(builder => builder
            //        .CreateTable("BlogPosts", table => table
            //            .Column<int>("Id", column => column.Identity())
            //            .Column<string>("Title")
            //            .Column<string>("Author")
            //            .Column<string>("Content")
            //            .Column<DateTime>("PublishedUtc")
            //            .Column<string>("Tags")
            //        ), false
            //    );
            //};
        }

        [Route("/")]
        public IActionResult Index()
        {
            // creating a blog post
            var post = new BlogPost
            {
                Title = "Hello YesSql",
                Author = "Bill",
                Content = "Hello",
                PublishedUtc = DateTime.UtcNow,
                Tags = new[] { "Hello", "YesSql" }
            };

            // saving the post to the database
            using (var session = _store.CreateSession())
            {
                session.Save(post);
            }

            // loading an single blog post
            using (var session = _store.CreateSession())
            {
                post = session.QueryAsync<BlogPost>().FirstOrDefault().Result;
            }

            return View(post);
        }
    }
}
